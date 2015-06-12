using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public enum FlowControlEnum { Default, Force, Block};

    #region PluginInfo
    [PluginInfo(Name = "FlowControl", Category = "Message", Help = "Enforces downstream Update", AutoEvaluate = true, Author = "velcrome")]
    #endregion PluginInfo
    public class MessageFlowControlNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput;

        [Input("Update", IsSingle=true, DefaultEnumEntry = "Block")]
        private IDiffSpread<FlowControlEnum> FFlowControl;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            var flow = FFlowControl[0];
            if ((FInput.IsChanged && flow == FlowControlEnum.Default) || // flow only when input changed
                flow == FlowControlEnum.Force || // always flow
                (flow == FlowControlEnum.Default && FFlowControl.IsChanged)) // also flow, when FlowControl resumed
            {
                FOutput.SliceCount = 0;
                FOutput.AssignFrom(FInput);
                FOutput.Flush();
            }
        }
    }
}