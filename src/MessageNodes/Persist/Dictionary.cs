using VVVV.Nodes.Generic.Memory;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;


namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "Dictionary", 
        Category = "Message", 
        Help = "Stores Messages", 
        Author = "velcrome")]
    public class MessageDictionaryNode : DictionaryNode<string, Message>
    {
    }

}
