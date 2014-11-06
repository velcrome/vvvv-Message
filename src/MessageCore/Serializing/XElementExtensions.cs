using System.Xml.Linq;


namespace VVVV.Packs.Message.Core.Serializing
{
    public static class XElementExtensions
    {
        public static XElement ToXElement(this Message message)
        {
            XElement xml = new XElement("Message");

            xml.Add(new XAttribute("address", message.Address));

            foreach (var key in message.Attributes)
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
