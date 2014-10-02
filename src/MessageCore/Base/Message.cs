#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using VVVV.Packs.Time;

#endregion usings

namespace VVVV.Packs.Message.Core{
	
	
	[DataContract]
	public class Message : ICloneable {
		
		// The inner MessageData.
		[DataMember]
		Dictionary<string, SpreadList> MessageData = new Dictionary<string, SpreadList>();

        public IEnumerable<string> Attributes
        {
            get { return MessageData.Keys; }
        }

		[DataMember]
		public VVVV.Packs.Time.Time TimeStamp {
			get;
			set;
		}
		
		[DataMember]
		public string Address{
			get;
			set;
		}

	
		public Message()
		{
		    TimeStamp = Time.Time.CurrentTime(); // init with local timezone
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
		
		public string GetConfig() {
			StringBuilder sb = new StringBuilder();
			
			foreach (string name in MessageData.Keys) {
				try {
					Type type = MessageData[name][0].GetType();
					sb.Append(", " + TypeIdentity.Instance.FindBaseAlias(type));
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
			
			sb.Append("Message "+Address+" ("+TimeStamp.LocalTime+" ["+TimeStamp.TimeZone.ToSerializedString()+"])\n");
			foreach (string name in MessageData.Keys) {
				
				sb.Append(" "+name + " \t: ");
				foreach(object o in MessageData[name]) {
					sb.Append(o.ToString()+" ");
				}
				sb.AppendLine();
			}
			return sb.ToString();
		}

//      use simple wildcard pattern: use * for any amount of characters (including 0) or ? for exactly one character.
        public bool AddressMatches(string pattern)
        {

            var regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace).IsMatch(Address);
        }
		

	}
}