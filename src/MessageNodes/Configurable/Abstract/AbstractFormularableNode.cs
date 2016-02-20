using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractFormularableNode : ConfigurableNode, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Message Formular", DefaultEnumEntry = "None", EnumName = "VVVV.Packs.Message.Core.Formular", Order = 2)]
        public IDiffSpread<EnumEntry> FFormular;
        protected bool FirstFrame = true;

        [Import]
        protected IHDEHost FHDEHost;



        protected override void InitializeWindow()
        {
            FWindow = new PinDefinitionPanel();
            Controls.Add(FWindow);
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            FFormular.Changed += HandleTypeChange;
            ((PinDefinitionPanel)FWindow).OnChange += ConfigNextFrame;

            var reg = MessageFormularRegistry.Instance;
            reg.TypeChanged += FormularChanged;

            EnumManager.UpdateEnum(reg.RegistryName, reg.Keys.First(), reg.Keys.ToArray());

            FHDEHost.MainLoop.OnRender += ConfigurePins;
        }

        private void ConfigNextFrame(object sender, System.EventArgs e)
        {
            FHDEHost.MainLoop.OnRender += ConfigurePins;
        }

        private void ConfigurePins(object sender, System.EventArgs e)
        {
            FirstFrame = false;
            FHDEHost.MainLoop.OnRender -= ConfigurePins;
            var forms = SetFormularFromConfig();

//            FConfig[0] = forms[0].ToString();
        }

        protected virtual void HandleTypeChange(IDiffSpread<EnumEntry> spread)
        {
            if (!FirstFrame) SetFormularFromRegistry();
        }

        public virtual IList<MessageFormular> SetFormularFromConfig()
        {
            var formular = new MessageFormular(FConfig[0]);

            var forms = SetFormular(formular, true);
            return forms;

        }

        public virtual IList<MessageFormular> SetFormularFromRegistry()
        {
            if (FFormular.IsAnyInvalid() || FFormular[0]==MessageFormular.DYNAMIC) return SetFormularFromConfig();

            var form = FFormular[0].Name;
            var formular = MessageFormularRegistry.Instance[form];

            var forms = SetFormular(formular);
            return forms;

        }

        protected virtual IList<MessageFormular> SetFormular(MessageFormular formular, bool forceChecked = false) 
        {
            var forms = new List<MessageFormular>();
            var rows = FWindow.Controls.OfType<RowPanel>().ToList();
                       
            var pinDef = FWindow as PinDefinitionPanel;
            pinDef.LayoutByFormular(formular, forceChecked);
            forms.Add(formular);
                
            return forms; //returns the Formulars.
        }

        protected virtual void FormularChanged(MessageFormularRegistry sender, MessageFormularChangedEvent e)
        {
            if (FFormular.IsAnyInvalid()) return;

            var used = false;
            
            foreach (var type in FFormular) 
                if (type.Name == e.Formular.Name) used = true;

            if (used) SetFormularFromRegistry();
        }

  
    }
}