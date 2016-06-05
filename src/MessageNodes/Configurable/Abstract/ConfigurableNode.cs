using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class ConfigurableNode :  IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Config("Configuration", DefaultString = "string Foo", Visibility=PinVisibility.True)]
        public IDiffSpread<string> FConfig;

        [Import()]
        protected ILogger FLogger;

        protected abstract void OnConfigChange(IDiffSpread<string> configSpread);
        public abstract void Evaluate(int SpreadMax);

        public ConfigurableNode()
        {
        }

        protected virtual void InitializeWindow() {
        }
        public virtual void OnImportsSatisfied()
        {
            InitializeWindow();
            FConfig.Changed += OnConfigChange;
        }
    }
}
