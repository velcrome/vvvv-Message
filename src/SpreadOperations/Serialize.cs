#region usings

using System.ComponentModel.Composition;
using System.IO;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;
using System.Runtime.Serialization;

#endregion usings

namespace VVVV.Nodes.Generic
{
	
	public class Serialize<T> : IPluginEvaluate
	{
		#region fields & pins
#pragma warning disable 649, 169
        [Input("Input")]
		IDiffSpread<T> FInput;
		
		[Output("Output", AutoFlush = false)]
		ISpread<Stream> FOutput;
		
		[Import]
		ILogger FLogger;
		
		protected DataContractResolver FResolver = null;

#pragma warning restore
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
#pragma warning disable 649, 169
        [Input("Input")]
		IDiffSpread<Stream> FInput;
		
		[Output("Output", AutoFlush = false)]
		ISpread<T> FOutput;
		
		[Import]
		ILogger FLogger;
		
		protected DataContractResolver FResolver = null;
#pragma warning restore
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