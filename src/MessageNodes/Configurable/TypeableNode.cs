using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Packs.Message.Core.Registry;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes
{
    public abstract class TypeableNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Message Type", DefaultString = "None", IsSingle = true)]
        public IDiffSpread<string> FType;

        [Config("Configuration", DefaultString = "string Foo")]
        public IDiffSpread<string> FConfig;

        [Import()]
        protected ILogger FLogger;

        public virtual void OnImportsSatisfied()
        {
            FConfig.Changed += HandleConfigChange;
            TypeRegistry.Instance.TypeChanged += ConfigChanged;
        }

        protected virtual void ConfigChanged(TypeRegistry sender, TypeChangedEvent e)
        {
            if (e.TypeName == FType[0]) FConfig[0] = e.Config;
        }

        protected abstract void HandleConfigChange(IDiffSpread<string> configSpread);
        public abstract void Evaluate(int SpreadMax);
    }
}