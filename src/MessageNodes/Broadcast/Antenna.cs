using VVVV.Nodes.Generic;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes.Broadcast
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
