using System;
using System.Collections;
using System.ComponentModel.Composition;
using VVVV.Packs.Message.Core;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Packs.Message.Nodes
{
    using Message = VVVV.Packs.Message.Core.Message;

    [PluginInfo(Name = "Edit", AutoEvaluate = true, Category = "Message", Help = "Updates one attribute of arbitrary Type", Tags = "Dynamic", Author = "velcrome")]
    public class MessageEditNode : DynamicPinNode
    {
        #pragma warning disable 649, 169


        [Input("Update", IsBang = true, Order = 5)]
        IDiffSpread<bool> FUpdate;
        #pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type, int binSize = -1)
        {
            var attr = new InputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = binSize;
            attr.Order = 3;
            attr.BinOrder = 4;
            attr.AutoValidate = false;  // need to sync all pins manually

            return attr;
        }


        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.SliceCount;

            if (FInput.SliceCount == 0 || FInput[0] == null)
            {
                FOutput.SliceCount = 0;
            	FOutput.Flush();
                return;
            }

            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = FInput[i];

                bool update = false;
                for (int j = 0; j < FUpdate.SliceCount; j++) update = update || FUpdate[j];
                
                if (update) ((ISpread)FValue.RawIOObject).Sync();
                if (FUpdate[i]) message.AssignFrom(FKey[0], ToISpread(FValue)[i]);

                FOutput[i] = message;
            }
            FOutput.Flush();  // sync
        }

    }

}
