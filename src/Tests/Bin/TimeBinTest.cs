using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Messaging.Serializing;
using VVVV.Packs.Time;

namespace VVVV.Packs.Messaging.Tests
{
    using Time = VVVV.Packs.Time.Time;
    
    [TestClass]
    public class TimeBinTest : TBinTest<Time>
    {

        [TestMethod]
        public void TimeBinToString()
        {
            var current = Time.CurrentTime();
            var bin = BinFactory.New(current);

            Assert.AreEqual(current, bin.First);

            Assert.AreEqual("Bin<Time> ["+ current.ToString() +"]", bin.ToString());
        }

        [TestMethod]
        public void TimeBinToJson()
        {
            var min = Time.MinUTCTime();
            var current = Time.CurrentTime();
            var currentAsString = "{\"UTC\":\"" + current.UniversalTime.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "\",\"ZoneId\":\"" + current.TimeZone.Id + "\"}"; 
            TBinToJson(min, current, "{\"UTC\":\"0001-01-01 00:00:00.0000\",\"ZoneId\":\"UTC\"}", currentAsString);
        }            

        [TestMethod]
        public void TimeBinToStream()
        {
            var current = Time.CurrentTime();
            var bin = BinFactory.New(current);

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.IsInstanceOfType(newBin, typeof(Bin<Time>));
            Assert.AreEqual(current.ToString(), newBin[0].ToString());


        }


    }
}
