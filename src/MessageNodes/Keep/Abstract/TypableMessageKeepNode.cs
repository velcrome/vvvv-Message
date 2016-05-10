using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class TypableMessageKeepNode : AbstractMessageKeepNode
    {
        public const string TOPIC = "Topic";
        public ISpread<EnumEntry> FUseAsID = null;
        private string EnumName;

        protected MessageFormular Formular = new MessageFormular(MessageFormular.DYNAMIC, "string Foo");

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            CreateEnumPin("Use as ID", new string[] { TOPIC });

            (FWindow as FormularLayoutPanel).Locked = true;
        }

        public void CreateEnumPin(string pinName, IEnumerable<string> entries)
        {
            EnumName = "Enum_" + this.GetHashCode().ToString();

            EnumManager.UpdateEnum(EnumName, entries.First(), entries.ToArray());

            var attr = new InputAttribute(pinName);
            attr.Order = 2;
            attr.AutoValidate = true;

            attr.EnumName = EnumName;

            Type pinType = typeof(ISpread<EnumEntry>);
            var pin = FIOFactory.CreateIOContainer(pinType, attr);
            FUseAsID = (ISpread<EnumEntry>)(pin.RawIOObject);
        }

        private void FillEnum(IEnumerable<string> fields)
        {
            var entries = Enumerable.Repeat(TOPIC, 1).Concat(fields);
            EnumManager.UpdateEnum(EnumName, entries.First(), entries.ToArray());
        }

        protected override void OnConfigChange(IDiffSpread<string> configSpread)
        {
            Formular = new MessageFormular("temp", configSpread[0]);
            FillEnum(Formular.FieldNames.ToArray());
        }

        protected override void OnSelectFormular(IDiffSpread<EnumEntry> spread)
        {
            base.OnSelectFormular(spread);

            var window = (FWindow as FormularLayoutPanel);
            var fields = window.Controls.OfType<FieldPanel>();

            foreach (var field in fields) field.Checked = true;
            window.Locked = FFormular[0] != MessageFormular.DYNAMIC;
        }
    }
}