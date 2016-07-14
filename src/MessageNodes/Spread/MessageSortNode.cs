using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Pack.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Sort", Category = "Message", Help = "Sort Messages by time", Tags = "Time", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageSortNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid())
                SpreadMax = 0;
            else SpreadMax = FInput.SliceCount;

            if (SpreadMax == 0)
            {
                FOutput.FlushNil();
                return;
            }

            if (!FInput.IsChanged) return;

            var result = FInput.OrderBy(message => message.TimeStamp.UniversalTime);
            FOutput.FlushResult(result);
        }
    }
}