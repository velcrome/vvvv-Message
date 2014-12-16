using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Distinct", Category = "Message.Spread", Help = "Removes redundancy and null",
        Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageDistinctNode : IPluginEvaluate
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
            {
                FOutput.SliceCount = 0;
                FOutput.Flush();
                return;
            }

            if (FInput.IsChanged)
            {

                var filtered = (from message in FInput
                                where message != null
                                select message).Distinct();

                FOutput.SliceCount = 0;
                FOutput.AssignFrom(filtered);
                FOutput.Flush();
            }
        }
    }
}