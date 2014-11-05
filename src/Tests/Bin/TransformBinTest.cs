using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;
using VVVV.Utils.VMath;


namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class TransformBinTest
    {

        [TestMethod]
        public void TransformBinToString()
        {
            var bin = Bin.New(typeof(Matrix4x4));
            bin.Add(new Matrix4x4());
            bin.Add(new Matrix4x4(1,1,1,1, 2,2,2,2, 3,4,5,6, 1,1,1,1)); 

            Assert.AreEqual(new Matrix4x4(), bin.First);
            Assert.AreEqual(new Matrix4x4(1, 1, 1, 1, 2, 2, 2, 2, 3, 4, 5, 6, 1, 1, 1, 1), bin[1]);

            Assert.AreEqual("Bin<Transform> [\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000, \n1,0000 1,0000 1,0000 1,0000\n2,0000 2,0000 2,0000 2,0000\n3,0000 4,0000 5,0000 6,0000\n1,0000 1,0000 1,0000 1,0000]", bin.ToString());
        }

        [TestMethod]
        public void TransformBinToJson()
        {
            var bin = Bin.New(typeof(Matrix4x4));
            bin.Add(new Matrix4x4());
            bin.Add(new Matrix4x4(1, 1, 1, 1, 2, 2, 2, 2, 3, 4, 5, 6, 1, 1, 1, 1));

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"Transform\":[{\"Values\":[0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0]},{\"Values\":[1.0,1.0,1.0,1.0,2.0,2.0,2.0,2.0,3.0,4.0,5.0,6.0,1.0,1.0,1.0,1.0]}]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.AreEqual(typeof(Bin<Matrix4x4>), newBin.GetType());
            Assert.AreEqual("Bin<Transform> [\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000, \n1,0000 1,0000 1,0000 1,0000\n2,0000 2,0000 2,0000 2,0000\n3,0000 4,0000 5,0000 6,0000\n1,0000 1,0000 1,0000 1,0000]", newBin.ToString());
        }

        public void TransformBinToStream()
        {
            var bin = Bin.New(typeof(Matrix4x4));
            bin.Add(new Matrix4x4());
            bin.Add(new Matrix4x4(1, 1, 1, 1, 2, 2, 2, 2, 3, 4, 5, 6, 1, 1, 1, 1));

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.AreEqual(typeof(Bin<Matrix4x4>), newBin.GetType());
            Assert.AreEqual("Bin<Transform> [\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000, \n1,0000 1,0000 1,0000 1,0000\n2,0000 2,0000 2,0000 2,0000\n3,0000 4,0000 5,0000 6,0000\n1,0000 1,0000 1,0000 1,0000]", newBin.ToString());
        }

    }
}
