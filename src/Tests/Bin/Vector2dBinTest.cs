using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;
using VVVV.Utils.VMath;


namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class Vector2dBinTest
    {

        [TestMethod]
        public void V2BinToString()
        {
            var bin = Bin.New(typeof(Vector2D));
            bin.Add(new Vector2D()); 
            bin.Add(new Vector2D(1, 2));

            Assert.AreEqual(new Vector2D(), bin.First);
            Assert.AreEqual(new Vector2D(1, 2), bin[1]);

            Assert.Inconclusive("Bin<Vector2d> [VVVV.Utils.VMath.Vector2D, VVVV.Utils.VMath.Vector2D]", bin.ToString());
        }

        [TestMethod]
        public void V2BinToJson()
        {
            var bin = Bin.New(typeof(Vector2D));
            bin.Add(new Vector2D());
            bin.Add(new Vector2D(1, 2));

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"Vector2d\":[{\"x\":0.0,\"y\":0.0},{\"x\":1.0,\"y\":2.0}]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.AreEqual(typeof(Bin<Vector2D>), newBin.GetType());

            Assert.AreEqual(new Vector2D(), newBin.First);
            Assert.AreEqual(new Vector2D(1, 2), newBin[1]);
        }

        [TestMethod]
        public void V2BinStream()
        {
            var bin = Bin.New(typeof(Vector2D));
            bin.Add(new Vector2D());
            bin.Add(new Vector2D(1, 2));

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.AreEqual(typeof(Bin<Vector2D>), newBin.GetType());

            Assert.AreEqual(new Vector2D(), newBin.First);
            Assert.AreEqual(new Vector2D(1, 2), newBin[1]);
        }

    }
}
