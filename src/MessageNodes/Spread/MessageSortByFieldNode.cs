using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Sort", Category = "Message", Version="Formular", Help = "Sort Messages by a field", Tags = "By Field", Warnings = "BinSizing Keys is ignored", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageSortByFieldNode : AbstractFieldSelectionNode
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("Descending", IsToggle = true)]
        protected IDiffSpread<bool> FDescending;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            FillEnum(Enumerable.Empty<string>());
        }

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

            if (FUseFields.IsAnyInvalid())
            {
                FOutput.FlushResult(FInput);
                return;
            }

            // flatten, to ignore binsize. optimize potential!
            List<EnumEntry> fieldNames = new List<EnumEntry>();
            foreach (var bin in FUseFields)
            {
                foreach (var entry in bin)
                    if (entry != null && !fieldNames.Contains(entry)) fieldNames.Add(entry);
            }

            IOrderedEnumerable<Message> ordered = FInput.OrderBy(message => 1); // noop sort

            for (int i = 0; i < fieldNames.Count; i++)
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
                        deleg = message => 
                                message[fieldName].IsAnyInvalid() 
                            || !message[fieldName].IsComparable() ? 1 // noop, when not possible
                             :  message[fieldName][0];
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