using System;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public delegate void FormularChanged(MessageFormular formular);

    public abstract class AbstractFormularableNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Config("Configuration", DefaultString = "string Foo", Visibility = PinVisibility.True)]
        private IDiffSpread<string> FConfig;

        [Input("Message Formular", DefaultEnumEntry = "None", EnumName = MessageFormularRegistry.RegistryName, Order = 2, IsSingle = true)]
        public IDiffSpread<EnumEntry> FFormularSelection;

        #region event

        public event FormularChanged Changed;

        #endregion event

        #region import services
        [Import]
        protected IHDEHost FHDEHost;

        [Import]
        protected IPluginHost2 PluginHost;

        [Import()]
        protected ILogger FLogger;

        protected bool SkippedFirst;
        public virtual void OnImportsSatisfied()
        {
            // Enum pin selects new formular
            FFormularSelection.Changed += OnSelectFormular;

            // base provider of Formulars
            var context = MessageFormularRegistry.Context;
            context.FormularChanged += FormularRemotelyChanged;

            // make sure the enum exits, can be empty and will be populated later
            EnumManager.UpdateEnum(MessageFormularRegistry.RegistryName, context.AllFormularNames.First(), context.AllFormularNames.ToArray());


            WatchConfig(true);
        }
        #endregion import services


        #region Formular Handling

        protected MessageFormular _formular = new MessageFormular(MessageFormular.DYNAMIC, "string Foo" );
        public virtual MessageFormular Formular
        {
            get
            {
                return _formular;
            }
            protected set
            {
                if (value == null) throw new ArgumentNullException("Formular cannot be null in " + GetType().Name + " at " + PluginHost.GetNodePath(false));

                _formular = value;

                var newConfig = _formular.Configuration;
                if (FConfig[0] != newConfig)
                {
                    WatchConfig(false);
                    FConfig[0] = newConfig;
                    WatchConfig(true);
                }

                Changed(_formular);
            }
        }

        protected void WatchConfig(bool activate)
        {
            if (activate)
                FConfig.Changed += OnConfig;
            else FConfig.Changed -= OnConfig;

        }

        private void OnConfig(IDiffSpread<string> spread)
        {
            // usually just the Default of the pin, any saved data will come next
            if (!SkippedFirst)
            {
                SkippedFirst = true;
                return;
            }

            if (FFormularSelection.IsAnyInvalid())
                Formular = new MessageFormular(MessageFormular.DYNAMIC, FConfig[0]);
            else Formular = new MessageFormular(FFormularSelection[0], FConfig[0]);
        }
        #endregion Formular Handling



        protected virtual void OnSelectFormular(IDiffSpread<EnumEntry> spread)
        {
            if (FFormularSelection.IsAnyInvalid())
            {
                FLogger.Log(LogType.Warning, "["+ this.GetType().Name + "] - Select a Formular. ID = " + PluginHost.GetNodePath(false));
                return;
            }

            var formularName = FFormularSelection[0].Name;

            Formular = RetrieveRegisteredFormular(formularName);
        }

        protected MessageFormular RetrieveRegisteredFormular(string formularName)
        {
            MessageFormular formular;

            if (formularName != MessageFormular.DYNAMIC)
            {
                var fromReg = MessageFormularRegistry.Context[formularName];
                if (fromReg == null) throw new RegistryException("[" + this.GetType().Name + "] - Tried to retrieve an unknown Formular \"" + formularName + "\". ID = " + PluginHost.GetNodePath(false));

                formular = fromReg.Clone() as MessageFormular; // safeguard against hot tampering with the formular in the registry
            }
            else
            {
                formular = new MessageFormular(MessageFormular.DYNAMIC, FConfig[0]); // fallback to what's been known to the node
            }
            return formular;
        }

        #region node formular update during runtime

        /// <summary>
        /// This will 
        /// </summary>
        /// <param name="sender">Usually Context</param>
        /// <param name="e">Contains the new Formular</param>
        protected virtual void FormularRemotelyChanged(MessageFormularRegistry sender, FormularChangedEventArgs e)
        {
            if (FFormularSelection.IsAnyInvalid()) return;  // strong typing yet undecided

            var used = from formularEntry in FFormularSelection
                       where formularEntry.Name == e.FormularName
                       where formularEntry.Name != MessageFormular.DYNAMIC
                       select true;

            if (used.Any())
            {
                Formular = e.Formular.Clone() as MessageFormular; // keep a copy
            }
        }

        #endregion node update during runtime

        #region Evaluate

//        protected abstract void OnConfigChange(IDiffSpread<string> configSpread);
        public abstract void Evaluate(int SpreadMax);
        #endregion

    }
}