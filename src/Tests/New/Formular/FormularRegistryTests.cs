using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;


namespace VVVV.Packs.Messaging.Tests
{
    [TestClass]
    public class FormularRegistryTests
    {
        [TestMethod]
        public void MultipleDefinitions()
        {
            var reg = MessageFormularRegistry.Context;

            var formular = new MessageFormular("A38", "string[3] Field");

            string lastChangedFormular = "";
            reg.FormularChanged += (sender, args) => lastChangedFormular = args.FormularName;

            Assert.AreEqual("", lastChangedFormular);
            var success = reg.Define("AddFormular", formular);

            Assert.IsTrue(success);
            Assert.AreEqual("A38", lastChangedFormular);

            try
            {
                reg.Define("Clash", formular);
                Assert.Fail("Formular by that name already exists from a different definer.");
            } catch (RegistryException) {}

            success = reg.Undefine("AddFormular", formular);
            Assert.IsTrue(success);

            success = reg.Define("NoClash", formular);
            Assert.IsTrue(success);

            var definitions = reg.AllFormularNames.Where(x => x != MessageFormular.DYNAMIC);

            Assert.AreEqual(1, definitions.Count());
            Assert.AreEqual("A38", definitions.First()); 
        }

    }
}