using System;
using System.Collections;
using VVVV.Packs.Message.Core;
using VVVV.Packs.Message.Core.Formular;
using VVVV.Packs.Message.Nodes;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Messaging.Configurable
{
 
    #region PluginInfo
    [PluginInfo(Name = "Set", Category = "Message", Help = "Adds or edits multiple Message Properties", Tags = "Typeable")]
    #endregion PluginInfo
    public class MessageSetNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Input")] private IDiffSpread<Message> FInput;

        [Output("Output", AutoFlush = false)] private Pin<Message> FOutput;
#pragma warning restore

        protected override IOAttribute DefinePin(FormularFieldDescriptor configuration)
        {
            var attr = new InputAttribute(configuration.Name); 
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = configuration.DefaultSize;
            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount + 1;

            return attr;
        }


        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.SliceCount;

            if (!FInput.IsChanged)
            {
                return;
            }

            if (FInput.SliceCount == 0 || FInput[0] == null)
            {
                FOutput.SliceCount = 0;
                return;
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];
                foreach (string name in FPins.Keys)
                {
                    var pin = (IEnumerable) ToISpread(FPins[name])[i];
                    message.AssignFrom(name, pin);
                }
            }

            FOutput.AssignFrom(FInput);
            FOutput.Flush();
        }
    }
}
