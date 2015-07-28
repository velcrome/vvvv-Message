using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
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
#pragma warning disable 649, 169
        [Input("Input")] 
        IDiffSpread<Message> FInput;

        [Input("Clear", IsSingle = true, IsToggle = true)] 
        IDiffSpread<bool> FInit;

        [Output("Output", AutoFlush = false, AllowFeedback = true)] 
        ISpread<Message> FOutput;

        private List<Message> lastFrame;

        [Import()] protected ILogger FLogger;
#pragma warning restore

        public MessageFrameDelayNode()
        {
            lastFrame = new List<Message>();
        }


        public void Evaluate(int SpreadMax)
        {
            if (FInit[0]) {
                if (FOutput.SliceCount > 0) {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                return;
            }

            lastFrame.Clear();

            if (FInput.IsChanged && !FInput.IsAnyInvalid())
            {
                lastFrame.AddRange(FInput);

                FOutput.SliceCount = lastFrame.Count;
                FOutput.AssignFrom(lastFrame);
                FOutput.Flush();
            }
        }


 

    }
}