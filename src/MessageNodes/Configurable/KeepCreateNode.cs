using System;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Create", AutoEvaluate = true, Category = "Message.Keep", Help = "Joins a permanent Message from custom dynamic pins", Version = "Formular", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageKeepCreateNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Set", IsBang = true, Order = 0)]
        ISpread<bool> FSet;

        [Input("Address", DefaultString = "State", Order = 1)]
        ISpread<string> FAddress;

        [Input("Reset", IsBang = true, Order = int.MaxValue - 2)]
        ISpread<bool> FReset;

        [Input("Spread Count", IsSingle = true, DefaultValue = 1, Order = int.MaxValue - 1)]
        ISpread<int> FSpreadCount;

        [Output("Output", AutoFlush = false)]
        Pin<Message> FOutput;

        protected MessageKeep MessageKeep = new MessageKeep(); // same as in AbstractStorageNode. 

#pragma warning restore


        protected override IOAttribute DefinePin(FormularFieldDescriptor configuration)
        {
            var attr = new InputAttribute(configuration.Name); 
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = configuration.DefaultSize;
            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount + 1;
            //attr.AutoValidate = false;  // need to sync all pins manually. Don't forget to Flush()
            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            bool totalReset = FConfig.IsChanged  || FReset[0];
            var oldCount =MessageKeep.Count;

            if (totalReset) MessageKeep.Clear();

            SpreadMax = FSpreadCount[0];
            SpreadMax = SpreadMax < 0 ? 0 : SpreadMax; // safeguard against negative binsizes
            FOutput.SliceCount = SpreadMax;

            bool anyChanged = false; 
            if (SpreadMax < oldCount) // remove superfluous entries
            {
                anyChanged = true;
                MessageKeep.RemoveRange(SpreadMax, oldCount - SpreadMax);
            }

            var formular = new MessageFormular(FConfig[0]);
            for (int i = MessageKeep.Count; i < SpreadMax; i++)
            {
                anyChanged = true; // new entry in Keep will require data
                MessageKeep.Add(new Message(formular));
            }

            // see if any Message needs to be set
            for (int i = 0; i < Math.Min(SpreadMax, FSet.SliceCount); i++)
                if (FSet[i] || anyChanged)
                {
                    anyChanged = true; break;
                }

            //// only now get the data from upstream
            //if (anyChanged) SyncPins();
            
            // ...and start filling messages
            int messageIndex = 0;
            foreach (var message in MessageKeep)
            {
                bool change = messageIndex >= oldCount || FSet[messageIndex]; // fresh ones always update
                if (change || totalReset )
                {
                    message.Address = FAddress[messageIndex];
                    message.TimeStamp = Time.Time.CurrentTime();

                    CopyFromPins(message, messageIndex);
                }
                FOutput[messageIndex] = message;
                messageIndex++;
            }

            if (anyChanged)  // if nothing happened, no need to flush
            {
                FOutput.Flush();
            }


    }
    }
}