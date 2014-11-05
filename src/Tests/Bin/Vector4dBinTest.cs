using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;
using VVVV.Utils.VMath;


namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class Vector4dBinTest
    {

        [TestMethod]
        public void V4BinToString()
        {
            var bin = Bin.New(typeof(Vector4D));
            bin.Add(new Vector4D());
            bin.Add(new Vector4D(1, 2, 3, 4));

            Assert.AreEqual(new Vector4D(), bin.First);
            Assert.AreEqual(new Vector4D(1, 2, 3, 4), bin[1]);

            Assert.Inconclusive("Bin<Vector4d> [VVVV.Utils.VMath.Vector4D, VVVV.Utils.VMath.Vector4D]", bin.ToString());
        }

        [TestMethod]
        public void V4BinToJson()
        {
            var bin = Bin.New(typeof(Vector4D));
            bin.Add(new Vector4D());
            bin.Add(new Vector4D(1, 2, 3, 4));

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"Vector4d\":[{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":0.0},{\"x\":1.0,\"y\":2.0,\"z\":3.0,\"w\":4.0}]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.AreEqual(typeof(Bin<Vector4D>), newBin.GetType());

            Assert.AreEqual(new Vector4D(), newBin.First);
            Assert.AreEqual(new Vector4D(1, 2, 3, 4), newBin[1]);
        }

        [TestMethod]
        public void V4BinToStream()
        {
            var bin = Bin.New(typeof(Vector4D));
            bin.Add(new Vector4D());
            bin.Add(new Vector4D(1, 2, 3, 4));

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.AreEqual(typeof(Bin<Vector4D>), newBin.GetType());

            Assert.AreEqual(new Vector4D(), newBin.First);
            Assert.AreEqual(new Vector4D(1, 2, 3, 4), newBin[1]);
        }

    }
}
