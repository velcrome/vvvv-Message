using System.ComponentModel.Composition;
using System.IO;
using VVVV.Core.Logging;

using VVVV.Packs.Message.Core.Serializing;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes.Serializing
{
    using Message = VVVV.Packs.Message.Core.Message;

    #region PluginInfo
    [PluginInfo(Name = "AsRaw", Category = "Message", Help = "Filter Messages", Tags = "Binary, Stream", Author = "velcrome")]
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
    [PluginInfo(Name = "AsMessage", Category = "Raw", Help = "Filter Messages", Tags = "Message, Binary, Stream", Author = "velcrome")]
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
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {

                FOutput[i] = BinaryExtensions.DeSerializeMessage(FInput[i]);
            }

            FOutput.Flush();
        }
    }

}
