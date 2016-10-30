using System.Xml.Linq;


namespace VVVV.Packs.Messaging.Serializing
{
    public static class XElementExtensions
    {
        public static XElement ToXElement(this Message message)
        {
            XElement xml = new XElement("Message");

            if (message == null) return xml;

            xml.Add(new XAttribute("Topic", message.Topic));

            foreach (var key in message.Fields)
            {
                Bin bin = message[key];
                var typeRecord = TypeIdentity.Instance[bin.GetInnerType()];
                if (typeRecord == null || typeRecord.CloneMethod == CloneBehaviour.Null) continue;

                var element = new XElement(key);
                element.Add(new XAttribute("type", typeRecord.Alias));
                for (int i = 0; i < bin.Count; i++)
                {
                    element.Add(new XElement(typeRecord.Alias, bin[i].ToString()));
                }
                xml.Add(element);
            }

            return xml;
        } 
    }
}
