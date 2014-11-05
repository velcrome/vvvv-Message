using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;
using VVVV.Utils.VColor;


namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class ColorBinTest
    {

        [TestMethod]
        public void ColorBinToString()
        {
            var bin = Bin.New(typeof(RGBAColor));
            bin.Add(new RGBAColor(0, 1, 0, 1)); // green
            bin.Add(new RGBAColor(1, 0, 0, 1)); // red

            Assert.AreEqual("ff00ff00", bin.First.ToString());
            Assert.AreEqual("ffff0000", bin[1].ToString());

            Assert.AreEqual("Bin<Color> [ff00ff00, ffff0000]", bin.ToString());
        }

        [TestMethod]
        public void ColorBinToJson()
        {
            var bin = Bin.New(typeof(RGBAColor));
            bin.Add(new RGBAColor(0, 1, 0, 1)); // green
            bin.Add(new RGBAColor(1, 0, 0, 1)); // red

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"Color\":[{\"R\":0.0,\"G\":1.0,\"B\":0.0,\"A\":1.0},{\"R\":1.0,\"G\":0.0,\"B\":0.0,\"A\":1.0}]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.AreEqual(typeof(Bin<RGBAColor>), newBin.GetType());
            Assert.AreEqual("Bin<Color> [ff00ff00, ffff0000]", newBin.ToString());
        }

        [TestMethod]
        public void ColorBinToStream()
        {
            var bin = Bin.New(typeof(RGBAColor));
            bin.Add(new RGBAColor(0, 1, 0, 1)); // green
            bin.Add(new RGBAColor(1, 0, 0, 1)); // red

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.AreEqual(typeof(Bin<RGBAColor>), newBin.GetType());
            Assert.AreEqual("Bin<Color> [ff00ff00, ffff0000]", newBin.ToString());
        }



    }
}
