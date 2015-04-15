using System;
using System.Collections;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils;
using System.Linq;
using System.Globalization;


namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "Read", AutoEvaluate = true, Category = "Message", Help = "Reads one attribute of arbitrary Type", Version="Dynamic", Tags = "Formular, Bin", Author = "velcrome")]
    public class MessageReadNode : DynamicPinNode
    {
        [Input("AvoidNil", IsSingle = true, IsToggle = true, DefaultBoolean = true, Order = 3)]
        protected ISpread<bool> FAvoidNil;

        protected override IOAttribute DefinePin(FormularFieldDescriptor field)
        {
            var attr = new OutputAttribute("Field");
            attr.BinVisibility = PinVisibility.Hidden;
            attr.Order = 1;
            attr.BinOrder = 3;
            attr.AutoFlush = false;  // need to sync all pins manually

            return attr;
        }


        public override void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && !FConfig.IsChanged && !FKey.IsChanged && !FAvoidNil.IsChanged) return;

            var binSpread = FBinSize.ToGenericISpread<int>();

            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            if (SpreadMax == 0)
            {
                if (FOutput.SliceCount > 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();

                    binSpread.SliceCount = 1;
                    binSpread[0] = 0;
                    binSpread.Flush();

                    var output = (ISpread)(FValue.RawIOObject);
                    output.SliceCount = 0;
                    output.Flush();

                    return;
                }
                else return;
            }

            FOutput.SliceCount = 0;
            FOutput.AssignFrom(FInput);
            FOutput.Flush();

            var keyCount = FKey.SliceCount;
            binSpread.SliceCount = SpreadMax;

            var Value = FValue.ToISpread();
            Value.SliceCount = SpreadMax * keyCount;

            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];
 
                var count = 0;
                var index = 0;
                foreach (var key in FKey)
                {
                    var type = TargetDynamicType;

                    var output = (Value[i*keyCount + index] as ISpread);
                    output.SliceCount = 0;

                    if (!message.Fields.Contains(key))
                    {
                        if (!FAvoidNil.IsAnyInvalid() && FAvoidNil[0])
                        {
                            output.SliceCount = 1;
                            output[0] = TypeIdentity.Instance.Default(type);
                        }
                    }
                    else
                    {
                        var inputBin = message[key];

                        if (type.IsAssignableFrom(inputBin.GetInnerType()))
                        {
                            output.SliceCount = inputBin.Count;
                            for (int j = 0; j < inputBin.Count; j++)
                                output[j] = inputBin[j];
                        }
                        else //if (inputBin.GetInnerType().IsCastableTo(type, true))
                        {
                            
                            output.SliceCount = inputBin.Count;
                            for (int j = 0; j < inputBin.Count; j++)
                                output[j] = Convert.ChangeType(inputBin[j], type, CultureInfo.InvariantCulture);
                        }
                        //else
                        //{
                        //    // illegal operation. fail silently.
                        //}
                    }
                    count += output.SliceCount;
                    index ++;
                        
                }
                binSpread[i] = count;
            }

            Value.Flush();
            binSpread.Flush();
        }

    }

}
