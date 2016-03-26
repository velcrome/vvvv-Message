using System;
using System.IO;
using VVVV.Nodes;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Serializing;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;


namespace VVVV.Packs.Messaging.Nodes.Serializing
{
    #region PluginInfo
    [PluginInfo(Name = "AsOscBundle", Category = "Message", Help = "Outputs OSC Bundle Streams", Tags = "Raw", Author = "velcrome")]
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
    [PluginInfo(Name = "AsOscMessage", Category = "Message", Help = "Outputs OSC Message Streams", Tags = "Raw", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageAsOscMessageNode : AbstractFormularableNode, IPluginEvaluate
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true)]
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
                FOutput[i] = FInput[i].ToOSC(FExtendedMode[0]);
            }
            FOutput.Flush();
        }

        protected override void OnConfigChange(IDiffSpread<string> configSpread)
        {
            throw new NotImplementedException();
        }
    }


    #region PluginInfo
    [PluginInfo(Name = "AsMessage", Category = "Raw", Help = "Converts OSC Bundles into Messages ", Tags = "OSC", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageOscAsMessageNode : IPluginEvaluate
    {
        #pragma warning disable 649, 169
        [Input("OSC")]
        IDiffSpread<Stream> FInput;

        [Input("Additional Topic", DefaultString = "", IsSingle = true)]
        IDiffSpread<string> FTopicAdd;

        [Input("Contract Topic Parts", DefaultValue = 1, IsSingle = true, MinValue = 1)]
        IDiffSpread<int> FContract;

        [Input("ExtendedMode", IsSingle = true, IsToggle = true, DefaultBoolean = true, BinVisibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FExtendedMode;

        [Output("Output", AutoFlush = false)]
        ISpread<Message> FOutput;
        #pragma warning restore

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

}
