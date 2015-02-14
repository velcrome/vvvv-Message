using System.Xml.Linq;


namespace VVVV.Packs.Messaging.Serializing
{
    public static class XElementExtensions
    {
        public static XElement ToXElement(this Message message)
        {
            XElement xml = new XElement("Message");

            xml.Add(new XAttribute("Topic", message.Topic));

            foreach (var key in message.Fields)
            {
                Bin bin = message[key];
                string alias = TypeIdentity.Instance.FindAlias(bin.GetInnerType());

                var spread = new XElement("Bin");
                spread.Add(new XAttribute("type", alias));
                spread.Add(new XAttribute("name", key));

                for (int i = 0; i < message[key].Count; i++)
                {
                    spread.Add(new XElement(alias, message[key][i].ToString()));
                }
                xml.Add(spread);
            }

            return xml;
        } 
    }
}
