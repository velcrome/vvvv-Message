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
    [PluginInfo(Name = "Write", AutoEvaluate = true, Category = "Message", Help = "Writes into Fields of arbitrary Type", Version = "Dynamic", Tags = "Formular, Bin", Author = "velcrome")]
    public class MessageWriteNode : TypeablePinNode, IDX11ResourceDataRetriever
    {
        [Input("AutoSense", Order = int.MaxValue - 1, IsSingle = true, IsToggle = true, DefaultBoolean = true, Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<bool> FAutoSense;

        [Input("Update", IsToggle = true, Order = int.MaxValue, DefaultBoolean = true)]
        protected IDiffSpread<bool> FUpdate;

        [Import()]
        protected IPluginHost FHost;

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;
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
            if (this.RenderRequest != null) { RenderRequest(this, this.FHost); }

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
            newData |= !FAutoSense[0]; // assume newData, if AutoSense is off.

            if (anyUpdate && newData) {
                SpreadMax = FInput.SliceCount;
                doFlush = true;

                for (int i = 0; i < SpreadMax; i++)
                {
                    Message message = FInput[i];
                       
                    var keyIndex = 0;
                    foreach (var key in FKey)
                    {
                        var fieldIndex = i * keyCount + keyIndex;
                        keyIndex++;

                        if (!FUpdate[fieldIndex]) continue;

                        if (!message.Fields.Contains(key)) message[key] = BinFactory.New(TargetDynamicType);
                        var value = ValueSpread[fieldIndex] as ISpread;

                        if (value.SliceCount > 0)
                        {
                            if (message[key].GetInnerType().IsAssignableFrom(TargetDynamicType))
                            {
                                // check if any relevant change occurred
                                if (!message[key].Equals(value as IEnumerable)) message.AssignFrom(key, value);
                            }
                            else
                            {
                                var casted = from slice in value as IEnumerable<object>
                                             let targetType = message[key].GetInnerType()
                                             select Convert.ChangeType(slice, targetType);

                                if (!message[key].Equals(casted)) message.AssignFrom(key, casted);
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
