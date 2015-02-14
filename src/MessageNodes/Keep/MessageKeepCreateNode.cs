using System.Linq;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Create", AutoEvaluate = true, Category = "Message.Keep", Help = "Joins a permanent Message from custom dynamic pins", Version = "Formular", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageKeepCreateNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("New", IsBang = true, Order = 0)]
        ISpread<bool> FNew;

        [Input("Topic", DefaultString = "State", Order = 1)]
        ISpread<string> FTopic;

        [Input("Reset", IsBang = true, Order = int.MaxValue - 2)]
        ISpread<bool> FReset;

        [Input("Count", IsSingle = true, DefaultValue = 1, Order = int.MaxValue - 1)]
        ISpread<int> FSpreadCount;

        [Output("Output", AutoFlush = false)]
        Pin<Message> FOutput;

        protected MessageKeep Keep = new MessageKeep(); // same as in AbstractStorageNode. 

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
            if (totalReset) Keep.Clear();
            
            var oldCount = Keep.Count;

            SpreadMax = FSpreadCount[0];
            SpreadMax = SpreadMax < 0 ? 0 : SpreadMax; // safeguard against negative binsizes
            FOutput.SliceCount = SpreadMax;

            bool anyChanged = FNew.Take(SpreadMax).Any(x => x); 
            if (SpreadMax < oldCount) // remove superfluous entries
            {
                anyChanged = true;
                Keep.RemoveRange(SpreadMax, oldCount - SpreadMax);
            }

            var formular = new MessageFormular(FConfig[0]);
            for (int i = Keep.Count; i < SpreadMax; i++)
            {
                anyChanged = true; // new entry in Keep will require data
                Keep.Add(new Message(formular));
            }

            // see if any Message needs to be set
            for (int i = 0; i < SpreadMax; i++)
                if (FNew[i])
                {
                    Keep[i] = new Message();
                    anyChanged = true; 
                }

            anyChanged = anyChanged || FTopic.IsChanged || FPins.Any(x => x.Value.ToISpread().IsChanged); 

            if (!anyChanged) return;

            //// only now get the data from upstream
            //if (anyChanged) SyncPins();
            
            // ...and start filling messages
            int messageIndex = 0;
            foreach (var message in Keep)
            {
///               bool change = messageIndex >= oldCount || FNew[messageIndex]; // fresh ones always update
//                if (change || totalReset)
//                {
//                    message.Topic = FTopic[messageIndex];
                    message.TimeStamp = Time.Time.CurrentTime();

                    CopyFromPins(message, messageIndex);
                //}
                FOutput[messageIndex] = message;
                messageIndex++;
            }
            FOutput.Flush();

    }
    }
}