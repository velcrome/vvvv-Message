using System;
using System.Collections.Generic;
using System.Linq;
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

        [Input("Descending", IsToggle = true)]
        private IDiffSpread<bool> FDescending;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;


#pragma warning restore

        protected override void FillEnum(IEnumerable<string> fields)
        {
            var special = new string[] { "Topic", "TimeStamp" };
            var enums = special.Concat(fields).ToArray();
            EnumManager.UpdateEnum(EnumName, enums.First(), enums);
        }

        public override void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid())
            {
                if (FOutput.SliceCount > 0)
                    FOutput.FlushNil();
                return;
            }
            SpreadMax = FInput.SliceCount;


            if (!FInput.IsChanged && !FUseFields.IsChanged && !FDescending.IsChanged) return;

            ISpread<EnumEntry> fieldNames = FUseFields[0]; // weird access
            if (fieldNames.IsAnyInvalid())
            {
                FOutput.FlushResult(FInput);
                return;
            }

             IOrderedEnumerable<Message> ordered = FInput.OrderBy(message => 1); // noop sort

            for (int i = 0; i < fieldNames.SliceCount; i++)
            {
                var fieldName = fieldNames[i].Name;
                Func<Message, object> deleg;

                switch (fieldName)
                {
                    case "Topic":
                        deleg = message => message.Topic;
                        break;
                    case "TimeStamp":
                        deleg = message => message.TimeStamp.UniversalTime;
                        break;
                    default:
                        deleg = message => message[fieldName].IsAnyInvalid() ? 1: message[fieldName][0];
                        break;
                }

                if (FDescending[i])
                    ordered = ordered.ThenByDescending(deleg);
                else ordered = ordered.ThenBy(deleg);
            }

            FOutput.FlushResult(ordered);

        }


    }
}