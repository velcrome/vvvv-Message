using System;
using System.Collections;
using System.Collections.Generic;
using VVVV.Packs.Message.Core.Formular;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Storage", AutoEvaluate = true, Category = "Join", Help = "Joins a permanent Message from custom dynamic pins", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class StorageJoinNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Spread Count", IsSingle = true, DefaultValue = 1)]
        ISpread<int> FSpreadCount;

        [Input("Address", DefaultString = "State")]
        ISpread<string> FAddress;

        [Input("Set", IsBang = true)]
        ISpread<bool> FSet;

        [Output("Output", AutoFlush = false)]
        Pin<Core.Message> FOutput;

        protected List<Core.Message> MessageKeep = new List<Core.Message>();

#pragma warning restore


        protected override IOAttribute DefinePin(string name, Type type, int binSize = -1)
        {
            var attr = new InputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = binSize;
            attr.Order = FCount;
            attr.BinOrder = FCount + 1;
            attr.AutoValidate = false;  // need to sync all pins manually. Don't forget to Flush()
            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            bool totalReset = FConfig.IsChanged;

            SpreadMax = FSpreadCount[0];
            FOutput.SliceCount = SpreadMax = SpreadMax < 0? 0 : SpreadMax;

            var oldCount = MessageKeep.Count;

            bool anyChanged = false; // will only pull upstream data, if data is necessary
            
 
            if (SpreadMax < oldCount) // remove superfluous entries
            {
                anyChanged = true;
                MessageKeep.RemoveRange(SpreadMax, oldCount - SpreadMax);
            }

            for (int i = MessageKeep.Count; i < SpreadMax; i++)
            {
                anyChanged = true; // new entry in Keep will require data
                MessageKeep.Add(new Core.Message(new MessageFormular(FConfig[0])));
            }

            // see if any Message needs to be set
            for (int i = 0; i < Math.Min(SpreadMax, FSet.SliceCount); i++) 
                if (FSet[i] || anyChanged)
                {
                    anyChanged = true; break;
                }

            // only now get the data from upstream
            if (anyChanged) SyncPins();
            
            // ...and start filling messages
            int messageIndex = 0;
            foreach (var message in MessageKeep)
            {
                bool change = messageIndex < oldCount ? FSet[messageIndex] : true; // fresh ones always update
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