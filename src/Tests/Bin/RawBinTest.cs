using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Serializing;

namespace VVVV.Packs.Message.Tests
{
    [TestClass]
    public class RawBinTest
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
            var bin = Bin.New(typeof(Stream));

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
            var bin = Bin.New(typeof(Stream));

            bin.Add(new MemoryStream());
            bin.Add(GenerateStreamFromString("lorem"));

            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };


            string json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("{\"Raw\":[\"\",\"lorem\"]}", json);

            var newBin = (Bin)JsonConvert.DeserializeObject(json, typeof(Bin));

            Assert.AreEqual(typeof(Bin<Stream>), newBin.GetType());

            Assert.AreEqual("", new StreamReader((Stream)newBin.First).ReadToEnd());
            Assert.AreEqual("lorem", new StreamReader((Stream)newBin[1]).ReadToEnd());

            
            Assert.AreEqual("Bin<Raw> [System.IO.MemoryStream, System.IO.MemoryStream]", newBin.ToString());
        }

        [TestMethod]
        public void RawBinToStream()
        {
            var bin = Bin.New(typeof(Stream));

            bin.Add(new MemoryStream());
            bin.Add(GenerateStreamFromString("lorem"));

            var stream = bin.Serialize();
            var newBin = stream.DeSerializeBin();

            Assert.AreEqual(typeof(Bin<Stream>), newBin.GetType());

            Assert.AreEqual("", new StreamReader((Stream)newBin.First).ReadToEnd());
            Assert.AreEqual("lorem", new StreamReader((Stream)newBin[1]).ReadToEnd());


            Assert.AreEqual("Bin<Raw> [System.IO.MemoryStream, System.IO.MemoryStream]", newBin.ToString());
        }

    }
}
