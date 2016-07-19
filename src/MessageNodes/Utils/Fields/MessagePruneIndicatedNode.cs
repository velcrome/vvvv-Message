using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Prune", Category = "Message", Help = "Removes all fields with the indicated Name of any Message ", Author = "velcrome")]
    #endregion PluginInfo
    public class MessagePruneIndicatedNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput;

        [Input("Prune FieldNames", DefaultString = "Foo")]
        private ISpread<string> FFilter;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;

            if (SpreadMax <= 0)
            {
                FOutput.FlushNil();
                return;
            }

            var keepOnly = new HashSet<string>(FFilter);

            foreach (var message in FInput)
            {
                foreach (var fieldName in message.Fields.ToArray())
                    if (keepOnly.Contains(fieldName))
                        message.Remove(fieldName);
            }

            FOutput.FlushResult(FInput);
        }
    }
}
