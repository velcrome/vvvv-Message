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
    [PluginInfo(Name = "Session",
        Category = "Message.Keep",
        Version = "Typeable", 
        AutoEvaluate = true,
        Help = "Stores Messages according to their id (which can be its Topic and/or specific fields) and removes them, if no change was detected for a certain time",
        Author = "velcrome")]
    public class TypableMessageSessionNode : TypableMessageKeepNode
    {
        #region fields & pins

        [Input("Retain Time", IsSingle = true, DefaultValue = -1.0, Order = 3)]
        public ISpread<double> FTime;

        [Output("Removed Messages", AutoFlush = false, Order = 2)]
        public ISpread<Message> FRemovedMessages;

        #endregion fields & pins

        //called when data for any output pin is requested
        public override void Evaluate(int SpreadMax)
        {
            var update = CheckReset();

            // inject all incoming messages and keep a list of all
            var idFields = from fieldName in FUseAsID
                            select fieldName.Name;

            if (!FInput.IsAnyInvalid())
                foreach (var message in FInput)
                {
                    var found = MatchOrInsert(message, idFields);
                }

            if (FTime[0] > 0) 
                if (RemoveOld()) update = true;

            if (UpKeep(update)) update = true;  
           
            // add all additional id fields to the changed message
            if (update && !Keep.QuickMode)
            {
                SpreadMax = FChangeOut.SliceCount;

                for (int i = 0; i < SpreadMax; i++)
                {
                    var change = FChangeOut[i];
                    var orig = Keep[FChangeIndexOut[i]];

                    foreach (var field in idFields)
                    {
                        if (field != TOPIC)
                            change.AssignFrom(field, orig[field]);
                    }
                }
            }
        }

        public Message MatchOrInsert(Message message, IEnumerable<string> idFields)
        {
            var compatibleBins = idFields.Intersect(message.Fields).ToArray();
            bool includeTopic = idFields.Contains(TOPIC) ;
            bool isCompatible = compatibleBins.Count() == idFields.Distinct().Count() - (includeTopic ? 1 : 0);

            var match = (
                           from keep in Keep
                           where isCompatible 
                           where !includeTopic || keep.Topic.Equals(message.Topic)
                           where compatibleBins.Length == 0 || (
                                    from fieldName in compatibleBins
                                    select (keep[fieldName] as Bin).Equals(message[fieldName] as Bin)
                                ).All(x => x)
                           select keep

                         ).DefaultIfEmpty(null).First();

            if (match == null)
            {
                Keep.Add(message); // record message
                return message;
            }
            else
            {
                match.InjectWith(message, true, true); // copy all attributes from message to matching record
                return match;
            }
        }

        private bool RemoveOld()
        {
            var validTime = Time.Time.CurrentTime() -
                            new TimeSpan(0, 0, 0, (int)Math.Floor(FTime[0]), (int)Math.Floor((FTime[0] * 1000) % 1000));

            var deadMessages =
                (
                from message in Keep.ToList()
                where message.TimeStamp < validTime
                let removed = Keep.Remove(message)
                select message
                );

            if (FRemovedMessages.SliceCount > 0 || deadMessages.Count() > 0)
            {
                FRemovedMessages.SliceCount = 0;
                FRemovedMessages.AssignFrom(deadMessages);
                FRemovedMessages.Flush();
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
