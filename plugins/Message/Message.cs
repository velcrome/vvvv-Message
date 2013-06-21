#region usings
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using VVVV.Utils.Message;
using VVVV.Utils.OSC;
using VVVV.Utils.Collections;



#endregion usings

namespace VVVV.Utils.Messaging{
	
	
	[DataContract]
	public class Message : ICloneable {
		
		// The inner MessageData.
		[DataMember]
		Dictionary<string, SpreadList> MessageData = new Dictionary<string, SpreadList>();
		
		[DataMember]
		public DateTime TimeStamp {
			get;
			set;
		}
		
		[DataMember]
		public string Address{
			get;
			set;
		}
		
		public Message() {
			TimeStamp = DateTime.Now;
		}

		public void Add(string name, object val) {
			//			name = name.ToLower();
			if (val is SpreadList) MessageData.Add(name, (SpreadList)val);
			else {
				MessageData.Add(name, new SpreadList());
				((SpreadList) MessageData[name]).Add(val);
			}
		}
		
		public void AssignFrom(string name, IEnumerable en) {
			//			name = name.ToLower();
			if (!MessageData.ContainsKey(name)) {
				MessageData.Add(name, new SpreadList());
			} else MessageData[name].Clear();
			
			foreach (object o in en) {
				MessageData[name].Add(o);
			}
		}
		
		public void AddFrom(string name, IEnumerable en) {
			//			name = name.ToLower();
			if (!MessageData.ContainsKey(name)) {
				MessageData.Add(name, new SpreadList());
			}
			
			foreach (object o in en) {
				MessageData[name].Add(o);
			}
		}
		
		public string GetConfig(Dictionary<Type, string> identities = null) {
			StringBuilder sb = new StringBuilder();
			
			if (identities == null) identities = new MessageResolver().Identity;
			
			foreach (string name in MessageData.Keys) {
				try {
					Type type = MessageData[name][0].GetType();
					sb.Append(", " + identities[type]);
					sb.Append(" " + name);
				} catch (Exception err) {
					// type not defined
					err.ToString(); // no warning
				}
			}
			return sb.ToString().Substring(2);
		}
		public SpreadList this[string name]
		{
			get { 
				if (MessageData.ContainsKey(name)) return MessageData[name];
					else return null;				
			} 
			set { MessageData[name] = (SpreadList) value; }
		}
		
		public object Clone() {
			Message m = new Message();
			m.Address = Address;
			m.TimeStamp = TimeStamp;
			
			foreach (string name in MessageData.Keys) {
				SpreadList list = MessageData[name];
				m.Add(name, list.Clone());
				
				// really deep cloning
				try {
					for(int i =0;i<list.Count;i++) {
						list[i] = ((ICloneable)list[i]).Clone();
					}
				} catch (Exception err) {
					err.ToString(); // no warning
					// not cloneble. so keep it
				}
			}
			
			return m;
		}
		
		public override string ToString() {
			var sb = new StringBuilder();
			
			sb.Append("Message "+Address+" ("+TimeStamp+")\n");
			foreach (string name in MessageData.Keys) {
				
				sb.Append(" "+name + " \t: ");
				foreach(object o in MessageData[name]) {
					sb.Append(o.ToString()+" ");
				}
				sb.AppendLine();
			}
			return sb.ToString();
		}
		
		public Stream ToOSC() {
			OSCBundle bundle = new OSCBundle(this.TimeStamp.ToFileTime());
			foreach (string name in MessageData.Keys)  {
				string[] address = Address.Split('.');
				string oscAddress = "";
			
                foreach (string part in address) {
					if (part.Trim() != "") oscAddress += "/" + part;
				}
				
				OSCMessage m = new OSCMessage(oscAddress+"/"+name);
				SpreadList bl = MessageData[name];
				for (int i=0;i<bl.Count;i++) m.Append(bl[i]);
				bundle.Append(m);
			}
			return new MemoryStream(bundle.BinaryData); // packs implicitly
		}
		
		public static Message FromOSC(Stream stream, string messageName = "OSC") {
			MemoryStream ms = new MemoryStream();
			stream.CopyTo(ms);
			byte[] bytes = ms.ToArray();
			int start = 0;
			OSCBundle bundle = OSCBundle.Unpack(bytes, ref start, (int)stream.Length);
			
			Message message = new Message();

//			yet unsupported: 
//			Message.TimeStamp = DateTime.FromFileTime(bundle.getTimeStamp());
			

			foreach (OSCMessage m in bundle.Values) {
				SpreadList sl = new SpreadList();
				sl.AssignFrom(m.Values); // does not clone implicitly
				
				string[] address = m.Address.Split('/');
				string attribName = address[address.Length-1];
				address[address.Length-1] = "";
				
				string messageAddress = "";
				foreach (string part in address) {
					if (part.Trim() != "") messageAddress += "."+part;
				}
				
				if (messageName.Trim() == "") message.Address = messageAddress.Substring(1);
					else message.Address = messageName + messageAddress;

				message[attribName] = sl;

			}
			return message;
		}
	}
}