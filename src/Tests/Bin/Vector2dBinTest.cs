using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Serializing;
using VVVV.Utils.VMath;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class Vector2dBinTest : TBinTest<Vector2D>
    {

        [TestMethod]
        public void V2BinToString()
        {
            var bin = BinFactory.New(typeof(Vector2D));
            bin.Add(new Vector2D()); 
            bin.Add(new Vector2D(1, 2));

            Assert.AreEqual(new Vector2D(), bin.First);
            Assert.AreEqual(new Vector2D(1, 2), bin[1]);

            Assert.Inconclusive("Bin<Vector2d> [VVVV.Utils.VMath.Vector2D, VVVV.Utils.VMath.Vector2D]", bin.ToString());
        }

        [TestMethod]
        public void V2BinToJson()
        {
            TBinToJson(new Vector2D(), new Vector2D(1, 2), "{\"x\":0.0,\"y\":0.0}", "{\"x\":1.0,\"y\":2.0}");
        }

        [TestMethod]
        public void V2BinStream()
        {
            var bin = BinFactory.New(typeof(Vector2D));
            bin.Add(new Vector2D());
            bin.Add(new Vector2D(1, 2));

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.IsInstanceOfType(newBin, typeof(Bin<Vector2D>));

            Assert.AreEqual(new Vector2D(), newBin.First);
            Assert.AreEqual(new Vector2D(1, 2), newBin[1]);
        }

    }
}
