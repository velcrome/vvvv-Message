using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

using VVVV.Packs.Messaging.Serializing;


namespace VVVV.Packs.Messaging.Tests
{

    using Time = VVVV.Packs.Time.Time;

    [TestClass]
    public class MessageSerialisationTest
    {

        [TestMethod]
        public void MessageToString()
        {
          var message = new Message();

          message.Topic = "Test";
          message.Init("foo", "bar");
          message.TimeStamp = Time.MinUTCTime();
          		
           Assert.AreEqual("Message Test (01.01.0001 01:00:00 [UTC])\n foo \t: bar \r\n", message.ToString());
        }

        [TestMethod]
        public void MessageToJson()
        {

            var message = new Message();
            message.Init("foo", "bar");
            message.Topic = "Test";
            message.TimeStamp = Time.MinUTCTime();

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(message, settings);
            Assert.AreEqual("{\"Topic\":\"Test\",\"TimeStamp\":{\"UTC\":\"0001-01-01 00:00:00.0000\",\"ZoneId\":\"UTC\"},\"Data\":{\"foo\":{\"string\":[\"bar\"]}}}", json);

            var newMessage = (Message)JsonConvert.DeserializeObject(json, typeof(Message));
            Assert.AreEqual(message.ToString(), newMessage.ToString());


        }

        [TestMethod]
        public void MessageToStream()
        {
            var message = new Message();
            message.Topic = "foo";

            message.Init("MrBoolean", true);
            message.Init("MrDouble", Math.PI);
            message.Init("MsString", "lorem");
            message["MsString"].Add("ipsum");

            message["Empty"] = BinFactory.New(typeof(bool));

            var stream = message.Serialize();
            var newMessage = stream.DeSerializeMessage();

            Assert.AreEqual(message["MrBoolean"].ToString(), newMessage["MrBoolean"].ToString());
            Assert.AreEqual(message["MsString"].ToString(), newMessage["MsString"].ToString());
            Assert.AreEqual(0, newMessage["Empty"].Count);


        }


    }
}
