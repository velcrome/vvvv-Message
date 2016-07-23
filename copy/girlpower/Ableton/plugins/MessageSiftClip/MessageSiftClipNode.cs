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
	[PluginInfo(Name = "SiftClip", Category = "Message", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class MessagePickTrackNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<Message> FInput;

		[Input("Index", MinValue = 0, AsInt = true)]
		public IDiffSpread<Vector2D> FIndex;

		[Output("Output", AutoFlush = false)]
		public ISpread<Message> FOutput;

		[Import()]
		public ILogger FLogger;
		
		protected Dictionary<int, HashSet<int>> Indices = new Dictionary<int, HashSet<int>>();
		
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FIndex.IsChanged) {
				Indices.Clear();
				foreach (var id in FIndex) {
					int track = (int) id.x;
					if (!Indices.ContainsKey(track))
						Indices[track] = new HashSet<int>();
					Indices[track].Add((int)id.y);
				}
			}

			if (FInput.IsAnyInvalid() || FIndex.IsAnyInvalid()) {
				if (FOutput.SliceCount > 0) {
					FOutput.SliceCount = 0;
					FOutput.Flush();
				}
				return;
			}
			
			FOutput.SliceCount = 0;

			var search = from message in FInput
				let track = (message["TrackId"] as Bin<int>).First
				where Indices.ContainsKey(track)
				let clip = (message["ClipId"] as Bin<int>).First
				where Indices[track].Contains(clip)
				select message;
			
			FOutput.AssignFrom(search);

			FOutput.Flush();
		}
	}
}
