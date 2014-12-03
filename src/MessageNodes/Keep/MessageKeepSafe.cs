using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Nodes;
using VVVV.PluginInterfaces.V2;

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
        // this one shall not show. remove by overwriting.
        public new ISpread<Message> FDefault;

        [Output("Changed Slice", Order = 1)]
        public ISpread<bool> FChanged;

        [Import()]
        protected IIOFactory FIOFactory;

        public readonly List<Message> MessageKeep = new List<Message>();

        //called when data for any output pin is requested
        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
        }

        public override void Evaluate(int SpreadMax)
        {
            if (FReset[0])
            {
                MessageKeep.Clear();
            }

            // inject all incoming messages and keep a list of all
            var changed = (
                    from message in FInput
                    where message != null
                    select MatchOrInsert(message)
                ).Distinct().ToList();

            SpreadMax = MessageKeep.Count;
            FChanged.SliceCount = FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var message = MessageKeep[i];
                FOutput[i] = message;
                FChanged[i] = changed.Contains(message);
            }
        }
        
        public Message MatchOrInsert(Message message)
        {

            var matched = (from keep in MessageKeep
                           where keep.Address == message.Address
                           select keep).ToList();

            if (matched.Count == 0)
            {
                MessageKeep.Add(message); // record message
                return message;
            }
            else
            {
                var found = matched.First(); // found a matching record

                var k = found += message; // copy all attributes from message to matching record
                found.TimeStamp = message.TimeStamp; // update time

                return found;
            }
        }
   
    }
}
