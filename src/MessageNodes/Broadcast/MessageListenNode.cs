using System.Linq;
using VVVV.Nodes.Generic;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes.Broadcast
{
    #region PluginInfo

    [PluginInfo(Name = "Listen",
        Category = "Message",
        Version = "Sift",
        Bugs = "only works with one instance of vvvv",
        AutoEvaluate = true,
        Help = "Receives Messages from last frame",
        Author = "velcrome",
        Tags = "Broadcast, Send")]

    #endregion PluginInfo

    public class MessageListenNode : ListenNode<Message>
    {

        #pragma warning disable 649, 169
        [Input("Filter", DefaultString = "*")]
        protected IDiffSpread<string> FFilter;
        #pragma warning restore
        
        public override void Evaluate(int SpreadMax)
        {
            var filtered = 
                from message in Receive
                    from filter in FFilter
                    where message.AddressMatches(filter)
                
                select message;

            FOutput.SliceCount = 0;
            FOutput.AssignFrom(filtered.Distinct());
            FOutput.Flush();

            FReceive.SliceCount = 1;
            FReceive[0] = FOutput.SliceCount > 0;
        }
    }

}
