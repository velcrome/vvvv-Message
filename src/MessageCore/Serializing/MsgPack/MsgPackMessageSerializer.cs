using MsgPack;
using System.Collections.Generic;
using MsgPack.Serialization;
using System.Linq;
using System;
using System.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Packs.Timing;

namespace VVVV.Packs.Messaging.Serializing
{
    public class MsgPackMessageSerializer : MessagePackSerializer<Message>
    {
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

            OwnerContext.ExtTypeCodeMapping.Add("Time", 0x10);
            OwnerContext.ExtTypeCodeMapping.Add("Raw", 0x11);

            TimeSerializer = new MsgPackTimeSerializer(OwnerContext);
            StreamSerializer = new MsgPackStreamSerializer(OwnerContext);

            OwnerContext.ExtTypeCodeMapping.Add("Vector2d", 0x12);
            OwnerContext.ExtTypeCodeMapping.Add("Vector3d", 0x13);
            OwnerContext.ExtTypeCodeMapping.Add("Vector4d", 0x14);
            OwnerContext.ExtTypeCodeMapping.Add("Transform", 0x15);
            OwnerContext.ExtTypeCodeMapping.Add("Color", 0x16);

            Vector2dSerializer = new MsgPackVector2dSerializer(OwnerContext);
            Vector3dSerializer = new MsgPackVector3dSerializer(OwnerContext);
            Vector4dSerializer = new MsgPackVector4dSerializer(OwnerContext);
            MatrixSerializer = new MsgPackMatrixSerializer(OwnerContext);
            ColorSerializer = new MsgPackColorSerializer(OwnerContext);
        }

        protected override void PackToCore(Packer packer, Message message)
        {
            packer.PackMapHeader(message.Data.Count + 2); // accomodate for all fields, topic and stamp

            packer.PackString("Topic");
            packer.PackString(message.Topic);

            packer.PackString("Stamp");
            packer.PackExtendedTypeValue(Code["Time"], TimeSerializer.PackSingleObject(message.TimeStamp));

            foreach (var fieldName in message.Fields)
            {
                var bin = message[fieldName];
                var typeRecord = TypeIdentity.Instance[bin.GetInnerType()];

                if (typeRecord == null) continue;
                if (typeRecord.CloneMethod == CloneBehaviour.Null) continue;

                packer.PackString(fieldName);

                if (bin.Count == 1)
                {
                    var alias = typeRecord.Alias;
                    PackSlice(packer, alias, bin.First);

                }
                else
                {
                    packer.PackArrayHeader(bin.Count);
                    var alias = typeRecord.Alias;
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
                                    break;
                case "Time":        raw = TimeSerializer.PackSingleObject((Time)item);
                                    break;
                case "Vector2d":    raw = Vector2dSerializer.PackSingleObject((Vector2D)item);
                                    break;
                case "Vector3d":    raw = Vector3dSerializer.PackSingleObject((Vector3D)item);
                                    break;
                case "Vector4d":    raw = Vector4dSerializer.PackSingleObject((Vector4D)item);
                                    break;
                case "Transform":   raw = MatrixSerializer.PackSingleObject((Matrix4x4)item);
                                    break;
                case "Color":       raw = Vector4dSerializer.PackSingleObject((RGBAColor)item);
                                    break;
                case "string":      packer.PackString((string)item); // Utf8, without BOM, as ambiguous str
                                    return; // preempt
                default:            packer.Pack(item); // all primitives
                                    return; // preempt
            }
            packer.PackExtendedTypeValue(Code[alias], raw);

        }

        protected bool AssertSuccess(bool success)
        {
            if (!success) throw new ParseMessageException("Internal msgpack-cli Error during Read().");
            return success;
        }

        /// <summary>
        /// Utility method to unpack from msgpack. 
        /// </summary>
        /// <remarks>Will be used recursively to parse nested messages</remarks>
        /// <param name="unpacker"></param>
        /// <returns></returns>
        /// <exception cref="TypeNotSupportedException">Thrown, when a datatype of msgpack is not supported, e.g. enums</exception>
        /// <exception cref="ParseMessageException">Generic exception </exception>
        /// <exception cref="OverflowException" >Thrown from msgpack, if a received long does not fit into an int.</exception>
        protected override Message UnpackFromCore(Unpacker unpacker)
        {
            var message = new Message();

            if (!unpacker.IsMapHeader) throw new ParseMessageException("Incoming msgpack stream does not seem to be a Map, only Maps can be transcoded to Message. Did you rewind Position?");

            var count = unpacker.ItemsCount;

            while (count > 0 && AssertSuccess(unpacker.Read()))
            {
                count--;

                if (!unpacker.LastReadData.IsRaw) throw new ParseMessageException("Only strings can be keys of supported dictionaries. Sorry, only Json style here");

                string fieldName = unpacker.LastReadData.AsString(); 

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
                AssertSuccess(unpacker.Read());

                if (unpacker.IsMapHeader) // single Message
                {
                    bin = BinFactory.New(UnpackFromCore(unpacker));
                }
                else if (unpacker.IsArrayHeader) // multiples!
                {
                    long binCount = unpacker.LastReadData.AsInt64();
                    if (binCount <= 0) continue; // cannot infer type, so skip

                    AssertSuccess(unpacker.Read()); // populates a new MessagePackObject, GC ahoy

                    if (unpacker.IsMapHeader) // multiple nested messages
                    {
                        bin = BinFactory.New<Message>(UnpackFromCore(unpacker));

                        for (int i = 1;i<binCount;i++)
                        {
                            AssertSuccess(unpacker.Read());
                            bin.Add(UnpackFromCore(unpacker));
                        }
                    }
                    else // multiple slices
                    {
                        bin = BinFromCurrent(unpacker);
                        var alias = TypeIdentity.Instance[bin.GetInnerType()].Alias;

                        for (int i=1;i<binCount;i++)
                        {
                            AssertSuccess(unpacker.Read());
                            var item = FromCurrent(unpacker, alias);
                            bin.Add(item);
                        }
                    }
                }
                else // single slice of non-Message
                {
                    bin = BinFromCurrent(unpacker);
                }

                message[fieldName] = bin;
            }
            return message;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="unpacker"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        /// <exception cref="ParseMessageException">thrown if any error is detected during parsing of a single Item</exception>
        protected object FromCurrent(Unpacker unpacker, string alias)
        {
            var data = unpacker.LastReadData;

            try
            {
                switch (alias) {
                    case "bool": return data.AsBoolean();
                    case "int": return data.AsInt32();
                    case "float": return data.AsSingle();
                    case "double": return data.AsDouble();
                    case "string": return data.AsString();
                    case "Message": return UnpackFromCore(unpacker);
                }
            }
            catch (Exception)
            {
                var pEx = new ParseMessageException("Types of \"" + alias + "\" must be encoded as a valid msgpack primitive, or as a sub-directory in the case of a nested message.");
                throw pEx;
            }

            try
            {
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
            } catch (Exception ex)
            {
                var pEx = new ParseMessageException("Typtes of \"" + alias + "\" must be encoded as a known msgpack Extension.", ex);
                throw pEx;
            } 

            throw new TypeNotSupportedException("There is no msgpack Serializer for the alias \"" + alias+"\".");
        }

        protected Bin BinFromCurrent(Unpacker unpacker)
        {
            var data = unpacker.LastReadData;

            if(data.IsArray || data.IsList ) throw new ParseMessageException("Message cannot handle nested arrays or lists.");
            if (data.IsNil) throw new ParseMessageException("Message cannot infer type for Nil.");

            var type = data.UnderlyingType;

            Bin bin = null;

            if (type == typeof(byte[]))
            {
                var ext = (MessagePackExtendedTypeObject)data;
                var binType = (
                            from mapping in Code
                            where mapping.Value == ext.TypeCode
                            let t = TypeIdentity.Instance[mapping.Key]?.Type
                            where t != null
                            select t
                            ).FirstOrDefault();
                if (binType == null) new ParseMessageException("No matching extension deserializer found in the SerializationContext");

                bin = BinFactory.New(binType);
            }
            else bin = type.IsPrimitive && AsInt.Contains(type) ? BinFactory.New<int>() : BinFactory.New(type); // check if small ints were converted down for saving traffic

            if (bin == null) throw new TypeNotSupportedException("Cannot find out type from msgpack");

            var alias = TypeIdentity.Instance[bin.GetInnerType()].Alias;
            bin.Add(FromCurrent(unpacker, alias));

            return bin;
        }

    }
}
