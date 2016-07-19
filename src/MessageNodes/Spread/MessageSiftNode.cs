using System.ComponentModel.Composition;
using System.Linq;

using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Pack.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Sift", Category = "Message.Spread", Help = "Filter Messages", Tags = "Wildcard", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageSiftWildCardNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")] 
        private IDiffSpread<Message> FInput;

        [Input("Filter", DefaultString = "*")] 
        private IDiffSpread<string> FFilter;

        [Output("Output", AutoFlush = false)] 
        private ISpread<Message> FOutput;

        [Output("NotFound", AutoFlush = false)] 
        private ISpread<Message> FNotFound;

        [Import()] protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid() || FFilter.IsAnyInvalid()) 
                SpreadMax = 0;
                else SpreadMax = FInput.SliceCount;
            
            if (SpreadMax == 0)
            {
                FOutput.FlushNil();
                FNotFound.FlushNil();
                return;
            }

            if (!FInput.IsChanged && !FFilter.IsChanged) return;

            FOutput.SliceCount = 0;
            FNotFound.SliceCount = 0;

            bool[] found = new bool[SpreadMax];
            for (int i = 0; i < SpreadMax; i++) found[i] = false;

            for (int i = 0; i < FFilter.SliceCount; i++)
            {
                string[] filter = FFilter[i].Split('.');
                var regex = FFilter[i].CreateWildCardRegex();
                
                for (int j = 0; j < SpreadMax; j++)
                {
                    if (!found[j]) found[j] = regex.IsMatch(FInput[j].Topic);
                }
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                if (found[i]) FOutput.Add(FInput[i]);
                else FNotFound.Add(FInput[i]);
            }
            FOutput.Flush();
            FNotFound.Flush();
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Sift", Category = "Message", Help = "Filter Messages", Tags = "Message", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageSiftMessageNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput;

        [Input("Filter")]
        private IDiffSpread<Message> FFilter;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;

        [Output("Former Index", AutoFlush = false)]
        private ISpread<int> FFormerIndex;

        [Output("Not Found", AutoFlush = false)]
        private ISpread<Message> FNotFound;

        [Output("Not Found Former Index", AutoFlush = false)]
        private ISpread<int> FNotFoundFormerIndex;

        [Import()]
        protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid()) SpreadMax = 0;
                else SpreadMax = FInput.SliceCount;

            if (SpreadMax == 0)
            {
                if (FOutput.SliceCount != 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();

                    FFormerIndex.SliceCount = 0;
                    FFormerIndex.Flush();
                }
                if (FNotFound.SliceCount != 0)
                {
                    FNotFound.SliceCount = 0;
                    FNotFound.Flush(); 
                    FNotFoundFormerIndex.SliceCount = 0;
                    FNotFoundFormerIndex.Flush();
                }
                return;
            }

            if (!FInput.IsChanged && !FFilter.IsChanged) return;

            FOutput.SliceCount = 0;
            FFormerIndex.SliceCount = 0;

            FNotFound.SliceCount = 0;
            FNotFoundFormerIndex.SliceCount = 0;

            for (int i = 0; i < SpreadMax;i++ )
            {
                var message = FInput[i];
                    
                if (FFilter.Contains(message))
                {
                    FOutput.Add(message);
                    FFormerIndex.Add(i);
                }
                else
                {
                    FNotFound.Add(message);
                    FNotFoundFormerIndex.Add(i);
                }
            }

            FOutput.Flush();
            FFormerIndex.Flush();
            
            FNotFound.Flush();
            FNotFoundFormerIndex.Flush();

        }
    }
}