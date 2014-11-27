using System.Linq;
using VVVV.Nodes.Generic;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes.Broadcast
{
    #region PluginInfo

    [PluginInfo(Name = "Radio",
        Category = "Message",
        AutoEvaluate = true,
        Help = "Receives Messages from last frame",
        Author = "velcrome",
        Tags = "Broadcast, Send, Antenne")]

    #endregion PluginInfo

    public class MessageRadioNode : RadioNode<Message>
    {

        #pragma warning disable 649, 169
        [Input("Filter", DefaultString = "*")]
        protected IDiffSpread<string> FFilter;
        #pragma warning restore
        
        public override void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = 0;

  
            var filtered = 
                from message in Receive
                    from filter in FFilter
                    where message.AddressMatches(filter)
                
                select message;

//            FOutput.AssignFrom(filtered);
            FOutput.AssignFrom(filtered.Distinct());
            FOutput.Flush();

            FReceive.SliceCount = 1;
            FReceive[0] = FOutput.SliceCount > 0;
        }
    }

}
