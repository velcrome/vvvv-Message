using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class ConfigurableNode :  UserControl, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Config("Configuration", DefaultString = "string Foo")]
        public IDiffSpread<string> FConfig;

        [Import()]
        protected ILogger FLogger;

        protected Panel FWindow;

        protected abstract void HandleConfigChange(IDiffSpread<string> configSpread);
        public abstract void Evaluate(int SpreadMax);

        public ConfigurableNode()
        {
            InitializeWindow();
        }

        protected virtual void InitializeWindow() {
            FWindow = new PicturePanel();
            Controls.Add(FWindow);

        }
        public virtual void OnImportsSatisfied()
        {
            FConfig.Changed += HandleConfigChange;
        }
    }
}
