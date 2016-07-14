using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Nodes;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Nodes.Messaging.Keep
{
    [PluginInfo(Name = "Safe",
        Category = "Message.Keep",
//        Version = "",
        AutoEvaluate = true,
        Help = "Stores Messages according to their address and keeps them updated",
        Author = "velcrome")]
    public class MessageKeepSafe : AbstractMessageKeepNode
    {
        public override void Evaluate(int SpreadMax)
        {

            var update = CheckReset();

            // early sync only, when not interested in input changes
            if (!ManageAllChanges)
                if (UpKeep(update)) update = true;


            foreach (var message in FInput)
            {
                if (message != null && message.Topic != "")
                {
                    var m = MatchOrInsert(message);
                    if (m != null) update = true;
                }
            }

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
                    FCountOut.FlushInt(Keep.Count);
                }            }
        }

        public Message MatchOrInsert(Message message)
        {
           
            var matched = (from keep in Keep
                           where keep.Topic == message.Topic
                           select keep).ToList();

            if (matched.Count == 0)
            {
                Keep.Add(message); // record message
                return message;
            }
            else
            {
                var found = matched.First(); // found a matching record

                found.InjectWith(message, true); // copy all attributes from message to matching record
                found.TimeStamp = message.TimeStamp; // forceUpdate time

                return found;
            }
        }
   
    }
}
