using System.IO;
using System.Linq;
using VVVV.Utils.OSC;


namespace VVVV.Packs.Messaging.Serializing
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Time = VVVV.Packs.Time.Time;

    public static class OSCExtensions
    {

        public static string AsAddress(string topic)
        {
            return "/" + string.Join("/", topic.Trim().Trim(new char[] { '.' }).Split('.'));
        }

        public static string AsTopic(string address)
        {
            return string.Join(".", address.Trim().Trim(new char[] { '/' }).Split('/'));
        }

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

                OSCMessage msg = new OSCMessage(oscAddress, extendedMode);
                Bin bin = message[name];
                for (int i = 0; i < bin.Count; i++) msg.Append(bin[i]);
                bundle.Append(msg);
            }
            return new MemoryStream(bundle.BinaryData); // packs implicitly
        }

        public static Stream ToOSC(this Message message, IEnumerable<FormularFieldDescriptor> fields, bool extendedMode = false)
        {
            string oscAddress = "";
            foreach (string part in message.Topic.Split('.'))
            {
                if (part.Trim() != "") oscAddress += "/" + part;
            }

            var msg = new OSCMessage(oscAddress, extendedMode);
            foreach (var field in fields)
            {
                var name = field.Name;

                Bin bin = message[field.Name];
                var size = bin == null ? 0 : bin.Count;
                var count = field.DefaultSize < 1 ? size : field.DefaultSize;

                for (int i = 0; i < count; i++)
                {
                    if (bin == null || i >= bin.Count)
                        msg.Append(TypeIdentity.Instance.NewDefault(field.Type)); // send out defaults to keep the integrity of the osc message
                    else msg.Append(bin[i]);
                }
                
            }
            return new MemoryStream(msg.BinaryData); // packs implicitly
        }

        private static string CommonPrefix(string[] addresses)
        {
            if (addresses.Length == 0) return "";
            if (addresses.Length == 1) return addresses[0];

            int prefixLength = 0;
            foreach (char c in addresses[0])
            {
                foreach (string s in addresses)
                {
                    if (s.Length <= prefixLength || s[prefixLength] != c)
                        return addresses[0].Substring(0, prefixLength);
                }
                prefixLength++;
            }
            return addresses[0]; // all strings identical up to shortest length
        }

        public static readonly Encoding OSCEncoding = new ASCIIEncoding();

        public static string PeekAddress(Stream stream)
        {
            if (stream == null || stream.Length == 0) return "";

            string address = "";

            using (var reader = new BinaryReader(stream, OSCEncoding, true))
            {
                while (reader.PeekChar() != ',' && stream.Position < stream.Length)
                {
                    var c = reader.ReadChars(4);

                    address += new string(c);

                }
            }

            return address.TrimEnd('\0'); // remove padding
        }


        public static Message FromOSC(Stream stream, Dictionary<string, IEnumerable<FormularFieldDescriptor>> parser, bool extendedMode = false)
        {
            if (stream == null || stream.Length == 0) return null;

            OSCPacket oscMessage;

            stream.Position = 0;
            var reader = new BinaryReader(stream);

            {
                var bytes = reader.ReadBytes((int)stream.Length);
                oscMessage = OSCPacket.Unpack(bytes, extendedMode);
            }

            if (oscMessage.IsBundle()) return null;

            if (!parser.ContainsKey(oscMessage.Address))
                return null; // skip if unknown address

            Message message = new Message();
            message.TimeStamp = Time.CurrentTime();
            message.Topic = AsTopic(oscMessage.Address);

            var fields = parser[oscMessage.Address].ToList();

            int pointer = 0;
            for (int i = 0; i<fields.Count();i++)
            {
                var field = fields[i];

                var count = field.DefaultSize; // todo: how to deal with -1 ?
                if (count < 0) count = 1;

                var bin = BinFactory.New(field.Type, count);

                for (int j = 0; (j < count) && (pointer < oscMessage.Values.Count); j++, pointer++) // stop short if running out of parser fields OR data from osc
                {
                    bin.Add(oscMessage.Values[pointer]); // implicit conversion
                }
                message[field.Name] = bin;
            }

            return message;
        }


        public static Message FromOSC(Stream stream, bool extendedMode = false, string messagePrefix = "", int contractAddress = 1)
        {
            if (stream == null || stream.Length <= 0) return null;

            stream.Position = 0;
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);

            if (bytes.Length == 0) return null;

            Message message = new Message();
            var pack = OSCPacket.Unpack(bytes, extendedMode);
            bool useNesting;

            OSCBundle bundle;
            if (pack.IsBundle())
            {
                bundle = (OSCBundle)pack;
                message.TimeStamp = bundle.getTimeStamp(); // if not set, will be 1.1.1900
                useNesting = true;
            }
            else
            {
                bundle = new OSCBundle(extendedMode);
                var m = (OSCMessage)pack;
                bundle.Append(m);
                message.TimeStamp = Time.CurrentTime();
                useNesting = false;
            }


            foreach (OSCMessage m in bundle.Values)
            {

                string[] address = m.Address.Trim(new char[] { '/' }).Split('/');

                string messageAddress = string.Join(".", address.Take(Math.Max(1, address.Length - contractAddress)).ToArray());

                if (messagePrefix.Trim() == "") message.Topic = messageAddress;
                    else message.Topic = messagePrefix + "." + messageAddress;
                
                // empty messages are usually used for requesting data
                if (m.Values.Count <= 0)
                {
                    // leave message emtpy, cannot infer type.
                }
                else
                {
                    var usedTypes = (
                                        from v in m.Values.ToArray()
                                        select v.GetType()
                                    ).Distinct();

                    string attribName = string.Join(".", address.Skip(Math.Max(0, address.Length - contractAddress)).ToArray());

                    if (usedTypes.Count() > 1)
                    {
                        var inner = new Message(message.Topic + "." + attribName);
                        var max = m.Values.Count;

                        for( int i=0;i< max;i++)
                        {
                            var item = m.Values[i];
                            var num = i.ToString();
                            inner[num] = BinFactory.New(item.GetType());
                            inner[num].Add(item);
                        }

                        if (useNesting)
                        {
                            message[attribName] = BinFactory.New(inner);
                        }
                        else
                        {
                            message = inner;
                        }
                    }
                    else
                    {
                        var bin = BinFactory.New(usedTypes.First());
                        bin.AssignFrom(m.Values);
                        message[attribName] = bin;
                    }
                }
            }

            return message;
        }
    }
}
