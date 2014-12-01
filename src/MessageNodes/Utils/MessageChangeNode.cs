using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Change", Category = "Message", Help = "Allows Feedback Loops for Messages",
        Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageChangeNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput;

        [Output("OnChange")]
        private ISpread<bool> FChanged;

        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            FChanged.SliceCount = 1;
            if (FInput.IsChanged)
            {
                FChanged[0] = !FInput.IsAnyInvalid();

            }
        }
    }
}