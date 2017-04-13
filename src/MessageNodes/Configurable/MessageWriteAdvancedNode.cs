using FeralTic.DX11;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.DX11;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils;


namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "Write", AutoEvaluate = true, Category = "Message", Help = "Writes into Fields of arbitrary Type", Version = "Advanced", Tags = "Formular, Bin", Author = "velcrome")]
    public class MessageWriteAdvancedNode : TypeablePinNode, IDX11ResourceDataRetriever
    {
        [Input("Message Field Count", Order = int.MaxValue -2, DefaultValue = 1.0, MinValue = 0)]
        protected IDiffSpread<int> FFieldCount;

        [Input("Force", Order = int.MaxValue - 1, IsSingle = true, IsToggle = true, DefaultBoolean = true, Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<bool> FForce;

        [Input("Update", IsToggle = true, Order = int.MaxValue, DefaultBoolean = true)]
        protected IDiffSpread<bool> FUpdate;

        protected override IOAttribute DefinePin(FormularFieldDescriptor field)
        {
            var attr = new InputAttribute("Field");
            attr.BinVisibility = PinVisibility.Hidden;

            attr.Order = 4;
            attr.BinOrder = 5;
            attr.BinSize = 1;

            attr.CheckIfChanged = true;

            return attr;
        }


        public override void Evaluate(int SpreadMax)
        {
            InitDX11Graph();

            if (FInput.IsAnyInvalid())
            {
                FOutput.FlushNil();
                return; // if no input, no further calculation.
            }

            bool doFlush = false;

            // any of the update slices is true -> update the plugin.
            var anyUpdate = FUpdate.Any();

            // Flush upstream changes through the plugin
            if (FInput.IsChanged)
            {
                FOutput.SliceCount = 0;
                FOutput.AssignFrom(FInput);
                doFlush = true;
            }
            else if (!anyUpdate) return;

            var keyCount = FKey.SliceCount;
            var ValueSpread = FValue.ToISpread();

            bool newData = ValueSpread.IsChanged; // changed pin
            newData |= FForce[0]; // assume newData, if AutoSense is off.

            if (anyUpdate && newData)
            {
                SpreadMax = FInput.SliceCount;
                doFlush = true;

                var fieldIndex = -1;
                for (int i = 0; i < SpreadMax; i++) // iterate all messages
                {
                    Message message = FInput[i];
                    var fieldCount = FFieldCount[i];
                    if (!FUpdate[i]) continue;

                    for (int j = 0; j < fieldCount; j++) // iterate all relevant fields for each message
                    {
                        fieldIndex++;
                        var key = FKey[fieldIndex];

                        if (!message.Fields.Contains(key)) message[key] = BinFactory.New(TargetDynamicType);
                        var spread = ValueSpread[fieldIndex] as ISpread;

                        if (spread.SliceCount > 0)
                        {
                            if (message[key].GetInnerType().IsAssignableFrom(TargetDynamicType))
                            {
                                // check if any relevant change occurred
                                if (!message[key].Equals(spread as IEnumerable)) message.AssignFrom(key, spread);
                            }
                            else
                            {
                                var targetType = message[key].GetInnerType();
                                var newBin = BinFactory.New(targetType, spread.SliceCount);

                                for (int k = 0; k < spread.SliceCount; k++) // iterate all elements in a field
                                {
                                    newBin[k] = Convert.ChangeType(spread[k], targetType);
                                }

                                if (!message[key].Equals(newBin)) message.AssignFrom(key, newBin);
                            }
                        }
                        else message[key].Clear();
                    }
                }
            }

            if (doFlush)
            {
                FOutput.Flush();
            }
        }

    }

}
