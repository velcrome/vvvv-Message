#region usings
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using VVVV.Packs.Messaging;
using VVVV.Utils;
using VVVV.Utils.VMath;


#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "SiftTrack", Category = "Message", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class MessageSiftTrackNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		public IDiffSpread<Message> FInput;

		[Input("Index", MinValue = 0, AsInt = true)]
		public IDiffSpread<int> FIndex;

		[Output("Output", AutoFlush = false)]
		public ISpread<Message> FOutput;

		[Import()]
		public ILogger FLogger;

		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInput.IsAnyInvalid() || FIndex.IsAnyInvalid()) {
				if (FOutput.SliceCount > 0) {
					FOutput.SliceCount = 0;
					FOutput.Flush();
				}
				return;
			}

			if (!FInput.IsChanged && !FIndex.IsChanged) return;
			
			FOutput.SliceCount = 0;

			var search = from message in FInput
				let track = (message["TrackId"] as Bin<int>).First
				where FIndex.Contains(track)
				select message;

			FOutput.AssignFrom(search);

			FOutput.Flush();
		}
	}
}
