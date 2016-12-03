using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Prune", Category = "Message", Help = "Removes all Fields with the indicated name from any Message ", Author = "velcrome")]
    #endregion PluginInfo
    public class MessagePruneIndicatedNode : IPluginEvaluate
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("Prune FieldNames", DefaultString = "Foo")]
        protected IDiffSpread<string> FFilter;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && !FFilter.IsChanged) return;
            if (FInput.IsAnyInvalid())
            {
                if (FOutput.SliceCount > 0) FOutput.FlushNil();
                return;
            }

            foreach (var message in FInput)
            {
                foreach (var fieldName in message.Fields.ToArray())
                    if (FFilter.Contains(fieldName))
                        message.Remove(fieldName);
            }

            FOutput.FlushResult(FInput);
        }
    }
}
