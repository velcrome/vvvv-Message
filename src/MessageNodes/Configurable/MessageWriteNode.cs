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
    [PluginInfo(Name = "Write", AutoEvaluate = true, Category = "Message", Help = "Writes into Fields of arbitrary Type", Version = "Dynamic", Tags = "Formular, Bin", Author = "velcrome")]
    public class MessageWriteNode : DynamicPinNode
    {

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
            if (!FInput.IsChanged && !FConfig.IsChanged && !FKey.IsChanged && !FValue.ToISpread().IsChanged) return;

            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            if (SpreadMax == 0)
            {
                if (FOutput.SliceCount > 0) // zero inputs -> zero outputs.
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                    return;
                }
                else return; // already zero'ed
            }

            FOutput.SliceCount = 0;
            FOutput.AssignFrom(FInput);
            FOutput.Flush();

            var keyCount = FKey.SliceCount;
            var Value = FValue.ToISpread();

            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];


                var index = 0;
                foreach (var key in FKey)
                {
                    var input = (Value[i * keyCount + index] as ISpread);

                    if (!message.Fields.Contains(key)) message[key] = BinFactory.New(TargetDynamicType);

                    if (input.SliceCount > 0) {
                        if (message[key].GetInnerType().IsAssignableFrom(TargetDynamicType))
                        {
                            if (!message[key].Equals(input as IEnumerable)) message.AssignFrom(key, input);
                        }
                            
                        else
                        {
                            IList casted = new ArrayList();

                            foreach (var slice in input)
                                casted.Add(Convert.ChangeType(slice, message[key].GetInnerType()));

                            if (!message[key].Equals(casted as IEnumerable)) message.AssignFrom(key, casted);

                        }

                    }
                    index++;

                }
            }
            Value.Flush();
        }

    }

}
