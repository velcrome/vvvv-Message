#region usings
using System;
using System.Collections;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using VVVV.Nodes.Messaging.Typing;
using VVVV.Pack.Messaging.Collections;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using VVVV.Pack.Messaging;

#endregion usings

namespace VVVV.Pack.Messaging {

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
        [Input("Message Type", DefaultString = "None", IsSingle=true)]
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

		#endregion fields & pins

        #region type
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
                        foreach (Type key in TypeIdentity.Instance.Keys)
					    {
                            if (TypeIdentity.Instance[key] == typeName)
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



    #region PluginInfo
    [PluginInfo(Name = "Message", AutoEvaluate=true, Category = "Join", Help = "Joins a Message from custom dynamic pins", Tags = "Dynamic, Bin, velcrome")]
    #endregion PluginInfo
    public class JoinMessageNode : DynamicNode
    {
        #pragma warning disable 649, 169
        [Input("Send", IsToggle = true, IsSingle = true, DefaultBoolean = true)]
        ISpread<bool> FSet;

        [Input("Address", DefaultString = "Event")]
        ISpread<string> FAddress;

        [Output("Output", AutoFlush = false)]
        Pin<Message> FOutput;
        #pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type)
        {
            var attr = new InputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = -1;
            attr.Order = FCount;
            attr.BinOrder = FCount + 1;
            attr.AutoValidate = false;  // need to sync all pins manually
            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            TypeUpdate();

            SpreadMax = 0;
            if (!FSet[0])
            {
                FOutput.SliceCount = 0;
                FOutput.Flush();
                return;
            }

            foreach (string name in FPins.Keys)
            {
                var pin = ToISpread(FPins[name]);
                pin.Sync();
                SpreadMax = Math.Max(pin.SliceCount, SpreadMax);
            }


            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                Message message = new Message();

                message.Address = FAddress[i];
                foreach (string name in FPins.Keys)
                {
                    message.AssignFrom(name, (IEnumerable)ToISpread(FPins[name])[i]);
                }
                FOutput[i] = message;

                // FLogger.Log(LogType.Debug, "== Message "+i+" == \n" + message.ToString());
                //	foreach (string name in message.GetDynamicMemberNames()) FLogger.Log(LogType.Debug, message[name].GetType()+" "+ name);
            }
            FOutput.Flush();
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Message", AutoEvaluate=true, Category = "Split", Help = "Splits a Message into custom dynamic pins", Tags = "Dynamic, Bin, velcrome")]
    #endregion PluginInfo
    public class SplitMessageNode : DynamicNode
    {
        public enum HoldEnum
        {
            Off,
            Message,
            Pin
        }

        #pragma warning disable 649, 169
        [Input("Input")]
        IDiffSpread<Message> FInput;

        [Input("Match Rule", DefaultEnumEntry = "All", IsSingle = true)]
        IDiffSpread<SelectEnum> FSelect;

        [Input("Hold if Nil", IsSingle = true, DefaultEnumEntry = "Message")]
        ISpread<HoldEnum> FHold; 

        [Output("Address", AutoFlush = false)]
        ISpread<string> FAddress;

        [Output("Timestamp", AutoFlush = false)]
        ISpread<string> FTimeStamp;

        #pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type)
        {
            var attr = new OutputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.AutoFlush = false;

            attr.Order = FCount;
            attr.BinOrder = FCount + 1;
            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            TypeUpdate();

            SpreadMax = (FSelect[0] != SelectEnum.First) ? FInput.SliceCount : 1;

            if (!FInput.IsChanged)
            {
                return;
            }

            bool empty = (FInput.SliceCount == 0) || (FInput[0] == null);

            if (empty && (FHold[0] == HoldEnum.Off))
            {
                foreach (string name in FPins.Keys)
                {
                    var pin = ToISpread(FPins[name]);
                    pin.SliceCount = 0;
                    pin.Flush();
                }
                FAddress.SliceCount = 0;
                FTimeStamp.SliceCount = 0;

                FAddress.Flush();
                FTimeStamp.Flush();

                return;
            }

            if (!empty)
            {
                foreach (string pinName in FPins.Keys)
                {
                    if (FSelect[0] == SelectEnum.All)
                    {
                        ToISpread(FPins[pinName]).SliceCount = SpreadMax;
                        FTimeStamp.SliceCount = SpreadMax;
                        FAddress.SliceCount = SpreadMax;
                    }
                    else
                    {
                        ToISpread(FPins[pinName]).SliceCount = 1;
                        FTimeStamp.SliceCount = 1;
                        FAddress.SliceCount = 1;
                    }
                }

                for (int i = (FSelect[0] == SelectEnum.Last) ? SpreadMax - 1 : 0; i < SpreadMax; i++)
                {
                    Message message = FInput[i];

                    FAddress[i] = message.Address;
                    FTimeStamp[i] = message.TimeStamp.ToString();
                    FAddress.Flush();
                    FTimeStamp.Flush();

                    foreach (string name in FPins.Keys)
                    {
                        var bin = (VVVV.PluginInterfaces.V2.NonGeneric.ISpread)ToISpread(FPins[name])[i];

                        SpreadList attrib = message[name];
                        int count = 0;

                        if (attrib == null)
                        {
                            if (FVerbose[0]) FLogger.Log(LogType.Debug, "\"" + FTypes[name] + " " + name + "\" is not defined in Message.");
                        }
                        else count = attrib.Count;

                        if ((count > 0) || (FHold[0] != HoldEnum.Pin))
                        {
                            bin.SliceCount = count;
                            for (int j = 0; j < count; j++)
                            {
                                bin[j] = attrib[j];
                            }
                            ToISpread(FPins[name]).Flush();
                        }
                        else
                        {
                            // keep old values in pin. do not flush
                        }

                    }
                }
            }
        }


        #region PluginInfo
        [PluginInfo(Name = "SetMessage", Category = "Message", Help = "Adds or edits a Message Property", Tags = "Dynamic, Bin, velcrome")]
        #endregion PluginInfo
        public class SetMessageNode : DynamicNode
        {
            #pragma warning disable 649, 169
            [Input("Input")]
            IDiffSpread<Message> FInput;

            [Output("Output", AutoFlush = false)]
            Pin<Message> FOutput;
            #pragma warning restore

            protected override IOAttribute DefinePin(string name, Type type)
            {
                var attr = new InputAttribute(name);
                attr.BinVisibility = PinVisibility.Hidden;
                attr.BinSize = -1;
                attr.Order = FCount;
                attr.BinOrder = FCount + 1;
                //                attr.AutoValidate = false;  // need to sync all pins manually
                return attr;
            }


            public override void Evaluate(int SpreadMax)
            {
                TypeUpdate();

                SpreadMax = FInput.SliceCount;

                if (!FInput.IsChanged)
                {
                    //				FLogger.Log(LogType.Debug, "skip set");
                    return;
                }


                if (FInput.SliceCount == 0 || FInput[0] == null)
                {
                    FOutput.SliceCount = 0;
                    return;
                }

                for (int i = 0; i < SpreadMax; i++)
                {
                    Message message = FInput[i];
                    foreach (string name in FPins.Keys)
                    {
                        var pin = (IEnumerable)ToISpread(FPins[name])[i];
                        message.AssignFrom(name, pin);
                    }
                }

                FOutput.AssignFrom(FInput);
                FOutput.Flush();
            }
        }

	}
   
}