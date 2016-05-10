using MsgPack;
using System.Linq;
using System.Collections.Generic;
using MsgPack.Serialization;
using System.Collections;
using System;

namespace VVVV.Packs.Messaging.Serializing
{
    /// <summary>
    ///		A custom serializer sample: Serialize <see cref="System.DateTime"/> as UTC.
    /// </summary>
    public class MsgPackMessageSerializer : MessagePackSerializer<Message>
    {
        // CAUTION: You MUST implement your serializer thread safe (usually, you can and you should implement serializer as immutable.)

        protected readonly HashSet<Type> AsInt;

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
                    if (bin.GetInnerType() != typeof(Message))
                        packer.Pack(bin.First);
                    else
                    {
                        var serializer = MessagePackSerializer.Get<Message>(OwnerContext);
                        serializer.PackTo(packer, bin.First as Message);
                    }
                }
                else
                {
                    packer.PackArrayHeader(bin.Count);
                    foreach (var item in bin) packer.Pack(item);
                }
            }
        }


        protected IEnumerable<Tuple<string, Bin>> NextBin(Unpacker unpacker)
        {
            while (unpacker.Read())
            {
                if (unpacker.LastReadData.IsRaw)
                {
                    string fieldName = unpacker.LastReadData.AsString();

                    unpacker.Read();
                    unpacker.UnpackSubtree();


                    if (unpacker.LastReadData.IsList)
                    {
                        var list = unpacker.LastReadData.AsList();

                        if (list.Count == 0) continue;

                        Bin bin;

                        var type = list.First().UnderlyingType;

                        if (type.IsPrimitive && AsInt.Contains(type))
                        {
                            bin = BinFactory.New(typeof(int));
                        }
                        else bin = BinFactory.New(type);

                        if (type != bin.GetInnerType())
                        {
                            var f = list.Select(boxed => Convert.ChangeType(boxed.ToObject(), bin.GetInnerType()));
                            bin.Add(f);
                        } else bin.Add(list.Select(boxed => boxed.ToObject() ) );

                        yield return new Tuple<string, Bin>( fieldName, bin );
                    }
                    else {

                        var data = unpacker.LastReadData;


                        if (data.IsDictionary)
                        {
                            var serializer = MessagePackSerializer.Get<Message>(OwnerContext);
                            var tmp =serializer.UnpackFrom(unpacker);

                        }


                        var type = data.UnderlyingType;
                        var bin = type.IsPrimitive && AsInt.Contains(type) ? BinFactory.New<int>() : BinFactory.New(type); // check if small ints were converted down for saving traffic

                        if (type != bin.GetInnerType())
                            bin.Add(Convert.ChangeType(data.ToObject(), bin.GetInnerType() ) );
                            else bin.Add(data.ToObject());

                        yield return new Tuple<string, Bin>(fieldName, bin);
                    }

                }
                

            }
        }

        protected override Message UnpackFromCore(Unpacker unpacker)
        {
            var message = new Message();

            var data = unpacker.LastReadData;


            if (AsInt.Contains(data.UnderlyingType))
            {
                var count = data.AsInt32();
                foreach ( var field in NextBin(unpacker))
                {
                    if (field.Item1 == "Topic")
                    {
                        message.Topic = field.Item2.First as string;
                        continue;
                    }
                    message[field.Item1] = field.Item2;
                }
            }

            if (data.IsDictionary)
            {
                foreach (var field in data.AsDictionary())
                {
                    var fieldName = field.Key.AsString();

//                    var bin = field.Value.


                }
            }


            return message;
        }
    }
}
