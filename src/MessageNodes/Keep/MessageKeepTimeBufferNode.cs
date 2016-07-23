using System.Linq;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "TimeBuffer", Category = "Message.Keep", Help = "Holds the last bunch of Messages that traveled through. Pick some or all.",
        Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageKeepTimeBufferNode : AbstractMessageKeepNode
    {

        #region fields & pins
        [Input("Retain Time", IsSingle = true, DefaultValue = -1.0, Order = 4)]
        public ISpread<double> FTime;

        [Input("Kill Flag Field", IsSingle = true, DefaultString ="KillNow", Order = 5)]
        public IDiffSpread<string> FKillField;

        [Output("Removed Messages", AutoFlush = false, Order = 2)]
        public ISpread<Message> FRemovedMessages;

        protected List<Message> DeadMessages = new List<Message>();

        #endregion fields & pins

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
        }

        //called when data for any output pin is requested
        public override void Evaluate(int SpreadMax)
        {
            var update = CheckReset();

            if (!FInput.IsAnyInvalid() && FInput.IsChanged)
                foreach (var message in FInput)
                {
                    if (!Keep.Contains(message)) {
                        Keep.Add(message);
                        update = true;
                    }
                }

            if (RemoveOldOrFlagged()) update = true;
            if (UpKeep(update)) update = true;

        }

        protected override bool CheckRemove()
        {
            var anyChange = false;
            if (!FRemove.IsAnyInvalid())
            {
                foreach (var message in FRemove)
                {
                    if (Keep.Contains(message))
                    {
                        anyChange |= Keep.Remove(message);
                        DeadMessages.Add(message);
                    }
                }
            }
            return anyChange;
        }

        protected override bool CheckReset()
        {
            var anyChange = CheckRemove();

            if (!FReset.IsAnyInvalid() && FReset[0])
            {
                DeadMessages.AddRange(Keep);

                Keep.Clear();
                anyChange = true;
            }
            return anyChange;
        }

        private bool RemoveOldOrFlagged()
        {
            var validTime = Time.Time.CurrentTime() -
                            new TimeSpan(0, 0, 0, (int)Math.Floor(FTime[0]), (int)Math.Floor((FTime[0] * 1000) % 1000));

            if (FTime[0] > 0)
                DeadMessages = DeadMessages.Concat(
                    from message in Keep.ToList()
                    where message.TimeStamp < validTime
                    let removed = Keep.Remove(message)
                    select message
                ).ToList();


            if (!FKillField.IsAnyInvalid() && FKillField[0].Trim() != "")
            {
                var killField = FKillField[0].Trim();
                foreach(var message in Keep)
                {
                    if (!message.Contains(killField)) continue;

                    var killFlag = message[killField].First as bool?;

                    if (killFlag != null && killFlag == true)
                    {
                        Keep.Remove(message);
                        DeadMessages.Add(message);
                    }
                }
            }

            if (FRemovedMessages.SliceCount > 0 || DeadMessages.Count() > 0)
            {
                FRemovedMessages.SliceCount = 0;
                FRemovedMessages.AssignFrom(DeadMessages);
                FRemovedMessages.Flush();

                DeadMessages.Clear();
                return true;
            }
            else
            {
                // output still empty, no need to flush
                return false;
            }
        }


    }
}