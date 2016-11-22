using System;
using System.Linq;
using System.Globalization;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils;
using VVVV.DX11;
using System.ComponentModel.Composition;
using FeralTic.DX11;

namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "Read", AutoEvaluate = true, Category = "Message", Help = "Reads spreadable Fields of arbitrary Type from all incoming Messages. Can convert some types.", Tags = "Typeable, Field", Author = "velcrome")]
    public class MessageReadNode : TypeablePinNode, IDX11ResourceDataRetriever
    {
        [Output("Message Bin Size", AutoFlush = false, Order = 4)]
        protected ISpread<int> FBinSize;

        [Import()]
        protected IPluginHost FHost;

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
            InitDX11Graph();

            if (!FInput.IsChanged && !FConfig.IsChanged && !FKey.IsChanged) return;

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
                    var output = (input[i*keyCount + index] as ISpread);

                    output.SliceCount = 0;
                    if (message.Fields.Contains(key))
                    {
                        var inputBin = message[key];
                        if (TargetDynamicType.IsAssignableFrom(inputBin.GetInnerType()))
                        {
                            output.SliceCount = inputBin.Count;
                            for (int j = 0; j < inputBin.Count; j++)
                                output[j] = inputBin[j];
                        }
                        else // will throw Exception, if Conversion is not possible
                        {
                            output.SliceCount = inputBin.Count;
                            for (int j = 0; j < inputBin.Count; j++)
                                output[j] = Convert.ChangeType(inputBin[j], TargetDynamicType, CultureInfo.InvariantCulture);
                        }
                    }
                    count += output.SliceCount; 
                    index ++; // next field within Message
                }
                FBinSize[i] = count;
            }

            input.Flush();
            FBinSize.Flush();
        }

        #region dx11
        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;

        protected void InitDX11Graph()
        {
            if (this.RenderRequest != null) { RenderRequest(this, this.FHost); }
        }
        #endregion dx11

    }

}
