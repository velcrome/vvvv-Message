using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils;

using VVVV.DX11;

namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "Write", AutoEvaluate = true, Category = "Message", Help = "Writes into Fields of arbitrary Type. ", Version = "Dynamic", Tags = "Formular, Bin", Author = "velcrome")]
    public class MessageWriteNode : TypeablePinNode, IDX11ResourceDataRetriever
    {
        [Input("Key", DefaultString = "Foo", Order = 2, BinOrder = 3, BinSize = -1, BinVisibility = PinVisibility.OnlyInspector)]
        public new ISpread<ISpread<string>> FKey;

        [Input("Force", Order = int.MaxValue - 1, IsSingle = true, IsToggle = true, DefaultBoolean = true, Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<bool> FForce;

        [Input("Update", IsToggle = true, Order = int.MaxValue, DefaultBoolean = true)]
        protected IDiffSpread<bool> FUpdate;

        [Output("Iterations", Order = int.MaxValue)]
        protected ISpread<int> FIterations;

        protected override IOAttribute DefinePin(FormularFieldDescriptor field)
        {
            var attr = new InputAttribute("Field");
            attr.BinVisibility = PinVisibility.Hidden;

            attr.Order = 4;
            attr.BinOrder = 5;
            attr.BinSize = 1;

            attr.BinVisibility = PinVisibility.Hidden;

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

            var dataSpread = FValue.ToISpread();

            bool newData = dataSpread.IsChanged; // changed pin
            newData |= FForce[0]; // assume newData, if AutoSense is off.

            if (anyUpdate && newData)
            {
                // Update and Force not included in spreading.
                SpreadMax = dataSpread.SliceCount.CombineWith(FKey).CombineWith(FInput);
                if (FUpdate.IsAnyInvalid() || FForce.IsAnyInvalid()) SpreadMax = 0;

                FIterations[0] = SpreadMax;

                doFlush = true;

                var fieldIndex = 0;
                for (int i = 0; i < SpreadMax; i++) // iterate spreaded (except update, which gets cut short)
                {
                    Message message = FInput[i];
                    
                    for (int j = 0; j < FKey[i].SliceCount; j++, fieldIndex++) // iterate all relevant fields for each message
                    {
                        if (!FUpdate[fieldIndex]) continue; // update is per-field

                        var key = FKey[i][j];
                        var spread = dataSpread[fieldIndex] as ISpread;

                        LazyInsert(message, key, spread);
                    }
                }
            }

            if (doFlush)
            {
                FOutput.Flush();
            }
        }

        private void LazyInsert(Message message, string fieldName, ISpread spread)
        {
            if (!message.Fields.Contains(fieldName)) message[fieldName] = BinFactory.New(TargetDynamicType);

            if (spread.SliceCount > 0)
            {
                if (message[fieldName].GetInnerType().IsAssignableFrom(TargetDynamicType))
                {
                    // check if any relevant change occurred
                    if (!message[fieldName].Equals(spread as IEnumerable)) message.AssignFrom(fieldName, spread);
                }
                else
                {
                    var targetType = message[fieldName].GetInnerType();
                    var newBin = BinFactory.New(targetType, spread.SliceCount);

                    for (int k = 0; k < spread.SliceCount; k++) // iterate all elements in a field
                    {
                        newBin[k] = Convert.ChangeType(spread[k], targetType);
                    }

                    if (!message[fieldName].Equals(newBin)) message.AssignFrom(fieldName, newBin);
                }
            }
            else message[fieldName].Clear();
        }
    }

}
