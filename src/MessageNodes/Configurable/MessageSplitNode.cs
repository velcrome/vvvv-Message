#region usings
using System;
using System.Collections;
using VVVV.Packs.Messaging.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils;

#endregion usings

namespace VVVV.Packs.Messaging.Nodes
{


    #region PluginInfo

    [PluginInfo(Name = "Split", AutoEvaluate = true, Category = "Message", Version = "Formular",
        Help = "Splits a Message into custom dynamic pins", Tags = "Formular, Bin", Author = "velcrome")]

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
        [Input("Input", Order = 0)]
        IDiffSpread<Message> FInput;

        [Input("Hold if Nil", IsSingle = true, DefaultEnumEntry = "Message", Order = 3)]
        ISpread<PinHoldEnum> FHold;

        [Output("Address", AutoFlush = false)]
        ISpread<string> FAddress;

        [Output("Timestamp", AutoFlush = false)]
        ISpread<Time.Time> FTimeStamp;

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
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;

            if (SpreadMax <= 0 && (FHold[0] == PinHoldEnum.Off))
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

            FTimeStamp.SliceCount = SpreadMax;
            FAddress.SliceCount = SpreadMax;

            foreach (string pinName in FPins.Keys)
            {
                ToISpread(FPins[pinName]).SliceCount = SpreadMax;
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];
                FAddress[i] = message.Address;
                FTimeStamp[i] = message.TimeStamp;

                foreach (string name in FPins.Keys)
                {
                    var targetPin = ToISpread(FPins[name]);
                    var targetBin = targetPin[i] as ISpread;

                    Bin sourceBin = message[name];
                    int count = 0;

                    if (sourceBin as object == null)
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

            FTimeStamp.Flush();
            FAddress.Flush();

            foreach (string pinName in FPins.Keys)
            {
                ToISpread(FPins[pinName]).Flush();
            }

        }

    }
}