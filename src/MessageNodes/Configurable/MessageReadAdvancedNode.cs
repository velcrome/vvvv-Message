using System;
using System.Linq;
using System.Globalization;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Utils;
using System.Collections.Generic;
using VVVV.DX11;
using System.ComponentModel.Composition;
using FeralTic.DX11;

namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "Read", 
        AutoEvaluate = true, 
        Category = "Message", 
        Version = "Advanced", 
        Help = "Reads spreadable Fields of arbitrary Type from all incoming Messages. Can convert some types. Features fully spreaded AvoidNil capabilities, and a handy Swapdim Flip", 
        Tags = "Formular, Bin", 
        Author = "velcrome")]
    public class MessageReadAdvancedNode : TypeablePinNode, IDX11ResourceHost
    {
        protected IIOContainer FAvoidNil;

        [Input("Enable AvoidNil", IsToggle = true, IsSingle = true, Order = 8)]
        protected IDiffSpread<bool> FAvoidNilEnable;

        [Input("Swap Dim", IsToggle = true, IsSingle = true, Order = 5)]
        protected ISpread<bool> FSwapDim;

        [Output("Message Bin Size", AutoFlush = false, Order = 4)]
        protected ISpread<int> FBinSize;

        [Import()]
        protected IPluginHost FHost;

        protected override void OnConfigChange(IDiffSpread<string> configSpread)
        {
            base.OnConfigChange(configSpread);

            var attr = new InputAttribute("AvoidNil");
            attr.BinVisibility = PinVisibility.OnlyInspector;
            attr.Order = 6;
            attr.BinOrder = 7;
            attr.BinSize = 1;
            attr.CheckIfChanged = true;

            Type pinType = typeof(ISpread<>).MakeGenericType((typeof(ISpread<>)).MakeGenericType(TargetDynamicType)); // the Pin is always a binsized one

            if (FAvoidNil != null) FAvoidNil.Dispose();
            FAvoidNil = FIOFactory.CreateIOContainer(pinType, attr);
        }

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
            InitDX11Graph();

            #region early break
            if (    
                    !FInput.IsChanged 
                 && !FConfig.IsChanged 
                 && !FKey.IsChanged 
                 && !FAvoidNil.ToISpread().IsChanged
                 && !FAvoidNilEnable.IsChanged 
                 && !FSwapDim.IsChanged
               ) return;

            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            if (SpreadMax == 0)
            {
                if (!FAvoidNilEnable.IsAnyInvalid() && FAvoidNilEnable[0])
                {
                    var force = FAvoidNil.ToISpread().IsChanged || FAvoidNilEnable.IsChanged;
                    if (force || FOutput.SliceCount > 0) // zero inputs -> zero outputs.
                    {
                        FOutput.FlushNil();

                        // set a default
                        FBinSize.SliceCount = 1;
                        FBinSize[0] = 1;
                        FBinSize.Flush();
                        FValue.ToISpread().SliceCount = 1;

                        var output = (FValue.ToISpread())[0] as ISpread;
                        var dummies = GetDefaults(FAvoidNil.ToISpread(), 0).ToList();
                        output.SliceCount = dummies.Count;
                        for (int j = 0; j < dummies.Count; j++)
                            output[j] = dummies[j];

                        FValue.ToISpread().Flush();
                        return;
                    }
                    else return; // already defaulted
                }
                else
                {
                    var force = !FAvoidNilEnable.IsAnyInvalid() && !FAvoidNilEnable[0] && FAvoidNilEnable.IsChanged;
                    if (force || FOutput.SliceCount > 0) // zero inputs -> zero outputs.
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
            }
            #endregion early break

            FOutput.FlushResult(FInput);

            var keyCount = FKey.SliceCount;
            FBinSize.SliceCount = SpreadMax;

            var binnedOutput = FValue.ToISpread();
            binnedOutput.SliceCount = SpreadMax * keyCount;

            if (!FSwapDim.IsAnyInvalid() && FSwapDim[0]) // fields first
            {
                for (int i = 0; i < keyCount; i++)
                {
                    var fieldName = FKey[i];
                    var count = 0;
                    var index = 0;
                    foreach (var message in FInput)
                    {
                        var output = (binnedOutput[i * keyCount + index] as ISpread);
                        SetData(message, index, fieldName, output);
                        count += output.SliceCount;
                        index++;
                    }
                    FBinSize[i] = count;
                }
            }
            else // messages first
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    Message message = FInput[i];
                    var count = 0; 
                    var index = 0;
                    foreach (var fieldName in FKey)
                    {
                        var output = (binnedOutput[i * keyCount + index] as ISpread);
                        SetData(message, index, fieldName, output);
                        count += output.SliceCount;
                        index++;
                    }
                    FBinSize[i] = count;
                }
            }

            binnedOutput.Flush();
            FBinSize.Flush();
        }

        protected void SetData(Message message, int avoidNilIndex, string fieldName, ISpread output)
        {
            output.SliceCount = 0;
            if (!message.Fields.Contains(fieldName))
            {
                // is setting a default necessary?
                if (!FAvoidNilEnable.IsAnyInvalid() && FAvoidNilEnable[0])
                {
                    var avoidNil = FAvoidNil.ToISpread() as ISpread;
                    var results = GetDefaults(avoidNil, avoidNilIndex).ToList();

                    output.SliceCount = results.Count;
                    for (int j = 0; j < results.Count; j++) output[j] = results[j];
                }
            }
            else // copy from Message
            {
                var inputBin = message[fieldName];
                output.SliceCount = inputBin.Count;
                if (TargetDynamicType.IsAssignableFrom(inputBin.GetInnerType()))
                {
                    for (int j = 0; j < inputBin.Count; j++)
                        output[j] = inputBin[j];
                }
                else // will throw Exception, if Conversion is not possible
                {
                    for (int j = 0; j < inputBin.Count; j++) 
                        output[j] = Convert.ChangeType(inputBin[j], TargetDynamicType, CultureInfo.InvariantCulture);
                }
            }
        }

        private IEnumerable<object> GetDefaults(ISpread avoidNil, int avoidNilIndex)
        {
            if (avoidNil == null
                 || avoidNil.SliceCount == 0
                 || (avoidNil[avoidNilIndex] as ISpread).SliceCount == 0
                 || (avoidNil[avoidNilIndex] as ISpread)[0] == null
               )
            { // something is fishy with AvoidNil pin, so fetch a safe default here
                var item = TypeIdentity.Instance[TargetDynamicType].Default();
                if (item != null)
                    yield return item;
            }
            else // copy all relevant ones from AvoidNil pin
            {
                var items = avoidNil[avoidNilIndex] as ISpread;
                for (int j = 0; j < items.SliceCount; j++)
                    yield return items[j];
            }
        }

    }

}
