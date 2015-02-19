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
                if (FOutput.IsAnyInvalid())
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                
                return;
            }

            bool newData = FUpdate[0] && FPins.Any(pinName => pinName.Value.ToISpread().IsChanged); // changed pins
            bool forceUpdate = FUpdate.IsChanged && FUpdate.Any(update => update); // upflank of FUpdate[0] 

            if (!FInput.IsChanged) {

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
                }
                FOutput.AssignFrom(FInput);
                FOutput.Flush();
            }
        }
    }
}
