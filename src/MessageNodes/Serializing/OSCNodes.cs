using System.Collections.Generic;
using System.IO;
using System.Linq;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

using VVVV.Packs.Messaging.Serializing;

namespace VVVV.Packs.Messaging.Nodes.Serializing
{
    #region PluginInfo
    [PluginInfo(Name = "AsOscBundle", Category = "Message", Version = "Raw", Help = "Outputs OSC Bundle Streams", Tags ="Stream", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageAsOscBundleNode : IPluginEvaluate
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true)]
        protected IDiffSpread<bool> FExtendedMode;
        
        [Output("OSC", AutoFlush = false)]
        protected ISpread<Stream> FOutput;

        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            if (!FInput.IsChanged && !FExtendedMode.IsChanged) return;

            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                FOutput[i] = FInput[i].ToOSC(FExtendedMode[0]);
            }
            FOutput.Flush();
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "AsOscMessage", Category = "Message", Version="Formular", Help = "Outputs OSC Message Streams", Tags = "Stream, Raw", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageAsOscMessageNode : AbstractFieldSelectionNode, IPluginEvaluate
    {
        [Input("Input", Order = 0)]
        protected IDiffSpread<Message> FInput;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true, Order = 1)]
        protected IDiffSpread<bool> FExtendedMode;

        [Output("OSC", AutoFlush = false)]
        protected ISpread<Stream> FOutput;



        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            if (!FInput.IsChanged && !FExtendedMode.IsChanged) return;

            FOutput.SliceCount = SpreadMax;


            for (int i = 0; i < SpreadMax; i++)
            {
                var fields = from fieldName in FUseFields[i]
                             select Formular[fieldName];

                FOutput[i] = FInput[i].ToOSC(fields, FExtendedMode[0]);
            }
            FOutput.Flush();
        }
    }


    #region PluginInfo
    [PluginInfo(Name = "FromOsc", Category = "Message", Version ="Raw", Help = "Converts varied OSC into Messages ", Tags = "OSC", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageFromOscNode : IPluginEvaluate
    {
        [Input("OSC")]
        protected IDiffSpread<Stream> FInput;

        [Input("Additional Topic", DefaultString = "", IsSingle = true)]
        protected IDiffSpread<string> FTopicAdd;

        [Input("Contract Topic Parts", DefaultValue = 1, IsSingle = true, MinValue = 1)]
        protected IDiffSpread<int> FContract;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true, BinVisibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<bool> FExtendedMode;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid())
            {
                SpreadMax = 0;
                if (FOutput.SliceCount != 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                return;
            }
            else SpreadMax = FInput.SliceCount;
            
            if (!FInput.IsChanged && !FTopicAdd.IsChanged && !FContract.IsChanged) return;

            FOutput.SliceCount = 0;

            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = OSCExtensions.FromOSC(FInput[i], FExtendedMode[0], FTopicAdd[0], FContract[0]);
                if (message != null) FOutput.Add(message);
            }
            FOutput.Flush();
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "FromOscMessage", Category = "Message", Version = "Formular", Help = "Converts OSC Bundles into Messages ", Tags = "OSC, Raw", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageFromOscMessageNode : AbstractFieldSelectionNode
    {
        [Input("OSC")]
        protected IDiffSpread<Stream> FInput;

        [Input("Topic", DefaultString = "")]
        protected IDiffSpread<string> FTopic;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true, BinVisibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<bool> FExtendedMode;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        [Output("Match Index", AutoFlush = false)]
        protected Pin<int> FMatch;

        [Output("Success", AutoFlush = false)]
        protected Pin<bool> FSuccess;

        protected Dictionary<string, IEnumerable<FormularFieldDescriptor>> Parser;

        public override void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid())
            {
                SpreadMax = 0;
                FOutput.FlushNil();
                FSuccess.FlushItem<bool>(false);
                FMatch.FlushNil();

                return;
            }
            else SpreadMax = FInput.SliceCount;

            var update = false;

            if (FTopic.IsChanged || FExtendedMode.IsChanged || FUseFields.IsChanged || Parser == null)
            {
                if (Parser == null)
                    Parser = new Dictionary<string, IEnumerable<FormularFieldDescriptor>>();
                    else Parser.Clear();
                
                for (int i=0;i<FTopic.SliceCount;i++)
                {
                    var fields = (
                                    from f in FUseFields[i]
                                    select Formular[f.Name]
                                 ).ToList();
                    Parser[FTopic[i]] = fields;
                }
                update = true;
            }

            if (!update && !FInput.IsChanged) return;

            FSuccess.SliceCount = SpreadMax;
            FOutput.SliceCount = 0;
            FMatch.SliceCount = 0;

            for (int i = 0; i < SpreadMax; i++)
            {
                var stream = FInput[i];
                stream.Position = 0;

                var address = OSCExtensions.PeekAddress(stream);
                stream.Position = 0;

                var isMatch = (
                                from a in FTopic
                                where address == a
                                select true
                                ).Any();

                Message message = isMatch? OSCExtensions.FromOSC(stream, Parser, FExtendedMode[0]) : null;

                if (message != null)
                {
                    FOutput.Add(message);
                    FSuccess[i] = true;
                    FMatch.Add( FTopic.IndexOf(address) );
                }
                else 
                {
                    FSuccess[i] = false;
                }
            }
            FOutput.Flush();
            FSuccess.Flush();
            FMatch.Flush();
        }
    }


}
