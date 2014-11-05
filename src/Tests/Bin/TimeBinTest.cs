using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;
using VVVV.Utils.VMath;

namespace VVVV.Packs.Message.Tests
{

    
    [TestClass]
    public class TimeBinTest
    {

        [TestMethod]
        public void TimeBinToString()
        {
            var current = Time.Time.CurrentTime();
            var bin = new Bin<Time.Time>(current);

            Assert.AreEqual(current, bin.First);

            Assert.AreEqual("Bin<Time> ["+ current.ToString() +"]", bin.ToString());
        }

        [TestMethod]
        public void TimeBinToJson()
        {
            var current = Time.Time.CurrentTime();
            var bin = new Bin<Time.Time>(current);

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);


            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.IsInstanceOfType(newBin, typeof(Bin<Time.Time>));
            Assert.AreEqual(current.ToString(), newBin[0].ToString());

            
        }

        [TestMethod]
        public void TimeBinToStream()
        {
            var current = Time.Time.CurrentTime();
            var bin = new Bin<Time.Time>(current);

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.IsInstanceOfType(newBin, typeof(Bin<Time.Time>));
            Assert.AreEqual(current.ToString(), newBin[0].ToString());


        }


    }
}
