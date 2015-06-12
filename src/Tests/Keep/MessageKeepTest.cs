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
            var message = new Message("A");
            message.Init("foo", "bar");
            var code = message.GetHashCode();

            var keep = new MessageKeep();
            keep.Add(message);

            keep.Sync();

            Assert.AreEqual(keep.IsChanged, false);
            message["foo"].First = "xxx";
            Assert.AreEqual(keep.IsChanged, true);

            message.Sync();

            Assert.AreEqual(keep.IsChanged, true);
            message["foo"].First = "yyy";
            Assert.AreEqual(keep.IsChanged, true);

            keep.QuickMode = false;
            Assert.AreEqual(keep.IsChanged, true);

            message.Sync();

            IEnumerable<int> indices;
            var changes = keep.Sync(out indices);

            Assert.AreEqual(keep.IsChanged, false);




        }


        [TestMethod]
        public void KeepTest_2()
        {
            var messageA = new Message("A");
            messageA["Num"] = BinFactory.New<int>(1, 2, 3);
            messageA["Foo"] = BinFactory.New<string>("foo", "bar");

            Assert.AreEqual(true, messageA.IsChanged);

            var messageB = new Message("B");
            messageB["Num"] = BinFactory.New<int>(4);
            messageB["Foo"] = BinFactory.New<string>("foo");

            var keep = new MessageKeep();
            keep.Add(messageA);
            keep.Add(messageB);
            keep.Sync();

            Assert.AreEqual(false, messageA.IsChanged);
            Assert.AreEqual(false, messageB.IsChanged);

            messageA.Init("Vector", new Vector4D(1, 0, 0, 1)); 

            Assert.AreEqual(true, messageA.IsChanged);
            Assert.AreEqual(false, messageB.IsChanged);

            keep.QuickMode = false;

            var keep2 = new MessageKeep();
            keep2.Add(messageA);
            keep2.Add(messageB);
            keep2.Sync();
            keep2.QuickMode = false;

            var changes = keep.Sync();
            Assert.AreEqual(false, keep.IsChanged);

            messageB["Num"].Add(5);

            Assert.AreEqual(true, keep.IsChanged);

            Assert.AreEqual(false, messageA.IsChanged);
            Assert.AreEqual(true, messageB.IsChanged);

            IEnumerable<int> indexes = null;
            changes = keep.Sync(out indexes);

            Assert.AreEqual(false, messageA.IsChanged);
            Assert.AreEqual(false, messageB.IsChanged);

            Assert.AreEqual(1, changes.Count());
            Assert.AreEqual(5, changes.First()["Num"][1]);

            Assert.AreEqual(1, indexes.First());
            Assert.AreEqual("Num" , changes.First().Fields.First());

            Assert.AreEqual(false, messageA.IsChanged);
            Assert.AreEqual(false, messageB.IsChanged);

            messageB.Init("int", 0, 1, 2);

            Assert.AreEqual(false, messageA.IsChanged);
            Assert.AreEqual(true, messageB.IsChanged);

            messageB.Add("int", 3, 4);

            changes = keep2.Sync(out indexes);

            Assert.AreEqual(1, changes.Count()); // only messageB
            Assert.AreEqual("B", changes.First().Topic);
            Assert.AreEqual(2, changes.First().Fields.Count());
            Assert.AreEqual("Bin<int> [0, 1, 2, 3, 4]", changes.First()["int"].ToString());
            Assert.AreEqual("Bin<int> [4, 5]", changes.First()["Num"].ToString());

            Assert.AreEqual(1, indexes.First());
            Assert.AreEqual("Num", changes.First().Fields.First());

        
        }



    }
}
