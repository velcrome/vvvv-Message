using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.Packs.Messaging.Nodes;
using System.Text;

namespace VVVV.Packs.Messaging.Tests
{
    using Time = VVVV.Packs.Time.Time;
    
    [TestClass]
    public class BinTest
    {
        VVVVProfile profile = new VVVVProfile();

        #region First
        [TestMethod]
        public void ChangeBinFirst()
        {
            var bin = BinFactory.New<int>(1, 2, 3, 4);
            bin.IsChanged = false;

            Assert.AreEqual(1, bin.First);
            Assert.IsFalse(bin.IsChanged);

            bin.First = 1;
            Assert.AreEqual(1, bin.First);
            Assert.IsTrue(bin.IsChanged);

            bin.IsChanged = false;
            bin.First = 42;
            Assert.AreEqual(42, bin.First);
            Assert.IsTrue(bin.IsChanged);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException), "Setting the First element of a bin with incompatible element is illegal!")]
        public void SetFirstInvalidObject()
        {
            Bin bin = BinFactory.New(1, 2, 3);
            bin.First = 1.0;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Setting the First element of a bin to null is illegal!")]
        public void SetFirstNull()
        {
            Bin bin = BinFactory.New(1, 2, 3);
            bin.First = null;
        }

        [TestMethod]
        [ExpectedException(typeof(TypeNotSupportedException), "Setting the First element of a bin with unregistered element is illegal!")]
        public void SetFirstUnregistered()
        {
            Bin bin = BinFactory.New(1, 2, 3);
            bin.First = new object();
        }

        #endregion first

        #region indexer

        [TestMethod]
        public void ChangeBinIndex()
        {
            var bin = BinFactory.New<int>(1, 2, 3, 4);
            bin.IsChanged = false;

            Assert.AreEqual(2, bin[1]);
            Assert.IsFalse(bin.IsChanged);

            bin[1] = 1;
            Assert.IsTrue(bin.IsChanged);
            Assert.AreEqual(1, bin[1]);

            bin.IsChanged = false;

            bin[4] = 1; // add one more
            Assert.IsTrue(bin.IsChanged);
            Assert.AreEqual(1, bin[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Setting an element of a bin to null is illegal!")]
        public void FailChangeBinIndexAbove()
        {
            Bin bin = BinFactory.New(1, 2, 3);
            bin[0] = null;
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException), "Setting an element of a bin to null is illegal!")]
        public void RangeBinIndex()
        {
            Bin bin = BinFactory.New(1, 2, 3);
            bin[9] = 4;
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException), "Setting an element of a bin to null is illegal!")]
        public void RangeBinIndexBelow()
        {
            Bin bin = BinFactory.New(1, 2, 3);
            bin[-1] = 4;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException), "Adding unregistered type to a bin<int> should throw exception!")]
        public void InsertInvalidObject()
        {
            Bin bin = BinFactory.New(1, 2, 3);
            bin[0] = "a";
        }
        #endregion indexer

        #region Add
        private Bin<T> Fresh<T>()
        {
            var bin = BinFactory.New<T>();
            Assert.IsTrue(bin.IsChanged);
            bin.IsChanged = false;
            return bin;
        }


        protected void AddSingle<T>(T pass)
        {
            Bin bin = Fresh<T>();
            bin.Add(pass);

            Assert.IsTrue(bin.IsChanged);
            Assert.AreEqual(pass, bin[0]);
            Assert.AreEqual(1, bin.Count);
        }

        [TestMethod]
        public void BinAddSingle()
        {
            AddSingle<bool>(true);
            AddSingle<int>(1);
            AddSingle<float>(1.0f);
            AddSingle<double>(1.0d);

            AddSingle<string>("ok");
            AddSingle<Stream>(new MemoryStream(Encoding.ASCII.GetBytes("pass")));

            AddSingle(new Vector2D());
            AddSingle(new Vector3D());
            AddSingle(new Vector4D());

            AddSingle(new Matrix4x4());
            AddSingle<RGBAColor>(VColor.Green);

            AddSingle(Time.CurrentTime());
            AddSingle(new Message("pass"));
        }

        protected void AddFail<T>(T pass, object fail)
        {
            Bin bin = Fresh<T>();
            try
            {
                bin.Add(fail);
                Assert.Fail("Adding a incompatible item to a bin should throw an exception!");
            }
            catch (BinTypeMismatchException)
            {
                // ok
            }

            Assert.IsFalse(bin.IsChanged);
            Assert.AreEqual(0, bin.Count);

            var failArray = new object[] { pass, fail, pass };
            

            try
            {
                bin.Add(failArray);
                Assert.Fail ("Adding a incompatible item to a bin should throw an exception!");
            } catch (BinTypeMismatchException)
            {
                // ok
            }

            Assert.IsTrue(bin.IsChanged); // first element passed -> dirty now
            Assert.AreEqual(1, bin.Count); // only the first default(T) will make it, after that it will stop short!
        }

        [TestMethod]
        public void FailAddSingle()
        {
            var init = TypeIdentity.Instance;

            AddFail<bool>(true, "fail");
            AddFail<int>(42, "fail");
            AddFail<float>(1, "fail");
            AddFail<double>(1, "fail");

            AddFail<string>("pass", new MemoryStream());
            AddFail<Stream>(new MemoryStream(), "fail");

            AddFail<Vector2D>(new Vector2D(), "fail");
            AddFail<Vector3D>(new Vector3D(), "fail");
            AddFail<Vector4D>(new Vector4D(), "fail");

            AddFail<Matrix4x4>(new Matrix4x4(), "fail");
            AddFail<RGBAColor>(VColor.Green, "fail");

            AddFail<Time>(Time.CurrentTime(), "fail");
            AddFail<Message>(new Message(), "fail");
        }

        protected void AddNull<T>(T pass)
        {
            Bin bin = Fresh<T>();

            try
            {
                bin.Add(null);
                Assert.Fail("Should not be able to add null!");
            } catch (ArgumentNullException)
            {
                // ok!
            }

            Assert.IsFalse(bin.IsChanged);
            Assert.AreEqual(0, bin.Count);

            var repeat = new object[] { pass, null, pass};

            try
            {
                bin.Add(repeat); // stop short after the first detection of null
                Assert.Fail("Cannot add an Enumerable containing null!");
            } catch (ArgumentNullException)
            {
                // ok!
            }

            Assert.IsTrue(bin.IsChanged);
            Assert.AreEqual(1, bin.Count);

        }


        [TestMethod]
        public void NullAddSingle()
        {
            AddNull<bool>(true);
            AddNull<int>(1);
            AddNull<float>(1);
            AddNull<double>(1);

            AddNull<string>("pass");
            AddNull<Stream>(new MemoryStream());

            AddNull<Vector2D>(new Vector2D());
            AddNull<Vector3D>(new Vector3D());
            AddNull<Vector4D>(new Vector4D());

            AddNull<Matrix4x4>(new Matrix4x4());
            AddNull<RGBAColor>(VColor.Green);

            AddNull<Time>(Time.CurrentTime());
            AddNull<Message>(new Message("pass"));
        }

        #endregion Add

        #region Equality
        [TestMethod]
        public void IsEqual()
        {
            var bin = BinFactory.New(1, 2, 3);

            var binA = BinFactory.New(1, 2, 3);
            var binB = BinFactory.New(1, 2);
            var binC = BinFactory.New(1.0, 2.0, 3.0);

            Assert.AreEqual(bin, binA);
            Assert.AreNotEqual(bin, binB);
            Assert.AreNotEqual(bin, binC);
        }
        #endregion Casting

    }
}
