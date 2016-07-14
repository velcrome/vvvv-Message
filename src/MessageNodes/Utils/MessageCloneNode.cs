using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Clone", Category = "Message", Help = "Clones Messages to prevent overriding an instance somewhere else. Slow.",
        Author = "velcrome")]
    #endregion PluginInfo
    public class MessageCloneNode : IPluginEvaluate
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
            if (FInput.IsChanged)
            {
                var clones = from message in FInput
                             where message != null
                             select message.Clone() as Message;

                FOutput.FlushResult(clones);
            }
        }
    }
}