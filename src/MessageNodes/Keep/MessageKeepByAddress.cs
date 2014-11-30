using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Messaging.Core;
using VVVV.Packs.Messaging.Core.Formular;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Messaging.Keep
{
    [PluginInfo(Name = "Safe",
        Category = "Message.Keep",
//        Version = "",
        AutoEvaluate = true,
        Help = "Stores Messages according to their address and keeps them updated",
        Author = "velcrome")]
    public class MessageKeepByAddress : IPluginEvaluate 
    {

        [Input("Input", Order = 0)]
        public ISpread<Message> FInput;

        [Input("Reset", IsSingle = true, IsBang = true, Order = int.MaxValue - 1)]
        public ISpread<bool> FReset;

        [Input("Replace Dump", Order = int.MaxValue, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<List<Message>> FReplaceData;

        [Output("Output", Order = 0)]
        public ISpread<Message> FOutput;

        [Output("Changed Slice", Order = 1)]
        public ISpread<bool> FChanged;

        [Output("Dump", Order = int.MaxValue, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<List<Message>> FDump;

        [Import()]
        protected IIOFactory FIOFactory;

        public readonly List<Message> MessageKeep = new List<Message>();

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
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
