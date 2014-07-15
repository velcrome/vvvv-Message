using System.ComponentModel.Composition;
using System.Xml.Linq;
using VVVV.Core.Logging;
using VVVV.Packs.Message;
using VVVV.Packs.Message.Serializing;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes.Serializing
{

    #region PluginInfo
    [PluginInfo(Name = "AsXElement", Category = "Message", Help = "Convert Messages", Tags = "Dynamic, velcrome, XML")]
    #endregion PluginInfo
    public class MessageAsXElementNode : IPluginEvaluate
    {
        #pragma warning disable 649, 169
        [Input("Input")]
        IDiffSpread<Message> FInput;

        [Output("Element", AutoFlush = false)]
        ISpread<XElement> FOutput;
        #pragma warning restore

        [Import()]
        protected ILogger FLogger;

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged) return;

            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                FOutput[i] = FInput[i].ToXElement();
            }
            FOutput.Flush();
        }
    }
}
