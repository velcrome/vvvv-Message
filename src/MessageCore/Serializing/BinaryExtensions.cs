using System;
using System.IO;
using VVVV.Pack.Game.Core;

using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Packs.Message.Core.Serializing
{
    public static class BinaryExtensions
    {

        public static Stream Serialize(this Message message)
        {
            Stream serialized = new MemoryStream();

            serialized.SetLength(0);
            serialized.WriteUint(message.Address.UnicodeLength());
            serialized.WriteUnicode(message.Address);

            // Todo: serialize timestamp.

            serialized.WriteUint((uint)message.MessageData.Count);
            foreach (var key in message.Attributes)
            {
                serialized.WriteUint(key.UnicodeLength());
                serialized.WriteUnicode(key);

                var binData = message[key].Serialize();

                serialized.WriteUint((uint)binData.Length);
                binData.CopyTo(serialized);
            }
            return serialized;
        }

        public static Message DeSerializeMessage(Stream input)
        {
            var message = new Message();
            input.Position = 0;

            uint addressLength = input.ReadUint();
            message.Address = input.ReadUnicode((int)addressLength);

           // Todo: deserialize timestamp


            while (input.Position < input.Length)
            {
                uint keyLength = input.ReadUint();
                string key = input.ReadUnicode((int)keyLength);

                uint binDataLength = input.ReadUint();
                Stream binData = new MemoryStream();
                input.CopyTo(binData, (int)binDataLength);

                message.Add(key, DeSerializeBin(binData));
            }

            return new Message();
        }

        public static Stream Serialize(this Bin bin)
        {
            Stream serialized = new MemoryStream();

            serialized.SetLength(0);
            serialized.WriteUint((uint)bin.Count);
            serialized.WriteUint(bin.GetType().ToString().UnicodeLength());
            serialized.WriteUnicode(bin.GetType().ToString());

            for (int i = 0; i < bin.Count; i++)
            {
                uint l = 0;
                if (bin.GetInnerType() == typeof(bool))
                {
                    l = 1;
                    serialized.WriteUint(l);
                    serialized.WriteBool((bool)bin[i]);
                }
                if (bin.GetInnerType() == typeof(int))
                {
                    l = 4;
                    serialized.WriteUint(l);
                    serialized.WriteInt((int)bin[i]);
                }
                if (bin.GetInnerType() == typeof(float))
                {
                    l = 4;
                    serialized.WriteUint(l);
                    serialized.WriteFloat((float)bin[i]);
                }
                if (bin.GetInnerType() == typeof(double))
                {
                    l = 8;
                    serialized.WriteUint(l);
                    serialized.WriteDouble((double)bin[i]);
                }
                if (bin.GetInnerType() == typeof(string))
                {
                    l = ((string)bin[i]).UnicodeLength();
                    serialized.WriteUint(l);
                    serialized.WriteUnicode((string)bin[i]);
                }

                if (bin.GetInnerType() == typeof(RGBAColor))
                {
                    l = 32;
                    serialized.WriteUint(l);
                    serialized.WriteDouble(((RGBAColor)bin[i]).R);
                    serialized.WriteDouble(((RGBAColor)bin[i]).G);
                    serialized.WriteDouble(((RGBAColor)bin[i]).B);
                    serialized.WriteDouble(((RGBAColor)bin[i]).A);
                }
                if (bin.GetInnerType() == typeof(Vector2D))
                {
                    l = 16;
                    serialized.WriteUint(l);
                    serialized.WriteDouble(((Vector2D)bin[i]).x);
                    serialized.WriteDouble(((Vector2D)bin[i]).y);
                }
                if (bin.GetInnerType() == typeof(Vector3D))
                {
                    l = 24;
                    serialized.WriteUint(l);
                    serialized.WriteDouble(((Vector3D)bin[i]).x);
                    serialized.WriteDouble(((Vector3D)bin[i]).y);
                    serialized.WriteDouble(((Vector3D)bin[i]).z);
                }
                if (bin.GetInnerType() == typeof(Vector4D))
                {
                    l = 32;
                    serialized.WriteUint(l);
                    serialized.WriteDouble(((Vector4D)bin[i]).x);
                    serialized.WriteDouble(((Vector4D)bin[i]).y);
                    serialized.WriteDouble(((Vector4D)bin[i]).z);
                    serialized.WriteDouble(((Vector4D)bin[i]).w);
                }
                if (bin.GetInnerType() == typeof(Matrix4x4))
                {
                    l = 128;
                    serialized.WriteUint(l);
                    for (int j = 0; j < 16; j++)
                    {
                        serialized.WriteDouble(((Matrix4x4)bin[i]).Values[j]);
                    }
                }
                if (bin.GetInnerType() == typeof(Stream))
                {
                    l = (uint)((Stream)bin[i]).Length;
                    serialized.WriteUint(l);
                    ((Stream)bin[i]).Position = 0;
                    ((Stream)bin[i]).CopyTo(serialized);
                }
                if (bin.GetInnerType() == typeof(Time.Time))
                {
                    // not implemented
                }

            }
            return serialized;
        }

        public static Bin DeSerializeBin(Stream input)
        {
            input.Position = 0;

            uint objcount = input.ReadUint();
            uint typeL = input.ReadUint();

            var type = Type.GetType(input.ReadUnicode((int)typeL));
            var bin = Bin.New(type);


            for (int i = 0; i < objcount; i++)
            {
                uint l = input.ReadUint();
                if (type == typeof(bool)) bin.Add(input.ReadBool());
                if (type == typeof(int)) bin.Add(input.ReadInt());
                if (type == typeof(float)) bin.Add(input.ReadFloat());
                if (type == typeof(double)) bin.Add(input.ReadDouble());
                if (type == typeof(string)) bin.Add(input.ReadUnicode((int)l));

                if (type == typeof(RGBAColor))
                {
                    double[] val = new double[4];
                    for (int j = 0; j < val.Length; j++) val[j] = input.ReadDouble();
                    RGBAColor res = new RGBAColor(val);
                    bin.Add(res);
                }
                if (type == typeof(Vector2D))
                {
                    Vector2D res = new Vector2D();
                    res.x = input.ReadDouble();
                    res.y = input.ReadDouble();
                    bin.Add(res);
                }
                if (type == typeof(Vector3D))
                {
                    Vector3D res = new Vector3D();
                    res.x = input.ReadDouble();
                    res.y = input.ReadDouble();
                    res.z = input.ReadDouble();
                    bin.Add(res);
                }
                if (type == typeof(Vector4D))
                {
                    Vector4D res = new Vector4D();
                    res.x = input.ReadDouble();
                    res.y = input.ReadDouble();
                    res.z = input.ReadDouble();
                    res.w = input.ReadDouble();
                    bin.Add(res);
                }
                if (type == typeof(Matrix4x4))
                {
                    Matrix4x4 res = new Matrix4x4();
                    for (int j = 0; j < 16; j++) res.Values[j] = input.ReadDouble();
                    bin.Add(res);
                }
                if (type == typeof(Stream))
                {
                    Stream res = new MemoryStream();
                    input.CopyTo(res, (int)l);
                    res.Position = 0;
                    bin.Add(res);
                }
            }
            return bin;
        }

    }
}
