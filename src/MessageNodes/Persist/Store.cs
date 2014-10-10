using VVVV.Pack.Game.Nodes;
using VVVV.PluginInterfaces.V2;



namespace VVVV.Packs.Message.Nodes
{
    using Message = VVVV.Packs.Message.Core.Message;

    [PluginInfo(
       Name = "Store",
       Category = "Messages",
       Help = "Stores Messages",
       AutoEvaluate = true,
       Tags = "velcrome")]
    public class MessageStoreNode : StoreNode<Message>
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
