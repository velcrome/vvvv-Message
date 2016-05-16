using MsgPack.Serialization;
using System;
using System.ComponentModel.Composition;
using System.IO;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging.Serializing;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes.Serializing
{
    #region PluginInfo
    [PluginInfo(Name = "AsMsgPack", Category = "Message.Raw", Help = "Serialize Messages to binary", Tags = "Stream", Author = "velcrome")]
    #endregion PluginInfo
    public class MsgPackSerializeNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Stream> FOutput;

        [Import()]
        protected ILogger FLogger;

        protected MsgPackMessageSerializer serializer;

        public void OnImportsSatisfied()
        {
            var context = new SerializationContext();
            context.CompatibilityOptions.PackerCompatibilityOptions = MsgPack.PackerCompatibilityOptions.PackBinaryAsRaw;

            serializer = new MsgPackMessageSerializer(context);
        }

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged) return;

            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;

            FOutput.SliceCount = SpreadMax;
//            FOutput.ResizeAndDispose(SpreadMax, () => new MemoryStream());

            for (int i = 0; i < SpreadMax; i++)
            {

                FOutput[i] = new MemoryStream(); // null exception. wrong usage of resiseAndDispose maybe?
                serializer.Pack(FOutput[i], FInput[i]);
            }
            FOutput.Flush();
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "FromMsgPack", Category = "Message.Raw", Help = "Convert Messages", Tags = "Stream", Author = "velcrome")]
    #endregion PluginInfo
    public class MsgPackAsMessageNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Input")]
        protected IDiffSpread<Stream> FInput;

        [Output("Message", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;

        protected MsgPackMessageSerializer serializer;

        public void OnImportsSatisfied()
        {
            var context = new SerializationContext();
            context.CompatibilityOptions.PackerCompatibilityOptions = MsgPack.PackerCompatibilityOptions.PackBinaryAsRaw;

            serializer = new MsgPackMessageSerializer(context);
        }

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged) return;

            SpreadMax = FInput.SliceCount;
            FOutput.SliceCount = 0;
            

            for (int i = 0; i < SpreadMax; i++)
            {
                if (FInput[i] != null && FInput[i].Length > 0)
                {
                    var message = serializer.Unpack(FInput[i]);
                    FOutput.Add(message);
                }
            }

            FOutput.Flush();
        }

    }

}
