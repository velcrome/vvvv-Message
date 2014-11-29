using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

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

        [Input("Filter", DefaultString = "Foo")]
        private ISpread<string> FFilter;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;

            if (SpreadMax <= 0)
                if (FOutput.SliceCount == 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                    return;
                }
                else return;

            foreach (var message in FInput)
            {
                foreach (var fieldName in FFilter)
                    message.Remove(fieldName);

            }
            SpreadMax = FInput.SliceCount;

            FOutput.SliceCount = 0;
            FOutput.Flush();
        }
    }
}