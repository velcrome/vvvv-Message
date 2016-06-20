using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractFormularableNode : ConfigurableNode, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Message Formular", DefaultEnumEntry = "None", EnumName = MessageFormularRegistry.RegistryName, Order = 2, IsSingle = true)]
        public IDiffSpread<EnumEntry> FFormularSelection;

        [Import]
        protected IHDEHost FHDEHost;

        [Import]
        protected IPluginHost2 PluginHost;


        protected MessageFormular _formular = new MessageFormular(MessageFormular.DYNAMIC, "string Foo" );
        public virtual MessageFormular Formular
        {
            get
            {
                return _formular;
            }
            protected set
            {
                _formular = value;

                var newConfig = _formular.Configuration;
                if (FConfig[0] != newConfig) FConfig[0] = newConfig;
            }
        }

        protected bool SkippedFirst;

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            // base provider of Formulars
            var reg = MessageFormularRegistry.Instance;
            reg.FormularChanged += FormularRemotelyChanged;

            EnumManager.UpdateEnum(MessageFormularRegistry.RegistryName, reg.AllFormularNames.First(), reg.AllFormularNames.ToArray());


            FConfig.Changed += OnConfig;
        }

        private void OnConfig(IDiffSpread<string> spread)
        {
            // usually just the Default of the pin, any saved data will come next
            if (!SkippedFirst)
            {
                SkippedFirst = true;

                FFormularSelection.Changed += OnSelectFormular;

                // base provider of Formulars
                var reg = MessageFormularRegistry.Instance;
                reg.FormularChanged += FormularRemotelyChanged;

                return;
            }

            if (FFormularSelection.IsAnyInvalid())
                Formular = new MessageFormular(MessageFormular.DYNAMIC, FConfig[0]);
                else Formular = new MessageFormular(FFormularSelection[0], FConfig[0]);
        }


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
                var fromReg = MessageFormularRegistry.Instance[formularName];
                if (fromReg == null) throw new RegistryException("[" + this.GetType().Name + "] - Tried to retrieve an unknown Formular \"" + formularName + "\". ID = " + PluginHost.GetNodePath(false));

                formular = fromReg.Clone() as MessageFormular;

                foreach (var field in formular.FieldDescriptors) field.IsRequired = false;
            }
            else
            {
                formular = new MessageFormular(MessageFormular.DYNAMIC, FConfig[0]);
            }
            return formular;
        }



        #region node formular update during runtime
        protected virtual void FormularRemotelyChanged(MessageFormularRegistry sender, FormularChangedEventArgs e)
        {
            if (FFormularSelection.IsAnyInvalid()) return;  // strong typing yet undecided

            var used = from formularEntry in FFormularSelection
                       where formularEntry.Name == e.FormularName
                       where formularEntry.Name != MessageFormular.DYNAMIC
                       select true;

            if (used.Any())
            {
                Formular = e.Formular;
            }

        }


        #endregion node update during runtime

    }
}