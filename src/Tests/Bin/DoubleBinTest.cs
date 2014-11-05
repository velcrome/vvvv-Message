using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;

namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class DoubleBinTest
    {

        [TestMethod]
        public void DoubleBinToString()
        {
            var bin = Bin.New(typeof(double));
            bin.Add(1337E+0);
            bin.Add(Math.PI);

            Assert.AreEqual((double)1337, bin.First);
            Assert.AreEqual(Math.PI, bin[1]);

            Assert.AreEqual("Bin<double> [1337, 3,14159265358979]", bin.ToString());
        }

        [TestMethod]
        public void DoubleBinToJson()
        {
            var bin = Bin.New(typeof(double));
            bin.Add((double)1337);
            bin.Add(Math.PI);

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"double\":[1337.0,3.1415926535897931]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.AreEqual(typeof(Bin<double>), newBin.GetType());
            Assert.AreEqual("Bin<double> [1337, 3,14159265358979]", newBin.ToString());
        }

        [TestMethod]
        public void DoubleBinToStream()
        {
            var bin = Bin.New(typeof(double));
            bin.Add((double)1337);
            bin.Add(Math.PI);

            var stream = bin.Serialize();
            var newBin = (Bin)stream.DeSerializeBin();

            Assert.AreEqual(typeof(Bin<double>), newBin.GetType());
            Assert.AreEqual("Bin<double> [1337, 3,14159265358979]", newBin.ToString());
        }

    }
}
