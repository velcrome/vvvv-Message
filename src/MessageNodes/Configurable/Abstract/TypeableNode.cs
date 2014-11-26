using System.ComponentModel.Composition;

using VVVV.Packs.Message.Core.Formular;
using VVVV.PluginInterfaces.V2;
using System.Linq;

namespace VVVV.Packs.Message.Nodes
{
    public abstract class TypeableNode : ConfigurableNode, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Message Formular", Order = 1, DefaultEnumEntry = "None", IsSingle = true, EnumName = "VVVV.Packs.Message.Core.Formular")]
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

          #region cast tools
        protected VVVV.PluginInterfaces.V2.NonGeneric.ISpread ToISpread(IIOContainer pin)
        {
            return (VVVV.PluginInterfaces.V2.NonGeneric.ISpread)(pin.RawIOObject);
        }

        protected VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread ToIDiffSpread(IIOContainer pin)
        {
            return (VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread)(pin.RawIOObject);
        }
        protected VVVV.PluginInterfaces.V2.ISpread<T> ToGenericISpread<T>(IIOContainer pin)
        {
            return (VVVV.PluginInterfaces.V2.ISpread<T>)(pin.RawIOObject);
        }
        #endregion cast tools
    }
}