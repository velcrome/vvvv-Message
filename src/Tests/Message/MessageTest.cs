using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using VVVV.DX11;

namespace VVVV.Packs.Messaging.Tests
{

    [TestClass]
    public class MessageTest
    {
        #region Init
        [TestMethod]
        public void InitTest()
        {
            var name = "field";
            var m = new Message("topic");
            m.Commit(this);

            Assert.IsFalse(m.IsChanged);
            m.Init(name, 1, 2, 3, 4);
            Assert.IsTrue(m.IsChanged);

            Assert.AreEqual(m[name], BinFactory.New(1, 2, 3, 4));
            Assert.AreEqual(m[name].Count, 4);
        }

        [TestMethod]
        public void InitWithInvalid()
        {
            var name = "field";
            var m = new Message("topic");

            try
            {
                m.Init(name, 1, 2, 3, "fail");
                Assert.Fail("Invalid elements should throw Exception");
            }
            catch (BinTypeMismatchException)
            {
                // ok!
            }
        }

        [TestMethod]
        public void InitWithNull()
        {
            var name = "field";
            var m = new Message("topic");

            try
            {
                m.Init(name, 1, 2, 3, null);
                Assert.Fail("Null elements should throw Exception");
            }
            catch (ArgumentNullException)
            {
                // ok!
            }
        }
        #endregion

        #region AssignFrom

        [TestMethod]
        public void AssignFromTest()
        {
            var name = "field";
            var m = new Message("topic");
            m.Commit(this);

            Assert.IsFalse(m.IsChanged);
            var data = Enumerable.Repeat(4, 4);
            m.AssignFrom(name, data);

            Assert.IsTrue(m.IsChanged);

            Assert.AreEqual(m[name], BinFactory.New(4, 4, 4, 4));
            Assert.AreEqual(m[name].Count, 4);
        }

        [TestMethod]
        public void AssignInvalidName()
        {
            var name = "this fails";
            var m = new Message("topic");

            try
            {
                m.AssignFrom(name, Enumerable.Repeat(4, 4), typeof(int));
                Assert.Fail("Invalid Names should throw Exception");
            }
            catch (ParseFormularException)
            {
                // ok!
            }
        }


        [TestMethod]
        public void AssignInvalidData()
        {
            var name = "field";
            var m = new Message("topic");

            try
            {
                m.AssignFrom(name, new object[] { 1, 2, "invalid" });
                Assert.Fail("Invalid elements should throw Exception");
            }
            catch (BinTypeMismatchException)
            {
                // ok!
            }
        }

        [TestMethod]
        public void AssignInvalidDataTyped()
        {
            var name = "field";
            var m = new Message("topic");

            try
            {
                m.AssignFrom(name, new object[] { 1, 2, "invalid" }, typeof(int));
                Assert.Fail("Invalid elements should throw Exception");
            }
            catch (BinTypeMismatchException)
            {
                // ok!
            }
        }

        [TestMethod]
        public void AssignInvalidType()
        {
            var name = "field";
            var m = new Message("topic");

            try
            {
                m.AssignFrom(name, new object[] { new object(), 2, 3 });
                Assert.Fail("Invalid elements should throw Exception");
            }
            catch (TypeNotSupportedException)
            {
                // ok!
            }
        }

        [TestMethod]
        public void AssignNull()
        {
            var name = "field";
            var m = new Message("topic");

            try
            {
                m.AssignFrom(name, null);
                Assert.Fail("Assigning null should throw Exception");
            }
            catch (ArgumentNullException)
            {
                // ok!
            }
        }

        [TestMethod]
        public void AssignStartingWithNull()
        {
            var name = "field";
            var m = new Message("topic");

            try
            {
                m.AssignFrom(name, new object[] { null, 2, 3 });
                Assert.Fail("Invalid elements should throw Exception");
            }
            catch (ArgumentNullException)
            {
                // ok!
            }
        }

        [TestMethod]
        public void AssignContainingNull()
        {
            var name = "field";
            var m = new Message("topic");

            try
            {
                m.AssignFrom(name, new object[] { 1, 2, null });
                Assert.Fail("Invalid elements should throw Exception");
            }
            catch (ArgumentNullException)
            {
                // ok!
            }
        }

        [TestMethod]
        public void AssignContainingNullTyped()
        {
            var name = "field";
            var m = new Message("topic");

            try
            {
                m.AssignFrom(name, new object[] { 1, 2, null }, typeof(int));
                Assert.Fail("Invalid elements should throw Exception");
            }
            catch (ArgumentNullException)
            {
                // ok!
            }
        }
        #endregion

        #region AddFrom


        #endregion

        #region Rename
        [TestMethod]
        public void RenameTest()
        {
            var name = "field";
            var m = new Message("topic");
            var data = Enumerable.Repeat(4, 4);
            m.AssignFrom(name, data);
            m.Commit(this);

            Assert.IsFalse(m.IsChanged);
            m.Rename(name, "newName");

            Assert.IsTrue(m.IsChanged);
            Assert.AreEqual(m["newName"], BinFactory.New(4, 4, 4, 4));
            Assert.AreEqual(m["newName"].Count, 4);
        }

        [TestMethod]
        public void RenameTestOverwrite()
        {
            var name = "field";
            var newName = "newName";
            var m = new Message("topic");
            m.Init(newName, 1);
            m.Init(name, 4,4,4,4);
            m.Commit(this);

            Assert.IsFalse(m.IsChanged);
            m.Rename(name, newName, true);

            Assert.IsTrue(m.IsChanged);
            Assert.AreEqual(m[newName], BinFactory.New(4, 4, 4, 4));
            Assert.AreEqual(m[newName].Count, 4);
        }

        [TestMethod]
        public void RenameTestOverwriteFalse()
        {
            var name = "field";
            var newName = "newName";
            var m = new Message("topic");
            m.Init(newName, 1);
            m.Init(name, 4);

            try
            {
                m.Rename(name, newName); // overwrite = false
                Assert.Fail("Overwriting Field should be illegal.");
            } catch (DuplicateFieldException)
            {
                //ok
            }

        }

        [TestMethod]
        public void RenameTestIllegalName()
        {
            var name = "field";
            var newName = "new Name";
            var m = new Message("topic");
            m.Init(name, 4);

            try
            {
                m.Rename(name, newName); 
                Assert.Fail("Illegal Field name should throw exception");
            }
            catch (ParseFormularException)
            {
                //ok
            }

        }
        #endregion

        #region Index

        [TestMethod]
        public void Set()
        {
            var name = "field";
            var m = new Message("topic");
            var data = Enumerable.Repeat(4, 4);
            m.Commit(this);

            Assert.IsFalse(m.IsChanged);
            m[name] = BinFactory.New(data);
            Assert.IsTrue(m.IsChanged);

            m.Commit(this);

            Assert.AreEqual(m[name], BinFactory.New(4, 4, 4, 4));
            Assert.AreEqual(m[name].Count, 4);
            Assert.IsFalse(m.IsChanged);
        }

        [TestMethod]
        public void SetIllegalName()
        {
            var name = "illegal field name";
            var m = new Message("topic");
            
            try
            {
                m[name] = BinFactory.New(1, 2, 3);
                Assert.Fail("Illegal Field name should throw exception");
            }
            catch (ParseFormularException)
            {
                //ok
            }
        }

        #endregion


        #region utils
        [TestMethod]
        public void EmptyAndRename()
        {
            var m = new Message("topic");

            Assert.IsTrue(m.IsEmpty);

            m.Init("field", 1, 2);
            var c = m.Commit(this);
            Assert.IsTrue(c);

            Assert.IsFalse(m.IsChanged);
            Assert.IsFalse(m.IsEmpty);

            m.Remove("field");
            Assert.IsTrue(m.IsEmpty);
            Assert.IsTrue(m.IsChanged);

            c = m.Commit(this); 
            Assert.IsTrue(c);

            c = m.Commit(this);
            Assert.IsFalse(c);
        }

        [TestMethod]
        public void Clone()
        {
            var m = new Message("topic");

            var deep = "field";
            m.Init(deep, 1, 2);

            var mess = "message";
            m.Init(mess, new Message("inner"));

            var n = m.Clone() as Message;

            Assert.AreEqual(m[deep], n[deep]); // same content...
            Assert.AreNotEqual(m[deep].GetHashCode(), n[deep].GetHashCode()); // ...but unique bin...

            m[deep].First = 42;
            Assert.AreNotEqual(m[deep].First, n[deep].First); // ...but unique bin...


            Assert.AreEqual(m[mess], n[mess]); // same content...
            Assert.AreNotEqual(m[mess].GetHashCode(), n[mess].GetHashCode()); // ...but unique bin

            var inner = m[mess].First as Message;
            inner.Topic = "changeIt!"; // only change once

            var innerCopy = n[mess].First as Message;
            Assert.AreEqual(inner.Topic, innerCopy.Topic); // both now have a topic called "changeIt!"

        }

        #endregion

        #region Inject

        [TestMethod]
        public void TestInjectDeep()
        {
            var a = new Message("Topic");
            a.Init("Field", 1, 2);

            var b = new Message("Topic");
            b.Init("Field", 1, 2);

            a.Commit(this);

            a.InjectWith(b, true);

            Assert.IsFalse(a.IsChanged);
            Assert.AreEqual(a["Field"], b["Field"]);
        }

        [TestMethod]
        public void TestInjectDeepWithDifference()
        {
            var a = new Message("Topic");
            a.Init("Field", 1, 2);

            var b = new Message("Topic");
            b.Init("Field", 1, 2, 3); // slightly different

            a.Commit(this);

            a.InjectWith(b, true);

            Assert.IsTrue(a.IsChanged);
            Assert.AreEqual(a["Field"], b["Field"]);
        }

        [TestMethod]
        public void TestInject()
        {
            var a = new Message("Topic");
            a.Init("Field", 1, 2);

            var b = new Message("Topic");
            b.Init("Field", 1, 2, 3);

            a.Commit(this);

            a.InjectWith(b, false);

            Assert.IsTrue(a.IsChanged);
            Assert.AreEqual(a["Field"], b["Field"]);
        }

        [TestMethod]
        public void TestInjectWithNew()
        {
            var a = new Message("Topic");
            a.Init("Field", 1, 2);

            var b = new Message("Topic");
            b.Init("Field", 1, 2);
            b.Init("Other", 1, 2);

            a.Commit(this);

            a.InjectWith(b, true);

            Assert.IsTrue(a.IsChanged);
            Assert.IsFalse(a["Field"].IsChanged);

            Assert.AreEqual(a["Field"], b["Field"]);
            Assert.AreEqual(a["Other"], b["Other"]);
        }
        #endregion
    }

}
