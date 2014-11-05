using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;


namespace VVVV.Packs.Message.Tests
{

    using Message = VVVV.Packs.Message.Core.Message;
    using Time = VVVV.Packs.Time.Time;

    [TestClass]
    public class MessageMethodTest
    {
        private Message fresh()
        {
            var message = new Message();

            message.Address = "Test";
            message.TimeStamp = Time.MinUTCTime();

            return message;
        }


        [TestMethod]
        public void MessageInit()
        {
            var message = fresh();

            message.Init("foo", "bar", "bar2");
            message.Init("num", 1, 2, 3);

            Assert.AreEqual("Message Test (01.01.0001 01:00:00 [UTC;0;UTC;UTC;UTC;;])\n foo \t: bar bar2 \r\n num \t: 1 2 3 \r\n", message.ToString());
        }

        [TestMethod]
        public void MessageAddFrom()
        {
            var message = fresh();

            message.AddFrom("foo", new string[] {"bar"});
            message.AddFrom("num", new int[] { 1, 2, 3 });
            Assert.AreEqual("Message Test (01.01.0001 01:00:00 [UTC;0;UTC;UTC;UTC;;])\n foo \t: bar \r\n num \t: 1 2 3 \r\n", message.ToString());

            message.AddFrom("num", new object[] { 4, 5.2f});
            Assert.AreEqual("Message Test (01.01.0001 01:00:00 [UTC;0;UTC;UTC;UTC;;])\n foo \t: bar \r\n num \t: 1 2 3 4 5 \r\n", message.ToString());

            message.AddFrom("num", new object[] { "6", true });
            Assert.AreEqual("Message Test (01.01.0001 01:00:00 [UTC;0;UTC;UTC;UTC;;])\n foo \t: bar \r\n num \t: 1 2 3 4 5 6 1 \r\n", message.ToString());

        }

        
        [TestMethod]
        public void MessageReplace()
        {
            var message = fresh();
            message.Init("foo", "bar");

            var newMessage = fresh();
            newMessage.Init("num", 1, 2, 3 );

            newMessage.ReplaceWith(message);
            Assert.AreEqual("Message Test (01.01.0001 01:00:00 [UTC;0;UTC;UTC;UTC;;])\n num \t: 1 2 3 \r\n", newMessage.ToString());

            newMessage.ReplaceWith(message, true);
            Assert.AreEqual("Message Test (01.01.0001 01:00:00 [UTC;0;UTC;UTC;UTC;;])\n foo \t: bar \r\n num \t: 1 2 3 \r\n", newMessage.ToString());


        }

    }
}
