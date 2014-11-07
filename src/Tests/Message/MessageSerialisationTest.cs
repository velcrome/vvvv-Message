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
    public class MessageSerialisationTest
    {

        [TestMethod]
        public void MessageToString()
        {
          var message = new Message();

          message.Address = "Test";
          message.Init("foo", "bar");
          message.TimeStamp = Time.MinUTCTime();
          		
           Assert.AreEqual("Message Test (01.01.0001 01:00:00 [UTC])\n foo \t: bar \r\n", message.ToString());
        }

        [TestMethod]
        public void MessageToJson()
        {

            var message = new Message();
            message.Init("foo", "bar");
            message.Address = "Test";
            message.TimeStamp = Time.MinUTCTime();

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(message, settings);
            var newMessage = (Message)JsonConvert.DeserializeObject(json, typeof(Message));


            Assert.AreEqual("{\"Address\":\"Test\",\"TimeStamp\":{\"UTC\":\"0001-01-01 00:00:00.0000\",\"ZoneId\":\"UTC\"},\"Data\":{\"foo\":{\"string\":[\"bar\"]}}}", json);

            Assert.AreEqual(message.ToString(), newMessage.ToString());


        }

        [TestMethod]
        public void MessageToStream()
        {
            var message = new Core.Message();
            message.Address = "foo";

            message.Init("MrBoolean", true);
            message.Init("MrDouble", Math.PI);
            message.Init("MsString", "lorem");
            message["MsString"].Add("ipsum");

            message["Empty"] = Bin.New(typeof (bool));

            var stream = message.Serialize();
            var newMessage = stream.DeSerializeMessage();

            Assert.AreEqual(message["MrBoolean"].ToString(), newMessage["MrBoolean"].ToString());
            Assert.AreEqual(message["MsString"].ToString(), newMessage["MsString"].ToString());
            Assert.AreEqual(0, newMessage["Empty"].Count);


        }


    }
}
