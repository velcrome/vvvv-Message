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
	
	public class Select<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		ISpread<T> FInput;
		
		[Input("Select", DefaultValue = 1, MinValue = 0)]
		ISpread<int> FSelect;
		
		[Output("Output", AutoFlush = false)]
		ISpread<T> FOutput;
		
		[Output("Former Slice", AutoFlush = false)]
		ISpread<int> FFormer;
		
		[Import]
		ILogger FLogger;
		
		protected DataContractResolver FResolver = null;
		
		
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = 0;
			FFormer.SliceCount = 0;
			
			if ((FInput.SliceCount >= 0) && (FInput[0] != null)) {
				
				for (int i=0;i<SpreadMax;i++) {
					T output = FInput[i];
					
					for (int j=0;j<FSelect[i];j++) {
						FOutput.Add(output);
						FFormer.Add(i%FInput.SliceCount);
					}
				}
			}
			FOutput.Flush();
			FFormer.Flush();
		}
		
	}
	
	
}