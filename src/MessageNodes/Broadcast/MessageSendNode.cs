using VVVV.Nodes.Generic;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes.Broadcast
{
    #region PluginInfo

    [PluginInfo(Name = "Send",
        Category = "Message.VVVV",
  //      Version = "",
        AutoEvaluate = true,
        Help = "Broadcasts Messages next frame to all Listeners",
        Author = "velcrome",
        Tags = "Listen")]

    #endregion PluginInfo

    public class MessageSendNode : SendNode<Message>
    {

    }
}
