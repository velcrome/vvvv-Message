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
#pragma warning disable 649, 169
        [Input("New", IsToggle = true, IsSingle = true, DefaultBoolean = true, Order = 0)]
        ISpread<bool> FNew;

        [Input("Topic", DefaultString = "Event", Order = 3, BinSize=1)]
        ISpread<ISpread<string>> FTopic;

        [Input("Spread Count", IsSingle = true, DefaultValue = 1, Order = 4)]
        ISpread<int> FSpreadCount;

        [Output("Output", AutoFlush = false)]
        ISpread<ISpread<Message>> FOutput;
#pragma warning restore

        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
        }

        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = (FNew.IsAnyInvalid() || !FNew.Any(x => x) || FTopic.IsAnyInvalid() || FSpreadCount.IsAnyInvalid()) ? 0 : FTopic.CombineWith(FSpreadCount).CombineWith(FType);
            
            if (SpreadMax == 0)
            {
                if (FOutput.SliceCount != 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                return;
            }

            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var formular = new MessageFormular(FConfig[i]);
                var count = FSpreadCount[i];
                FOutput[i].SliceCount = count;

                for (int j = 0; j < count; j++)
                {
                    Message message = new Message();
                    message.Topic = FTopic[i][j];
                    foreach (var field in formular.Fields)
                    {
                        message[field] = Bin.New(formular[field].Type);

                        var binsize = formular[field].DefaultSize;
                        if (binsize > 0) message[field].SetCount(binsize);
                    }

                    FOutput[i][j] = message;
                }
            }
            FOutput.Flush();
        }
    }
}