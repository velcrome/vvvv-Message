using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;

namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class BinTest
    {

        [TestMethod]
        public void TestToString()
        {
            var bin = Bin.New(typeof(string));
            bin.Add("lorem");
            bin.Add("ipsum");

            Assert.AreEqual("lorem", bin.First);
            Assert.AreEqual("ipsum", bin[1]);

            Assert.AreEqual("Bin<string> [lorem, ipsum]", bin.ToString());
        }

        [TestMethod]
        public void TestToJson()
        {
            var bin = Bin.New(typeof(string));
            bin.Add("lorem");
            bin.Add("ipsum");
            
            var settings = new JsonSerializerSettings
                               {Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None};


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"Type\":\"string\",\"Bin\":[\"lorem\",\"ipsum\"]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof (Bin));

            Assert.AreEqual(typeof(Bin<string>), newBin.GetType());
            Assert.AreEqual("Bin<string> [lorem, ipsum]", newBin.ToString());
        }

    }
}
