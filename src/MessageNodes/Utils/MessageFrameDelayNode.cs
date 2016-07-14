using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "FrameDelay", Category = "Message", Help = "Allows Feedback Loops for Messages",
        Author = "velcrome", AutoEvaluate = true)]
    #endregion PluginInfo
    public class MessageFrameDelayNode : IPluginEvaluate
    {
        [Input("Input")] 
        public IDiffSpread<Message> FInput;

        [Input("Clear", IsSingle = true, IsToggle = true)] 
        public IDiffSpread<bool> FInit;

        [Output("Output", AutoFlush = false, AllowFeedback = true)] 
        public ISpread<Message> FOutput;

        protected List<Message> _lastFrame = new List<Message>();

        [Import()] protected ILogger FLogger;

        public void Evaluate(int SpreadMax)
        {
            if (FInit[0]) {
                FOutput.FlushNil();
                return;
            }

            _lastFrame.Clear();

            if (FInput.IsChanged && !FInput.IsAnyInvalid())
            {
                _lastFrame.AddRange(FInput);
                FOutput.FlushResult(_lastFrame);
            }
        }
    }
}