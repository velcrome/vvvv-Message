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
        [Input("Message Formular", DefaultEnumEntry = "None", EnumName = MessageFormularRegistry.RegistryName, Order = 2, IsSingle = true)]
        public IDiffSpread<EnumEntry> FFormular;

        [Import]
        protected IHDEHost FHDEHost;

        [Import]
        protected IPluginHost2 PluginHost;

        protected bool SkippedFirst;

        protected override void InitializeWindow()
        {
            // don't call inherited InitializeWindow, so the placeholder pic will disappear
            
            FWindow = new FormularLayoutPanel();
            Controls.Add(FWindow);
            var reg = MessageFormularRegistry.Instance;

            // make sure the enum is available.
            EnumManager.UpdateEnum(MessageFormularRegistry.RegistryName, reg.AllFormularNames.First(), reg.AllFormularNames.ToArray());
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            // base provider of Formulars
            var reg = MessageFormularRegistry.Instance;
            reg.FormularChanged += FormularRemotelyChanged;

            FFormular.Changed += OnSelectFormular;
            ((FormularLayoutPanel)FWindow).Change += OnChangeLayout;

            FConfig.Changed += OnConfig;
        }

        private void OnConfig(IDiffSpread<string> spread)
        {
            // usually just the Default of the pin, any saved data will come next
            if (!SkippedFirst)
            {
                SkippedFirst = true;
                return;
            }

            var formular = new MessageFormular(MessageFormular.DYNAMIC, FConfig[0]);

            if (!FFormular.IsAnyInvalid()) formular.Name = FFormular[0].Name; 
            UpdateWindow(formular, true);
        }


        private void OnChangeLayout(object sender, FormularChangedEventArgs e)
        {
            var config = e.Formular.Configuration;
            if (config != FConfig[0]) FConfig[0] = config;
        }


        protected virtual void OnSelectFormular(IDiffSpread<EnumEntry> spread)
        {
            if (FFormular.IsAnyInvalid())
            {
                FLogger.Log(LogType.Warning, "["+ this.GetType().Name + "] - Select a Formular. ID = " + PluginHost.GetNodePath(false));
                return;
            }

            var formularName = FFormular[0].Name;

            if (formularName != MessageFormular.DYNAMIC)
            {
                var fromReg = MessageFormularRegistry.Instance[formularName];
                if (fromReg == null) return;

                var formular = new MessageFormular(formularName, fromReg.FieldDescriptors);

                foreach (var field in formular.FieldDescriptors) field.IsRequired = false;

                UpdateWindow(formular);

                var newConfig = (FWindow as FormularLayoutPanel).Formular.Configuration;
                if (FConfig[0] != newConfig) FConfig[0] = newConfig;
            }
            else
            {
                (FWindow as FormularLayoutPanel).CanEditFields = true;
                var formular = new MessageFormular(MessageFormular.DYNAMIC, FConfig[0]);
                UpdateWindow(formular);
            }

        }


        #region node formular update during runtime
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

        private void UpdateWindow(MessageFormular formular, bool append = false)
        {
            if (formular == null) return;

            var layoutPanel = FWindow as FormularLayoutPanel;

            if (append) {
                var old = layoutPanel.Formular;
                foreach (var field in formular.FieldDescriptors)
                    old.Append(field, true);
                formular = old;
            }
            
            layoutPanel.Formular = formular;
            layoutPanel.CanEditFields = formular.IsDynamic;
        }
        #endregion node update during runtime

    }
}