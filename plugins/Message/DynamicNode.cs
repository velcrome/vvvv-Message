#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;

using System.Dynamic;
using System.Reflection;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Hosting;
using VVVV.Nodes;
using VVVV.Core.Logging;
using VVVV.Utils.OSC;

#endregion usings

namespace VVVV.Utils.Message{

	public abstract class DynamicNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		
		#region Enum
		public enum SelectEnum
		{
			All,
			First,
			Last
		}
		#endregion Enum
		
		#region fields & pins
		[Config ("Configuration", DefaultString="string Foo")]
		public IDiffSpread<string> FConfig;
		
		[Input ("Verbose", Visibility = PinVisibility.OnlyInspector, IsSingle = true, DefaultBoolean = true)]
		public ISpread<bool> FVerbose;
		
		[Import()]
		protected ILogger FLogger;
		
		[Import()]
		protected IIOFactory FIOFactory;
		
		protected Dictionary<string, IIOContainer> FPins = new Dictionary<string, IIOContainer>();
		protected Dictionary<string, string> FTypes = new Dictionary<string, string>();

		protected int FCount = 2;
		#endregion fields & pins
		
		
		#region pin management
		public DynamicNode () {
		}
		
		
		public void OnImportsSatisfied() {
			FConfig.Changed += HandleConfigChange;
		}
		
		protected void HandleConfigChange(IDiffSpread<string> configSpread) {
			FCount = 0;
			List<string> invalidPins = FPins.Keys.ToList();
			
			string[] config = configSpread[0].Trim().Split(',');
			foreach (string pinConfig in config) {
				string[] pinData = pinConfig.Trim().Split(' ');
				
				try {
					string type = pinData[0].ToLower();
					string name = pinData[1];
					
					bool create = false;
					if (FPins.ContainsKey(name) && FPins[name] != null) {
						invalidPins.Remove(name);
						
						if (FTypes.ContainsKey(name)) {
							if (FTypes[name] != type) {
								FPins[name].Dispose();
								FPins[name] = null;
								create = true;
							}
							
						} else {
							// key is in FPins, but no type defined. should never happen
							create = true;
						}
					} else {
						FPins.Add(name, null);
						create = true;
					}
					
					if (create) {
						
		//				if (type == Message.Identities[typeof(double)] ) {}
						Dictionary<Type, string> ident = new MessageResolver().Identity;
						
						switch (type) {
							case "double" : FPins[name] = CreatePin<double>(name);break;
							case "float" : FPins[name] = CreatePin<float>(name);break;
							case "int" : FPins[name] = CreatePin<int>(name);break;
							case "bool" : FPins[name] = CreatePin<bool>(name);break;
							case "vector2d" : FPins[name] = CreatePin<Vector2D>(name);break;
							case "vector3d" : FPins[name] = CreatePin<Vector3D>(name);break;
							case "vector4d" : FPins[name] = CreatePin<Vector3D>(name);break;
							case "string" : FPins[name] = CreatePin<string>(name);break;
							case "color" : FPins[name] = CreatePin<RGBAColor>(name);break;
							case "transform" : FPins[name] = CreatePin<Matrix4x4>(name);
							break;
							default:  FLogger.Log(LogType.Debug, "Type "+type + " not supported!");break;
						}
						FTypes.Add(name, type);
					}
					FCount+=2;  // type and name are 2
				} catch (Exception ex) {
					var e = ex;
					FLogger.Log(LogType.Debug, ex.ToString());
					
					FLogger.Log(LogType.Debug, "Invalid Descriptor in Config Pin");
				}
			}
			foreach (string name in invalidPins) {
				FPins[name].Dispose();
				FPins.Remove(name);
				FTypes.Remove(name);
			}
		}
		
		#endregion pin management
		
		#region tools
		
		protected VVVV.PluginInterfaces.V2.NonGeneric.ISpread GetISpreadData(IIOContainer pin, int index) {
			return (VVVV.PluginInterfaces.V2.NonGeneric.ISpread) ((VVVV.PluginInterfaces.V2.NonGeneric.ISpread)(pin.RawIOObject))[index];
		}
		
		protected VVVV.PluginInterfaces.V2.ISpread<T> GetGenericISpreadData<T>(IIOContainer pin, int index) {
			return (VVVV.PluginInterfaces.V2.ISpread<T>) ((VVVV.PluginInterfaces.V2.ISpread<ISpread<T>>)(pin.RawIOObject))[index];
		}
		
		protected VVVV.PluginInterfaces.V2.NonGeneric.ISpread ToISpread(IIOContainer pin) {
			return (VVVV.PluginInterfaces.V2.NonGeneric.ISpread) ((VVVV.PluginInterfaces.V2.NonGeneric.ISpread)(pin.RawIOObject));
		}
		
		protected VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread ToIDiffSpread(IIOContainer pin) {
			return (VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread) ((VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread)(pin.RawIOObject));
		}
		protected VVVV.PluginInterfaces.V2.ISpread<T> ToGenericISpread<T>(IIOContainer pin) {
			return (VVVV.PluginInterfaces.V2.ISpread<T>) ((VVVV.PluginInterfaces.V2.ISpread<T>)(pin.RawIOObject));
		}
		
		#endregion tools
		
		#region abstract methods
		protected abstract IIOContainer<ISpread<ISpread<T>>> CreatePin<T>(string name);
		
		public abstract void Evaluate(int SpreadMax);
		
		#endregion abstract methods
	}

}