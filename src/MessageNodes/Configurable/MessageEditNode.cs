using System;
using System.Collections;
using System.Linq;
using VVVV.Packs.Messaging.Nodes;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Packs.Time;
using VVVV.PluginInterfaces.V2.NonGeneric;


namespace VVVV.Nodes.Messaging.Nodes
{
 
    #region PluginInfo
    [PluginInfo(Name = "Edit", 
        Category = "Message", 
        Help = "Adds or edits multiple Message Fields", 
        AutoEvaluate = true, 
        Version = "Formular", 
        Tags = "Formular, Bin",
        Author = "velcrome")]
    #endregion PluginInfo
    public class MessageEditNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Input", Order = 0)] 
        IDiffSpread<Message> FInput;

        [Input("Update", IsToggle = true, Order = int.MaxValue, DefaultBoolean = true)]
        IDiffSpread<bool> FUpdate;

        [Output("Output", AutoFlush = false)] 
        Pin<Message> FOutput;

#pragma warning restore

        protected override IOAttribute DefinePin(FormularFieldDescriptor field)
        {
            var attr = new InputAttribute(field.Name);

            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = field.DefaultSize;

            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount+1;

            attr.CheckIfChanged = true;

            return attr;
        }


        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() || FUpdate.IsAnyInvalid() ? 0 : FInput.SliceCount;

            if (SpreadMax == 0)
            {
                if (FOutput.SliceCount > 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                
                return;
            }

            var newData = FUpdate[0] && FPins.Any(x => x.Value.ToISpread().IsChanged); // changed pins
            var forceUpdate = FUpdate.IsChanged && FUpdate[0]; // flank to FUpdate[0] == true

            if (newData || forceUpdate)
            {
                // ...and start filling messages
                int messageIndex = 0;
                foreach (var message in FInput)
                {
                    if (FUpdate[messageIndex])
                        CopyFromPins(message, messageIndex, !forceUpdate);
                    messageIndex++;
                }
                FOutput.AssignFrom(FInput);
                FOutput.Flush();
            }

        
        }
    }
}
