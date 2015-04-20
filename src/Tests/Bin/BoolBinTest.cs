using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Messaging.Serializing;
using System.IO;
using ProtoBuf;
using System.Xml;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class BoolBinTest : TBinTest<bool>
    {

        [TestMethod]
        public void BoolBinToString()
        {

            var bin = BinFactory.New(typeof(bool));
            bin.Add(true);
            bin.Add(false);

            Assert.AreEqual(true, bin.First);
            Assert.AreEqual(false, bin[1]);

            Assert.AreEqual("Bin<bool> [True, False]", bin.ToString());
        }

   
        [TestMethod]
        public void BoolBinToJson()
        {
            TBinToJson(true, false, "true", "false");
        }
            

        [TestMethod]
        public void BoolBinToStream()
        {
            var bin = BinFactory.New(typeof(bool));
            bin.Add(true);
            bin.Add(false);

            var stream = bin.Serialize();
            var newBin = (Bin)stream.DeSerializeBin();

            Assert.IsInstanceOfType(newBin, typeof(Bin<bool>));
            Assert.AreEqual("Bin<bool> [True, False]", newBin.ToString());
        }


        [TestMethod]
        public void BoolBinToProtobuf()
        {
            var bin = BinFactory.New(typeof(bool));
            bin.Add(true);
            bin.Add(false);

            var stream = new MemoryStream();
//            Serializer.Serialize<Bin>(stream, bin);

            var writer = new ProtoWriter(stream, null, null);
            ProtoWriter.WriteFieldHeader(0, WireType.String, writer);
            ProtoWriter.WriteString("test", writer);
            stream.Flush();

            var r = new StreamReader(stream);
            var s = r.ReadToEnd();







            var newBin = ProtoBuf.Serializer.Deserialize<Bin>(stream);

            Assert.IsInstanceOfType(newBin, typeof(Bin<bool>));
            Assert.AreEqual("Bin<bool> [True, False]", newBin.ToString());
        }

    }
}
