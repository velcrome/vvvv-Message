using System.Xml.Linq;
using VVVV.Pack.Game.Core;
using VVVV.Packs.Message.Core;
using System.Xml;

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
                Bin bin = message[key];

                var spread = new XElement("Spread");

                spread.Add(new XAttribute("name", key));
                spread.Add(new XAttribute("type", TypeIdentity.Instance[bin.GetInnerType()].ToString()));

                for (int i = 0; i < message[key].Count; i++)
                {
                    spread.Add(new XElement(TypeIdentity.Instance[bin.GetInnerType()].ToString(), message[key][i].ToString()));
                }


                xml.Add(spread);
            }

            return xml;
        } 
    }
}
