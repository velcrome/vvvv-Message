using System;
using VVVV.Packs.Message.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Packs.Message.Nodes
{
    using Message = VVVV.Packs.Message.Core.Message;

    [PluginInfo(Name = "Read", AutoEvaluate = true, Category = "Message", Help = "Reads one attribute of arbitrary Type", Tags = "Dynamic", Author = "velcrome")]
    public class MessageReadNode : DynamicPinNode
    {

        protected override IOAttribute DefinePin(string name, Type type, int binSize = -1)
        {
            var attr = new OutputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.Order = 1;
            attr.BinOrder = 2;
            attr.AutoFlush = false;  // need to sync all pins manually

            return attr;
        }


        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.SliceCount;
            var output = ((ISpread) (FValue.RawIOObject));

            if (FInput.SliceCount == 0 || FInput[0] == null)
            {
                FOutput.SliceCount = output.SliceCount = 0;
                FOutput.Flush();
                output.Flush();
                return;
            }

            FOutput.SliceCount = SpreadMax;
            output.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];
                var spread = (ISpread) output[i];
                var bin = message[FKey[0]];

                try
                {
                    spread.SliceCount = bin.Count;

                    var type = TypeIdentity.Instance.FindType(FAlias[0].Name);

                    for (int j = 0; j < bin.Count; j++)
                    {
                        spread[j] = Convert.ChangeType(bin[j], type);
                    }
                } catch (Exception)
                {
                    spread.SliceCount = 0;
                }
                FOutput[i] = message;
            }
            FOutput.Flush();
            output.Flush();
        }

    }

}
