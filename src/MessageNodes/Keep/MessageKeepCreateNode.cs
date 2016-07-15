using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Create", AutoEvaluate = true, Category = "Message.Keep", Help = "Joins a permanent Message from custom dynamic pins", Version = "Formular", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageKeepCreateNode : TypeablePinsNode
    {
#pragma warning disable 649, 169
        [Input("Topic", DefaultString = "State", Order = 1)]
        public IDiffSpread<string> FTopic;

        [Input("AutoSense", Order = int.MaxValue - 2, IsSingle = true, IsToggle = true, DefaultBoolean = true, Visibility = PinVisibility.True)]
        IDiffSpread<bool> FAutoSense;

        [Input("Update", IsToggle = true, Order = int.MaxValue-1, DefaultBoolean = true)]
        IDiffSpread<bool> FUpdate;

        [Input("Count", IsSingle = true, DefaultValue = 1, Order = int.MaxValue)]
        public IDiffSpread<int> FSpreadCount;

        [Output("Output", AutoFlush = false)]
        public Pin<Message> FOutput;

        [Output("Changed Message", Order = int.MaxValue - 2, AutoFlush = false, Visibility = PinVisibility.Hidden)]
        public Pin<int> FChangeIndexOut;

        [Output("Message Diff", Order = int.MaxValue - 1, AutoFlush = false)]
        public Pin<Message> FChangeOut;

        [Output("Internal Count", Order = int.MaxValue, AutoFlush = false)]
        public ISpread<int> FCountOut;
#pragma warning restore

        protected MessageKeep Keep = new MessageKeep(false); // same as in AbstractStorageNode. 


        private bool _reset;
        public bool ResetNecessary
        {
            get
            {
                if (_reset)
                {
                    _reset = false;
                    Keep.Clear();
                    return true;
                }
                return false;
            }
            private set
            {
                _reset = value;
            }
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            Changed += formular => ResetNecessary = true;


        }


        protected virtual bool UpKeep(bool force = false)
        {
            if (!force && !Keep.IsChanged)
            {
                if (!Keep.QuickMode && FChangeIndexOut.SliceCount != 0)
                {
                    FChangeIndexOut.FlushNil();
                    FChangeOut.FlushNil();
                }
                return false;
            }

            if (Keep.QuickMode)
            {
                Keep.Sync();
            }
            else
            {
                IEnumerable<Message> changes;
                IEnumerable<int> indexes;
                changes = Keep.Sync(out indexes);

                FChangeIndexOut.FlushResult(indexes);
                FChangeOut.FlushResult(changes);
            }

            FOutput.FlushResult(Keep);
            FCountOut.FlushInt(Keep.Count);

            return true;
        }



        protected override IOAttribute SetPinAttributes(FormularFieldDescriptor field)
        {
            var attr = new InputAttribute(field.Name); 
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = field.DefaultSize;
            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount + 1;
            attr.CheckIfChanged = true;

            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            if (RemovePinsFirst) RetryConfig();

            SpreadMax = FSpreadCount.IsAnyInvalid() || FTopic.IsAnyInvalid() ? 0 : FSpreadCount[0];
            SpreadMax = Math.Max(SpreadMax, 0); // safeguard against negative binsizes

//          Reset?
            var anyUpdate = ResetNecessary;

            var forceUpdate = !FAutoSense[0] || FAutoSense.IsChanged;
          
            var newData = FPins.Any(x => x.Value.ToISpread().IsChanged); // changed pins
            newData |= forceUpdate; // if update is forced, then predent it is new Data
            
            var newTopic = FTopic.IsChanged;
            newTopic |= forceUpdate; // if update is forced, then pretend it is a new Topic

//          remove superfluous entries
            if (SpreadMax < Keep.Count) 
            {
                anyUpdate = true;
                Keep.RemoveRange(SpreadMax, Keep.Count - SpreadMax);
            }

//          add new entries
            for (int i = Keep.Count; i < SpreadMax; i++)
            {
                anyUpdate = true; // new entry in Keep will require data

                newData = true;
                newTopic = true;

                var message = new Message(Formular);
                message.Topic = FTopic[i];
                Keep.Add(message);
            }

//          check update pin
            anyUpdate |= FUpdate.Any();

            if (anyUpdate && (newData || newTopic))
            {
                // ...and start filling messages
                int messageIndex = 0;
                foreach (var message in Keep)
                {
                    // only copy, when Update is true for this message
                    if (newData && FUpdate[messageIndex] && CopyFromPins(message, messageIndex, !forceUpdate)) 
                        anyUpdate = true;
                    
                    if (newTopic && FUpdate[messageIndex] && message.Topic != FTopic[messageIndex]) 
                    {
                        message.Topic = FTopic[messageIndex];
                        anyUpdate = true;
                    }
                    messageIndex++;
                }
            }

            if (Keep.IsChanged)
            {
                UpKeep(anyUpdate);
            }
            else  // no change, so make sure, none is reported
            {
                if (FChangeOut.SliceCount > 0)
                {
                    FChangeOut.SliceCount = 0;
                    FChangeIndexOut.SliceCount = 0;
                }
            }

        }



    }
}