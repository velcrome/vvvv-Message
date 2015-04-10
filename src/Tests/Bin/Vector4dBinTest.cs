using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Serializing;
using VVVV.Utils.VMath;


namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class Vector4dBinTest : TBinTest<Vector4D>
    {

        [TestMethod]
        public void V4BinToString()
        {
            var bin = BinFactory.New(typeof(Vector4D));
            bin.Add(new Vector4D());
            bin.Add(new Vector4D(1, 2, 3, 4));

            Assert.AreEqual(new Vector4D(), bin.First);
            Assert.AreEqual(new Vector4D(1, 2, 3, 4), bin[1]);

            Assert.Inconclusive("Bin<Vector4d> [VVVV.Utils.VMath.Vector4D, VVVV.Utils.VMath.Vector4D]", bin.ToString());
        }

        [TestMethod]
        public void V4BinToJson()
        {
            TBinToJson(new Vector4D(), new Vector4D(1, 2, 3, 4), "{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":0.0}", "{\"x\":1.0,\"y\":2.0,\"z\":3.0,\"w\":4.0}");
        }     

        [TestMethod]
        public void V4BinToStream()
        {
            var bin = BinFactory.New(typeof(Vector4D));
            bin.Add(new Vector4D());
            bin.Add(new Vector4D(1, 2, 3, 4));

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.IsInstanceOfType(newBin, typeof(Bin<Vector4D>));

            Assert.AreEqual(new Vector4D(), newBin.First);
            Assert.AreEqual(new Vector4D(1, 2, 3, 4), newBin[1]);
        }

    }
}
