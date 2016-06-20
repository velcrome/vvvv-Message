using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class ConfigurableNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Config("Configuration", DefaultString = "string Foo", Visibility = PinVisibility.True)]
        public IDiffSpread<string> FConfig;

        [Import()]
        protected ILogger FLogger;

        protected abstract void OnConfigChange(IDiffSpread<string> configSpread);
        public abstract void Evaluate(int SpreadMax);

        public ConfigurableNode()
        {
        }


        public virtual void OnImportsSatisfied()
        {
            FConfig.Changed += OnConfigChange;
        }
    }
}