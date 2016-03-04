using System.Linq;
using VVVV.Packs.Messaging.Nodes;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using System;


namespace VVVV.Nodes.Messaging.Nodes
{
 
    #region PluginInfo
    [PluginInfo(Name = "Edit", 
        Category = "Message", 
        AutoEvaluate = true, 
        Help = "Adds or edits multiple Message FieldNames", 
        Version = "Formular", 
        Tags = "Formular, Bin",
        Author = "velcrome")]
    #endregion PluginInfo
    public class MessageEditNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Input", Order = 0)] 
        IDiffSpread<Message> FInput;

        [Input("AutoSense", Order = int.MaxValue-1, IsSingle = true, IsToggle = true, DefaultBoolean = false, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FAutoSense;

        [Input("Update", IsToggle = true, Order = int.MaxValue, DefaultBoolean = true)]
        IDiffSpread<bool> FUpdate;

        [Output("Output", AutoFlush = false)] 
        Pin<Message> FOutput;

#pragma warning restore

        protected override IOAttribute SetPinAttributes(FormularFieldDescriptor field)
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
            if (FInput.IsAnyInvalid())
            {
                if (FOutput.SliceCount > 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                    return; // if no input, no further calculation.
                }
                return;
            }

            bool doFlush = false;

            // is any Update slice checked?
            var anyUpdate = FUpdate.Any(x => x);
 
            // Flush upstream changes through the plugin
            if (FInput.IsChanged)
            {
                FOutput.SliceCount = 0;
                FOutput.AssignFrom(FInput); // push change from upstream if valid
                doFlush = true;
            }
            else if (!anyUpdate) return; // if no update and no change, no need to flush! 

            bool newData = FPins.Any(pinName => pinName.Value.ToISpread().IsChanged); // changed pins
            newData |= !FAutoSense[0]; // assume newData, if AutoSense is off.

            if (anyUpdate && newData) {
                int messageIndex = 0;
                foreach (var message in FInput)
                {
                    if (FUpdate[messageIndex])
                    doFlush |= CopyFromPins(message, messageIndex, FAutoSense[0]);
                        
                    messageIndex++;
                }
            }

            if (doFlush) FOutput.Flush();


            if (RemovePinsFirst) RetryConfig();

        }
    }
}
