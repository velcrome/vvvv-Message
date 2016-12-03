using Microsoft.VisualStudio.TestTools.UnitTesting;
using VVVV.Packs.Messaging;
using MsgPack.Serialization;
using VVVV.Packs.Messaging.Serializing;
using System.IO;
using System.Linq;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using System.Text;
using VVVV.Packs.Messaging.Nodes;

namespace VVVV.Packs.Messaging.Tests
{
    using Time = VVVV.Packs.Time.Time;

    [TestClass]
    public class MsgPackTest
    {

        #region basics

        VVVVProfile profile = new VVVVProfile();

        private SerializationContext Setup()
        {

            var context = new SerializationContext();
            context.CompatibilityOptions.PackerCompatibilityOptions = MsgPack.PackerCompatibilityOptions.PackBinaryAsRaw;
            //        context.DefaultDateTimeConversionMethod = DateTimeConversionMethod.Native;



            context.Serializers.RegisterOverride(new MsgPackMessageSerializer(context));

            return context;
        }



        [TestMethod]
        public void SerializePrimitives()
        {
            var context = Setup();

            var orig = new Message("Test");

            //            orig.Init("time", Time.CurrentTime());


            orig.Init("b", true, false, false);
            orig.Init("i", 1, 2, 3);
            orig.Init("f", 1.0f, 2.0f, 3.0f);
            orig.Init("d", 1.0d, 2.0d, 3.0d);
            orig.Init("s", "A", "ョ", "asdfa");



            orig.Init("bs", true);
            orig.Init("is", 1);
            orig.Init("fs", 1.0f);
            orig.Init("ds", 1.0d);
            orig.Init("ss", "ョ");


            var serializer = MessagePackSerializer.Get<Message>(context);

            var stream = new MemoryStream();
            serializer.Pack(stream, orig);

            stream.Position = 0;
            var copy = serializer.Unpack(stream);

            Assert.AreEqual(orig, copy);
        }


        [TestMethod]
        public void SerializeNestedMessages()
        {
            var context = Setup();

            var orig = new Message("Test");

            var m1 = new Message("Inner");
            m1.TimeStamp = Time.CurrentTime();
            m1.Init("Foo", "Bar");

            orig.Init("m1", m1);
            orig.Init("m", new Message("Inner"), new Message("OtherInner"));

            orig.Init("test", 1, 2, 3);


            var serializer = MessagePackSerializer.Get<Message>(context);
            var stream = new MemoryStream();
            serializer.Pack(stream, orig);
            stream.Position = 0;
            var copy = serializer.Unpack(stream);

            Assert.AreEqual(orig, copy);
        }

        [TestMethod]
        public void SerializeExtended()
        {
            var context = Setup();

            var orig = new Message("Test");

            orig.Init("v2", new Vector2D(1, 2), new Vector2D(2, 4));
            orig.Init("v3", new Vector3D(1, 2, 3), new Vector3D(2, 4, 8));
            orig.Init("v4", new Vector4D(1, 2, 3, 4), new Vector4D(2, 4, 8, 16));

            orig.Init("c", VColor.Red, VColor.Blue, VColor.Green);
            orig.Init("t", VMath.IdentityMatrix, new Matrix4x4(), VMath.IdentityMatrix);

            orig.Init("v2s", new Vector2D(1, 2));
            orig.Init("v3s", new Vector3D(1, 2, 3));
            orig.Init("v4s", new Vector4D(1, 2, 3, 4));

            orig.Init("c1", VColor.Red);
            orig.Init("t1", VMath.IdentityMatrix);
            var serializer = MessagePackSerializer.Get<Message>(context);

            var stream = new MemoryStream();
            serializer.Pack(stream, orig);

            stream.Position = 0;
            var copy = serializer.Unpack(stream);

            Assert.AreEqual(orig, copy);
        }

        [TestMethod]
        public void SerializeStreams()
        {
            var init = TypeIdentity.Instance;

            var context = Setup();
            var orig = new Message("Test");

            var ms = new MemoryStream(Encoding.ASCII.GetBytes("vvvv"));

            orig.Init("r", ms, new MemoryStream(), ms);
            orig.Init("r1", new MemoryStream());

            var serializer = MessagePackSerializer.Get<Message>(context);

            var stream = new MemoryStream();
            serializer.Pack(stream, orig);

            stream.Position = 0;
            var copy = serializer.Unpack(stream);


            foreach (var fieldName in orig.Fields)
            {
                var origBin = orig[fieldName] as Bin<Stream>;
                var copyBin = copy[fieldName] as Bin<Stream>;

                for (int i = 0; i < VMath.Max(origBin.Count, copyBin.Count); i++)
                {
                    byte[] origRaw = new byte[origBin[i].Length];
                    origBin[i].Read(origRaw, 0, origRaw.Length);

                    byte[] copyRaw = new byte[copyBin[i].Length];
                    copyBin[i].Read(copyRaw, 0, copyRaw.Length);

                    Assert.AreEqual(Encoding.ASCII.GetString(origRaw), Encoding.ASCII.GetString(copyRaw));
                }
            }


            // Assert.AreEqual(orig, copy);  //fails because of Stream not being deep-checkable

        }
        #endregion
    }
}