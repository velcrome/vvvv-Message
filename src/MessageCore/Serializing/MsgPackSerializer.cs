using MsgPack;
using System.Linq;
using System.Collections.Generic;
using MsgPack.Serialization;
using System.Collections;
using System;
using System.IO;

namespace VVVV.Packs.Messaging.Serializing
{
    /// <summary>
    ///		A custom serializer sample: Serialize <see cref="System.DateTime"/> as UTC.
    /// </summary>
    public class MsgPackMessageSerializer : MessagePackSerializer<Message>
    {
        // CAUTION: You MUST implement your serializer thread safe (usually, you can and you should implement serializer as immutable.)

        protected readonly HashSet<Type> AsInt;
        protected delegate bool Next(out object data);

        // ownerContext should be match the context to be registered.
        public MsgPackMessageSerializer(SerializationContext ownerContext) : base(ownerContext)
        {
            var subtypes = new Type[] { typeof(byte), typeof(sbyte), typeof(Int16), typeof(UInt16), typeof(UInt32), typeof(Int64), typeof(UInt64) };
            AsInt = new HashSet<Type>(subtypes);
        }

        protected override void PackToCore(Packer packer, Message message)
        {
            packer.PackMapHeader(message.Data.Count + 1);

            packer.PackString("Topic");
            packer.PackString(message.Topic);

            foreach (var fieldName in message.Fields)
            {
                packer.PackString(fieldName);

                var bin = message[fieldName];
                if (bin.Count == 1)
                {
                    if (bin.GetInnerType() == typeof(Message))
                    {
                        PackToCore(packer, (Message)bin.First);
                    }
                    else if (bin.GetInnerType() == typeof(Stream))
                    {
                        var stream = (Stream)bin.First;
                        byte[] raw = new byte[stream.Length];

                        stream.Position = 0;
                        stream.Write(raw, 0, (int)stream.Length);

                        packer.PackRaw(raw);

                    }
                    else packer.Pack(bin.First);

                }
                else
                {
                    packer.PackArrayHeader(bin.Count);
                    foreach (var item in bin) packer.Pack(item);
                }
            }
        }

        /// <summary>
        /// Utility method to unpack from msgpack. 
        /// </summary>
        /// <remarks>Will be used recursively to parse nested messages</remarks>
        /// <param name="unpacker"></param>
        /// <returns></returns>
        /// <exception cref="TypeNotSupportedException"
        /// <exception cref="EmptyBinException"
        /// <exception cref="ParseMessageException"
        /// <exception cref="InvalidCastException"
        /// <exception cref="OverflowException" 
        protected override Message UnpackFromCore(Unpacker unpacker)
        {
            var message = new Message();

            if (!unpacker.IsMapHeader) throw new ParseMessageException("Incoming msgpack stream does not seem to be a Map, only Maps can be transcoded to Message. Did you rewind Position?");

            var count = unpacker.ItemsCount;

            string fieldName;
            while (count > 0 && unpacker.ReadString(out fieldName))
            {
                count--;

                if (fieldName == "Topic") {
                    string topic;
                    unpacker.ReadString(out topic);
                    message.Topic = topic;
                    continue;
                }

                if (fieldName == "Stamp")
                {
                    // todo: add time to msgpack
                    continue;
                }

                Bin bin;
                unpacker.Read();

                if (unpacker.IsMapHeader) // single Message
                {
                    bin = BinFactory.New<Message>(UnpackFromCore(unpacker));
                }
                else if (unpacker.IsArrayHeader) // multiples!
                {
                    long binCount = unpacker.LastReadData.AsInt64();
                    if (binCount <= 0) continue; // cannot infer type, so skip

                    if (unpacker.IsMapHeader) // multiple nested messages
                    {
                        bin = BinFactory.New<Message>();
                        for (int i = 0;i<binCount;i++)
                        {
                            bin.Add(UnpackFromCore(unpacker));
                        }
                    }
                    else // multiple slices
                    {
                        unpacker.Read();
                        bin = BinFromCurrent(unpacker);
                        for (int i=1;i<binCount;i++)
                        {
                            bin.Add(GetNext(unpacker, bin.GetInnerType()));
                        }
                    }
                }
                else // single item
                {
                    bin = BinFromCurrent(unpacker);
                }

                message[fieldName] = bin;

            }
            return message;
        }

        private object GetNext(Unpacker unpacker, Type targetType)
        {
            unpacker.Read();
            var data = unpacker.LastReadData;

            if (data.IsRaw)
            {
                return RawFromCurrent(unpacker);
            }

            if (data.UnderlyingType != targetType)
                return Convert.ChangeType(data.ToObject(), targetType);
            else return data.ToObject();
        }

        private Bin BinFromCurrent(Unpacker unpacker)
        {
            var data = unpacker.LastReadData;

            if(data.IsArray || data.IsList ) throw new ParseMessageException("Message cannot handle nested arrays or lists.");
            if (data.IsNil) throw new EmptyBinException("Message cannot infer type for Nil.");

            if (data.IsRaw)
            {
                var rawBin = BinFactory.New<Stream>(RawFromCurrent(unpacker));
                return rawBin;
            }

            var type = data.UnderlyingType;
            var bin = type.IsPrimitive && AsInt.Contains(type) ? BinFactory.New<int>() : BinFactory.New(type); // check if small ints were converted down for saving traffic

            if (type != bin.GetInnerType())
                bin.Add(Convert.ChangeType(data.ToObject(), bin.GetInnerType()));
            else bin.Add(data.ToObject());

            return bin;
        }

        private Stream RawFromCurrent(Unpacker unpacker)
        {
            var raw = unpacker.LastReadData.AsBinary();
            var stream = new MemoryStream(raw);
            return stream;
        }
    }
}
