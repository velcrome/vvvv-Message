using System;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractFormularableNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Config("Configuration", DefaultString = "string Foo", Visibility = PinVisibility.True)]
        private IDiffSpread<string> FConfig;

        [Input("Message Formular", DefaultEnumEntry = "None", EnumName = MessageFormularRegistry.RegistryName, Order = 2, IsSingle = true)]
        public IDiffSpread<EnumEntry> FFormularSelection;

        #region event
        /// <summary>
        /// Informs all subscribers about a pending change. 
        /// </summary>
        /// <remarks>Formular will reflect the status quo, the event argument the new version.</remarks>
        public event FormularChanged FormularUpdate;

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

            // make sure the enum exits, at least with "None", might be populated later during runtime
            EnumManager.UpdateEnum(MessageFormularRegistry.RegistryName, context.AllFormularNames.First(), context.AllFormularNames.ToArray());

            // (mandatory) changes to the config will make sure, the pin layout is restored during loading, even when not all input pins are valid
            WatchConfig(true);
        }

        public void Dispose()
        {
            // base provider of Formulars
            var context = MessageFormularRegistry.Context;
            context.FormularChanged -= FormularRemotelyChanged;

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

                var newConfig = value.Configuration;
                if (FConfig[0] != newConfig)
                {
                    WatchConfig(false);
                    FConfig[0] = newConfig;
                    WatchConfig(true);
                }
                if (FormularUpdate != null) FormularUpdate(this, value); // raise event before actual update

                _formular = value;

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
            if (!SkippedFirst)
            {
                // usually just the Default of the pin, any saved data will come next
                SkippedFirst = true;
                return;
                // default of the pin already mimmicks default Formular for this node
            }

            var formular = Formular;
            formular.Require(RequireEnum.None);

            if (FFormularSelection.IsAnyInvalid())
                formular.Name = MessageFormular.DYNAMIC;
            else formular.Name = FFormularSelection[0].Name;

            // overwrite fiels that occur in FConfig
            var config = new MessageFormular(MessageFormular.DYNAMIC, FConfig[0]);
            foreach (var field in config.FieldDescriptors)
            {
                formular[field.Name] = field;
            }

            Formular = formular;
        }
        #endregion Formular Handling


        #region node formular update during runtime
        protected virtual void OnSelectFormular(IDiffSpread<EnumEntry> spread)
        {
            if (FFormularSelection.IsAnyInvalid()  || string.IsNullOrWhiteSpace( FFormularSelection[0].Name) )
            {
                FLogger.Log(LogType.Warning, "[\""+ this.GetType().Name + "\"] - Select a Formular. ID = " + PluginHost.GetNodePath(false));
                return;
            }

            var formularName = FFormularSelection[0].Name;
            if (Formular.Name == formularName) return;

            var backup = new MessageFormular(formularName, FConfig[0]); // local backup
            try
            {
                MessageFormular formular;
                if (formularName != MessageFormular.DYNAMIC)
                {
                    formular = RetrieveFormular(formularName);
                    formular.Require(RequireEnum.NoneBut, backup);
                }
                else {
                    formular = backup; // fallback to what's been known to the node
                    formular.Name = formularName;
                }

                Formular = formular;
            }
            catch (RegistryException)
            {
                Formular = backup;
            }
        }

        /// <summary>
        /// Tries to retrieve a valid Formular by name from the registry 
        /// </summary>
        /// <param name="formularName"></param>
        /// <exception cref="RegistryException">When Formular is unknown to the registry</exception>
        /// <returns>A clone of the Formular in the registry</returns>
        protected MessageFormular RetrieveFormular(string formularName)
        {
            MessageFormular formular;

            var fromReg = MessageFormularRegistry.Context[formularName];
            if (fromReg == null) throw new RegistryException("[" + this.GetType().Name + "] - Tried to retrieve an unknown Formular \"" + formularName + "\". ID = " + PluginHost.GetNodePath(false));

            formular = fromReg.Clone() as MessageFormular; // safeguard against hot tampering with the formular in the registry
            return formular;
        }

        /// <summary>
        /// This will be called when a Formular is changed in the registry, to make all affected nodes comply with potential re-configuration
        /// </summary>
        /// <param name="sender">Usually Context</param>
        /// <param name="e">Contains the new Formular</param>
        protected virtual void FormularRemotelyChanged(MessageFormularRegistry sender, MessageFormular formular)
        {
            if (FFormularSelection.IsAnyInvalid()) return;  // before and during first frame input pins might not be valid yet


            // little silly to expect spreaded formular input here, because it spreads nowhere else :(
            var used = from formularEntry in FFormularSelection
                       where formularEntry.Name == formular.Name
                       where formularEntry.Name != MessageFormular.DYNAMIC
                       select true;

            if (used.Any())
            {
                formular = formular.Clone() as MessageFormular; // keep a copy
                formular.Require(RequireEnum.NoneBut, Formular);
                Formular = formular;
            }
        }

        #endregion node update during runtime

        #region Evaluate

        public abstract void Evaluate(int SpreadMax);

        #endregion

    }
}