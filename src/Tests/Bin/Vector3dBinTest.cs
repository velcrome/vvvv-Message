using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;
using VVVV.Utils.VMath;


namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class Vector3dBinTest
    {

        [TestMethod]
        public void V3BinToString()
        {
            var bin = Bin.New(typeof(Vector3D));
            bin.Add(new Vector3D());
            bin.Add(new Vector3D(1, 2, 3));

            Assert.AreEqual(new Vector3D(), bin.First);
            Assert.AreEqual(new Vector3D(1, 2, 3), bin[1]);

            Assert.Inconclusive("Bin<Vector3d> [VVVV.Utils.VMath.Vector3D, VVVV.Utils.VMath.Vector3D]", bin.ToString());
        }

        [TestMethod]
        public void V3BinToJson()
        {
            var bin = Bin.New(typeof(Vector3D));
            bin.Add(new Vector3D());
            bin.Add(new Vector3D(1, 2, 3));

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"Vector3d\":[{\"x\":0.0,\"y\":0.0,\"z\":0.0},{\"x\":1.0,\"y\":2.0,\"z\":3.0}]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.AreEqual(typeof(Bin<Vector3D>), newBin.GetType());

            Assert.AreEqual(new Vector3D(), newBin.First);
            Assert.AreEqual(new Vector3D(1, 2, 3), newBin[1]);
        }

        [TestMethod]
        public void V3BinToStream()
        {
            var bin = Bin.New(typeof(Vector3D));
            bin.Add(new Vector3D());
            bin.Add(new Vector3D(1, 2, 3));


            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.AreEqual(typeof(Bin<Vector3D>), newBin.GetType());

            Assert.AreEqual(new Vector3D(), newBin.First);
            Assert.AreEqual(new Vector3D(1, 2, 3), newBin[1]);
        }
    }
}
