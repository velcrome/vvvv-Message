using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Packs.Message.Nodes
{
    using Message = VVVV.Packs.Message.Core.Message;

    [PluginInfo(Name = "S+H", Category = "Message", Help = "Sample and hold a Message", Author = "velcrome")]
    public class MessageSAndHNode : SAndH<Message>
    {
    }

    // better than the GetSlice (Node), because it allows binning and Index Spreading
    [PluginInfo(Name = "GetSlice", Category = "Message", Help = "GetSlice Messages", Author = "velcrome")]
    public class MessageGetSliceNode : GetSlice<Message>
    {
    }

    [PluginInfo(Name = "Select", Category = "Message", Help = "Select Messages", Author = "velcrome")]
    public class MessageSelectNode : Select<Message>
    {
    }

    [PluginInfo(Name = "Cons", Category = "Message", Version = "Bin", Help = "Concatenate Messages", Author = "velcrome")]
    public class MessageConsNode : Cons<Message>
    {
    }

    [PluginInfo(Name = "Zip", Category = "Message", Version = "Bin", Help = "Zip Messages", Author = "velcrome")]
    public class MessageZipBinNode : Zip<IInStream<Message>>
    {
    }

    [PluginInfo(Name = "Zip", Category = "Message", Help = "Zip Messages", Tags = "", Author = "velcrome")]
    public class MessageZipNode : Zip<Message>
    {
    }

    [PluginInfo(Name = "UnZip", Category = "Message", Help = "UnZip Messages", Tags = "", Author = "velcrome")]
    public class MessageUnZipNode : Unzip<IInStream<Message>>
    {
    }
}
