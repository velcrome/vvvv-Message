using System.Linq;
using VVVV.Packs.Messaging.Nodes;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;


namespace VVVV.Nodes.Messaging.Nodes
{
 
    #region PluginInfo
    [PluginInfo(Name = "Edit", 
        Category = "Message", 
        AutoEvaluate = true, 
        Help = "Adds or edits multiple Message Fields", 
        Version = "Formular", 
        Tags = "Formular, Bin",
        Author = "velcrome")]
    #endregion PluginInfo
    public class MessageEditNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Input", Order = 0)] 
        IDiffSpread<Message> FInput;

        [Input("AutoSense", Order = -1, IsSingle = true, IsToggle = true, DefaultBoolean = false, Visibility = PinVisibility.OnlyInspector)]
        IDiffSpread<bool> FDetectChange;

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
            var anyUpdate = FUpdate.Any(x => x);

            // Flush upstream changes through the plugin
            if (FInput.IsChanged)
            {
                FOutput.SliceCount = 0;

                if (FInput.IsAnyInvalid())
                {
                    if (FOutput.SliceCount > 0)
                    {
                        FOutput.Flush();
                        return; // if no input, no further calculation.
                    }
                }
                else
                {
                    FOutput.AssignFrom(FInput); // push change from upstream if valid
                    FOutput.Flush();
                }
            }
            else // no change from upstream.
            {
                if (anyUpdate) // push change that Write will be performing
                {
                    FOutput.AssignFrom(FInput);
                    FOutput.Flush();
                }
            }

            bool newData = FPins.Any(pinName => pinName.Value.ToISpread().IsChanged); // changed pins
            bool forceUpdate = !FDetectChange[0] || FDetectChange.IsChanged;

            if (!FInput.IsChanged &&
                 !FConfig.IsChanged &&
                 !newData
             ) return; // if no change, no furter calc


            if (anyUpdate && (forceUpdate || newData)) {
                int messageIndex = 0;
                foreach (var message in FInput)
                {
                    if (FUpdate[messageIndex])
                    CopyFromPins(message, messageIndex, true) ;
                        messageIndex++;
                }
            }
        }
    }
}
