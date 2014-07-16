#region usings
using System;
using System.ComponentModel.Composition;
using VVVV.Pack.Message;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;
using System.Runtime.Serialization;

#endregion usings

namespace VVVV.Nodes.Generic
{
	
	public class SAndH<T> : IPluginEvaluate
	{
		#region fields & pins
#pragma warning disable 649, 169
        [Input("Input", AutoValidate = false)]
        Pin<T> FInput;
		
		[Input("Set", IsBang = true, IsSingle = true)]
		ISpread<bool> FSet;
		
		[Input("Clone", IsToggle = true, IsSingle = true, Visibility = PinVisibility.OnlyInspector, DefaultBoolean = true)]
		ISpread<bool> FClone;
		
		[Output("Output", AutoFlush = false)]
		ISpread<T> FOutput;
		
		[Import]
		ILogger FLogger;
		
		protected DataContractResolver FResolver = null;

#pragma warning restore
        #endregion fields & pins

        public void Evaluate(int SpreadMax)
		{
			if (FSet[0] == false) return;
            FInput.Sync();
			
			SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            
			FOutput.SliceCount = SpreadMax;
			
			bool clone = FClone[0] && (!FInput.IsAnyInvalid()) && (FInput[0] is ICloneable);
			for (int i=0;i<FInput.SliceCount;i++) {
								
				T output;
				if (clone) output = (T)((ICloneable)FInput[i]).Clone();
					else output = FInput[i];
				FOutput[i] = output;
			}
			
			FOutput.Flush();
		}
		
	}
	
	
}