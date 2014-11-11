#region usings
using System;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "DynEnum", Category = "String", AutoEvaluate=true, Help = "Basic template with one string in/out", Tags = "")]
	#endregion PluginInfo
	public class StringDynEnumNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Input", DefaultString = "vvvv")]
		public IDiffSpread<string> FInput;

		public ISpread<EnumEntry> FEnum = null;
		private string EnumName;

		[Output("Output")]
		public ISpread<string> FOutput;
       
		[Import()]
        protected IIOFactory FIOFactory;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		

		public void OnImportsSatisfied() {
			CreateEnumPin("Enum", new string[] {"vvvv"});
		}

		
		public void CreateEnumPin(string pinName, string[] entries)
        {
			EnumName = "Enum_" + this.GetHashCode().ToString();

        	EnumManager.UpdateEnum(EnumName, entries[0], entries);

        	var attr = new InputAttribute(pinName);
            attr.Order = 3;
            attr.AutoValidate = true;  

        	attr.EnumName = EnumName;

            Type pinType = typeof(ISpread<EnumEntry>); 
        	var pin = FIOFactory.CreateIOContainer(pinType, attr);
        	FEnum = (ISpread<EnumEntry>)(pin.RawIOObject);
        }
		
		private void FillEnum(string[] entries) {
			EnumManager.UpdateEnum(EnumName, entries[0], entries);
		}
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			var enumPin = FEnum;
			SpreadMax = enumPin.SliceCount;
			FOutput.SliceCount = SpreadMax;

			for (int i = 0; i < SpreadMax; i++)
				FOutput[i] = enumPin[i].Name;

			if (FInput.IsChanged && FInput.SliceCount > 0) {
				FillEnum(FInput.ToArray());
			}
			//FLogger.Log(LogType.Debug, "Logging to Renderer (TTY)");
		}
	}
}
