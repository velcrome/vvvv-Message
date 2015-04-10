using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Serializing;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class FloatBinTest : TBinTest<float>
    {
        [TestMethod]
        public void FloatBinToJson()
        {
            TBinToJson(-23.0f, float.MaxValue, "-23.0", "3.40282347E+38");
        }


        [TestMethod]
        public void FloatBinToString()
        {
            var bin = BinFactory.New(typeof(float));
            bin.Add(1337);
            bin.Add(Math.PI);

            Assert.AreEqual((float)1337, bin.First);
            Assert.AreEqual((float)Math.PI, bin[1]);

            Assert.AreEqual("Bin<float> [1337, 3,141593]", bin.ToString());
        }

        [TestMethod]
        public void FloatBinToStream()
        {
            var bin = BinFactory.New(typeof(float));
            bin.Add(1337);
            bin.Add(Math.PI);

            var stream = bin.Serialize();
            var newBin = (Bin)stream.DeSerializeBin();

            Assert.IsInstanceOfType(newBin, typeof(Bin<float>));
            Assert.AreEqual("Bin<float> [1337, 3,141593]", newBin.ToString());
        }

    }
}
