using VVVV.Nodes.Generic;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes.Broadcast
{
    #region PluginInfo

    [PluginInfo(Name = "Send",
        Category = "Message",
        Version = "VVVV",
        AutoEvaluate = true,
        Help = "Broadcasts Messages next frame to all Listeners",
        Author = "velcrome",
        Tags = "Broadcast, Send, Listen")]

    #endregion PluginInfo

    public class MessageSendNode : SendNode<Message>
    {

    }
}
