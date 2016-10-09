using System;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Common", Category = "Message", Help = "Find identical Messages in two spreads", Tags = "Message", Author = "velcrome", Ignore = true)]
    #endregion PluginInfo
    public class MessageSiftMessageNode : IPluginEvaluate
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("Filter")]
        protected IDiffSpread<Message> FFilter;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        [Output("Former Index", AutoFlush = false)]
        protected ISpread<int> FFormerIndex;

        [Output("Not Found", AutoFlush = false)]
        protected ISpread<Message> FNotFound;

        [Output("Not Found Former Index", AutoFlush = false)]
        protected ISpread<int> FNotFoundFormerIndex;

        [Import()]
        protected ILogger FLogger;

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

            for (int i = 0; i < SpreadMax; i++)
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
