using System;
using System.Linq;
using System.Globalization;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "Read", AutoEvaluate = true, Category = "Message", Help = "Reads one attribute of arbitrary Type", Version="Dynamic", Tags = "Formular, Bin", Author = "velcrome")]
    public class MessageReadNode : TypeablePinNode
    {
        [Input("AvoidNil", IsSingle = true, IsToggle = true, DefaultBoolean = true, Order = 3)]
        protected IDiffSpread<bool> FAvoidNil;

        [Output("Message Bin Size", AutoFlush = false, Order = 4)]
        protected ISpread<int> FBinSize;



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

            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            if (SpreadMax == 0)
            {
                if (FOutput.SliceCount > 0) // zero inputs -> zero outputs.
                {
                    FOutput.FlushNil();

                    FBinSize.SliceCount = 1;
                    FBinSize[0] = 0;
                    FBinSize.Flush();

                    FValue.ToISpread().FlushNil();

                    return;
                }
                else return; // already zero'ed
            }

            FOutput.FlushResult(FInput);

            var keyCount = FKey.SliceCount;
            FBinSize.SliceCount = SpreadMax;

            var input = FValue.ToISpread();
            input.SliceCount = SpreadMax * keyCount;

            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];
 
                var count = 0;
                var index = 0;
                foreach (var key in FKey)
                {
                    var type = TargetDynamicType;

                    var output = (input[i*keyCount + index] as ISpread);
                    output.SliceCount = 0;

                    if (!message.Fields.Contains(key))
                    {
                        if (!FAvoidNil.IsAnyInvalid() && FAvoidNil[0])
                        {
                            output.SliceCount = 1;
                            output[0] = TypeIdentity.Instance.NewDefault(type);
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
                        else // will throw Exception, if Conversion is not possible
                        {
                            
                            output.SliceCount = inputBin.Count;
                            for (int j = 0; j < inputBin.Count; j++)
                                output[j] = Convert.ChangeType(inputBin[j], type, CultureInfo.InvariantCulture);
                        }

                    }
                    count += output.SliceCount;
                    index ++;
                        
                }
                FBinSize[i] = count;
            }

            input.Flush();
            FBinSize.Flush();
        }

    }

}
