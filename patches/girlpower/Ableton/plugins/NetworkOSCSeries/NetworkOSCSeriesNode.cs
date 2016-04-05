#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using VVVV.Utils.OSC;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "OSCSeries", Category = "Network", Help = "Basic raw template which copies up to count bytes from the input to the output", Tags = "")]
	#endregion PluginInfo
	public class NetworkOSCSeriesNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<Stream> FInput;

		[Output("Address")]
		public ISpread<string> FAddress;

		[Output("Argument")]
		public ISpread<ISpread<string>> FArgument;

		[Output("Type")]
		public ISpread<string> FType;

		[Output("Match")]
		public ISpread<int> FMatch;

		#endregion fields & pins

		//called when all inputs and outputs defined above are assigned from the host
		public void OnImportsSatisfied()
		{
		}

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			if (FInput.SliceCount <= 0 || FInput[0] == null) spreadMax = 0;
			
			FAddress.SliceCount = 
			FArgument.SliceCount =
			FType.SliceCount =
			FMatch.SliceCount = spreadMax;
			
			for (int i = 0; i < spreadMax; i++) {
				var length = (int) FInput[i].Length;
				
				byte[] buffer = new byte[length];
				FInput[i].Read(buffer, 0, length);
				
				var osc = OSCPacket.Unpack(buffer, false) as OSCMessage;
				
				FAddress[i] = osc.Address;
				string type ="";
				
				var argCount = osc.Values.Count;
				FArgument[i].SliceCount = argCount;
				
				for (int j =0;j<argCount;j++) {
					var arg = osc.Values[j];
					FArgument[i][j] = arg.ToString();
					type += arg.GetType() == typeof(int)    ? "i" : "";
					type += arg.GetType() == typeof(float)  ? "f" : "";
					type += arg.GetType() == typeof(double) ? "d" : "";
					type += arg.GetType() == typeof(string) ? "s" : "";
				}
				
				FType[i] = type;
			}
				
		}
	}
}
