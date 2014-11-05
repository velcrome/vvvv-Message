using System;
using System.IO;
using System.Linq;
using VVVV.Pack.Game.Core.Serializing;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Packs.Message.Core.Serializing
{
    public static class BinaryExtensions
    {

        public static Stream Serialize(this Message message)
        {
            Stream serialized = new MemoryStream();

//            serialized.SetLength(0);
            serialized.WriteUint(message.Address.UnicodeLength());
            serialized.WriteUnicode(message.Address);

            var t = message.TimeStamp;
            var time = t.UniversalTime.ToString("yyyy-MM-dd HH:mm:ss.ffff");
            var zone = t.TimeZone.Id;

            serialized.WriteUint(time.UnicodeLength());
            serialized.WriteUnicode(time);

            serialized.WriteUint(zone.UnicodeLength());
            serialized.WriteUnicode(zone);

            serialized.WriteUint((uint)message.Attributes.Count());
            foreach (var key in message.Attributes)
            {
                serialized.WriteUint(key.UnicodeLength());
                serialized.WriteUnicode(key);

                var binData = message[key].Serialize();
                binData.Position = 0;

//                serialized.WriteUint((uint)binData.Length);
                binData.CopyTo(serialized);
            }
            return serialized;
        }

        public static Message DeSerializeMessage(this Stream input)
        {
            var message = new Message();
            input.Position = 0;

            uint addressLength = input.ReadUint();
            message.Address = input.ReadUnicode((int)addressLength);

            uint l = input.ReadUint();
            var t = input.ReadUnicode((int)l);
            var utcTime = Time.Time.StringAsTime("UTC", t, "yyyy-MM-dd HH:mm:ss.ffff");

            l = input.ReadUint();
            var z = input.ReadUnicode((int)l);

            message.TimeStamp = Time.Time.ChangeTimezone(utcTime, z);

            uint attributeCount = input.ReadUint();

            for (int i = 0; i < attributeCount;i++)
            {
                uint keyLength = input.ReadUint();
                string key = input.ReadUnicode((int)keyLength);

//                uint binLength = input.ReadUint();
  
                message.Data[key] = DeSerializeBin(input);
            }

            return message;
        }

        public static Stream Serialize(this Bin bin)
        {
            Stream serialized = new MemoryStream();

            serialized.WriteUint((uint)bin.Count);

            var alias = TypeIdentity.Instance.FindAlias(bin.GetInnerType());
            serialized.WriteUint(alias.UnicodeLength());
            serialized.WriteUnicode(alias);

            for (int i = 0; i < bin.Count; i++)
            {
                if (bin.GetInnerType() == typeof(bool))
                {
                    serialized.WriteBool((bool)bin[i]);
                }
                if (bin.GetInnerType() == typeof(int))
                {
                    serialized.WriteInt((int)bin[i]);
                }
                if (bin.GetInnerType() == typeof(float))
                {
                    serialized.WriteFloat((float)bin[i]);
                }
                if (bin.GetInnerType() == typeof(double))
                {
                    serialized.WriteDouble((double)bin[i]);
                }
                if (bin.GetInnerType() == typeof(string))
                {
                    var l = ((string)bin[i]).UnicodeLength();
                    serialized.WriteUint(l);
                    if (l>0) serialized.WriteUnicode((string)bin[i]);
                }

                if (bin.GetInnerType() == typeof(RGBAColor))
                {
                    serialized.WriteDouble(((RGBAColor)bin[i]).R);
                    serialized.WriteDouble(((RGBAColor)bin[i]).G);
                    serialized.WriteDouble(((RGBAColor)bin[i]).B);
                    serialized.WriteDouble(((RGBAColor)bin[i]).A);
                }
                if (bin.GetInnerType() == typeof(Vector2D))
                {
                    serialized.WriteDouble(((Vector2D)bin[i]).x);
                    serialized.WriteDouble(((Vector2D)bin[i]).y);
                }
                if (bin.GetInnerType() == typeof(Vector3D))
                {
                    serialized.WriteDouble(((Vector3D)bin[i]).x);
                    serialized.WriteDouble(((Vector3D)bin[i]).y);
                    serialized.WriteDouble(((Vector3D)bin[i]).z);
                }
                if (bin.GetInnerType() == typeof(Vector4D))
                {
                    serialized.WriteDouble(((Vector4D)bin[i]).x);
                    serialized.WriteDouble(((Vector4D)bin[i]).y);
                    serialized.WriteDouble(((Vector4D)bin[i]).z);
                    serialized.WriteDouble(((Vector4D)bin[i]).w);
                }
                if (bin.GetInnerType() == typeof(Matrix4x4))
                {
                    for (int j = 0; j < 16; j++)
                    {
                        serialized.WriteDouble(((Matrix4x4)bin[i]).Values[j]);
                    }
                }
                if (bin.GetInnerType() == typeof(Stream))
                {
                    var l = ((Stream)bin[i]).Length;
                    serialized.WriteUint((uint)l);
                    if (l > 0)
                    {
                        ((Stream) bin[i]).Position = 0;
                        ((Stream) bin[i]).CopyTo(serialized, (int) l);
                    }
                }
                if (bin.GetInnerType() == typeof(Time.Time))
                {
                    var t = (Time.Time) bin[i];
                    var time = t.UniversalTime.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                    var zone = t.TimeZone.Id;

                    serialized.WriteUint(time.UnicodeLength());
                    serialized.WriteUnicode(time);

                    serialized.WriteUint(zone.UnicodeLength());
                    serialized.WriteUnicode(zone);
                }

            }
            serialized.Position = 0;
            return serialized;
        }

        public static Bin DeSerializeBin(this Stream input)
        {
            uint objCount = input.ReadUint();

            uint aliasLength = input.ReadUint();
            var alias = input.ReadUnicode((int) aliasLength);
            var type = TypeIdentity.Instance.FindType(alias);
            var bin = Bin.New(type);


            for (int i = 0; i < objCount; i++)
            {
                if (type == typeof(bool)) bin.Add(input.ReadBool());
                if (type == typeof(int)) bin.Add(input.ReadInt());
                if (type == typeof(float)) bin.Add(input.ReadFloat());
                if (type == typeof(double)) bin.Add(input.ReadDouble());
                if (type == typeof(string))
                {
                    uint l = input.ReadUint();
                    if (l > 0) 
                        bin.Add(input.ReadUnicode((int)l));
                        else bin.Add("");
                }

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
                    uint l = input.ReadUint();
                    if (l > 0)
                    {
                        input.CopyTo(res, (int) l);
                        res.Position = 0;
                    }
                    bin.Add(res);
                }
                if (type == typeof(Time.Time))
                {
                    uint l = input.ReadUint();
                    var t =input.ReadUnicode((int)l);
                    var utcTime = Time.Time.StringAsTime("UTC", t, "yyyy-MM-dd HH:mm:ss.ffff");

                    l = input.ReadUint();
                    var z = input.ReadUnicode((int)l);

                    var timestamp = Time.Time.ChangeTimezone(utcTime, z);
                    bin.Add(timestamp);
                }
            }
            return bin;
        }

    }
}
