using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Packs.Messaging.Nodes
{
    // better than the GetSlice (Node), because it allows binning and Index Spreading
    [PluginInfo(Name = "GetSlice", Category = "Message", Help = "GetSlice Messages", Author = "velcrome")]
    public class MessageGetSliceNode : GetSlice<Message>
    {
    }

    [PluginInfo(Name = "Select", Category = "Message", Help = "Select Messages", Author = "vvvv-sdk")]
    public class MessageSelectNode : Select<Message>
    {
    }

    [PluginInfo(Name = "Cons", Category = "Message", Version = "Bin", Help = "Cons Messages", Author = "vvvv-sdk")]
    public class MessageConsNode : Cons<Message>
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
