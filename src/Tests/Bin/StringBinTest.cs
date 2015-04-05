using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Serializing;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class StringBinTest
    {

        [TestMethod]
        public void StringBinToString()
        {
            var bin = BinFactory.New(typeof(string));
            bin.Add("lorem");
            bin.Add("");
            bin.Add("ipsum");

            Assert.AreEqual("lorem", bin.First);
            Assert.AreEqual("ipsum", bin[2]);

            Assert.AreEqual("Bin<string> [lorem, , ipsum]", bin.ToString());
        }

        [TestMethod]
        public void StringBinToJson()
        {
            var bin = BinFactory.New(typeof(string));
            bin.Add("lorem");
            bin.Add("");
            bin.Add("ipsum");
            
            var settings = new JsonSerializerSettings
                               {Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None};


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"string\":[\"lorem\",\"\",\"ipsum\"]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof (Bin));

            Assert.IsInstanceOfType(newBin, typeof(Bin<string>));
            Assert.AreEqual("Bin<string> [lorem, , ipsum]", newBin.ToString());
        }

        [TestMethod]
        public void StringBinToStream()
        {
            var bin = BinFactory.New(typeof(string));
//            field.Init("lorem");
            bin.Add("");
//            field.Init("ipsum");

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.IsInstanceOfType(newBin, typeof(Bin<string>) );
            Assert.AreEqual("", newBin[0]);
            
            
            //            Assert.AreEqual("Bin<string> [lorem, , ipsum]", newBin.ToString());
        }

    }
}
