using System;
using System.IO;
using VVVV.Packs.VObjects;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Packs.Message.Core.Serializing
{
    public static class BinaryExtensions
    {

        public static void Serialize(this SpreadList list)
        {
            Stream serialized = new MemoryStream();

            serialized.SetLength(0);
            serialized.WriteUint((uint)list.Count);
            serialized.WriteUint(list.GetType().ToString().UnicodeLength());
            serialized.WriteUnicode(list.GetType().ToString());

            for (int i = 0; i < list.Count; i++)
            {
                uint l = 0;
                if (list.GetType() == typeof(bool))
                {
                    l = 1;
                    serialized.WriteUint(l);
                    serialized.WriteBool((bool)list[i]);
                }
                if (list.GetType() == typeof(int))
                {
                    l = 4;
                    serialized.WriteUint(l);
                    serialized.WriteInt((int)list[i]);
                }
                if (list.GetType() == typeof(float))
                {
                    l = 4;
                    serialized.WriteUint(l);
                    serialized.WriteFloat((float)list[i]);
                }
                if (list.GetType() == typeof(double))
                {
                    l = 8;
                    serialized.WriteUint(l);
                    serialized.WriteDouble((double)list[i]);
                }
                if (list.GetType() == typeof(string))
                {
                    l = ((string)list[i]).UnicodeLength();
                    serialized.WriteUint(l);
                    serialized.WriteUnicode((string)list[i]);
                }

                if (list.GetType() == typeof(RGBAColor))
                {
                    l = 32;
                    serialized.WriteUint(l);
                    serialized.WriteDouble(((RGBAColor)list[i]).R);
                    serialized.WriteDouble(((RGBAColor)list[i]).G);
                    serialized.WriteDouble(((RGBAColor)list[i]).B);
                    serialized.WriteDouble(((RGBAColor)list[i]).A);
                }
                if (list.GetType() == typeof(Vector2D))
                {
                    l = 16;
                    serialized.WriteUint(l);
                    serialized.WriteDouble(((Vector2D)list[i]).x);
                    serialized.WriteDouble(((Vector2D)list[i]).y);
                }
                if (list.GetType() == typeof(Vector3D))
                {
                    l = 24;
                    serialized.WriteUint(l);
                    serialized.WriteDouble(((Vector3D)list[i]).x);
                    serialized.WriteDouble(((Vector3D)list[i]).y);
                    serialized.WriteDouble(((Vector3D)list[i]).z);
                }
                if (list.GetType() == typeof(Vector4D))
                {
                    l = 32;
                    serialized.WriteUint(l);
                    serialized.WriteDouble(((Vector4D)list[i]).x);
                    serialized.WriteDouble(((Vector4D)list[i]).y);
                    serialized.WriteDouble(((Vector4D)list[i]).z);
                    serialized.WriteDouble(((Vector4D)list[i]).w);
                }
                if (list.GetType() == typeof(Matrix4x4))
                {
                    l = 128;
                    serialized.WriteUint(l);
                    for (int j = 0; j < 16; j++)
                    {
                        serialized.WriteDouble(((Matrix4x4)list[i]).Values[j]);
                    }
                }
                if (list.GetType() == typeof(Stream))
                {
                    l = (uint)((Stream)list[i]).Length;
                    serialized.WriteUint(l);
                    ((Stream)list[i]).Position = 0;
                    ((Stream)list[i]).CopyTo(serialized);
                }
                if (list.GetType() == typeof(Time.Time))
                {
                    // not implemented
                }

            
            }
        }

        public static SpreadList DeSerialize(Stream input)
        {
            input.Position = 0;

            uint objcount = input.ReadUint();
            uint typeL = input.ReadUint();

            var type = Type.GetType(input.ReadUnicode((int)typeL));

            var list = new SpreadList();


            for (int i = 0; i < objcount; i++)
            {
                uint l = input.ReadUint();
                if (type == typeof(bool)) list.Add(input.ReadBool());
                if (type == typeof(int)) list.Add(input.ReadInt());
                if (type == typeof(float)) list.Add(input.ReadFloat());
                if (type == typeof(double)) list.Add(input.ReadDouble());
                if (type == typeof(string)) list.Add(input.ReadUnicode((int)l));

                if (type == typeof(RGBAColor))
                {
                    double[] val = new double[4];
                    for (int j = 0; j < val.Length; j++) val[j] = input.ReadDouble();
                    RGBAColor res = new RGBAColor(val);
                    list.Add(res);
                }
                if (type == typeof(Vector2D))
                {
                    Vector2D res = new Vector2D();
                    res.x = input.ReadDouble();
                    res.y = input.ReadDouble();
                    list.Add(res);
                }
                if (type == typeof(Vector3D))
                {
                    Vector3D res = new Vector3D();
                    res.x = input.ReadDouble();
                    res.y = input.ReadDouble();
                    res.z = input.ReadDouble();
                    list.Add(res);
                }
                if (type == typeof(Vector4D))
                {
                    Vector4D res = new Vector4D();
                    res.x = input.ReadDouble();
                    res.y = input.ReadDouble();
                    res.z = input.ReadDouble();
                    res.w = input.ReadDouble();
                    list.Add(res);
                }
                if (type == typeof(Matrix4x4))
                {
                    Matrix4x4 res = new Matrix4x4();
                    for (int j = 0; j < 16; j++) res.Values[j] = input.ReadDouble();
                    list.Add(res);
                }
                if (type == typeof(Stream))
                {
                    Stream res = new MemoryStream();
                    input.CopyTo(res, (int)l);
                    res.Position = 0;
                    list.Add(res);
                }
            }
            return list;
        }

    }
}
