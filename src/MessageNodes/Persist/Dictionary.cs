using VVVV.Nodes.Generic.Memory;
using VVVV.PluginInterfaces.V2;


namespace VVVV.Packs.Message.Nodes
{
    using Message = VVVV.Packs.Message.Core.Message;

    [PluginInfo(Name = "Dictionary", 
        Category = "Message", 
        Help = "Stores Messages", 
        Author = "velcrome")]
    public class MessageDictionaryNode : DictionaryNode<string, Message>
    {
    }

}
