using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Messaging.Serializing;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class RawBinTest : TBinTest<Stream>
    {
        public Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        [TestMethod]
        public void RawBinToString()
        {
            var bin = BinFactory.New(typeof(Stream));

            bin.Add(new MemoryStream());
            bin.Add(GenerateStreamFromString("lorem"));

            Assert.IsInstanceOfType(bin.First, typeof(MemoryStream));
            Assert.IsInstanceOfType(bin[1], typeof(MemoryStream));

            Assert.AreEqual("", new StreamReader((Stream) bin.First).ReadToEnd() ) ;
            Assert.AreEqual("lorem", new StreamReader((Stream)bin[1]).ReadToEnd());

            Assert.AreEqual("Bin<Raw> [System.IO.MemoryStream, System.IO.MemoryStream]", bin.ToString());
        }

        [TestMethod]
        public void RawBinToJson()
        {
            var lorem = new MemoryStream(System.Text.Encoding.UTF8.GetBytes ("lorem"));
            TBinToJson(new MemoryStream(), lorem, "\"\"", "\"lorem\"");

            //Assert.AreEqual("", new StreamReader((Stream)newBin.First).ReadToEnd());
            //Assert.AreEqual("lorem", new StreamReader((Stream)newBin[1]).ReadToEnd());
        }

        [TestMethod]
        public void RawBinToStream()
        {
            var bin = BinFactory.New(typeof(Stream));

            bin.Add(new MemoryStream());
            bin.Add(GenerateStreamFromString("lorem"));

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.IsInstanceOfType(newBin, typeof(Bin<Stream>));

            Assert.AreEqual("", new StreamReader((Stream)newBin.First).ReadToEnd());
            Assert.AreEqual("lorem", new StreamReader((Stream)newBin[1]).ReadToEnd());


            Assert.AreEqual("Bin<Raw> [System.IO.MemoryStream, System.IO.MemoryStream]", newBin.ToString());
        }

    }
}
