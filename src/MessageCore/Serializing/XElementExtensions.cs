using System.Xml.Linq;
using VVVV.Packs.Message.Core;


namespace VVVV.Packs.Message.Serializing
{
    public static class XElementExtensions
    {
        public static XElement ToXElement(this Core.Message message)
        {
            XElement xml = new XElement("Message");

            xml.Add(new XAttribute("address", message.Address));

            foreach (var key in message.Attributes)
            {
                SpreadList data = message[key];

                var spread = new XElement("Spread");

                spread.Add(new XAttribute("name", key));
                spread.Add(new XAttribute("type", TypeIdentity.Instance[data.SpreadType].ToString()));

                for (int i = 0; i < message[key].Count; i++)
                {
                    spread.Add(new XElement(TypeIdentity.Instance[data.SpreadType].ToString(), message[key][i].ToString()));
                }


                xml.Add(spread);
            }

            return xml;
        } 
    }
}
