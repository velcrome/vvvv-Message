using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Serializing;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class IntBinTest : TBinTest<int>
    {

        [TestMethod]
        public void IntBinToString()
        {
            var bin = BinFactory.New(typeof(int));
            bin.Add(1337);
            bin.Add(int.MaxValue);

            Assert.AreEqual(1337, bin.First);
            Assert.AreEqual(int.MaxValue, bin[1]);

            Assert.AreEqual("Bin<int> [1337, 2147483647]", bin.ToString());
        }

        [TestMethod]
        public void IntBinToJson()
        {
            TBinToJson(1337, int.MaxValue, "1337", "2147483647");
        }
            

        [TestMethod]
        public void IntBinToStream()
        {
            var bin = BinFactory.New(typeof(int));
            bin.Add(1337);
            bin.Add(int.MaxValue);

            var stream = bin.Serialize();
            var newBin = (Bin) stream.DeSerializeBin();

            Assert.IsInstanceOfType(newBin, typeof(Bin<int>));
            Assert.AreEqual("Bin<int> [1337, 2147483647]", newBin.ToString());
        }


    }
}
