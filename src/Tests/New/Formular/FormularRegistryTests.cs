using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using VVVV.Packs.Messaging;

namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class FormularRegistryTests
    {
        [TestMethod]
        public void MultipleDefinitions()
        {
            var reg = MessageFormularRegistry.Instance;

            var formular = new MessageFormular("string[3] Field", "A38");

            string lastChangedFormular = "";
            reg.TypeChanged += (sender, args) => lastChangedFormular = args.FormularName;

            Assert.AreEqual("", lastChangedFormular);
            var success = reg.Define("AddFormular", formular);

            Assert.IsTrue(success);
            Assert.AreEqual("A38", lastChangedFormular);

            try
            {
                reg.Define("Clash", formular);
                Assert.Fail("Formular by that name already exists from a different source.");
            } catch (RegistryException) {}

            success = reg.Undefine("AddFormular", formular);
            Assert.IsTrue(success);

            success = reg.Define("NoClash", formular);
            Assert.IsTrue(success);

            var definitions = reg.Names.Where(x => x != MessageFormular.DYNAMIC);

            Assert.AreEqual(1, definitions.Count());
            Assert.AreEqual("A38", definitions.First()); 
        }

    }
}