using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Sort", Category = "Message Formular", Help = "Sort Messages by a field", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageSortByFieldNode : AbstractFieldSelectionNode
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;


#pragma warning restore

        public override void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid())
                SpreadMax = 0;
            else SpreadMax = FInput.SliceCount;

            if (SpreadMax == 0)
            {
                FOutput.FlushNil();
                return;
            }

            if (!FInput.IsChanged && !FUseFields.IsChanged) return;

            FOutput.SliceCount = 0;

            var fieldNames = FUseFields[0];
            if (fieldNames.IsAnyInvalid())
            {
                FOutput.FlushResult(FInput);
            }
            else
            {
                var ordered = FInput.OrderBy(message => message[fieldNames[0].Name][0]);

                for (int i = 1; i < fieldNames.SliceCount; i++)
                    ordered = ordered.ThenBy(message => message[fieldNames[i].Name][0]);

                FOutput.FlushResult(ordered);
            }
        }
    }
}