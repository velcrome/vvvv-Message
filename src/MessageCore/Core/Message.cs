#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using VVVV.Packs.Time;

#endregion usings

namespace VVVV.Packs.Messaging {
    public delegate void MessageChanged(Message original, Message change);

  

	
	[DataContract]
	public class Message : ICloneable
    {
        #region Properties and Fields
        // Access to the the inner Data.
        public IEnumerable<string> Fields
        {
            get { return Data.Keys; }
        }

		[DataMember(Order = 0)]
		public string Topic{
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

        public event MessageChanged Changed;
        #endregion

        #region Constructor
        public Message()
        {
            Topic = "vvvv";
            TimeStamp = Time.Time.CurrentTime(); // init with local timezone
        }

        public Message(string topic)
        {
            Topic = topic;
            TimeStamp = Time.Time.CurrentTime(); // init with local timezone
        }

       
        public Message(MessageFormular formular)
            : base()
        {
            SetFormular(formular);
        }
        #endregion

        #region Formular
        public MessageFormular GetFormular()
        {
			return new MessageFormular(this, true);
		}

        public void SetFormular(MessageFormular formular)
        {
            foreach (string field in formular.Fields)
            {
                Data[field] = Bin.New( formular[field].Type ); // Type
                var count = formular[field].DefaultSize;
                count = count <= -1 ? 1 : count;
                Data[field].SetCount(count); // Count
            }
        }
        #endregion

        #region Bin Handling
        public void Init(string name, params object[] values)
        {
            AssignFrom(name, values);
        }

        public void Add(string name, params object[] values)
        {
            AddFrom(name, values);
        }

        public void AssignFrom(string name, IEnumerable en)
        {
            var obj = en.Cast<object>().DefaultIfEmpty(new object()).First();

            var type = (obj != null) ? TypeIdentity.Instance.FindBaseType(obj.GetType()) : null;

            if (!Data.ContainsKey(name) || ((type != null) && (type != Data[name].GetInnerType())))
            {
                Data.Remove(name);
                Data.Add(name, Bin.New(type));
            }
            else
            {
                Data[name].Clear();
            }

            foreach (object o in en)
            {
                Data[name].Add(o); // implicit cast
            }
        }

        public void AddFrom(string name, IEnumerable en)
        {
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

        #endregion

        #region Bin Access

        public Bin this[string name]
		{
			get { 
				if (Data.ContainsKey(name)) return Data[name];
					else return null;				
			} 
			set { Data[name] = (Bin) value; }
		}

        #endregion

        #region Matching

        //      use simple wildcard pattern: use * for any amount of characters (including 0) or ? for exactly one character.
        public bool AddressMatches(string wildcardPattern)
        {

            var regexPattern = "^" + Regex.Escape(wildcardPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return regex.IsMatch(Topic ?? "");
        }

        public static Message operator +(Message one, Message two)
        {
            one.ReplaceWith(two, true);
            return one;
        }

        public static Message operator *(Message one, Message two)
        {
            one.ReplaceWith(two, false);
            return one;
        }

        protected void ReplaceWith(Message message, bool AllowNew = false)
        {
            if (this.Equals(message)) return;


            var keys = message.Fields;
            if (!AllowNew) keys = keys.Intersect(this.Fields);

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
        #endregion

        #region Change Management

        public bool ConfirmChanges()
        {
            var changedFields = new List<string>();
            foreach (var field in Fields)
            {
                if (Data[field].ConfirmChanges())
                {
                    changedFields.Add(field);
                }
            }

            if (changedFields.Count > 0)
            {
                var changedMessage = new Message(this.Topic);

                foreach (var field in changedFields)
                    changedMessage.Data[field] = Data[field].BackUp;

                Changed(this, changedMessage);
                TimeStamp = Time.Time.CurrentTime();
                
                return true;
            }
            else return false;
        }



        #endregion

        #region Utils
        public object Clone() {
            // might be faster when utilizing binary serialisation.

			Message m = new Message();
			m.Topic = Topic;
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
			
			sb.Append("Message "+Topic+" ("+TimeStamp.LocalTime+" ["+TimeStamp.TimeZone.Id+"])\n");
			foreach (string name in Data.Keys.OrderBy(x => x)) {
				
				sb.Append(" "+name + " \t: ");
				foreach(object o in Data[name]) {
					sb.Append(o.ToString()+" ");
				}
				sb.AppendLine();
			}
			return sb.ToString();
		}
        #endregion 

    }
}