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
    [PluginInfo(Name = "Cache",
        Category = "Message.Keep",
        Version = "Typeable", 
        AutoEvaluate = true,
        Help = "Stores Messages and removes them, if no change was detected for a certain time",
        Author = "velcrome")]
    public class TypableMessageCacheNode : TypableMessageKeepNode
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
                    MatchOrInsert(message, idFields);

            if (FTime[0] > 0) 
                if (RemoveOld()) update = true;

            if (UpKeep(true)) update = true;

            // changed pins are now valid.
            
            if (update)
            {
                SpreadMax = FChangeDataOut.SliceCount;

                for (int i = 0; i < SpreadMax; i++)
                {
                    var change = FChangeDataOut[i];
                    var orig = Keep[FChangeOut[i]];

                    foreach (var field in idFields)
                    {
                        change.AssignFrom(field, orig[field]);
                    }
                }

                DumpKeep(Keep.Count);
            }
        }

        public Message MatchOrInsert(Message message, IEnumerable<string> idFields)
        {
            var compatibleBins = idFields.Intersect(message.Fields);
            bool isCompatible = compatibleBins.Count() == idFields.Distinct().Count();

            var matched = (from keep in Keep
                           where isCompatible
                           from fieldName in compatibleBins
                           where keep[fieldName] == message[fieldName]// slicewise check of Bins' equality
                           select keep).ToList();

            if (matched.Count == 0)
            {
                Keep.Add(message); // record message
                return message;
            }
            else
            {
                var found = matched.First(); // found a matching record
                var k = found += message; // copy all attributes from message to matching record
     
                return found;
            }
        }

        private bool RemoveOld()
        {
            var validTime = Time.Time.CurrentTime() -
                            new TimeSpan(0, 0, 0, (int)Math.Floor(FTime[0]), (int)Math.Floor((FTime[0] * 1000) % 1000));

            var deadMessages = 
                from message in Keep
                    where message.TimeStamp < validTime
                    let removed = Keep.Remove(message)
                    select message;


            if (FRemovedMessages.SliceCount > 0 || deadMessages.Count() != 0)
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
