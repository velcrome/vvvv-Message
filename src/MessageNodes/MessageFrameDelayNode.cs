using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes
{
    using Message = VVVV.Packs.Message.Core.Message;

    #region PluginInfo
    [PluginInfo(Name = "FrameDelay", Category = "Message", Help = "Allows Feedback Loops for Messages",
        Tags = "velcrome", AutoEvaluate = true)]
    #endregion PluginInfo
    public class MessageFrameDelayNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")] private IDiffSpread<Packs.Message.Core.Message> FInput;

        [Input("Default")] private ISpread<Packs.Message.Core.Message> FDefault;

        [Input("Initialize", IsSingle = true, IsBang = true)] private IDiffSpread<bool> FInit;

        [Output("Message", AutoFlush = false, AllowFeedback = true)] private ISpread<Packs.Message.Core.Message> FOutput;

        private List<Packs.Message.Core.Message> lastFrame;

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