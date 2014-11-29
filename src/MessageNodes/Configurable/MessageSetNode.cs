using System;
using System.Collections;
using VVVV.Packs.Messaging.Nodes;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Packs.Time;

namespace VVVV.Nodes.Messaging.Nodes
{
 
    #region PluginInfo
    [PluginInfo(Name = "Edit", Category = "Message", Help = "Adds or edits multiple Message Fields", Tags = "Typeable")]
    #endregion PluginInfo
    public class MessageSetNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Input", Order = 0)] 
        IDiffSpread<Message> FInput;

        [Input("Update Timestamp", IsToggle = true, Order = int.MaxValue - 1, Visibility = PinVisibility.OnlyInspector, IsSingle = true, DefaultBoolean = true)]
        IDiffSpread<bool> FUpdateTimestamp;

        [Input("Update", IsBang = true, Order = int.MaxValue, DefaultBoolean = true)]
        IDiffSpread<bool> FUpdate;

        [Output("Output", AutoFlush = false)] private Pin<Message> FOutput;
#pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type, int binSize = -1)
        {
            var attr = new InputAttribute(name);
            attr.AutoValidate = false;  // need to sync all pins manually. Don't forget to Flush()

            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = binSize;
            
            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount+1;
            
            return attr;
        }


        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 :FInput.SliceCount;

            if (SpreadMax <= 0)
                if (FOutput.SliceCount == 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                    return;
                }
                else return;


            var updateTimestamp = FUpdateTimestamp[0];
            bool update = false;

            for (int j = 0; j < FUpdate.SliceCount && j < SpreadMax; j++)
                if (FUpdate[j])
                {
                    update = true;
                    break;
                }

            // sync pins only if necessary. might cause confusion in the gui, when a link's input and output don't match 
            if (update) foreach (var pinName in FPins.Keys)
                {
                    ToISpread(FPins[pinName]).Sync();
                }

            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];


                if (FUpdate[i])
                {
                    foreach (var pinName in FPins.Keys)
                    {
                        var pin = ToISpread(FPins[pinName]);
                        message.AssignFrom(pinName, pin[i] as IEnumerable);
                    }
                    if (updateTimestamp) message.TimeStamp = Time.CurrentTime();
                }
                FOutput[i] = message;
            }

            FOutput.Flush();  // sync manually

        }
    }
}
