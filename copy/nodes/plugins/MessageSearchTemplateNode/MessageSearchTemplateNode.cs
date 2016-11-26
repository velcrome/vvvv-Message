#region usings
using System;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using VVVV.Packs.Messaging;
using VVVV.Utils;


#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "SearchTemplate", Category = "Message", Help = "Basic search template with example LINQ query", Tags = "Sift, LINQ")]
	#endregion PluginInfo
	public class MessageSearchTemplateNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		public IDiffSpread<Message> FInput; 

		[Input("Filter")]
		public IDiffSpread<string> FFilter; 

		[Output("Output", AutoFlush = false)]
		public ISpread<Message> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			// save some performance when no work necessary
			if (FInput.IsAnyInvalid()) {
				if (FOutput.SliceCount > 0) FOutput.FlushNil();
				return;
			}
			if (!FInput.IsChanged && !FFilter.IsChanged) return;

			// start working it out
			var result = 
				from message in FInput
				let bin = message["MyField"] as Bin<string> // identify as string
				where bin != null
				where !bin.IsAnyInvalid() && !FFilter.IsAnyInvalid() // safe against nil
				where FFilter.Contains(bin.First)
				select message;
			
			// publish data back to vvvv
			FOutput.FlushResult(result);
		}
	}
}
