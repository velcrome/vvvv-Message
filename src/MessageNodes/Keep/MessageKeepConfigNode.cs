using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "ConfigKeep", AutoEvaluate = true, Category = "Message", Help = "Joins a permanent Message from custom dynamic pins", Version = "Formular", Tags = "Dynamic, Bin", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageKeepConfigNode : TypeablePinsNode
    {
        [Input("Topic", DefaultString = "State", Order = 1)]
        public IDiffSpread<string> FTopic;

        [Input("Force", Order = int.MaxValue - 2, IsSingle = true, IsToggle = true, DefaultBoolean = true, Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<bool> FForce;

        [Input("Update", IsToggle = true, Order = int.MaxValue-1, DefaultBoolean = true)]
        protected IDiffSpread<bool> FUpdate;

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

        protected MessageKeep Keep = new MessageKeep(false); // same as in AbstractMessageKeepNode. 


        private bool _reset = true;
        public bool FResetNecessary
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
            FormularUpdate += (sender, formular) => FResetNecessary = true;
        }

        // copy and paste from AbstractMessageKeepNode.cs
        protected virtual bool UpKeep(bool force = false)
        {
            if (!force && !Keep.IsChanged)
            {
                if (!Keep.QuickMode && FChangeOut.SliceCount != 0)
                {
                    FChangeOut.FlushNil();
                    FChangeIndexOut.FlushNil();
                }

                var recentCommits = from message in Keep
                                    where message.HasRecentCommit()
                                    select message;

                foreach (var message in recentCommits)
                {
                    message.Commit(Keep);
                }
                return false;
            }

            if (Keep.QuickMode)
            {
                Keep.Sync();
            }
            else
            {
                IEnumerable<int> indexes;
                var changes = Keep.Sync(out indexes);

                FChangeIndexOut.FlushResult(indexes);
                FChangeOut.FlushResult(changes);
            }

            FOutput.FlushResult(Keep);
            FCountOut.FlushItem<int>(Keep.Count);
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
            InitDX11Graph();

            bool warnPinSafety = false;
            if (RemovePinsFirst) warnPinSafety = !RetryConfig(); // defer PinConnectionException until end of method, if not successful

            SpreadMax = FSpreadCount.IsAnyInvalid() || FTopic.IsAnyInvalid() ? 0 : FSpreadCount[0];
            SpreadMax = Math.Max(SpreadMax, 0); // safeguard against negative binsizes

//          Reset?
            var anyUpdate = FResetNecessary;
            var forceUpdate = FForce[0] || FForce.IsChanged;
          
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

            if (Keep.IsChanged || Keep.IsSweeping)
            {
                UpKeep(anyUpdate);
            }
            else  // no change, so make sure, none is reported
            {
                if (FChangeOut.SliceCount > 0)
                {
                    FChangeOut.FlushNil();
                    FChangeIndexOut.FlushNil();
                }
            }

            if (warnPinSafety)
                throw new PinConnectionException("Manually remove unneeded links first! [ConfigKeep]. ID = [" + PluginHost.GetNodePath(false) +"]");
        }

        // copy and paste from AbstractMessageKeepNode.cs
        public new void Dispose()
        {
            Keep.Clear(); // dont wan't any lingering commits
            base.Dispose();
        }

    }
}