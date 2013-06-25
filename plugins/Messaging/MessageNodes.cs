#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using VVVV.Nodes.Messaging;
using VVVV.PluginInterfaces.V2;

using VVVV.Core.Logging;
using VVVV.Utils.Collections;
using VVVV.Utils.Messaging;

#endregion usings

namespace VVVV.Nodes {
	#region PluginInfo
	[PluginInfo(Name = "Message", Category = "Join", Help = "Joins a Message from custom dynamic pins", Tags = "Dynamic, Bin, velcrome")]
	#endregion PluginInfo
	public class JoinMessageNode : DynamicNode {
		
		[Input("Send", IsBang=true, IsSingle=true)]
		ISpread<bool> FSet;
		
		[Input("Address", DefaultString = "Event")]
		ISpread<string> FAddress;
		
		[Output("Output", AutoFlush = false)]
		Pin<Utils.Messaging.Message> FOutput;
		
		protected override IOAttribute DefinePin(string name, Type type) {
			var attr = new InputAttribute(name);
			attr.BinVisibility = PinVisibility.Hidden;
			attr.BinSize = 1;
			attr.Order = FCount;
			attr.BinOrder = FCount+1;
			attr.AutoValidate = false;  // need to sync all pins manually
			return attr;
		}
		
		public override void Evaluate(int SpreadMax)
		{
			TypeUpdate();
			
			SpreadMax = 0;
			if (!FSet[0]) {
				//				FLogger.Log(LogType.Debug, "skip join");
				FOutput.SliceCount = 0;
				FOutput.Flush();
				return;
			}
			
			foreach (string name in FPins.Keys) {
				var pin = ToISpread(FPins[name]);
				pin.Sync();
				SpreadMax = Math.Max(pin.SliceCount, SpreadMax);
			}
			
			
			FOutput.SliceCount = SpreadMax;
			for (int i=0; i<SpreadMax;i++) {
                Utils.Messaging.Message message = new Utils.Messaging.Message();
				
				message.Address = FAddress[i];
				foreach (string name in FPins.Keys) {
					message.AssignFrom(name, (IEnumerable) ToISpread(FPins[name])[i] );
				}
				FOutput[i] = message;
				
				// FLogger.Log(LogType.Debug, "== Message "+i+" == \n" + message.ToString());
				//	foreach (string name in message.GetDynamicMemberNames()) FLogger.Log(LogType.Debug, message[name].GetType()+" "+ name);
			}
			FOutput.Flush();
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Message", Category = "Split", Help = "Splits a Message into custom dynamic pins", Tags = "Dynamic, Bin, velcrome")]
	#endregion PluginInfo
	public class SplitMessageNode : DynamicNode {
		public enum HoldEnum
		{
			Off,
			Message,
			Pin
		}
		
		[Input("Input")]
        IDiffSpread<Utils.Messaging.Message> FInput;
		
		[Input("Match Rule", DefaultEnumEntry="All", IsSingle = true)]
		IDiffSpread<SelectEnum> FSelect;
		
		[Input("Hold if Nil", IsSingle = true, DefaultEnumEntry = "Message")]
		ISpread<HoldEnum> FHold;
		
		[Output("Address", AutoFlush = false)]
		ISpread<string> FAddress;
		
		[Output("Timestamp", AutoFlush = false)]
		ISpread<string> FTimeStamp;
		
		//		[Output("Configuration", AutoFlush = false)]
		//		ISpread<string> FConfigOut;
		
		protected override IOAttribute DefinePin(string name, Type type) {
			var attr = new OutputAttribute(name);
			attr.BinVisibility = PinVisibility.Hidden;
			attr.AutoFlush = false;
			
			attr.Order = FCount;
			attr.BinOrder = FCount+1;
		    return attr;
		}
		
		public override void Evaluate(int SpreadMax)
		{
			TypeUpdate();
			
			SpreadMax = (FSelect[0] != SelectEnum.First) ? FInput.SliceCount : 1;
			
			if (!FInput.IsChanged) {
				//				FLogger.Log(LogType.Debug, "skip split");
				return;
			}
			
			bool empty = (FInput.SliceCount==0) || (FInput[0] == null);
			
			if (empty && (FHold[0] == HoldEnum.Off )) {
				foreach (string name in FPins.Keys) {
					var pin = ToISpread(FPins[name]);
					pin.SliceCount = 0;
					pin.Flush();
				}
				FAddress.SliceCount = 0;
				FTimeStamp.SliceCount =0;
				
				FAddress.Flush();
				FTimeStamp.Flush();
				
				return;
			}
			
			if (!empty) {
				foreach(string pinName in FPins.Keys) {
					if (FSelect[0] == SelectEnum.All) {
						ToISpread(FPins[pinName]).SliceCount = SpreadMax;
						FTimeStamp.SliceCount = SpreadMax;
						FAddress.SliceCount = SpreadMax;
					}
					else {
						ToISpread(FPins[pinName]).SliceCount = 1;
						FTimeStamp.SliceCount = 1;
						FAddress.SliceCount = 1;
					}
				}
				
				for (int i= (FSelect[0] == SelectEnum.Last)?SpreadMax-1:0;i<SpreadMax;i++) {
                    Utils.Messaging.Message message = FInput[i];
					
					FAddress[i] = message.Address;
					FTimeStamp[i] = message.TimeStamp.ToString();
					FAddress.Flush();
					FTimeStamp.Flush();
					
					foreach (string name in FPins.Keys) {
                        var bin = (VVVV.PluginInterfaces.V2.NonGeneric.ISpread) ToISpread(FPins[name])[i];
						
						SpreadList attrib = message[name];
						int count = 0;
						
						if (attrib == null) {
							if (FVerbose[0]) FLogger.Log(LogType.Debug, "\"" + FTypes[name]+" " + name + "\" is not defined in Message.");
						} else count = attrib.Count;
						
						if ((count > 0) || (FHold[0] != HoldEnum.Pin))
						{
							bin.SliceCount = count;
							for (int j = 0;j<count;j++) {
								bin[j] = attrib[j];
							}
							ToISpread(FPins[name]).Flush();
						} else {
							// keep old values in pin. do not flush
						}
						
					}
				}
			}
		}

		
		#region PluginInfo
		[PluginInfo(Name = "SetMessage", Category = "Message", Help = "Adds or edits a Message Property", Tags = "Dynamic, Bin, velcrome")]
		#endregion PluginInfo
		public class SetMessageNode : DynamicNode {
			[Input("Input")]
            IDiffSpread<Message> FInput;
			
			[Output("Output", AutoFlush=false)]
            Pin<Message> FOutput;

            protected override IOAttribute DefinePin(string name, Type type)
            {
                var attr = new InputAttribute(name);
                attr.BinVisibility = PinVisibility.Hidden;
                attr.BinSize = 1;
                attr.Order = FCount;
                attr.BinOrder = FCount + 1;
//                attr.AutoValidate = false;  // need to sync all pins manually
                return attr;
            }			

		
			public override void Evaluate(int SpreadMax) {
				SpreadMax = FInput.SliceCount;
				
				if (!FInput.IsChanged) {
					//				FLogger.Log(LogType.Debug, "skip set");
					return;
				}
				
				
				if (FInput.SliceCount==0 || FInput[0] == null) {
					FOutput.SliceCount = 0;
					return;
				}

				for (int i=0;i<SpreadMax;i++) {
                    Utils.Messaging.Message message = FInput[i];
					foreach (string name in FPins.Keys) {
						var pin = (IEnumerable) ToISpread(FPins[name])[i];
						message.AssignFrom(name, pin);
					}
				}

				FOutput.AssignFrom(FInput);
				FOutput.Flush();
			}
		}
		
		#region PluginInfo
		[PluginInfo(Name = "MessageType", AutoEvaluate = true,  Category = "Message", Help = "Define a high level Template for Messages", Tags = "Dynamic, Bin, velcrome")]
		#endregion PluginInfo
		public class MessageTypeMessageNode : IPluginEvaluate
		{
			[Input("Type Name", DefaultString = "Event")]
			public ISpread<string> FName;
			
			[Input("Configuration", DefaultString = "string Foo")]
			public ISpread<string> FConfig;
			
			[Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = false)]
			public IDiffSpread<bool> FUpdate;
			
			public void Evaluate(int SpreadMax)
			{
				if (!FUpdate[0])
				{
					if (FUpdate.IsChanged) TypeDictionary.IsChanged = false; // has updated last frame, but not anymore
					return;
				}
				SpreadMax = FName.SliceCount;
				
				TypeDictionary.IsChanged = true;
				for (int i = 0; i < SpreadMax; i++)
				{
					var dict = TypeDictionary.Instance;
					
					if (dict.ContainsKey(FName[i]))
					dict[FName[i]] = FConfig[i];
					else dict.Add(FName[i], FConfig[i]);
				}
			}
		}
		
		#region PluginInfo
		[PluginInfo(Name = "AsOSC", Category = "Message", Help = "Outputs OSC Bundle Strings", Tags = "Dynamic, OSC, velcrome")]
		#endregion PluginInfo
		public class MessageMessageAsOscNode : IPluginEvaluate {
			[Input("Input")]
            IDiffSpread<Utils.Messaging.Message> FInput;
			
			[Output("OSC", AutoFlush = false)]
			ISpread<Stream> FOutput;
			
			public void Evaluate(int SpreadMax) {
				if (FInput.SliceCount <= 0 || FInput[0] == null) SpreadMax = 0;
				else SpreadMax = FInput.SliceCount;
				
				if (!FInput.IsChanged) return;
				FOutput.SliceCount = SpreadMax;
				
				for (int i=0;i<SpreadMax;i++) {
					FOutput[i] = FInput[i].ToOSC();
				}
				FOutput.Flush();
			}
		}
		
		
		#region PluginInfo
		[PluginInfo(Name = "AsMessage", Category = "Message, OSC", Help = "Converts OSC Bundles into Messages ", Tags = "Dynamic, OSC, velcrome")]
		#endregion PluginInfo
		public class MessageOscAsMessageNode : IPluginEvaluate {
			[Input("OSC")]
			IDiffSpread<Stream> FInput;
			
			[Input("Additional Address", DefaultString="OSC", IsSingle = true)]
			IDiffSpread<string> FAddress;

            [Input("Contract Address Parts", DefaultValue= 1, IsSingle = true, MinValue = 1)]
            IDiffSpread<int> FContract;
            
            [Output("Output", AutoFlush = false)]
            ISpread<Message> FOutput;
			
			public void Evaluate(int SpreadMax) {
				
				if (!FInput.IsChanged && !FAddress.IsChanged && !FContract.IsChanged) return;
				
				if (FInput.SliceCount <= 0 || FInput[0] == null) SpreadMax = 0;
				    else SpreadMax = FInput.SliceCount;
				
				FOutput.SliceCount = SpreadMax;
				
				for (int i=0;i<SpreadMax;i++) {
                    Message message = Message.FromOSC(FInput[i], FAddress[0], FContract[0]);
					FOutput[i] = message;
				}
				FOutput.Flush();
			}
		}
		
		#region PluginInfo
		[PluginInfo(Name = "Info", Category = "Message", Help = "Help to Debug Messages", Tags = "Dynamic, TTY, velcrome")]
		#endregion PluginInfo
		public class MessageInfoNode : IPluginEvaluate {
			[Input("Input")]
            IDiffSpread<Utils.Messaging.Message> FInput;
			
			[Input("Print Message", IsBang = true)]
			IDiffSpread<bool> FPrint;
			
			[Output("Address", AutoFlush = false)]
			ISpread<string> FAddress;
			
			[Output("Timestamp", AutoFlush = false)]
			ISpread<string> FTimeStamp;
			
			[Output("Output", AutoFlush = false)]
			ISpread<string> FOutput;
			
			[Output("Configuration", AutoFlush = false)]
			ISpread<string> FConfigOut;
			
			[Import()]
			protected ILogger FLogger;
			
			public void Evaluate(int SpreadMax) {
				if (FInput.SliceCount <= 0 || FInput[0] == null) SpreadMax = 0;
				else SpreadMax = FInput.SliceCount;
				
				if (!FInput.IsChanged) return;
				
				FOutput.SliceCount = SpreadMax;
				FTimeStamp.SliceCount = SpreadMax;
				FAddress.SliceCount = SpreadMax;
				FConfigOut.SliceCount = SpreadMax;
				
				Dictionary<Type, string> identities = new MessageResolver().Identity;
				
				for (int i=0;i<SpreadMax;i++) {
                    Utils.Messaging.Message m = FInput[i];
					FOutput[i]= m.ToString();
					FAddress[i] = m.Address;
					FTimeStamp[i] = m.TimeStamp.ToString();
					StringBuilder config = new StringBuilder();
					FConfigOut[i] = FInput[i].GetConfig(identities);
					
					if (FPrint[i]) {
						StringBuilder sb = new StringBuilder();
						sb.Append("== Message "+i+" ==") ;
						sb.AppendLine();
						
						sb.AppendLine(FInput[i].ToString()) ;
						sb.Append("====\n");
						FLogger.Log(LogType.Debug, sb.ToString());
					}
				}
				FAddress.Flush();
				FTimeStamp.Flush();
				FOutput.Flush();
				FConfigOut.Flush();
			}
		}
		
		
		#region PluginInfo
		[PluginInfo(Name = "Sift", Category = "Message", Help = "Filter Messages", Tags = "Dynamic, velcrome")]
		#endregion PluginInfo
		public class MessageSiftNode : IPluginEvaluate {
			[Input("Input")]
            IDiffSpread<Utils.Messaging.Message> FInput;
			
			[Input("Filter", DefaultString = "*")]
			IDiffSpread<string> FFilter;
			
			[Output("Output", AutoFlush = false)]
            ISpread<Utils.Messaging.Message> FOutput;
			
			[Output("NotFound", AutoFlush = false)]
            ISpread<Utils.Messaging.Message> FNotFound;
			
			[Import()]
			protected ILogger FLogger;
			
			public void Evaluate(int SpreadMax) {
				if (!FInput.IsChanged) return;
				
				SpreadMax = FInput.SliceCount;
				
				FOutput.SliceCount = 0;
				bool[] found = new bool[SpreadMax];
				for (int i=0;i<SpreadMax;i++) found[i] = false;
				
				for (int i=0;i<FFilter.SliceCount;i++) {
					string[] filter = FFilter[i].Split('.');
					
					for (int j=0;j<SpreadMax;j++) {
						if (!found[j]) {
							string[] message = FInput[j].Address.Split('.');
							
							if (message.Length < filter.Length) continue;
							
							bool match = true;
							for (int k=0;k<filter.Length;k++) {
								if ((filter[k].Trim() != message[k].Trim()) && (filter[k].Trim() != "*")) match = false;
							}
							found[j] = match;
						}
					}
				}
				
				for (int i=0;i<SpreadMax;i++) {
					if (found[i]) FOutput.Add(FInput[i]);
					else FNotFound.Add(FInput[i]);
				}
				FOutput.Flush();
				FNotFound.Flush();
			}
		}
		
		#region PluginInfo
		[PluginInfo(Name = "AsJson", Category = "Message", Help = "Filter Messages", Tags = "Dynamic, velcrome, JSON")]
		#endregion PluginInfo
		public class MessageAsJsonStringNode : IPluginEvaluate {
			[Input("Input")]
            IDiffSpread<Utils.Messaging.Message> FInput;
			
			[Output("String", AutoFlush = false)]
			ISpread<string> FOutput;
			
			private MessageResolver FResolver;
			
			[Import()]
			protected ILogger FLogger;
			
			public MessageAsJsonStringNode () {
				FResolver = new MessageResolver();
			}
			
			
			public void Evaluate(int SpreadMax) {
				if (!FInput.IsChanged) return;
				
				FOutput.SliceCount = SpreadMax;
				JsonSerializer ser = new JsonSerializer();
				
				JsonSerializerSettings settings = new JsonSerializerSettings();
				settings.Formatting = Formatting.None;
				settings.TypeNameHandling = TypeNameHandling.None;
				
				for (int i=0;i<SpreadMax;i++) {
					string s = JsonConvert.SerializeObject(FInput[i], settings);
					
					FOutput[i] = s != null? s : "";
				}
				FOutput.Flush();
//				FStreamOutput.Flush();
			}
		}
		
		#region PluginInfo
		[PluginInfo(Name = "AsMessage", Category = "Message, Json", Help = "Filter Messages", Tags = "Dynamic, velcrome, JSON")]
		#endregion PluginInfo
		public class JsonStringAsMessageNode : DynamicNode {
			[Input("Input")]
			IDiffSpread<string> FInput;
			
			[Output("Message", AutoFlush = false)]
            ISpread<Utils.Messaging.Message> FOutput;
			
			private MessageResolver FResolver;
			
			[Import()]
			protected ILogger FLogger;
			
			public JsonStringAsMessageNode () {
				FResolver = new MessageResolver();
			}
			
			protected override IOAttribute DefinePin(string name, Type type) {
				return null;
				
			}
			
			protected override void HandleConfigChange(IDiffSpread<string> configSpread) {
				FLogger.Log(LogType.Debug, "nothing");
				
			}
			
			public override void Evaluate(int SpreadMax) {
				TypeUpdate();
				
				if (!FInput.IsChanged) return;
				
				FOutput.SliceCount = SpreadMax;
				
				for (int i=0;i<SpreadMax;i++) {

                    FOutput[i] = JsonConvert.DeserializeObject<Utils.Messaging.Message>(FInput[i]);
				}
				
				FOutput.Flush();
			}
		}


        [PluginInfo(Name = "FrameDelay", Category = "Message", Help = "Allows Feedback Loops for Messages", Tags = "velcrome", AutoEvaluate = true)]
        public class MessageFrameDelayNode : IPluginEvaluate
        {
			[Input("Input")]
            IDiffSpread<Utils.Messaging.Message> FInput;

            [Input("Default")]
            ISpread<Utils.Messaging.Message> FDefault;
            
            [Input("Initialize", IsSingle = true, IsBang = true)]
			IDiffSpread<bool> FInit;

        	[Output("Message", AutoFlush = false, AllowFeedback = true)]
            ISpread<Utils.Messaging.Message> FOutput;

            private List<Utils.Messaging.Message> lastMessage;
			
			[Import()]
			protected ILogger FLogger;

            public MessageFrameDelayNode()
            {
                lastMessage = new List<Utils.Messaging.Message>();
			}
			
			
			public void Evaluate(int SpreadMax) {
				if (!FInput.IsChanged && !FInit.IsChanged) return;
				
				if (FInit[0])
				{
				    lastMessage.Clear();
                    lastMessage.AddRange(FDefault);
				}
				
			    FOutput.SliceCount = lastMessage == null ? 0 : lastMessage.Count;
                
                FOutput.AssignFrom(lastMessage);
			    lastMessage.Clear();
                lastMessage.AddRange(FInput);
				FOutput.Flush();
			}            
        }

		[PluginInfo(Name = "Cons", Category = "Message", Help = "Concatenates all Messages", Tags = "velcrome")]
        public class MessageConsNode : Cons<Utils.Messaging.Message>
		{}
		
		
		[PluginInfo(Name = "Buffer", Category = "Message", Help = "Buffers all Messages", Tags = "velcrome")]
        public class MessageBufferNode : BufferNode<Utils.Messaging.Message>
		{}
		
		
		[PluginInfo(Name = "Queue", Category = "Message", Help = "Queues all Messages", Tags = "velcrome")]
        public class MessageQueueNode : QueueNode<Utils.Messaging.Message>
		{}
		
		
		[PluginInfo(Name = "RingBuffer", Category = "Message", Help = "Ringbuffers all Messages", Tags = "velcrome")]
        public class MessageRingBufferNode : RingBufferNode<Utils.Messaging.Message>
		{}
		
		[PluginInfo(Name = "Serialize", Category = "Message", Help = "Makes binary from Messages", Tags = "Raw")]
        public class MessageSerializeNode : Serialize<Utils.Messaging.Message>
		{
			
			public MessageSerializeNode() : base() {
				FResolver = new MessageResolver();
			}
		}
		
		[PluginInfo(Name = "DeSerialize", Category = "Message", Help = "Creates Messages from binary", Tags = "Raw")]
        public class MessageDeSerializeNode : DeSerialize<Utils.Messaging.Message>
		{
			public MessageDeSerializeNode() : base() {
				FResolver = new MessageResolver();
			}
		}
		
		[PluginInfo(Name = "S+H", Category = "Message", Help = "Save a Message", Tags = "Dynamic, velcrome")]
        public class MessageSAndHNode : SAndH<Utils.Messaging.Message>
		{}
		
		[PluginInfo(Name = "GetSlice", Category = "Message", Help = "GetSlice Messages", Tags = "Dynamic, velcrome")]
        public class MessageGetSliceNode : GetSlice<Utils.Messaging.Message>
		{}
		
		
		[PluginInfo(Name = "Select", Category = "Message", Help = "Select Messages", Tags = "Dynamic, velcrome")]
        public class MessageSelectNode : Select<Utils.Messaging.Message>
		{}
		
	}
	
}