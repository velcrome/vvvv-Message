using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;


namespace VVVV.Packs.Messaging.Nodes
{
    public enum FlowControlEnum { Block, Default, Inspect, Force };

    #region PluginInfo
    [PluginInfo(Name = "Change", Category = "Message.Spread", Help = "Detects and handles upstream changes and forwards them downstream.",
        Author = "velcrome")]
    #endregion PluginInfo
    public class MessageChangeNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput;

        [Input("Update", IsSingle=true, DefaultEnumEntry = "Default")]
        private IDiffSpread<FlowControlEnum> FFlowControl;
        
        [Output("Change")]
        private ISpread<bool> FChange;
        
        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;


        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            var flow = FFlowControl[0];
            FChange.SliceCount = 0;

            switch (flow)
            {
                case FlowControlEnum.Block:
                    FChange.AssignFrom(Enumerable.Repeat(false, 1));
                    break;
                case FlowControlEnum.Default:
                    if (FInput.IsChanged && !FInput.IsAnyInvalid())
                    {
                        FOutput.SliceCount = 0;
                        FOutput.AssignFrom(FInput);
                        FOutput.Flush();

                        FChange.AssignFrom(Enumerable.Repeat(true, 1));
                    }
                    else FChange.AssignFrom(Enumerable.Repeat(false, 1));
                    break;
                case FlowControlEnum.Inspect:
                    var change = FInput.Select(message => message.IsChanged);
                    if (FInput.IsChanged || change.Any())
                    {
                        FOutput.SliceCount = 0;
                        FOutput.AssignFrom(FInput);
                        FOutput.Flush();
    
                        FChange.AssignFrom(change);
                    }
                    else FChange.AssignFrom(Enumerable.Repeat(false, 1));
                    break;
                case FlowControlEnum.Force: 
                    FOutput.SliceCount = 0;
                    FOutput.AssignFrom(FInput);
                    FOutput.Flush();

                    FChange.AssignFrom(Enumerable.Repeat(true, 1));
                    break;
                default: 
                    FChange.AssignFrom(Enumerable.Repeat(false, 1)); 
                    break;
            }
        }
    }
}