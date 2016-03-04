using System;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Default", Category = "Message", Version = "Formular", Help = "Joins a Message from custom dynamic pins", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageDefaultNode : AbstractFormularableNode
    {
        [Input("New", IsBang = true, DefaultBoolean = false, Order = 0)]
        protected ISpread<bool> FNew;

        [Input("Topic", DefaultString = "Event", Order = 3)]
        protected ISpread<string> FTopic;

        [Input("Spread Count per Formular", IsSingle = true, DefaultValue = 1, Order = 5)]
        protected ISpread<int> FSpreadCount;

        [Output("Output", AutoFlush = false)]
        protected ISpread<ISpread<Message>> FOutput;

        protected bool ForceNewDefaults = true;

        public override void OnImportsSatisfied()
        {
 	        base.OnImportsSatisfied();
            (FWindow as FormularLayoutPanel).Locked = true;
  //          FOutput.Connected += HandleOutputConnect;
        }

        private void HandleOutputConnect(object sender, PinConnectionEventArgs args)
        {
            ForceNewDefaults = true;
        }
        
        protected override void OnConfigChange(IDiffSpread<string> configSpread)
        {
            ForceNewDefaults = true;
        }

        protected override void OnSelectFormular(IDiffSpread<EnumEntry> spread)
        {
            base.OnSelectFormular(spread);

            var window = (FWindow as FormularLayoutPanel);
            var fields = window.Controls.OfType<FieldPanel>();

            foreach (var field in fields) field.Checked = true;
            
            window.Locked = FFormular[0] != MessageFormular.DYNAMIC;

        }


        public override void Evaluate(int SpreadMax)
        {
            // graceful fallback when being fed bad data
            if (FNew.IsAnyInvalid() || FTopic.IsAnyInvalid() || FSpreadCount.IsAnyInvalid())
            {
                FOutput.SliceCount = 0;
                FOutput.Flush();
                return;
            }

            if (!FNew.Any(x => x) && !ForceNewDefaults)
            {
                if (FOutput.SliceCount != 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                return;
            }

            FOutput.SliceCount = FConfig.SliceCount;

            var counter = 0;
            for (int i = 0; i < FConfig.SliceCount; i++)
            {
                FOutput[i].SliceCount = 0;

                var formular = new MessageFormular(FConfig[i], FFormular[0]);
                
                var count = FSpreadCount[i];
                for (int j = 0; j < count; j++)
                {
                    if (FNew[counter] || ForceNewDefaults)
                    {
                        Message message = new Message();
                        message.Topic = FTopic[counter];
                        foreach (var field in formular.FieldNames)
                        {
                            int binsize = formular[field].DefaultSize;
                            binsize = binsize > 0 ? binsize : 1;
                            var type = formular[field].Type;

                            message[field] = BinFactory.New(type, binsize);

                            for (int slice = 0; slice < binsize; slice++)
                            {
                                message[field].Add(TypeIdentity.Instance.NewDefault(type));
                            }
                        }

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