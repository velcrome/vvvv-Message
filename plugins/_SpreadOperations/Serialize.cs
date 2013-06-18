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
	
	public class Serialize<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		IDiffSpread<T> FInput;
		
		[Output("Output", AutoFlush = false)]
		ISpread<Stream> FOutput;
		
		[Import]
		ILogger FLogger;
		
		protected DataContractResolver FResolver = null;
		
		
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if (!FInput.IsChanged) return;
			
			if (FInput.SliceCount <= 0 || FInput[0] == null) SpreadMax = 0;
				else SpreadMax = FInput.SliceCount;
			
			FOutput.SliceCount = SpreadMax;
			

			
			var serializer = FResolver == null ?
			new DataContractSerializer(typeof(T)) :
			new DataContractSerializer(typeof(T),null, 65536, false, false, null,FResolver);
			
		//	string xmlString;
			for (int i=0;i<SpreadMax;i++) {
				Stream output = new MemoryStream();
				serializer.WriteObject(output, FInput[i]);
				FOutput[i] = output;
			}
			
			FOutput.Flush();
		}
		
	}
	
	public class DeSerialize<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		IDiffSpread<Stream> FInput;
		
		[Output("Output", AutoFlush = false)]
		ISpread<T> FOutput;
		
		[Import]
		ILogger FLogger;
		
		protected DataContractResolver FResolver = null;
		
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if (!FInput.IsChanged ) return;
			
			FOutput.SliceCount = SpreadMax;
			
			for (int i=0;i<SpreadMax;i++) {
				var serializer = FResolver == null ?
				new DataContractSerializer(typeof(T)) :
				new DataContractSerializer(typeof(T),null, 65536, false, false, null,FResolver);
				Stream s = FInput[i];
				// Call the Deserialize method and cast to the object type.
				FOutput[i] = (T)serializer.ReadObject(s);
			}
			FOutput.Flush();
			
		}
	}
	
}