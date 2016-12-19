using System;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Default", Category = "Message", Version = "Formular", Help = "Constructs a Message according to a fixed Formular", Tags = "Init, Create", Warnings = "Not optimized for heavy usage.", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageDefaultNode : AbstractFormularableNode
    {
        [Input("New", IsBang = true, DefaultBoolean = false, Order = 0)]
        protected ISpread<bool> FNew;

        // spread it
        [Input("Formular", DefaultEnumEntry = "None", EnumName = MessageFormularRegistry.RegistryName, Order = 2, IsSingle = false)]
        public override IDiffSpread<EnumEntry> FFormularSelection
        {
            get;
            set;
        }


        [Input("Topic", DefaultString = "Event", Order = 3)]
        protected ISpread<string> FTopic;

        [Input("Spread Count", DefaultValue = 1, Order = 5)]
        protected ISpread<int> FSpreadCount;

        [Output("Output", AutoFlush = false)]
        protected ISpread<ISpread<Message>> FOutput;

        protected bool ForceNewDefaults = true;

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            FormularUpdate += (sender, formular) => ForceNewDefaults = true;
        }

        public override MessageFormular Formular
        {
            get
            {
                return base.Formular;
            }
            protected set
            {
                // interject new Formular, make it fit for DefaultNode handling
                if (value != null)
                {
                    if (value.IsDynamic)
                    {
                        value = new MessageFormular(value.Name, "");
                    }
                    else value.Require(RequireEnum.All);
                }
                base.Formular = value;
            }
        }

        public override void Evaluate(int SpreadMax)
        {
            // graceful fallback when being fed bad data
            if (FNew.IsAnyInvalid() || FTopic.IsAnyInvalid() || FSpreadCount.IsAnyInvalid())
            {
                FOutput.FlushNil();
                return;
            }

            if (!FNew.Any() && !ForceNewDefaults)
            {
                FOutput.FlushNil();
                return;
            }

            SpreadMax = FFormularSelection.SliceCount; // numbers of supported Formulars
            FOutput.SliceCount = SpreadMax;

            var counter = 0;
            for (int i = 0; i < SpreadMax; i++)
            {
                var formularName = FFormularSelection[i].Name;
                MessageFormular formular;
                try
                {
                    formular = RetrieveFormular(formularName);
                } catch (RegistryException)
                {
                    formular = null;
                }

                if (formular == null || formular.IsDynamic)
                    formular = new MessageFormular(formularName, ""); // empty fallback, in case not available or dynamic

                FOutput[i].SliceCount = 0;

                var count = FSpreadCount[i];
                for (int j = 0; j < count; j++)
                {
                    if (FNew[counter] || ForceNewDefaults)
                    {
                        Message message = new Message(formular);
                        message.Topic = FTopic[counter];
                        FOutput[i].Add(message);
                    }
                    counter++;
                }
            }
            FOutput.Flush();

            ForceNewDefaults = false;
        }
    }
}