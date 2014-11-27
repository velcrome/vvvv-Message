using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "RemoveBin", Category = "Message", Help = "Filter Messages", Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageRemoveBinNode : IPluginEvaluate
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

        [Import()]
        protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged) return;

            SpreadMax = FInput.SliceCount;

            FOutput.SliceCount = 0;
            FNotFound.SliceCount = 0;
            bool[] found = new bool[SpreadMax];
            for (int i = 0; i < SpreadMax; i++) found[i] = false;

            for (int i = 0; i < FFilter.SliceCount; i++)
            {
                string[] filter = FFilter[i].Split('.');

                for (int j = 0; j < SpreadMax; j++)
                {
                    if (!found[j]) found[j] = FInput[j].AddressMatches(FFilter[i]);
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
}