using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Packs.Messaging.Nodes;

namespace VVVV.Packs.Messaging.Tests
{
    using Time = VVVV.Packs.Time.Time;


    [TestClass]
    public class BinFactoryTest
    {
        VVVVProfile profile = new VVVVProfile();

        [TestMethod]
        public void BinCreationEmpty()
        {
            var init = TypeIdentity.Instance;

            Bin bin;

            bin = BinCreationEmpty<bool>();
            bin = BinCreationEmpty<int>();
            bin = BinCreationEmpty<float>();
            bin = BinCreationEmpty<double>();

            bin = BinCreationEmpty<string>();
            bin = BinCreationEmpty<Stream>();

            bin = BinCreationEmpty<RGBAColor>();
            bin = BinCreationEmpty<Matrix4x4>();
            bin = BinCreationEmpty<Vector2D>();
            bin = BinCreationEmpty<Vector3D>();
            bin = BinCreationEmpty<Vector4D>();

            bin = BinCreationEmpty<Time>();
            bin = BinCreationEmpty<Message>();
        }

        private Bin<T> BinCreationEmpty<T>()
        {
            var bin = BinFactory.New<T>();
            Assert.AreEqual(bin.IsChanged, true);

            Assert.AreEqual(bin.GetInnerType(), typeof(T));
            Assert.IsInstanceOfType(bin, typeof(Bin<T>));
            Assert.AreEqual(bin.Count, 0);
            return bin;
        }

        [TestMethod]
        public void BinCreationSingle()
        {
            var init = TypeIdentity.Instance;

            Bin bin;

            bin = BinCreationSingle<bool>(false);
            bin = BinCreationSingle<int>(12);
            bin = BinCreationSingle<float>(0.1f);
            bin = BinCreationSingle<double>(0.1d);

            bin = BinCreationSingle<string>("Test");

            var bytes = Encoding.ASCII.GetBytes("Test Raw");
            bin = BinCreationSingle<Stream>(new MemoryStream(bytes));

            bin = BinCreationSingle<RGBAColor>(VColor.Green);
            bin = BinCreationSingle<Matrix4x4>(VMath.IdentityMatrix);
            bin = BinCreationSingle<Vector2D>(new Vector2D(1d,2d));
            bin = BinCreationSingle<Vector3D>(new Vector3D(1,2,3));
            bin = BinCreationSingle<Vector4D>(new Vector4D(1,2,3,4));

            bin = BinCreationSingle<Time>(Time.MinUTCTime());
            bin = BinCreationSingle<Message>(new Message("test"));
        }

        private Bin<T> BinCreationSingle<T>(T item)
        {
            var bin = BinFactory.New<T>(item); // works the same without <T>
            Assert.AreEqual(bin.GetInnerType(), typeof(T));
            Assert.IsInstanceOfType(bin, typeof(Bin<T>));
            Assert.AreEqual(bin.Count, 1);
            Assert.IsInstanceOfType(bin.First, typeof(T));
            Assert.AreEqual(bin[0], item);
            Assert.AreEqual(bin.IsChanged, true);
            return bin;
        }

        [TestMethod]
        public void BinCreationMultiple()
        {
            var init = TypeIdentity.Instance;

            Bin bin;

            bin = BinCreationWithMultiples(false, true, false);
            bin = BinCreationWithMultiples(1, 2, 3);
            bin = BinCreationWithMultiples(0.1f, 0.2f, 0.3f);
            bin = BinCreationWithMultiples(0.1d, 0.2d, 0.3d);

            bin = BinCreationWithMultiples("TestA", "TestB", "TestC");

            var bytes = Encoding.ASCII.GetBytes("Test Raw");
            bin = BinCreationWithMultiples<Stream>(new MemoryStream(bytes), new MemoryStream(bytes), new MemoryStream(bytes));

            bin = BinCreationWithMultiples(VColor.Red, VColor.Green, VColor.Blue);
            bin = BinCreationWithMultiples(VMath.IdentityMatrix, new Matrix4x4(), VMath.IdentityMatrix);
            bin = BinCreationWithMultiples(new Vector2D(1d, 2d), new Vector2D(), new Vector2D());
            bin = BinCreationWithMultiples(new Vector3D(1, 2, 3), new Vector3D(), new Vector3D());
            bin = BinCreationWithMultiples(new Vector4D(1, 2, 3, 4), new Vector4D(), new Vector4D());

            bin = BinCreationWithMultiples(Time.MinUTCTime(), Time.CurrentTime(), Time.MinUTCTime());
            bin = BinCreationWithMultiples(new Message("test"), new Message("a"), new Message("b"));
        }
        private Bin<T> BinCreationWithMultiples<T>(T itemA, T itemB, T itemC)
        {
            var bin = BinFactory.New<T>(itemA, itemB, itemC);
            Assert.AreEqual(bin.GetInnerType(), typeof(T));
            Assert.IsInstanceOfType(bin, typeof(Bin<T>));
            Assert.AreEqual(bin.Count, 3);
            Assert.IsInstanceOfType(bin[0], typeof(T));
            Assert.IsInstanceOfType(bin[1], typeof(T));
            Assert.IsInstanceOfType(bin[2], typeof(T));

            Assert.AreEqual(bin[0], itemA);
            Assert.AreEqual(bin[1], itemB);
            Assert.AreEqual(bin[2], itemC);

            Assert.IsInstanceOfType(bin.First, typeof(T));
            Assert.AreEqual(bin.IsChanged, true);

            return bin;
        }
    }
}
