#region usings
using System;
using System.Collections;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2.NonGeneric;

#endregion usings

namespace VVVV.Packs.Messaging.Nodes
{


    #region PluginInfo

    [PluginInfo(Name = "Message", AutoEvaluate = true, Category = "Split",
        Help = "Splits a Message into custom dynamic pins", Tags = "Dynamic, Bin, velcrome")]

    #endregion PluginInfo

    public class MessageSplitNode : DynamicPinsNode
    {
        public enum PinHoldEnum
        {
            Off,
            Message,
            Pin
        }


#pragma warning disable 649, 169
        [Input("Input", Order = 0)] private IDiffSpread<Message> FInput;

        [Input("Match Rule", DefaultEnumEntry = "All", IsSingle = true, Order = 2)] private IDiffSpread<HoldEnum>
            FSelect;

        [Input("Hold if Nil", IsSingle = true, DefaultEnumEntry = "Message", Order = 3)] private ISpread<PinHoldEnum> FHold;

        [Output("Address", AutoFlush = false)] private ISpread<string> FAddress;

        [Output("Timestamp", AutoFlush = false)] private ISpread<Time.Time> FTimeStamp;

#pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type, int binSize = -1)
        {
            var attr = new OutputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.AutoFlush = false;

            attr.Order = DynPinCount;
            attr.BinOrder = DynPinCount + 1;
            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = (FSelect[0] != HoldEnum.First) ? FInput.SliceCount : 1;

            if (!FInput.IsChanged)
            {
                return;
            }

            bool empty = (FInput.SliceCount == 0) || (FInput[0] == null);

            if (empty && (FHold[0] == PinHoldEnum.Off))
            {
                foreach (string name in FPins.Keys)
                {
                    var pin = ToISpread(FPins[name]);
                    pin.SliceCount = 0;
                    pin.Flush();
                }
                FAddress.SliceCount = 0;
                FTimeStamp.SliceCount = 0;

                FAddress.Flush();
                FTimeStamp.Flush();

                return;
            }

            if (!empty)
            {
                foreach (string pinName in FPins.Keys)
                {
                    if (FSelect[0] == HoldEnum.All)
                    {
                        ToISpread(FPins[pinName]).SliceCount = SpreadMax;
                        FTimeStamp.SliceCount = SpreadMax;
                        FAddress.SliceCount = SpreadMax;
                    }
                    else
                    {
                        ToISpread(FPins[pinName]).SliceCount = 1;
                        FTimeStamp.SliceCount = 1;
                        FAddress.SliceCount = 1;
                    }
                }

                for (int i = (FSelect[0] == HoldEnum.Last) ? SpreadMax - 1 : 0; i < SpreadMax; i++)
                {
                    Message message = FInput[i];

                    FAddress[i] = message.Address;
                    FAddress.Flush();

                    FTimeStamp[i] = message.TimeStamp;
                    FTimeStamp.Flush();

                    foreach (string name in FPins.Keys)
                    {
                        var targetPin = ToISpread(FPins[name]);
                        var targetBin = targetPin[i] as ISpread;
                        
                        Bin sourceBin = message[name];
                        int count = 0;

                        if (sourceBin == null)
                        {
                            if (FDevMode[0])
                                FLogger.Log(LogType.Debug,
                                            "\"" + FTypes[name] + " " + name + "\" is not defined in Input Message.");
                        }
                        else count = sourceBin.Count;

                        if ((count > 0) || (FHold[0] != PinHoldEnum.Pin))
                        {
                            targetBin.SliceCount = count;
                            for (int j = 0; j < count; j++)
                            {
                                targetBin[j] = sourceBin[j];
                            }
                            targetPin.Flush();
                        }
                        else
                        {
                            // keep old values in pin. do not flush
                        }

                    }
                }
            }
        }

    }
}