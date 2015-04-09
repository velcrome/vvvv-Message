using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

using VVVV.Packs.Messaging.Serializing;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class BoolBinTest
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
            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };

            var bin = BinFactory.New(typeof(bool));
            bin.Add(true);

            string json = JsonConvert.SerializeObject(bin, settings);
            Assert.AreEqual("true", json);

            var newBin = JsonConvert.DeserializeObject(json, typeof(Bin<bool>)) as Bin;

            Assert.IsInstanceOfType(newBin, typeof(Bin<bool>));
            Assert.AreEqual("Bin<bool> [True]", newBin.ToString());


            bin.Add(false);
            json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("[true,false]", json);

            newBin = JsonConvert.DeserializeObject(json, typeof(Bin<bool>)) as Bin;
            Assert.IsInstanceOfType(newBin, typeof(Bin<bool>));
            Assert.AreEqual("Bin<bool> [True, False]", newBin.ToString());
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

    }
}
