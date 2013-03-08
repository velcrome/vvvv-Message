#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Utils.Collections;
using VVVV.Core.Logging;
using VVVV.Nodes;

using System.Collections.Generic;
using VVVV.PluginInterfaces.V2.Graph;

#endregion usings

namespace VVVV.Nodes
{
	public struct FrameDelayLink<T> {
		public Pin<T> RemotePin;
	}
	
	public class Frame<T> : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		IDiffSpread<T> FInput;
		
		[Input("Link")]
		Pin<FrameDelayLink<T>> FLink;
		
		[Input("Initialize", IsSingle = true, IsBang=true)]
		ISpread<bool> FInit;
		
		[Output("Dummy")]
		ISpread<T> FOutput;
		
		[Import]
		ILogger FLogger;
		
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			if (!FLink.PluginIO.IsConnected) throw new Exception("Corrupt or missing Link. Frame must be used in conjunction with Delayer");
			if (!FInput.IsChanged) return;
			
			try {
				Pin<T> shortcut = FLink[0].RemotePin;
				
				if (FInit[0]) {
					FOutput.SliceCount = 0;
					shortcut.SliceCount = 0;
				} else {
					FOutput.SliceCount = FInput.SliceCount;
					FOutput.AssignFrom(FInput);
					
					
					shortcut.SliceCount = FInput.SliceCount;
					shortcut.AssignFrom(FInput);
				}
			} catch (Exception err) {
				err.ToString(); // no warning
				throw new Exception("Corrupt or missing Link. Frame plugin must only be used in conjunction with Delayer plugin");
			}
		}
		
	}
	
	public class Delayer<T> : IPluginEvaluate where T:ICloneable
	{
		#region fields & pins
		[Input("Input", AutoValidate = false)]
		Pin<T> FInput;
		
		[Input("Clone", DefaultBoolean =false, IsSingle = true)]
		ISpread<bool> FClone;

		[Output("Link")]
		ISpread<FrameDelayLink<T>> FLink;
		
		[Output("Output", AutoFlush = false)]
		Pin<T> FOutput;
		
		private FrameDelayLink<T> Link = new FrameDelayLink<T>();
		
		[Import]
		ILogger FLogger;
		
		#endregion fields & pins
		
		public void Evaluate(int SpreadMax)
		{
			SpreadMax = FInput.SliceCount;
			FLink.SliceCount = 1;
			Link.RemotePin = FInput;
			FLink[0] = Link;

			if (FClone[0]) {
				FOutput.SliceCount = SpreadMax;
				for (int i=0;i<SpreadMax;i++) {
					FOutput[i] = (T)FInput[i].Clone();
				}
			} else FOutput.AssignFrom(FInput);
			
			FOutput.Flush();
			
			
			
		}
	}
	
}