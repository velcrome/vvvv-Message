using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractFieldSelectionNode : AbstractFormularableNode
    {
        public ISpread<ISpread<EnumEntry>> FUseFields = null;
        private string EnumName;

        protected MessageFormular Formular;

        [Import()]
        protected IIOFactory FIOFactory;

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            CreateEnumPin("Use Fields", new string[] { "Foo" });
        }

        public void CreateEnumPin(string pinName, IEnumerable<string> entries)
        {
            EnumName = "Enum_" + this.GetHashCode().ToString();

            EnumManager.UpdateEnum(EnumName, entries.First(), entries.ToArray());

            var attr = new InputAttribute(pinName);
            attr.Order = 2;
            attr.AutoValidate = true;

            attr.BinVisibility = PinVisibility.OnlyInspector;
            attr.BinSize = -1;
            attr.BinOrder = 3;
            attr.CheckIfChanged = true;

            attr.EnumName = EnumName;

            Type pinType = typeof(ISpread<ISpread<EnumEntry>>);
            var pin = FIOFactory.CreateIOContainer(pinType, attr);
            FUseFields = (ISpread<ISpread<EnumEntry>>)(pin.RawIOObject);
        }

        private void FillEnum(IEnumerable<string> fields)
        {
            var enums = fields.ToArray();

            if (enums.Count() > 0)
                EnumManager.UpdateEnum(EnumName, enums.First(), enums);
            else EnumManager.UpdateEnum(EnumName, MessageFormular.DYNAMIC, new string[] { MessageFormular.DYNAMIC });
        }

        protected override void OnConfigChange(IDiffSpread<string> configSpread)
        {
            Formular = new MessageFormular(MessageFormular.DYNAMIC, FConfig[0]);
            FillEnum(Formular.FieldNames);
        }

        protected override void OnSelectFormular(IDiffSpread<EnumEntry> spread)
        {
            base.OnSelectFormular(spread); // inject Formular into 

            var window = (FWindow as FormularLayoutPanel);
            var fields = window.Controls.OfType<FieldPanel>();

            foreach (var field in fields) field.Checked = true;
            window.Locked = FFormular[0] != MessageFormular.DYNAMIC;
        }


    }
}
