using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractFormularableNode : ConfigurableNode, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Message Formular", DefaultEnumEntry = "None", EnumName = "VVVV.Packs.Message.Core.Formular", Order = 2)]
        public IDiffSpread<EnumEntry> FFormular;

        [Import]
        protected IHDEHost FHDEHost;

        protected override void InitializeWindow()
        {
            // don't call inherited InitializeWindow, so the placeholder pic will disappear
            
            FWindow = new FormularLayoutPanel();
            Controls.Add(FWindow);
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            // base provider of Formulars
            var reg = MessageFormularRegistry.Instance;
            reg.TypeChanged += FormularRemotelyChanged;

            // dummy enum, will be populated from registry
            EnumManager.UpdateEnum(reg.RegistryName, reg.Keys.First(), reg.Keys.ToArray());

            FFormular.Changed += OnSelectFormular;
            ((FormularLayoutPanel)FWindow).Change += OnChangeLayout;

            // one shot
   //         FConfig.Changed += OnInitConfig;
        }



        private void OnChangeLayout(object sender, FormularChangedEventArgs e)
        {
            var config = e.Formular.Configuration;
            if (config != FConfig[0]) FConfig[0] = config;
        }


        protected virtual void OnSelectFormular(IDiffSpread<EnumEntry> spread)
        {
            var formularName = FFormular[0].Name;

            if (formularName != MessageFormular.DYNAMIC)
            {
                var formular = MessageFormularRegistry.Instance[formularName];
                OnChangeFormular(this, new FormularChangedEventArgs(formular));

                var newConfig = (FWindow as FormularLayoutPanel).Formular.Configuration;
                if (FConfig[0] != newConfig) FConfig[0] = newConfig;
            }
            else
            {
                (FWindow as FormularLayoutPanel).CanEditFields = true;
                var formular = new MessageFormular(FConfig[0], MessageFormular.DYNAMIC);
                OnChangeFormular(this, new FormularChangedEventArgs(formular));
            }

        }


        #region node formular update during runtime
        private void OnChangeFormular(object sender, FormularChangedEventArgs e)
        {
            if (e.Formular == null) return;

            var layoutPanel = FWindow as FormularLayoutPanel;
            layoutPanel.CanEditFields = e.Formular.IsDynamic;
            layoutPanel.Formular = e.Formular;
        }


        private void FormularRemotelyChanged(MessageFormularRegistry sender, FormularChangedEventArgs e)
        {
            if (FFormular.IsAnyInvalid()) return;  // strong typing yet undecided

            var used = from formularEntry in FFormular
                       where formularEntry.Name == e.FormularName
                       where formularEntry.Name != MessageFormular.DYNAMIC
                       select true;

            if (used.Any(use => use))
            {
                var layoutPanel = FWindow as FormularLayoutPanel;

                layoutPanel.CanEditFields = e.Formular.IsDynamic;
                layoutPanel.Formular = e.Formular;
            }

            var newConfig = (FWindow as FormularLayoutPanel).Formular.Configuration;
            if (FConfig[0] != newConfig) FConfig[0] = newConfig;
        }
        #endregion node update during runtime

    }
}