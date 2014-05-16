#region usings
using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;
using System.Runtime.Serialization;

#endregion usings

namespace VVVV.Nodes.Generic
{
	
	public class GetSlice<T> : IPluginEvaluate
	{
		#region fields & pins
        #pragma warning disable 649, 169
        [Input("Input", BinSize = 1)]
		IDiffSpread<ISpread<T>> FInput;

		[Input("Index", DefaultValue = 0)]
		ISpread<int> FIndex;

		[Output("Output", AutoFlush = false, BinVisibility = PinVisibility.OnlyInspector)]
		ISpread<ISpread<T>> FOutput;
		
		[Import]
		ILogger FLogger;
		
		protected DataContractResolver FResolver = null;
        #pragma warning restore
        #endregion fields & pins

        public void Evaluate(int SpreadMax)
		{
			SpreadMax = FIndex.SliceCount;
			FOutput.SliceCount = SpreadMax;
			
			for (int i=0;i<SpreadMax;i++) {
				FOutput[i] = FInput[FIndex[i]];
			}
			
			FOutput.Flush();
		}
		
	}
	
	
}