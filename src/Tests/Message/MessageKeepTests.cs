using Microsoft.VisualStudio.TestTools.UnitTesting;
using VVVV.Packs.Messaging;



namespace VVVV.Packs.Messaging.Tests
{

    using Time = VVVV.Packs.Time.Time;

    [TestClass]
    public class MessageKeepTest
    {


        [TestMethod]
        public void FillKeep()
        {
            var message = fresh();

            message.Init("foo", "bar", "bar2");
            message.Init("num", 1, 2, 3);

            Assert.AreEqual("Message Test (01.01.0001 01:00:00 [UTC])\n foo \t: bar bar2 \r\n num \t: 1 2 3 \r\n", message.ToString());
        }

   

    }
}
