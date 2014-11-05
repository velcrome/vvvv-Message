using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;

namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class IntBinTest
    {

        [TestMethod]
        public void IntBinToString()
        {
            var bin = Bin.New(typeof(int));
            bin.Add(1337);
            bin.Add(int.MaxValue);

            Assert.AreEqual(1337, bin.First);
            Assert.AreEqual(int.MaxValue, bin[1]);

            Assert.AreEqual("Bin<int> [1337, 2147483647]", bin.ToString());
        }

        [TestMethod]
        public void IntBinToJson()
        {
            var bin = Bin.New(typeof(int));
            bin.Add(1337);
            bin.Add(int.MaxValue);

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"int\":[1337,2147483647]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.AreEqual(typeof(Bin<int>), newBin.GetType());
            Assert.AreEqual("Bin<int> [1337, 2147483647]", newBin.ToString());
        }

        [TestMethod]
        public void IntBinToStream()
        {
            var bin = Bin.New(typeof (int));
            bin.Add(1337);
            bin.Add(int.MaxValue);

            var stream = bin.Serialize();
            var newBin = (Bin) stream.DeSerializeBin();

            Assert.AreEqual(typeof (Bin<int>), newBin.GetType());
            Assert.AreEqual("Bin<int> [1337, 2147483647]", newBin.ToString());
        }


    }
}
