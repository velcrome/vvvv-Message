using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class KeepTest
    {
        #region Simple

        [TestMethod]
        public void QuickMode()
        {
            var keep = new MessageKeep();
            keep.QuickMode = true;

            var message = new Message("Test");

            List<Message> diffs = new List<Message>();
            message.ChangedWithDetails += (orig, diff) => { diffs.Add(diff); } ;

            keep.Add(message);
            message.Init("Field", 1, 2, 3);

            Assert.IsTrue(message.IsChanged);
            Assert.IsTrue(keep.IsChanged);
            Assert.AreEqual(0, diffs.Count);

            var change = keep.Sync();

            Assert.AreEqual(1, diffs.Count);
            Assert.AreEqual(message.Topic, change.First().Topic);

            Assert.IsFalse(message.IsChanged);
            Assert.IsFalse(keep.IsChanged);

            Assert.IsNull(change); // when in quickmode outputs null

            change = keep.Sync();

        }

        [TestMethod]
        public void FullModeInit()
        {
            var keep = new MessageKeep();
            keep.QuickMode = false;

            var message = new Message("Test");
            keep.Add(message);
            keep.Sync();

            Assert.IsFalse(message.IsChanged);
            Assert.IsFalse(keep.IsChanged);

            message.Init("Field", 1, 2, 3);

            Assert.IsTrue(message.IsChanged);
            Assert.IsTrue(keep.IsChanged);

            IEnumerable<int> changedIndices;
            var change = keep.Sync(out changedIndices);
            var diff = change.First();

            Assert.IsFalse(message.IsChanged);
            Assert.IsFalse(keep.IsChanged);

            Assert.AreEqual(message.Topic, diff.Topic);
            Assert.AreEqual(message["Field"], diff["Field"]);
        }

        [TestMethod]
        public void FullModeTopic()
        {
            var keep = new MessageKeep();
            keep.QuickMode = false;

            var message = new Message("Test");
            message.Sync();

            keep.Add(message);

            Assert.IsFalse(message.IsChanged);
            Assert.IsTrue(keep.IsChanged); // message was added

            keep.Sync();

            Assert.IsFalse(keep.IsChanged);

            message.Topic = "Refresh";

            Assert.IsTrue(message.IsChanged);
            Assert.IsTrue(keep.IsChanged);

            IEnumerable<int> changedIndices;
            var change = keep.Sync(out changedIndices);
            var diff = change.First();

            Assert.IsFalse(message.IsChanged);
            Assert.IsFalse(keep.IsChanged);

            Assert.AreEqual(message.Topic, diff.Topic);
            Assert.IsTrue(diff.IsEmpty);
        }


        [TestMethod]
        public void FullModeRemove()
        {
            var keep = new MessageKeep();
            keep.QuickMode = false;

            var message = new Message("Test");
            message.Init("Field", 1, 2, 3);

            
            message.Sync();

            Assert.IsFalse(message.IsEmpty);

            keep.Add(message);

            Assert.IsFalse(message.IsChanged);
            Assert.IsTrue(keep.IsChanged); // message was added

            keep.Sync();

            message.Remove("Field");

            Assert.IsTrue(message.IsChanged);
            Assert.IsTrue(keep.IsChanged);

            IEnumerable<int> changedIndices;
            var change = keep.Sync(out changedIndices);
            var diff = change.First();

            Assert.IsFalse(message.IsChanged);
            Assert.IsFalse(keep.IsChanged);

            Assert.IsTrue(diff.IsEmpty);
        }
        #endregion
    }
}
