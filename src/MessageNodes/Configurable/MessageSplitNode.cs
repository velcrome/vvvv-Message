#region usings
using System;
using System.Collections;
using VVVV.Packs.Message.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Packs.Message.Nodes {
    using Message = VVVV.Packs.Message.Core.Message;

    #region Enum
    public enum SelectEnum
    {
        All,
        First,
        Last
    }
    #endregion Enum

    #region PluginInfo

    [PluginInfo(Name = "Message", AutoEvaluate = true, Category = "Split",
        Help = "Splits a Message into custom dynamic pins", Tags = "Dynamic, Bin, velcrome")]

    #endregion PluginInfo

    public class MessageSplitNode : DynamicPinsNode
    {
        public enum HoldEnum
        {
            Off,
            Message,
            Pin
        }

#pragma warning disable 649, 169
        [Input("Input", Order = 0)] private IDiffSpread<Message> FInput;

        [Input("Match Rule", DefaultEnumEntry = "All", IsSingle = true, Order = 2)] private IDiffSpread<SelectEnum>
            FSelect;

        [Input("Hold if Nil", IsSingle = true, DefaultEnumEntry = "Message", Order = 3)] private ISpread<HoldEnum> FHold;

        [Output("Address", AutoFlush = false)] private ISpread<string> FAddress;

        [Output("Timestamp", AutoFlush = false)] private ISpread<string> FTimeStamp;

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
            SpreadMax = (FSelect[0] != SelectEnum.First) ? FInput.SliceCount : 1;

            if (!FInput.IsChanged)
            {
                return;
            }

            bool empty = (FInput.SliceCount == 0) || (FInput[0] == null);

            if (empty && (FHold[0] == HoldEnum.Off))
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
                    if (FSelect[0] == SelectEnum.All)
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

                for (int i = (FSelect[0] == SelectEnum.Last) ? SpreadMax - 1 : 0; i < SpreadMax; i++)
                {
                    Message message = FInput[i];

                    FAddress[i] = message.Address;
                    FTimeStamp[i] = message.TimeStamp.ToString();
                    FAddress.Flush();
                    FTimeStamp.Flush();

                    foreach (string name in FPins.Keys)
                    {
                        var bin = (VVVV.PluginInterfaces.V2.NonGeneric.ISpread) ToISpread(FPins[name])[i];

                        Bin attrib = message[name];
                        int count = 0;

                        if (attrib == null)
                        {
                            if (FVerbose[0])
                                FLogger.Log(LogType.Debug,
                                            "\"" + FTypes[name] + " " + name + "\" is not defined in Message.");
                        }
                        else count = attrib.Count;

                        if ((count > 0) || (FHold[0] != HoldEnum.Pin))
                        {
                            bin.SliceCount = count;
                            for (int j = 0; j < count; j++)
                            {
                                bin[j] = attrib[j];
                            }
                            ToISpread(FPins[name]).Flush();
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