using System;
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
#pragma warning disable 649, 169
        [Input("New", IsToggle = true, IsSingle = true, DefaultBoolean = true, Order = 0)]
        ISpread<bool> FNew;

        [Input("Address", DefaultString = "Event", Order = 3)]
        ISpread<string> FAddress;

        [Input("Spread Count", IsSingle = true, DefaultValue = 1, Order = 4)]
        ISpread<int> FSpreadCount;


        [Output("Output", AutoFlush = false)]
        Pin<Message> FOutput;
#pragma warning restore

        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
        }

        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = 0;
            if (!FNew[0])
            {
                FOutput.SliceCount = 0;
                FOutput.Flush();
                return;
            }

            if (!FSpreadCount.IsAnyInvalid()) SpreadMax = FSpreadCount[0];

            var formular = new MessageFormular(FConfig[0]);

            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = new Message();
                

                message.Address = FAddress[i];
                foreach (var field in formular.Fields)
                {
                    message[field] = Bin.New(formular[field].Type);

                    var binsize = formular[field].DefaultSize;
                    if (binsize > 0) message[field].SetCount(binsize);
            
                }

                
                FOutput[i] = message;
            }
            FOutput.Flush();
        }
    }
}