using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;


namespace VVVV.Packs.Messaging.Nodes
{
    public enum FlowControlEnum{
        Default,
        Inspect,
        SinceLastFrame,
        Block,
        Force
    };

    #region PluginInfo
    [PluginInfo(Name = "Change", Category = "Message", Tags = "Flow", Help = "Detects and handles upstream changes and forwards them downstream.",
        Author = "velcrome")]
    #endregion PluginInfo
    public class MessageChangeNode : IPluginEvaluate
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("Update", IsSingle=true, DefaultEnumEntry = "Default")]
        protected IDiffSpread<FlowControlEnum> FFlowControl;

        [Input("Filter", IsToggle = true, IsSingle = true, DefaultBoolean = true)]
        protected ISpread<bool> FFilter;

        [Input("Force", IsSingle =true, IsBang = true)]
        protected ISpread<bool> FForce;

        [Output("Change", IsBang = true, AutoFlush = false)]
        protected ISpread<bool> FChange;
        
        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            var flow = FFlowControl[0];
            FChange.SliceCount = 0;

            if (FForce[0]) flow = FlowControlEnum.Force;

            switch (flow)
            {
                case FlowControlEnum.Default:
                    if (FInput.IsChanged && !FInput.IsAnyInvalid())
                    {
                        FOutput.FlushResult(FInput);
                        FChange.FlushBool(true);
                    }
                    else FChange.FlushBool(false);
                    break;
                case FlowControlEnum.Inspect:
                    var change = FInput.Select(message => message.IsChanged);
                    if (FInput.IsChanged || change.Any())
                    {
                        if (FFilter[0])
                            FOutput.FlushResult(FInput.Where(message => message.IsChanged));
                            else FOutput.FlushResult(FInput);
                        FChange.FlushResult(change);
                    }
                    else FChange.FlushBool(false);
                    break;
                case FlowControlEnum.SinceLastFrame:
                    var changed = FInput.Select(message => message.HasRecentCommit());
                    if (FInput.IsChanged || changed.Any())
                    {
                        if (FFilter[0])
                            FOutput.FlushResult(FInput.Where(message => message.HasRecentCommit()));
                        else FOutput.FlushResult(FInput);
                        FChange.FlushResult(changed);
                    }
                    else FChange.FlushBool(false);
                    break;
                case FlowControlEnum.Block:
                    FChange.FlushBool(false);
                    break;
                case FlowControlEnum.Force: 
                    FOutput.FlushResult(FInput);
                    FChange.FlushBool(true);
                    break;
                default: // all bases covered
                    FChange.FlushBool(false);
                    break;
            }
        }
    }
}