using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Serializing;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using System.Collections.Generic;
using System.Linq;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class MessageKeepTest
    {

        [TestMethod]
        public void KeepTest()
        {
            var message = new Message();
            message["Num"] = BinFactory.New<int>(1, 2, 3);
            message["Foo"] = BinFactory.New<string>("foo", "bar");

            Assert.AreEqual(true, message.IsChanged);

            var message2 = new Message();
            message2["Num"] = BinFactory.New<int>(4);
            message2["Foo"] = BinFactory.New<string>("foo");

            var keep = new MessageKeep();
            keep.Add(message);
            keep.Add(message2);
            keep.Sync();

            Assert.AreEqual(false, message.IsChanged);
            Assert.AreEqual(false, message2.IsChanged);

            message.Init("Vector", new Vector4D(1, 0, 0, 1)); 

            Assert.AreEqual(true, message.IsChanged);
            Assert.AreEqual(false, message2.IsChanged);

            keep.QuickMode = false;
            var changes = keep.Sync();

            Assert.AreEqual(false, keep.IsChanged);

            message2["Num"].Add(5);

            Assert.AreEqual(true, keep.IsChanged);

            Assert.AreEqual(false, message.IsChanged);
            Assert.AreEqual(true, message2.IsChanged);

            IEnumerable<int> indexes = null;
            changes = keep.Sync(out indexes);

            Assert.AreEqual(false, message.IsChanged);
            Assert.AreEqual(false, message2.IsChanged);

            Assert.AreEqual(1, changes.Count());

            Assert.AreEqual(5, changes.First()["Num"][1]);

            Assert.AreEqual(1, indexes.First());
            Assert.AreEqual("Num" , changes.First().Fields.First());
        
        }
    }
}
