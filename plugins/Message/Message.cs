#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.Serialization;
using System.Security.Permissions;

using System.Dynamic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Xml;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Nodes;
using VVVV.Core.Logging;
using VVVV.Utils.OSC;
using VVVV.Utils.Collections;

using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Utils.Message{
	
	
	[KnownType(typeof(BinList))]
	[Serializable]
	public class Message : ISerializable, ICloneable {
		
		// The inner dictionary.
		[DataMember]
		Dictionary<string, BinList> dictionary = new Dictionary<string, BinList>();
		
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
		
		protected Message(SerializationInfo info, StreamingContext context)
		{
			// TODO: validate inputs before deserializing. See http://msdn.microsoft.com/en-us/library/ty01x675(VS.80).aspx
			foreach (SerializationEntry entry in info)
			{
				this.Add(entry.Name, entry.Value);
			}
		}
		
		// does not matter if you add a
		public void Add(string name, object val) {
			//			name = name.ToLower();
			if (val is BinList) dictionary.Add(name, (BinList)val);
			else {
				dictionary.Add(name, new BinList());
				((BinList) dictionary[name]).Add(val);
			}
		}
		
		public void AssignFrom(string name, IEnumerable en) {
			//			name = name.ToLower();
			if (!dictionary.ContainsKey(name)) {
				dictionary.Add(name, new BinList());
			} else dictionary[name].Clear();
			
			foreach (object o in en) {
				dictionary[name].Add(o);
			}
		}
		
		public void AddFrom(string name, IEnumerable en) {
			//			name = name.ToLower();
			if (!dictionary.ContainsKey(name)) {
				dictionary.Add(name, new BinList());
			}
			
			foreach (object o in en) {
				dictionary[name].Add(o);
			}
		}
		
		public string GetConfig(Dictionary<Type, string> identities = null) {
			StringBuilder sb = new StringBuilder();
			
			if (identities == null) identities = new MessageResolver().Identity;
			
			foreach (string name in dictionary.Keys) {
				try {
					Type type = dictionary[name][0].GetType();
					sb.Append(", " + identities[type]);
					sb.Append(" " + name);
				} catch (Exception err) {
					// type not defined
					err.ToString(); // no warning
				}
			}
			return sb.ToString().Substring(2);
		}
		
		public IEnumerable<string> GetDynamicMemberNames() {
			
			return dictionary.Keys;
		}
		
		public BinList this[string name]
		{
			get { return dictionary[name]; }
			set { dictionary[name] = (BinList) value; }
		}
		
		public object Clone() {
			Message m = new Message();
			m.Address = Address;
			m.TimeStamp = TimeStamp;
			
			foreach (string name in dictionary.Keys) {
				BinList list = dictionary[name];
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
		
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			foreach (var kvp in dictionary)
			{
				info.AddValue(kvp.Key, kvp.Value);
			}
		}
		
		
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			
			sb.Append("Message "+Address+" ("+TimeStamp+")\n");
			foreach (string name in dictionary.Keys) {
				
				sb.Append(" "+name + " \t: ");
				foreach(object o in dictionary[name]) {
					sb.Append(o.ToString()+" ");
				}
				sb.AppendLine();
			}
			return sb.ToString();
		}
		
		public Stream ToOSC() {
			OSCBundle bundle = new OSCBundle(this.TimeStamp.ToFileTime());
			foreach (string name in dictionary.Keys)  {
				string[] address = Address.Split('.');
				string oscAddress = "";
				foreach (string part in address) {
					if (part.Trim() != "") oscAddress += "/" + part;
				}
				
				OSCMessage m = new OSCMessage(oscAddress+"/"+name);
				BinList bl = dictionary[name];
				for (int i=0;i<bl.Count;i++) m.Append(bl[i]);
				bundle.Append(m);
			}
			return new MemoryStream(bundle.BinaryData); // packs implicitly
		}
		
		public static Message FromOSC(Stream stream) {
			MemoryStream ms = new MemoryStream();
			stream.CopyTo(ms);
			byte[] bytes = ms.ToArray();
			int start = 0;
			OSCBundle bundle = OSCBundle.Unpack(bytes, ref start, (int)stream.Length);
			
			Message message = new Message();

//			yet unsupported: 
//			message.TimeStamp = DateTime.FromFileTime(bundle.getTimeStamp());
			foreach (OSCMessage m in bundle.Values) {
				BinList bl = new BinList();
				
				bl.AssignFrom(m.Values); // does not clone implicitly
				
				string[] address = m.Address.Split('/');
				string name = address[address.Length-1];
				address[address.Length-1] = "";
				
				string messageAddress = "";
				foreach (string part in address) {
					if (part.Trim() != "") messageAddress += "."+part;
				}
				message.Address = messageAddress.Substring(1);
				message[name] = bl;
			}
			return message;
		}
		
		
		
	}
	
	
}