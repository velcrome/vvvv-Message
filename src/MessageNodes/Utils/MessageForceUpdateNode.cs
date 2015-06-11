using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "ForceUpdate", Category = "Message", Help = "Enforces downstream Update", AutoEvaluate = true, Author = "velcrome")]
    #endregion PluginInfo
    public class MessageForceUpdateNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput;

        [Input("Update", IsSingle=true)]
        private IDiffSpread<bool> FUpdate;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged || FUpdate[0])
            {
                FOutput.SliceCount = 0;
                FOutput.AssignFrom(FInput);
                FOutput.Flush();
            }
        }
    }
}