using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Messaging.Core;
using VVVV.Packs.Messaging.Core.Formular;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractStorageNode : TypeableNode
    {
        [Input("Input", Order = 0)]
        public ISpread<Message> FInput;

        public ISpread<EnumEntry> FUseAsID = null;
        private string EnumName;

        [Input("Reset", IsSingle = true, Order = int.MaxValue-1)]
        public ISpread<bool> FReset;

        [Input("Replace Dump", Order = int.MaxValue)]
        public ISpread<List<Message>> FReplaceData;

        [Output("Output", Order = 0)]
        public ISpread<Message> FOutput;

        [Output("Changed Slice", Order = 1)]
        public ISpread<bool> FChanged;

        [Output("Dump", Order = 2)]
        public ISpread<List<Message>> FDump;

        [Import()]
        protected IIOFactory FIOFactory;

        protected List<Message> data = new List<Message>();

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            CreateEnumPin("Use as ID", new string[] { "Foo" });
        }

        public void CreateEnumPin(string pinName, string[] entries)
        {
            EnumName = "Enum_" + this.GetHashCode().ToString();

            EnumManager.UpdateEnum(EnumName, entries[0], entries);

            var attr = new InputAttribute(pinName);
            attr.Order = 2;
            attr.AutoValidate = true;

            attr.EnumName = EnumName;

            Type pinType = typeof(ISpread<EnumEntry>);
            var pin = FIOFactory.CreateIOContainer(pinType, attr);
            FUseAsID = (ISpread<EnumEntry>)(pin.RawIOObject);
        }

        private void FillEnum(string[] entries)
        {
            EnumManager.UpdateEnum(EnumName, entries[0], entries);
        }

        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
            var form = new MessageFormular(configSpread[0]);

            FillEnum(form.Fields.ToArray());
        }


        public List<bool> Match()
        {
            var spreadMax = FInput.SliceCount;
            var changed = new List<bool>(); //init with false
            for (int i = 0; i < data.Count; i++) changed.Add(false);

            var id = FUseAsID[0].Name;
            for (int i = 0; i < spreadMax; i++)
            {
                var fresh = FInput[i];

                // incompatible, if new input has no field with id.
                if (fresh == null || !fresh.Attributes.Contains(id)) continue;

                var matched = (from message in data
                               where message.Attributes.Contains(id)
                               // incompatible if old saved message has no field with id
                               where message[id] == fresh[id]
                               // slicewise check of Bins' equality
                               select message).ToList();

                if (matched.Count == 0)
                {
                    data.Add(FInput[i]); // record message
                    changed.Add(true);   // mark message as changed
                }
                else
                {
                    var found = matched.First(); // found a matching record
                    changed[data.IndexOf(found)] = true; //mark message as changed

                    var k = found += FInput[i]; // copy all attributes from message to matching record
                    found.TimeStamp = FInput[i].TimeStamp; // update time
                }
            }
            return changed;
        }


        public override void Evaluate(int SpreadMax)
        {
            throw new NotImplementedException();
        }
    }
}