using VVVV.Packs.Message.Core;
using VVVV.PluginInterfaces.V2;


namespace VVVV.Nodes.Messaging.Persist
{
    #region PluginInfo
    [PluginInfo(Name = "Cache", Category = "Message", Help = "Caches Messages for a certain time", Author = "velcrome", Tags = "")]
    #endregion PluginInfo
    public class Vector3DCacheNode : CacheNode<Message> { }

}
