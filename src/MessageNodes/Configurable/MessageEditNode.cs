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
    [PluginInfo(Name = "Edit", Category = "Message", Help = "Adds or edits multiple Message Fields", Version = "Formular", Tags = "Formular, Bin")]
    #endregion PluginInfo
    public class MessageEditNode : DynamicPinsNode
    {
#pragma warning disable 649, 169
        [Input("Input", Order = 0)] 
        IDiffSpread<Message> FInput;

        [Input("Update Timestamp", IsToggle = true, Order = int.MaxValue - 1, Visibility = PinVisibility.OnlyInspector, IsSingle = true, DefaultBoolean = true)]
        IDiffSpread<bool> FUpdateTimestamp;

        [Input("Update", IsBang = true, Order = int.MaxValue, DefaultBoolean = true)]
        IDiffSpread<bool> FUpdate;

        [Output("Output", AutoFlush = false)] 
        Pin<Message> FOutput;

#pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type, int binSize = -1)
        {
            var attr = new InputAttribute(name);

            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = binSize;

            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount+1;

 //         Manual Sync seems a good idea, but had some glitches, because binsize for Color and string will only be updated, if actual input had changed.  
 //           attr.AutoValidate = false;  // need to sync all pins manually. Don't forget to Sync()

            return attr;
        }


        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 :FInput.SliceCount;

            if (SpreadMax <= 0)
                if (FOutput.SliceCount == 0 || FOutput[0]== null)
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

            // sync pins only if necessary. might cause confusion in the gui, when a link's input and output don't visually match 
            // 
            //if (update) foreach (var pinName in FPins.Keys)
            //    {
            //        ToISpread(FPins[pinName]).Sync();
            //    }

            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];


                if (FUpdate[i])
                {
                    foreach (var pinName in FPins.Keys)
                    {
                        var pin = ToISpread(FPins[pinName]);
                        var spread = pin[i] as VVVV.PluginInterfaces.V2.NonGeneric.ISpread;
                        message.AssignFrom(pinName, VVVV.Utils.SpreadUtils.ToEnumerable(spread));
                    }
                    if (updateTimestamp) message.TimeStamp = Time.CurrentTime();
                }
                FOutput[i] = message;
            }

            FOutput.Flush();  // sync manually

        }
    }
}
