using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Packs.Messaging.Nodes
{

    [PluginInfo(Name = "Select", Category = "Message", Help = "Select Messages", Author = "vvvv-sdk")]
    public class MessageSelectNode : Select<Message>
    {
    }

    [PluginInfo(Name = "Zip", Category = "Message", Version = "Bin", Help = "Zip Messages", Author = "vvvv-sdk")]
    public class MessageZipBinNode : Zip<IInStream<Message>>
    {
    }

    [PluginInfo(Name = "UnZip", Category = "Message", Help = "UnZip Messages", Tags = "", Author = "vvvv-sdk")]
    public class MessageUnZipNode : Unzip<IInStream<Message>>
    {
    }


}
