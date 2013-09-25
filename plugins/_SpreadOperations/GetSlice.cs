#region usings
using System;
using System.ComponentModel.Composition;
using System.IO;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Utils.Collections;
using VVVV.Core.Logging;
using VVVV.Nodes;

using System.Collections.Generic;
using VVVV.PluginInterfaces.V2.Graph;


using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
#endregion usings

namespace VVVV.Nodes
{
	
	public class GetSlice<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", BinSize = 1)]
		IDiffSpread<ISpread<T>> FInput;

		[Input("Index", DefaultValue = 0)]
		ISpread<int> FIndex;

		[Output("Output", AutoFlush = false, BinVisibility = PinVisibility.OnlyInspector)]
		ISpread<ISpread<T>> FOutput;
		
		[Import]
		ILogger FLogger;
		
		protected DataContractResolver FResolver = null;
		
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