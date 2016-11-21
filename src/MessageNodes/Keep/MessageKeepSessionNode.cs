#region usings
using System;
using System.Collections.Generic;
using System.Linq;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Packs.Messaging;
#endregion usings

namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "SessionKeep",
        Category = "Message",
        Version = "Formular", 
        AutoEvaluate = true,
        Help = "Stores Messages according to their id (which can be its Topic and/or specific fields) and removes them, if no change was detected for a certain time",
        Author = "velcrome")]
    public class TypableMessageSessionNode : TypableMessageKeepNode
    {
        #region fields & pins
        [Input("Retain Time", IsSingle = true, DefaultValue = -1.0, Order = 4)]
        public ISpread<double> FTime;


        [Output("Removed Messages", AutoFlush = false, Order = 2)]
        public ISpread<Message> FRemovedMessages;

        List<Message> DeadMessages = new List<Message>();

        #endregion fields & pins

        //called when data for any output pin is requested
        public override void Evaluate(int SpreadMax)
        {
            var update = CheckReset();

            // early sync only, when not interested in input changes
            if (!ManageAllChanges && UpKeep(update)) update = true;

            if (!FInput.IsAnyInvalid() && FInput.IsChanged)
                foreach (var message in FInput)
                {
                    var found = MatchOrInsert(message);
                    if (found != null) update = true;
                }


            if (RemoveOld()) update = true;

            if (ManageAllChanges)
            {
                if (UpKeep(update)) update = true;

            }
            else {
                var change = Keep.Sync(); // not interested input changes, so sync right away
                if (change != null && change.Count() > 0) update = true;

                if (update)
                {
                    FOutput.FlushResult(Keep);
                    FCountOut.FlushItem<int>(Keep.Count);
                }
            }

            // add all additional id fields to the changed message
            if (update && !Keep.QuickMode)
            {
                SpreadMax = FChangeOut.SliceCount;

                for (int i = 0; i < SpreadMax; i++)
                {
                    var change = FChangeOut[i];
                    var orig = Keep[FChangeIndexOut[i]];

                    var idFields = from fieldName in FUseAsID
                                   select fieldName.Name; 
                    
                    foreach (var field in idFields)
                    {
                        if (field != TOPIC)
                            change.AssignFrom(field, orig[field]);
                    }
                }
            }
        }

        public Message MatchOrInsert(Message message)
        {
            // inject all incoming messages and keep a list of all
            var idFields = from fieldName in FUseAsID
                           select fieldName.Name;

            var match = MatchByField(message, idFields);

            if (match == null)
            {
                Keep.Add(message); // record message
                return message;
            }
            else
            {
                match.InjectWith(message, true); // copy all attributes from message to matching record
                return match;
            }
        }

        protected Message MatchByField(Message message, IEnumerable<string> idFields)
        {
            // make sure all messages have the field
            var missingFields = from fieldName in idFields.Distinct()
                                where fieldName != TOPIC
                                where !message.Fields.Contains(fieldName)
                                select fieldName;
            foreach (var fieldName in missingFields) message[fieldName] = BinFactory.New(Formular[fieldName].Type);

            var bins = (
                                    from fieldName in idFields.Distinct()
                                    where fieldName != TOPIC
//                                    where message.Fields.Contains(fieldName)
                                    select fieldName
                                 ).ToArray();

            bool includeTopic = idFields.Contains(TOPIC);

//            bool isCompatible = compatibleBins.Count() == idFields.Distinct().Count() - (includeTopic ? 1 : 0);

            var match = (
                           from keep in Keep
 //                          where isCompatible
                           where !includeTopic || keep.Topic.Equals(message.Topic)
                           where bins.Length == 0 || (
                                    from fieldName in bins
                                    select (keep[fieldName] as Bin).Equals(message[fieldName] as Bin)
                                ).All(x => x)
                           select keep

                         ).DefaultIfEmpty(null).First();
            return match;
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
                    else
                    {
                        var idFields = from fieldName in FUseAsID
                                       select fieldName.Name;

                        var match = MatchByField(message, idFields);

                        if (match != null)
                        {
                            anyChange = true;

                            DeadMessages.Add(match);
                            Keep.Remove(match);
                        }
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
        
        private bool RemoveOld()
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

            if (FRemovedMessages.SliceCount > 0 || DeadMessages.Count() > 0)
            {
                FRemovedMessages.FlushResult(DeadMessages);
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
