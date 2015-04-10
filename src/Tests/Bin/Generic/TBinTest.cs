using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using VVVV.Packs.Messaging;

namespace VVVV.Packs.Messaging.Tests
{
    public class TBinTest<T>
    {
        [TestMethod]
        public void TBinToJson(T first, T second, string firstAsString, string secondAsString)
        {
            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None };
            var alias = TypeIdentity.Instance.FindAlias(typeof(T));

            var bin = BinFactory.New(typeof(T));
            bin.Add(first);

            string json = JsonConvert.SerializeObject(bin, settings);
            Assert.AreEqual(firstAsString, json);

            var newBin = JsonConvert.DeserializeObject(json, typeof(Bin<T>)) as Bin;
            Assert.IsInstanceOfType(newBin, typeof(Bin<T>));
            Assert.AreEqual("Bin<" + alias + "> [" + first.ToString() + "]", newBin.ToString(), true);

            bin.Add(second);
            json = JsonConvert.SerializeObject(bin, settings);

            Assert.AreEqual("[" + firstAsString + "," + secondAsString + "]", json);

            newBin = JsonConvert.DeserializeObject(json, typeof(Bin<T>)) as Bin;
            Assert.IsInstanceOfType(newBin, typeof(Bin<T>));
            Assert.AreEqual("Bin<" + alias + "> [" + first.ToString() + ", " + second.ToString() + "]", newBin.ToString(), true);

        }
    }
}
