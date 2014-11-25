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
    [PluginInfo(Name = "Message", AutoEvaluate=true, Category = "Split", Help = "Splits a Message into custom dynamic pins", Tags = "Dynamic, Bin, velcrome")]
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
        [Input("Input")]
        IDiffSpread<Message> FInput;

        [Input("Match Rule", DefaultEnumEntry = "All", IsSingle = true)]
        IDiffSpread<SelectEnum> FSelect;

        [Input("Hold if Nil", IsSingle = true, DefaultEnumEntry = "Message")]
        ISpread<HoldEnum> FHold; 

        [Output("Address", AutoFlush = false)]
        ISpread<string> FAddress;

        [Output("Timestamp", AutoFlush = false)]
        ISpread<string> FTimeStamp;

        #pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type, int binSize = -1)
        {
            var attr = new OutputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.AutoFlush = false;

            attr.Order = FCount;
            attr.BinOrder = FCount + 1;
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
                        var bin = (VVVV.PluginInterfaces.V2.NonGeneric.ISpread)ToISpread(FPins[name])[i];

                        Bin attrib = message[name];
                        int count = 0;

                        if (attrib == null)
                        {
                            if (FVerbose[0]) FLogger.Log(LogType.Debug, "\"" + FTypes[name] + " " + name + "\" is not defined in Message.");
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


        #region PluginInfo
        [PluginInfo(Name = "SetMessage", Category = "Message", Help = "Adds or edits a Message Property", Tags = "Dynamic, Bin, velcrome")]
        #endregion PluginInfo
        public class SetMessagePinsNode : DynamicPinsNode
        {
            #pragma warning disable 649, 169
            [Input("Input")]
            IDiffSpread<Message> FInput;

            [Output("Output", AutoFlush = false)]
            Pin<Message> FOutput;
            #pragma warning restore

            protected override IOAttribute DefinePin(string name, Type type, int binSize = -1)
            {
                var attr = new InputAttribute(name);
                attr.BinVisibility = PinVisibility.Hidden;
                attr.BinSize = binSize;
                attr.Order = FCount;
                attr.BinOrder = FCount + 1;

                return attr;
            }


            public override void Evaluate(int SpreadMax)
            {
                SpreadMax = FInput.SliceCount;

                if (!FInput.IsChanged)
                {
                    //				FLogger.Log(LogType.Debug, "skip set");
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
                        var pin = (IEnumerable)ToISpread(FPins[name])[i];
                        message.AssignFrom(name, pin);
                    }
                }

                FOutput.AssignFrom(FInput);
                FOutput.Flush();
            }
        }

	}
   
}