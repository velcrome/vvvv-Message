using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;

namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class BoolBinTest
    {

        [TestMethod]
        public void BoolBinToString()
        {
            var bin = Bin.New(typeof(bool));
            bin.Add(true);
            bin.Add(false);

            Assert.AreEqual(true, bin.First);
            Assert.AreEqual(false, bin[1]);

            Assert.AreEqual("Bin<bool> [True, False]", bin.ToString());
        }

        [TestMethod]
        public void BoolBinToJson()
        {
            var bin = Bin.New(typeof(bool));
            bin.Add(true);
            bin.Add(false);

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"bool\":[true,false]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.AreEqual(typeof(Bin<bool>), newBin.GetType());
            Assert.AreEqual("Bin<bool> [True, False]", newBin.ToString());
        }

        [TestMethod]
        public void BoolBinToStream()
        {
            var bin = Bin.New(typeof(bool));
            bin.Add(true);
            bin.Add(false);

            var stream = bin.Serialize();
            var newBin = (Bin)stream.DeSerializeBin();

            Assert.AreEqual(typeof(Bin<bool>), newBin.GetType());
            Assert.AreEqual("Bin<bool> [True, False]", newBin.ToString());
        }

    }
}
