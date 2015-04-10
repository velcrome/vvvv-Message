using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Serializing;
using VVVV.Utils.VMath;


namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class TransformBinTest : TBinTest<Matrix4x4>
    {

        [TestMethod]
        public void TransformBinToString()
        {
            var bin = BinFactory.New(typeof(Matrix4x4)) as Bin<Matrix4x4>;
            bin.Add(new Matrix4x4());
            bin.Add(new Matrix4x4(1,1,1,1, 2,2,2,2, 3,4,5,6, 1,1,1,1)); 

            Assert.AreEqual(new Matrix4x4(), bin.First);
            Assert.AreEqual(new Matrix4x4(1, 1, 1, 1, 2, 2, 2, 2, 3, 4, 5, 6, 1, 1, 1, 1), bin[1]);

            Assert.AreEqual("Bin<Transform> [\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000, \n1,0000 1,0000 1,0000 1,0000\n2,0000 2,0000 2,0000 2,0000\n3,0000 4,0000 5,0000 6,0000\n1,0000 1,0000 1,0000 1,0000]", bin.ToString());
        }

        [TestMethod]
        public void TransformBinToJson()
        {
            var plain = new Matrix4x4();
            var numbered = new Matrix4x4(1, 1, 1, 1, 2, 2, 2, 2, 3, 4, 5, 6, 1, 1, 1, 1);

            TBinToJson(plain, numbered, "{\"Values\":[0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0,0.0]}", "{\"Values\":[1.0,1.0,1.0,1.0,2.0,2.0,2.0,2.0,3.0,4.0,5.0,6.0,1.0,1.0,1.0,1.0]}");
       }

        public void TransformBinToStream()
        {
            var bin = BinFactory.New(typeof(Matrix4x4));
            bin.Add(new Matrix4x4());
            bin.Add(new Matrix4x4(1, 1, 1, 1, 2, 2, 2, 2, 3, 4, 5, 6, 1, 1, 1, 1));

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.AreEqual(typeof(Bin<Matrix4x4>), newBin.GetType());
            Assert.AreEqual("Bin<Transform> [\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000\n0,0000 0,0000 0,0000 0,0000, \n1,0000 1,0000 1,0000 1,0000\n2,0000 2,0000 2,0000 2,0000\n3,0000 4,0000 5,0000 6,0000\n1,0000 1,0000 1,0000 1,0000]", newBin.ToString());
        }

    }
}
