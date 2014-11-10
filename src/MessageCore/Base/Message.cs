#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using VVVV.Pack.Game.Core;
using System.Linq;
using VVVV.Packs.Message.Core.Formular;
using VVVV.Packs.Time;

#endregion usings

namespace VVVV.Packs.Message.Core{
	
	
	[DataContract]
	public class Message : ICloneable {
		
		// The inner Data.
        public IEnumerable<string> Attributes
        {
            get { return Data.Keys; }
        }

		[DataMember(Order = 0)]
		public string Address{
			get;
			set;
		}

        [DataMember(Order = 1)]
        public VVVV.Packs.Time.Time TimeStamp
        {
            get;
            set;
        }
		

        [DataMember(Order = 2)]
        internal Dictionary<string, Bin> Data = new Dictionary<string, Bin>();

	
		public Message()
		{
		    Address = "vvvv";
            TimeStamp = Time.Time.CurrentTime(); // init with local timezone
		}

        public Message(MessageFormular formular) 
        {
            Address = "vvvv";
            TimeStamp = Time.Time.CurrentTime(); // init with local timezone
            SetConfig(formular);
        }

        
        public void Init(string name, params object[] values)
        {
            AssignFrom(name, values);
        }

        public void Add(string name, params object[] values)
        {
            AddFrom(name, values);
        }
		
		public void AssignFrom(string name, IEnumerable en) {
			if (!Data.ContainsKey(name))
			{
			    var o = en.Cast<object>().First();
                var type = TypeIdentity.Instance.FindBaseType(o.GetType());
                Data.Add(name, Bin.New(type));
			} else Data[name].Clear();
			
			foreach (object o in en) {
				Data[name].Add(o); // implicit cast
			}
		}
		
		public void AddFrom(string name, IEnumerable en) {
            if (!Data.ContainsKey(name))
            {
                AssignFrom(name, en);
            }
            else
            {
                foreach (object o in en)
                {
                    Data[name].Add(o); // implicit cast
                }
            }
		}

        public void Remove(string name)
        {
            Data.Remove(name);
        }


        public static Message operator + (Message one, Message two)
	    {
	        one.ReplaceWith(two, true);
            return one;
	    }

        public static Message operator * (Message one, Message two)
        {
            one.ReplaceWith(two, false);
            return one;
        }

        protected void ReplaceWith(Message message, bool AllowNew = false)
        {
            var keys = message.Attributes;
            if (!AllowNew) keys = keys.Intersect(this.Attributes);

            foreach (var name in keys)
            {
                if (!this.Data.ContainsKey(name))
                {
                    this.AssignFrom(name, message.Data[name]);
                }
                else
                {
                    var newType = message.Data[name].GetInnerType();
                    var oldType = this.Data[name].GetInnerType();

                    if (newType.IsCastableTo(oldType)) this.Data[name].AssignFrom(message.Data[name]); // autocast.

                    else
                    {
                        throw new Exception("Cannot replace Bin<" + TypeIdentity.Instance.FindAlias(oldType) +
                                            "> with Bin<" + TypeIdentity.Instance.FindAlias(newType) + "> implicitly.");
                    }
                }
            }
        }
		
		public string GetConfig(bool withCount = false) {
			return new MessageFormular(this).ToString(withCount);
		}

        public void SetConfig(MessageFormular formular)
        {
            foreach (string field in formular.Fields)
            {
                Data[field] = Bin.New(formular.GetType(field)); // Type
                var count = formular.GetCount(field);
                count = count <= -1 ? 1 : count;
                Data[field].SetCount(count); // Count
            }
        }

		public Bin this[string name]
		{
			get { 
				if (Data.ContainsKey(name)) return Data[name];
					else return null;				
			} 
			set { Data[name] = (Bin) value; }
		}
		
		public object Clone() {
			Message m = new Message();
			m.Address = Address;
			m.TimeStamp = TimeStamp;
			
			foreach (string name in Data.Keys) {
				Bin list = Data[name];
				m.AssignFrom(name, list.Clone());
				
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
			
			sb.Append("Message "+Address+" ("+TimeStamp.LocalTime+" ["+TimeStamp.TimeZone.Id+"])\n");
			foreach (string name in Data.Keys.OrderBy(x => x)) {
				
				sb.Append(" "+name + " \t: ");
				foreach(object o in Data[name]) {
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