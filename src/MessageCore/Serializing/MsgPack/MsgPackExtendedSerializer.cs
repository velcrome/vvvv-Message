using MsgPack;
using MsgPack.Serialization;

namespace VVVV.Packs.Messaging.Serializing
{
    using System;
    using System.IO;
    using Utils.VColor;
    using Utils.VMath;
    using Time = VVVV.Packs.Time.Time;

    public class MsgPackTimeSerializer : MessagePackSerializer<Time>
    {
        string Encoding = "yyyy-MM-dd HH:mm:ss.ffff";

        public MsgPackTimeSerializer(SerializationContext ownerContext) : base(ownerContext)
        {
            OwnerContext.ExtTypeCodeMapping.Add("Time", 0x10);
            OwnerContext.ExtTypeCodeMapping.Add("Vector2d", 0x11);
            OwnerContext.ExtTypeCodeMapping.Add("Vector3d", 0x12);
            OwnerContext.ExtTypeCodeMapping.Add("Vector4d", 0x13);
            OwnerContext.ExtTypeCodeMapping.Add("Raw", 0x14);
            OwnerContext.ExtTypeCodeMapping.Add("Color", 0x15);
            OwnerContext.ExtTypeCodeMapping.Add("Transform", 0x16);
        }

        protected override void PackToCore(Packer packer, Time time)            
        {

            //            packer.Pack(Time.TimeStamp(time.UniversalTime));
            packer.PackString(time.ZoneTime.ToString(Encoding));
            packer.PackString(time.TimeZone.Id);
        }

        protected override Time UnpackFromCore(Unpacker unpacker)
        {
            //            var timestamp = unpacker.LastReadData.AsDouble();
            var timeString = unpacker.LastReadData.AsString();

            unpacker.Read();
            string zoneId = unpacker.LastReadData.AsString();

            var time = Time.StringAsTime(zoneId, timeString, Encoding);
            
//            var time = Time.ValueAsTime(zoneId, timestamp);
            return time;
        }
    }

    public class MsgPackVector2dSerializer : MessagePackSerializer<Vector2D>
    {
        public MsgPackVector2dSerializer(SerializationContext ownerContext) : base(ownerContext)
        {
        }

        protected override void PackToCore(Packer packer, Vector2D vector)
        {
            packer.Pack(vector.x);
            packer.Pack(vector.y);
        }

        protected override Vector2D UnpackFromCore(Unpacker unpacker)
        {
            var x = unpacker.LastReadData.AsDouble();
            unpacker.Read();
            var y = unpacker.LastReadData.AsDouble();
            return new Vector2D(x, y);
        }
    }

    public class MsgPackVector3dSerializer : MessagePackSerializer<Vector3D>
    {
        public MsgPackVector3dSerializer(SerializationContext ownerContext) : base(ownerContext)
        {
        }

        protected override void PackToCore(Packer packer, Vector3D vector)
        {
            packer.Pack(vector.x);
            packer.Pack(vector.y);
            packer.Pack(vector.z);
        }

        protected override Vector3D UnpackFromCore(Unpacker unpacker)
        {
            var x = unpacker.LastReadData.AsDouble();
            unpacker.Read();
            var y = unpacker.LastReadData.AsDouble();
            unpacker.Read();
            var z = unpacker.LastReadData.AsDouble();
            return new Vector3D(x, y, z);
        }
    }

    public class MsgPackVector4dSerializer : MessagePackSerializer<Vector4D>
    {
        public MsgPackVector4dSerializer(SerializationContext ownerContext) : base(ownerContext)
        {
        }

        protected override void PackToCore(Packer packer, Vector4D vector)
        {
            packer.Pack(vector.x);
            packer.Pack(vector.y);
            packer.Pack(vector.z);
            packer.Pack(vector.w);
        }

        protected override Vector4D UnpackFromCore(Unpacker unpacker)
        {
            var x = unpacker.LastReadData.AsDouble();
            unpacker.Read();
            var y = unpacker.LastReadData.AsDouble();
            unpacker.Read();
            var z = unpacker.LastReadData.AsDouble();
            unpacker.Read();
            var w = unpacker.LastReadData.AsDouble();
            return new Vector4D(x, y, z, w);
        }
    }

    public class MsgPackColorSerializer : MessagePackSerializer<RGBAColor>
    {
        public MsgPackColorSerializer(SerializationContext ownerContext) : base(ownerContext)
        {
        }

        protected override void PackToCore(Packer packer, RGBAColor color)
        {
            packer.Pack(color.A);
            packer.Pack(color.R);
            packer.Pack(color.G);
            packer.Pack(color.B);
        }

        protected override RGBAColor UnpackFromCore(Unpacker unpacker)
        {
            var a = unpacker.LastReadData.AsDouble();
            unpacker.Read();
            var r = unpacker.LastReadData.AsDouble();
            unpacker.Read();
            var g = unpacker.LastReadData.AsDouble();
            unpacker.Read();
            var b = unpacker.LastReadData.AsDouble();
            return new RGBAColor(a, r, g, b);
        }
    }

    public class MsgPackMatrixSerializer : MessagePackSerializer<Matrix4x4>
    {
        public MsgPackMatrixSerializer(SerializationContext ownerContext) : base(ownerContext)
        {
        }

        protected override void PackToCore(Packer packer, Matrix4x4 matrix)
        {
            for (int i = 0; i < 16; i++)
                packer.Pack(matrix.Values[i]);
        }

        protected override Matrix4x4 UnpackFromCore(Unpacker unpacker)
        {
            double[] buffer = new double[16];
            buffer[0] = unpacker.LastReadData.AsDouble();

            for (int i = 1; i < 16; i++)
            {
                unpacker.Read();
                buffer[i] = unpacker.LastReadData.AsDouble();
            }
            var matrix = new Matrix4x4();
            matrix.Values = buffer;
            return matrix;
        }
    }

    public class MsgPackStreamSerializer : MessagePackSerializer<Stream>
    {
        public MsgPackStreamSerializer(SerializationContext ownerContext) : base(ownerContext)
        {
        }

        protected override void PackToCore(Packer packer, Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(buffer, 0, (int)stream.Length);

            packer.PackBinary(buffer);
        }

        protected override Stream UnpackFromCore(Unpacker unpacker)
        {
            byte[] buffer = unpacker.LastReadData.AsBinary();
            var stream = new MemoryStream();
            stream.Write(buffer, 0, buffer.Length);

            return stream;
        }
    }
}
