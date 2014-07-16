
using System.Linq;
using System.Security.Cryptography;
using VVVV.Nodes.Generic;
using VVVV.Packs.Message.Core;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Messaging.Broadcast
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
        [Input("Filter", DefaultString = "*")]
        private IDiffSpread<string> FFilter;

        public override void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = 0;

  
            var filtered = 
                from message in Receive
                    from filter in FFilter
                    where message.AddressMatches(filter)
                
                select message;

            FOutput.AssignFrom(filtered.Distinct());
            FOutput.Flush();

        }
    }

}
