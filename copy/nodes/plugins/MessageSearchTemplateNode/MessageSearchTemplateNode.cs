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
	[PluginInfo(Name = "SearchTemplate", Category = "Message", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class MessageSearchTemplateNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<Message> FInput; 

		[Output("Output", AutoFlush = false)]
		public ISpread<Message> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInput.IsAnyInvalid()) {
				if (FOutput.SliceCount > 0) {
					FOutput.SliceCount = 0;
					FOutput.Flush();
				}
				return;
			}
			
			FOutput.SliceCount = 0;

			FOutput.AssignFrom(
				from message in FInput
				let bin = message["Foo"] as Bin<string>
				where bin.First == "bar"
				select message
			);
			
			FOutput.Flush();
		}
	}
}
