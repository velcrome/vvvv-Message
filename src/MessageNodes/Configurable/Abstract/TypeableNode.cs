using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Packs.Message.Core.Formular;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes
{
    public abstract class TypeableNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Message Formular", DefaultEnumEntry = "None", IsSingle = true, EnumName = "VVVV.Packs.Message.Core.Formular")]
        public IDiffSpread<EnumEntry> FType;

        [Config("Configuration", DefaultString = "string Foo")]
        public IDiffSpread<string> FConfig;

        [Import()]
        protected ILogger FLogger;

        public virtual void OnImportsSatisfied()
        {
            FConfig.Changed += HandleConfigChange;
            FType.Changed += HandleTypeChange;
            MessageFormularRegistry.Instance.TypeChanged += ConfigChanged;
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

        protected abstract void HandleConfigChange(IDiffSpread<string> configSpread);
        public abstract void Evaluate(int SpreadMax);

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