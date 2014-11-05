using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;

namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class FloatBinTest
    {

        [TestMethod]
        public void FloatBinToString()
        {
            var bin = Bin.New(typeof(float));
            bin.Add(1337);
            bin.Add(Math.PI);

            Assert.AreEqual((float)1337, bin.First);
            Assert.AreEqual((float)Math.PI, bin[1]);

            Assert.AreEqual("Bin<float> [1337, 3,141593]", bin.ToString());
        }

        [TestMethod]
        public void FloatBinToJson()
        {
            var bin = Bin.New(typeof(float));
            bin.Add(1337);
            bin.Add(Math.PI);

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"float\":[1337.0,3.14159274]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.AreEqual(typeof(Bin<float>), newBin.GetType());
            Assert.AreEqual("Bin<float> [1337, 3,141593]", newBin.ToString());
        }


        [TestMethod]
        public void FloatBinToStream()
        {
            var bin = Bin.New(typeof(float));
            bin.Add(1337);
            bin.Add(Math.PI);

            var stream = bin.Serialize();
            var newBin = (Bin)stream.DeSerializeBin();

            Assert.AreEqual(typeof(Bin<float>), newBin.GetType());
            Assert.AreEqual("Bin<float> [1337, 3,141593]", newBin.ToString());
        }

    }
}
