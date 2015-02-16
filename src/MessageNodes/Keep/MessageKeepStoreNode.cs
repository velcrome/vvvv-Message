using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{

    //[PluginInfo(
    //   Name = "Store",
    //   Category = "Message.Keep",
    //   Help = "Stores Messages",
    //   AutoEvaluate = true,
    //   Tags = "velcrome")]
    public class MessageKeepStoreNode : StoreNode<Message>
    {
        protected override void Sort()
        {
            FElements.Sort(delegate(Message x, Message y)
                           {
                               return (x.TimeStamp > y.TimeStamp) ? 1 : 0;
                           });

            return;
        }
    }
}
