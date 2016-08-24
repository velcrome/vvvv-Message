using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class ChangeEventTests
    {
        [TestMethod]
        public void WithoutDetails()
        {
            var message = new Message("Test");

            // fresh message is always changed until first sync
            Assert.IsTrue(message.IsChanged);

            var count = 0;
            message.Changed += orig => count++;

            // report ctor change of Message now
            var hasChanged = message.Commit(this);
            Assert.IsTrue(hasChanged);
            Assert.AreEqual(1, count);

            message.Init("Field", "a", "b", "c");

            // report next one
            hasChanged = message.Commit(this);
            Assert.IsTrue(hasChanged);
            Assert.AreEqual(2, count);

            // stay silent, because no change
            hasChanged = message.Commit(this);
            Assert.IsFalse(hasChanged);
            Assert.AreEqual(2, count);
        }


        [TestMethod]
        public void WithDetails()
        {
            var message = new Message("Test");

            List<Message> diffs = new List<Message>();

            // will be notified on sync only
            message.Init("Field", 1, 2, 3);

            // set up anonymous watcher, simply recording all changes
            message.ChangedWithDetails += (orig, diff) => { diffs.Add(diff); };

            Assert.IsTrue(message.IsChanged);
            Assert.AreEqual(0, diffs.Count);

            var hasChanged = message.Commit(this);
            Assert.IsTrue(hasChanged);

            // notification received
            Assert.AreEqual(1, diffs.Count);

            // change corresponds
            Assert.AreEqual(message.Topic, diffs.First().Topic);
            Assert.AreEqual((Bin<int>)message["Field"], diffs.First()["Field"]);

            Assert.IsFalse(message.IsChanged);

            // sync again, should stay silent this time
            hasChanged = message.Commit(this);
            Assert.IsFalse(hasChanged);
            Assert.AreEqual(1, diffs.Count);

            message.Topic = "ChangedTopic";
            message.Init("Other", 1.68, 3.14);

            // sync again, check changes
            hasChanged = message.Commit(this);
            Assert.IsTrue(hasChanged);
            Assert.AreEqual(2, diffs.Count);

            // change corresponds
            Assert.AreEqual(message.Topic, diffs.Last().Topic);
            Assert.AreEqual((Bin<double>)message["Other"], diffs.Last()["Other"]);

            // really only "Other"
            Assert.AreEqual(1, diffs.Last().Fields.Count());
        }

        [TestMethod]
        public void TopicWithDetails()
        {
            var message = new Message("Test");

            List<Message> diffs = new List<Message>();

            message.Init("Field", 1, 2, 3);
            message.Commit(this); // clear now

            // set up anonymous watcher, simply recording all changes
            message.ChangedWithDetails += (orig, diff) => { diffs.Add(diff); };

            Assert.IsFalse(message.IsChanged);

            message.Topic = "OtherTopic";

            var hasChanged = message.Commit(this);
            Assert.IsTrue(hasChanged);

            // notification received
            Assert.AreEqual(1, diffs.Count);

            // change corresponds
            Assert.AreEqual(message.Topic, diffs.First().Topic);

            Assert.IsTrue(diffs.Last().IsEmpty);
            Assert.IsFalse(message.IsChanged);

            message.Topic = "  OtherTopic   "; // adding whitespaces should not matter
            Assert.AreEqual("OtherTopic", message.Topic);

            Assert.IsFalse(message.IsChanged);

            // sync again, should stay silent this time
            hasChanged = message.Commit(this);
            Assert.IsFalse(hasChanged);
            Assert.AreEqual(1, diffs.Count);

        }
    }
}