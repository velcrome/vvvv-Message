using System.ComponentModel.Composition;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using System.Linq;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractFormularableNode : ConfigurableNode, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Message Formular", DefaultEnumEntry = "None", IsSingle = true, EnumName = "VVVV.Packs.Message.Core.Formular", Order = 2)]
        public IDiffSpread<EnumEntry> FType;

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            FType.Changed += HandleTypeChange;
            var reg = MessageFormularRegistry.Instance;
            reg.TypeChanged += ConfigChanged;

            EnumManager.UpdateEnum(reg.RegistryName, reg.Keys.First(), reg.Keys.ToArray());
        }

        private void HandleTypeChange(IDiffSpread<EnumEntry> spread)
        {
            var form = FType[0].Name;
            if (form != MessageFormular.DYNAMIC) FConfig[0] = MessageFormularRegistry.Instance[form].ToString(true);
        }

        protected virtual void ConfigChanged(MessageFormularRegistry sender, MessageFormularChangedEvent e)
        {
            if (!FType.IsAnyEmpty() && e.Formular.Name == FType[0].Name) FConfig[0] = e.Formular.ToString(true);
        }

  
    }
}