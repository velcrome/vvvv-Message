using System.IO;
using System.Linq;
using VVVV.Utils.OSC;


namespace VVVV.Packs.Messaging.Serializing
{
    using System;
    using Time = VVVV.Packs.Time.Time;

    public static class OSCExtensions
    {

        public static Stream ToOSC(this Message message, bool extendedMode = false)
        {
            OSCBundle bundle = new OSCBundle(message.TimeStamp, extendedMode);
            foreach (string name in message.Fields)
            {
                string oscAddress = "";

                foreach (string part in message.Topic.Split('.'))
                {
                    if (part.Trim() != "") oscAddress += "/" + part;
                }

                foreach (string part in name.Split('.'))
                {
                    if (part.Trim() != "") oscAddress += "/" + part;
                }

                OSCMessage m = new OSCMessage(oscAddress, extendedMode);
                Bin bl = message[name];
                for (int i = 0; i < bl.Count; i++) m.Append(bl[i]);
                bundle.Append(m);
            }
            return new MemoryStream(bundle.BinaryData); // packs implicitly
        }

        public static Message FromOSC(Stream stream, bool extendedMode = false, string messagePrefix = "", int contractAddress = 1)
        {
            Message message = new Message();

            MemoryStream ms = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(ms);
            byte[] bytes = ms.ToArray();

            if (bytes.Length == 0) return null;

            var pack = OSCPacket.Unpack(bytes, extendedMode);

            OSCBundle bundle;
            if (pack.IsBundle())
            {
                bundle = (OSCBundle)pack;
                message.TimeStamp = bundle.getTimeStamp();
            } else {
                bundle = new OSCBundle(extendedMode);
                var m = (OSCMessage)pack;
                bundle.Append(m);
                message.TimeStamp = Time.CurrentTime();
            }


            foreach (OSCMessage m in bundle.Values)
            {

                string[] address = m.Address.Trim(new char[] { '/' }).Split('/');

                string messageAddress = string.Join(".", address.Take(Math.Max(1, address.Length - contractAddress)).ToArray());
                if (messagePrefix.Trim() == "")
                    message.Topic = messageAddress;
                else message.Topic = messagePrefix + "." + messageAddress;
                
                Bin bin = null;
                // empty messages are usually used for requesting data
                if (m.Values.Count <= 0)
                {
                    // leave message emtpy
                }
                else
                {
                    // Todo: mixing of types in a singular message is not allowed right now! however, many uses of osc do mix values
                    bin = BinFactory.New(m.Values[0].GetType());
                    bin.AssignFrom(m.Values);

                    string attribName = string.Join(".", address.Skip(Math.Max(0, address.Length - contractAddress)).ToArray());
                    message[attribName] = bin;
                }
            }

            return message;
        }
    }
}
