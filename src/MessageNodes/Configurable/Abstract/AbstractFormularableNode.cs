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
//        protected bool FirstFrame = true;

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
            reg.TypeChanged += FormularChanged;

            // dummy enum, will be populated from registry
            EnumManager.UpdateEnum(reg.RegistryName, reg.Keys.First(), reg.Keys.ToArray());

            FFormular.Changed += (e) => OnChangeFormula(this, new FormularChangedEventArgs(null));
            ((FormularLayoutPanel)FWindow).Change += OnChangeFormula;

            FConfig.Changed += (e) => OnChangeFormula(this, new FormularChangedEventArgs(new MessageFormular(FConfig[0]) ));
        }


        #region node formular update during runtime


        
        private void OnChangeFormula(object sender, FormularChangedEventArgs e)
        {
            var isDynamic = FFormular.IsAnyInvalid()  || FFormular[0] == MessageFormular.DYNAMIC;


            MessageFormular formular = null;
            if (!FFormular.IsAnyInvalid() && !isDynamic) // 
            {
                var formularName = FFormular[0].Name;
                formular = MessageFormularRegistry.Instance[formularName];
            }

            formular = e.Formular == null ? formular : e.Formular;

            if (formular != null)
            {
                var layoutPanel = FWindow as FormularLayoutPanel;
                layoutPanel.CanEditFields = isDynamic;

                FConfig[0] = layoutPanel.Configuration;
                layoutPanel.Formular = formular;
            }
        }


        private void FormularChanged(MessageFormularRegistry sender, FormularChangedEventArgs e)
        {
            if (FFormular.IsAnyInvalid()) return;  // strong typing yet undecided

            var used = from formularEntry in FFormular
                       where formularEntry.Name == e.FormularName
                       select true;

            if (used.Any(use => use))
            {
                var formular = SetFormular(e.Formular, false);
                if (formular != null) FConfig[0] = (FWindow as FormularLayoutPanel).Configuration;
            }
        }
        #endregion node update during runtime

        #region update gui
        //protected MessageFormular SetWindowFromConfig(string config)
        //{
        //    var formular = new MessageFormular(config);
        //    return SetFormular(formular, true);
        //}

        protected virtual MessageFormular SetFormular(MessageFormular formular, bool forceChecked = false) 
        {
            var layoutPanel = FWindow as FormularLayoutPanel;
            layoutPanel.LayoutByFormular(formular, forceChecked); // no potential side effects by passing a reference from the registry, fields will be cloned 
            layoutPanel.CanEditFields = (FFormular.SliceCount == 0) || FFormular[0].Name == MessageFormular.DYNAMIC;

            layoutPanel.Invalidate();
            return formular; 
        }
        #endregion update gui


    }
}