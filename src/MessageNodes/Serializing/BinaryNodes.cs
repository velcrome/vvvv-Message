using System.ComponentModel.Composition;
using System.IO;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging.Serializing;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes.Serializing
{
    #region PluginInfo
    [PluginInfo(
        Name = "AsRaw", 
        Category = "Message.Raw", 
        Help = "Serialize Messages to binary", 
        Tags = "Proprietary, Stream", 
        Author = "velcrome, microdee", 
        Ignore = true)]
    #endregion PluginInfo
    public class MessageSerializeNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        IDiffSpread<Message> FInput;

        [Output("Output", AutoFlush = false)]
        ISpread<Stream> FOutput;

        [Import()]
        protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged) return;

            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                var s = FInput[i].Serialize();
                FOutput[i] = s;
            }
            FOutput.Flush();
        }
    }

    #region PluginInfo
    [PluginInfo
        (Name = "AsMessage", 
        Category = "Message.Raw", 
        Help = "Convert  Messages",
        Tags = "Binary, Stream", 
        Author = "velcrome, microdee",
        Ignore = true)]
    #endregion PluginInfo
    public class RawAsMessageNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        IDiffSpread<Stream> FInput;

        [Output("Message", AutoFlush = false)]
        ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged) return;

            SpreadMax = FInput.SliceCount;
            FOutput.SliceCount = 0;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (FInput[i] != null && FInput[i].Length > 0)
                {
                    var stream = BinaryExtensions.DeSerializeMessage(FInput[i]);
                    FOutput.Add(stream);
                }
            }

            FOutput.Flush();
        }
    }

}
