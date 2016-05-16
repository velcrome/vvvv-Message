using MsgPack;
using System.Collections.Generic;
using MsgPack.Serialization;
using System.Collections;
using System;
using System.IO;

namespace VVVV.Packs.Messaging.Serializing
{
    using Utils.VColor;
    using Utils.VMath;
    using Time = VVVV.Packs.Time.Time;


    public class MsgPackMessageSerializer : MessagePackSerializer<Message>
    {
        // CAUTION: You MUST implement your serializer thread safe (usually, you can and you should implement serializer as immutable.)

        protected readonly HashSet<Type> AsInt;
        protected ExtTypeCodeMapping Code
        {
            get
            {
                return OwnerContext.ExtTypeCodeMapping;
            }
        }


        protected MsgPackTimeSerializer TimeSerializer;
        protected MsgPackVector2dSerializer Vector2dSerializer;
        protected MsgPackVector3dSerializer Vector3dSerializer;
        protected MsgPackVector4dSerializer Vector4dSerializer;

        protected MsgPackMatrixSerializer MatrixSerializer;
        protected MsgPackColorSerializer ColorSerializer;
        protected MsgPackStreamSerializer StreamSerializer;

        // ownerContext should be match the context to be registered.
        public MsgPackMessageSerializer(SerializationContext ownerContext) : base(ownerContext)
        {
            var subtypes = new Type[] { typeof(byte), typeof(sbyte), typeof(Int16), typeof(UInt16), typeof(UInt32), typeof(Int64), typeof(UInt64) };
            AsInt = new HashSet<Type>(subtypes);

            TimeSerializer = new MsgPackTimeSerializer(OwnerContext);
            Vector2dSerializer = new MsgPackVector2dSerializer(OwnerContext);
            Vector3dSerializer = new MsgPackVector3dSerializer(OwnerContext);
            Vector4dSerializer = new MsgPackVector4dSerializer(OwnerContext);

            MatrixSerializer = new MsgPackMatrixSerializer(OwnerContext);
            ColorSerializer = new MsgPackColorSerializer(OwnerContext);

            StreamSerializer = new MsgPackStreamSerializer(OwnerContext);
        }

        protected override void PackToCore(Packer packer, Message message)
        {
            packer.PackMapHeader(message.Data.Count + 1); // accomodate for all fields, topic and stamp

            packer.PackString("Topic");
            packer.PackString(message.Topic);

//            packer.PackString("Stamp");
//            packer.PackExtendedTypeValue(Code["Time"], TimeSerializer.PackSingleObject(message.TimeStamp));

            foreach (var fieldName in message.Fields)
            {
                packer.PackString(fieldName);

                var bin = message[fieldName];
                if (bin.Count == 1)
                {
                    var alias = TypeIdentity.Instance.FindAlias(bin.GetInnerType());
                    PackSlice(packer, alias, bin.First);

                }
                else
                {
                    packer.PackArrayHeader(bin.Count);
                    var alias = TypeIdentity.Instance.FindAlias(bin.GetInnerType());
                    foreach (var item in bin)
                    {
                        PackSlice(packer, alias, item);
                    }
                }
            }
        }

        private void PackSlice(Packer packer, string alias, object item)
        {
            if (alias == "Message") {
                PackToCore(packer, (Message)item);
                return;
            }

            byte[] raw;
            switch (alias)
            {
                case "Raw":         raw = StreamSerializer.PackSingleObject((Stream)item);
                                    packer.PackExtendedTypeValue(Code[alias], raw);
                                    break;
                case "Time":        raw = TimeSerializer.PackSingleObject((Time)item);
                                    packer.PackExtendedTypeValue(Code[alias], raw);
                                    break;
                case "Vector2d":    raw = Vector2dSerializer.PackSingleObject((Vector2D)item);
                                    packer.PackExtendedTypeValue(Code[alias], raw);
                                    break;
                case "Vector3d":    raw = Vector3dSerializer.PackSingleObject((Vector3D)item);
                                    packer.PackExtendedTypeValue(Code[alias], raw);
                                    break;
                case "Vector4d":    raw = Vector4dSerializer.PackSingleObject((Vector4D)item);
                                    packer.PackExtendedTypeValue(Code[alias], raw);
                                    break;
                case "Transform":   raw = MatrixSerializer.PackSingleObject((Matrix4x4)item);
                                    packer.PackExtendedTypeValue(Code[alias], raw);
                                    break;
                case "Color":       raw = Vector4dSerializer.PackSingleObject((RGBAColor)item);
                                    packer.PackExtendedTypeValue(Code[alias], raw);
                                    break;
                case "string":      packer.PackString((string)item);
                                    break;
                default:            packer.Pack(item); // all primitives
                                    break;
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
                    MessagePackExtendedTypeObject ext;
                    unpacker.ReadMessagePackExtendedTypeObject(out ext);
                    message.TimeStamp = TimeSerializer.UnpackSingleObject(ext.GetBody());
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

                    unpacker.Read();

                    if (unpacker.IsMapHeader) // multiple nested messages
                    {
                        bin = BinFactory.New<Message>();

                        for (int i = 0;i<binCount;i++)
                        {
                            bin.Add(UnpackFromCore(unpacker));
                            unpacker.Read();
                        }
                    }
                    else // multiple slices
                    {
                        bin = BinFromCurrent(unpacker);
                        var alias = TypeIdentity.Instance.FindAlias(bin.GetInnerType());

                        for (int i=1;i<binCount;i++)
                        {
                            unpacker.Read();
                            var item = FromCurrent(unpacker, alias);
                            bin.Add(item);
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

        private object FromCurrent(Unpacker unpacker, string alias)
        {
            var data = unpacker.LastReadData;

            switch (alias) {
                case "bool":    return data.AsBoolean();
                case "int" :    return data.AsInt32();
                case "float":   return data.AsSingle();
                case "double":  return data.AsDouble();
                case "string":  return data.AsString();
                case "Message": return UnpackFromCore(unpacker);
            }

            var ext = (MessagePackExtendedTypeObject)data;

            switch (alias)
            {
                case "Time": return TimeSerializer.UnpackSingleObject(ext.GetBody());
                case "Vector2d": return Vector2dSerializer.UnpackSingleObject(ext.GetBody());
                case "Vector3d": return Vector3dSerializer.UnpackSingleObject(ext.GetBody());
                case "Vector4d": return Vector4dSerializer.UnpackSingleObject(ext.GetBody());
                case "Color": return ColorSerializer.UnpackSingleObject(ext.GetBody());
                case "Transform": return MatrixSerializer.UnpackSingleObject(ext.GetBody());
                case "Raw": return StreamSerializer.UnpackSingleObject(ext.GetBody());
            }
            throw new TypeNotSupportedException("There is no msgpack Serializer for the alias " + alias);
        }

        private Bin BinFromCurrent(Unpacker unpacker)
        {
            var data = unpacker.LastReadData;

            if(data.IsArray || data.IsList ) throw new ParseMessageException("Message cannot handle nested arrays or lists.");
            if (data.IsNil) throw new ParseMessageException("Message cannot infer type for Nil.");

            var type = data.UnderlyingType;

            Bin bin = null;

            if (type == typeof(byte[]))
            {
                var ext = (MessagePackExtendedTypeObject)data;
                foreach (var mapping in Code)
                {
                    if (mapping.Value == ext.TypeCode) {
                        bin = BinFactory.New(TypeIdentity.Instance.FindType(mapping.Key));
                        break;
                    }
                }
            }
            else bin = type.IsPrimitive && AsInt.Contains(type) ? BinFactory.New<int>() : BinFactory.New(type); // check if small ints were converted down for saving traffic

            if (bin == null) throw new TypeNotSupportedException("Cannot find out type from msgpack");

            var alias = TypeIdentity.Instance.FindAlias(bin.GetInnerType());
            bin.Add(FromCurrent(unpacker, alias));

            return bin;
        }

    }
}
