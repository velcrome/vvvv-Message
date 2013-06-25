#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;

using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using VVVV.Utils.Messaging;

#endregion usings

namespace VVVV.Nodes.Messaging {

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
        [Config("Message Type", DefaultString = "None", IsSingle=true)]
        public IDiffSpread<string> FType;
       
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
	    public TypeIdentity Identity;

		#endregion fields & pins

        #region type
        
        public DynamicNode()
        {
            Identity = new TypeIdentity();
        }


        protected bool TypeUpdate()
        {
            if (!TypeDictionary.IsChanged && !FType.IsChanged) return false;
            
            TypeDictionary dict = TypeDictionary.Instance;
            if (!dict.ContainsKey(FType[0])) return false;

            if (FType[0].ToLower() == "none") return false;

            FConfig[0] = dict[FType[0]];

            return true;
        }
        #endregion type


        #region pin management
 
		
		
		public void OnImportsSatisfied() {
			FConfig.Changed += HandleConfigChange;
		}
		
		protected virtual void HandleConfigChange(IDiffSpread<string> configSpread) {
			FCount = 0;
			List<string> invalidPins = FPins.Keys.ToList();
			
			string[] config = configSpread[0].Trim().Split(',');
			foreach (string pinConfig in config) {
				string[] pinData = pinConfig.Trim().Split(' ');
				
				try {
					string typeName = pinData[0].ToLower();
					string name = pinData[1];
					
					bool create = false;
					if (FPins.ContainsKey(name) && FPins[name] != null) {
						invalidPins.Remove(name);
						
						if (FTypes.ContainsKey(name)) {
							if (FTypes[name] != typeName) {
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
					    Type type = typeof(string);
                        foreach (Type key in Identity.Keys)
					    {
					        if (Identity[key] == typeName)
					        {
					            type = key;
					        }
					    }

                        IOAttribute attr = DefinePin(name, type); // each implementation of DynamicNode must create its own InputAttribute or OutputAttribute (
					    Type pinType = typeof (ISpread<>).MakeGenericType((typeof (ISpread<>)).MakeGenericType(type)); // the Pin is always a binsized one
					    FPins[name] = FIOFactory.CreateIOContainer(pinType, attr);

						FTypes.Add(name, typeName);
					}
					FCount+=2; // total pincount. always add two to account for data pin and binsize pin
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
		
		protected VVVV.PluginInterfaces.V2.NonGeneric.ISpread ToISpread(IIOContainer pin) {
			return (VVVV.PluginInterfaces.V2.NonGeneric.ISpread)(pin.RawIOObject);
		}
		
		protected VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread ToIDiffSpread(IIOContainer pin) {
			return (VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread)(pin.RawIOObject);
		}
		protected VVVV.PluginInterfaces.V2.ISpread<T> ToGenericISpread<T>(IIOContainer pin) {
			return (VVVV.PluginInterfaces.V2.ISpread<T>)(pin.RawIOObject);
		}
		
		#endregion tools
		
		#region abstract methods
		protected abstract IOAttribute DefinePin(string name, Type type);
		
		public abstract void Evaluate(int SpreadMax);
		
		#endregion abstract methods
	}

    public class TypeDictionary : Dictionary<string, string>{
        private static TypeDictionary instance;

        public static bool IsChanged
        {
            get;
            set;
        }
        
        public static TypeDictionary Instance {
            get
            {
                if (instance == null) instance = new TypeDictionary();
                return instance;
            }
        }
    }



   
}