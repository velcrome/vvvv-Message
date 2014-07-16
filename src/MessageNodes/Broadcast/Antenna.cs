using VVVV.Nodes.Generic;
using VVVV.Packs.Message.Core;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Messaging.Broadcast
{
    #region PluginInfo

    [PluginInfo(Name = "Antenne",
        Category = "Message",
        AutoEvaluate = true,
        Help = "Broadcasts Messages next frame to all Radios",
        Author = "velcrome",
        Tags = "Broadcast, Send, Radio")]

    #endregion PluginInfo

    public class MessageAntennaNode : AntennaNode<Message>
    {

    }
}
