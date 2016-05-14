using Microsoft.VisualStudio.TestTools.UnitTesting;
using VVVV.Packs.Messaging;
using MsgPack.Serialization;
using VVVV.Packs.Messaging.Serializing;
using System.IO;
using System.Linq;

[TestClass]
public class MsgPackTest
{
    #region basics

    private SerializationContext Setup()
    {

        var context = new SerializationContext();
        context.Serializers.RegisterOverride(new MsgPackMessageSerializer(context));

        return context;
    }

    

    [TestMethod]
    public void Message()
    {
        var context = Setup();

        var orig = new Message("Test");

        //orig.Init("m1", new Message("Inner"));

        //orig.Init("b", true, false, false);
        //orig.Init("i", 1, 2, 3);
        //orig.Init("f", 1.0f, 2.0f, 3.0f);
        //orig.Init("d", 1.0d, 2.0d, 3.0d);

        //orig.Init("s1", "A", "ョ", "asdfa");

        //orig.Init("b1", true);
        //orig.Init("i1", 1);
        //orig.Init("f1", 1.0f);
        //orig.Init("d1", 1.0d);
        //orig.Init("s1", "ョ");

        orig.Init("r1", new MemoryStream());



        var serializer = MessagePackSerializer.Get<Message>(context);

        var stream = new MemoryStream();
        serializer.Pack(stream, orig);

        stream.Position = 0;
        var copy = serializer.Unpack(stream);

        Assert.AreEqual(orig.Topic, copy.Topic);

        foreach (var fieldName in orig.Fields.Where( f => f != "m1"))
        {
            Assert.AreEqual(orig[fieldName], copy[fieldName]);
        }
    }

    #endregion
}
