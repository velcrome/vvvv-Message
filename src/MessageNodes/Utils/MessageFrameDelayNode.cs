using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "FrameDelay", Category = "Message", Help = "Allows Feedback Loops for Messages",
        Tags = "velcrome", AutoEvaluate = true)]
    #endregion PluginInfo
    public class MessageFrameDelayNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")] private IDiffSpread<Message> FInput;

        [Input("Default")] private ISpread<Message> FDefault;

        [Input("Initialize", IsSingle = true, IsBang = true)] private IDiffSpread<bool> FInit;

        [Output("Message", AutoFlush = false, AllowFeedback = true)] private ISpread<Message> FOutput;

        private List<Message> lastFrame;

        [Import()] protected ILogger FLogger;
#pragma warning restore

        public MessageFrameDelayNode()
        {
            lastFrame = new List<Message>();
        }


        public void Evaluate(int SpreadMax)
        {
            if (FInit[0])
            {
                lastFrame.Clear();
                lastFrame.AddRange(FDefault);
            }
            else
            {
                lastFrame.Clear();

                if (FInput.SliceCount > 0 && FInput[0] != null) lastFrame.AddRange(FInput);
            }

            FOutput.SliceCount = lastFrame.Count;

            FOutput.AssignFrom(lastFrame);
            FOutput.Flush();
        }


 

    }
}