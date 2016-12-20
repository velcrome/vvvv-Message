using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "FrameDelay", Category = "Message", Help = "Allows Feedback Loops for Messages. Can be NIL'ed at will.",
        Author = "velcrome", AutoEvaluate = true)]
    #endregion PluginInfo
    public class MessageFrameDelayNode : IPluginEvaluate
    {
        [Input("Input")] 
        public IDiffSpread<Message> FInput;

        [Input("Clear", IsSingle = true, IsToggle = true)] 
        public IDiffSpread<bool> FClear;

        [Output("Output", AutoFlush = false, AllowFeedback = true)] 
        public ISpread<Message> FOutput;

        protected List<Message> LastFrame = new List<Message>();

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged)
                LastFrame.AssignFrom(FInput);

            if (!FClear.IsAnyInvalid() && FClear[0])
            {
                FOutput.FlushNil();
            }
            else FOutput.FlushResult(LastFrame);
        }
    }
}