using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            (FWindow as FormularLayoutPanel).Locked = true;
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
            EnumManager.UpdateEnum(EnumName, fields.First(), fields.ToArray());
        }

        protected override void OnConfigChange(IDiffSpread<string> configSpread)
        {
            Formular = new MessageFormular(MessageFormular.DYNAMIC, FConfig[0]);
            FillEnum(Formular.FieldNames.ToArray());
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
