using System;
using System.Linq;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils;


namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "Read", AutoEvaluate = true, Category = "Message", Help = "Reads one attribute of arbitrary Type", Version="Dynamic", Tags = "Formular, Bin", Author = "velcrome")]
    public class MessageReadNode : DynamicPinNode
    {

        protected override IOAttribute DefinePin(FormularFieldDescriptor field)
        {
            var attr = new OutputAttribute("Field");
            attr.BinVisibility = PinVisibility.Hidden;
            attr.Order = 1;
            attr.BinOrder = 2;
            attr.AutoFlush = false;  // need to sync all pins manually

            return attr;
        }


        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;

            if (SpreadMax <= 0)
                if (FOutput.SliceCount == 0 || FOutput[0] == null)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                    return;
                }
                else return;

            if (!FInput.IsChanged) return;

            FOutput.SliceCount = SpreadMax;

            var Value = (ISpread)(FValue.RawIOObject);
            Value.SliceCount = SpreadMax;
            
            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];
                var spread = Value[i] as ISpread;
                spread.SliceCount = FKey.SliceCount;
 
                var index = 0;
                foreach (var key in FKey)
                {
                    var type = TargetDynamicType;

                    object input;
                    if (!message.Fields.Contains(key)) 
                        input = TypeIdentity.Instance.Default(type);
                    else
                    {
                        input = message[key].First; // automatically returns a default if not existing
                    }

                    if (input.GetType().IsCastableTo(type, true))
                        input = Convert.ChangeType(input, type);
                    else input = TypeIdentity.Instance.Default(type);

                    spread[index] = input;
                    index++;
                }

                FOutput[i] = message;
            
            }
            Value.Flush();
            FOutput.Flush();
        }

    }

}
