using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;


namespace VVVV.Packs.Message.Tests
{


    [TestClass]
    public class MessageTest
    {

        [TestMethod]
        public void MessageToString()
        {
          //  var message = new Message();
        //    Assert.AreEqual("Bin<time> [" + current.ToString() + "]", bin.ToString());
        }

        [TestMethod]
        public void MessageToJson()
        {
            //var current = Time.Time.CurrentTime();
            //var bin = new Bin<Time.Time>(current);

            //var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            //string json = JsonConvert.SerializeObject(bin, settings);


            //var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            //Assert.IsInstanceOfType(newBin, typeof(Bin<Time.Time>));
            //Assert.AreEqual(current.ToString(), newBin[0].ToString());


        }

        [TestMethod]
        public void MessageToStream()
        {
            var message = new Core.Message();
            message.Address = "foo";

            message.Add("MrBoolean", true);
            message.Add("MrDouble", Math.PI);
            message.Add("MsString", "lorem");
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
