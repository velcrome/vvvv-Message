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
            MessageFormularRegistry.Instance.TypeChanged += ConfigChanged;
        }

        protected virtual void ConfigChanged(MessageFormularRegistry sender, MessageFormularChangedEvent e)
        {
            if (e.Formular.Name == FType[0]) FConfig[0] = e.Formular.ToString();
        }

        protected abstract void HandleConfigChange(IDiffSpread<string> configSpread);
        public abstract void Evaluate(int SpreadMax);
    }
}