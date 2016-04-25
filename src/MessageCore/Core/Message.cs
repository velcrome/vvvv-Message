#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using VVVV.Packs.Time;
using Newtonsoft.Json;
using VVVV.Packs.Messaging.Serializing;


#endregion usings

namespace VVVV.Packs.Messaging {
    using Time = VVVV.Packs.Time.Time;

    public delegate void MessageChangedWithDetails(Message original, Message change);
    public delegate void MessageChanged(Message original);
	
//	[DataContract]
    [JsonConverter(typeof(JsonMessageSerializer))]
	public class Message : ICloneable //, ISerializable
    {
        #region Properties and FieldNames
        // Access to the the inner Data.
        public IEnumerable<string> Fields
        {
            get { return Data.Keys; }
        }

        private string _topic;
        private bool _internalChange = false;
        
        public string Topic{
			get {
                return _topic;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("Topic cannot be null, empty or clear.");
                // todo: validate topic to be namespace-syntax?
                _internalChange = true;
                _topic = value;
            }
		}

        public Time TimeStamp
        {
            get;
            set;
        }

        public MessageFormular Formular
        {
            get
            {
                return new MessageFormular(this, true);
            }
            set
            {
                foreach (string field in value.FieldNames)
                {
                    Data[field] = BinFactory.New(value[field].Type); // Type
                    var count = value[field].DefaultSize;
                    count = count <= -1 ? 1 : count;
                    Data[field].Count = count;
                }
            }
        }

        internal Dictionary<string, Bin> Data = new Dictionary<string, Bin>();

        public event MessageChangedWithDetails ChangedWithDetails;
        public event MessageChanged Changed;
        #endregion

        #region Constructor
        public Message()
        {
            Topic = "vvvv";
            TimeStamp = Time.CurrentTime(); // init with local timezone
        }

        public Message(string topic)
        {
            Topic = topic;
            TimeStamp = Time.CurrentTime(); // init with local timezone
        }

       
        public Message(MessageFormular formular)
            : base()
        {
            Formular = formular;
        }
        #endregion

        #region Bin Handling
 
        /// <summary>Initializes a new named Field. Use comma separated values to init the field.</summary>
        /// <param name="field">The field to be added</param>
        /// <param name="values"></param>
        public void Init(string name, params object[] values)
        {
            AssignFrom(name, values);
        }

        public void Add(string name, params object[] values)
        {
            AddFrom(name, values);
        }

        /// <summary>Initializes a new field, or overwrites an existing one. Can optionally be typed strongly.</summary>
        /// <param name="field">The field to be added</param>
        /// <param name="values">IEnumerable containing </param>
        /// <param name="type"></param>
        /// <exception cref="ParseFormularException">This exception is thrown if the fieldName contains invalid characters.</exception>
        /// <exception cref="EmptyBinException">This exception is thrown if values is or contains null.</exception>
        /// <exception cref="InvalidCastException">This exception is thrown if a value is added to a bin that cannot be cast to the bin's type.</exception>
        /// <exception cref="BinTypeMismatchException">This exception is thrown if a value is added to a bin that cannot be cast to the bin's type.</exception>
        /// <exception cref="TypeNotSupportedException">This exception is thrown if the new bin's type is not registered in TypeIdentity.</exception>
        /// <exception cref="ArgumentNullException">This exception is thrown if attempt is made to add null to a the bin.</exception>
        public void AssignFrom(string fieldName, IEnumerable values, Type type = null)
        {
            if (!FormularFieldDescriptor.IsValidFieldName(fieldName)) throw new ParseFormularException("\"" + fieldName + "\" is not a valid name for a Message's field. Only use alphanumerics, dots, hyphens and underscores. ");

            if (type == null)
            {
                if (values == null) throw new ArgumentNullException("Attempted to Assign null to a new Bin.");

                var gen = values.GetType().GenericTypeArguments;

                // in case en is not generic, pick the first one and reflect
                if (gen == null || gen.Count() != 1)
                {
                    var tmp = values.Cast<object>();

                    if (tmp.Count() == 0) throw new EmptyBinException("Cannot add an empty bin without information about its type. Consider adding it to AssignFrom()");

                    var obj = tmp.FirstOrDefault();

                    if (obj == null) throw new ArgumentNullException("Cannot assign null to a new Bin.");
                    type = obj.GetType();
                }
                else type = values.GetType().GenericTypeArguments[0];
            }

            type = TypeIdentity.Instance.FindBaseType(type); // break it down.
            if (type == null) throw new TypeNotSupportedException("The assignment for the Field [" + fieldName + "] failed, type is not supported: " + this.Topic);

            // replace if type does not match
            if (!Data.ContainsKey(fieldName) || type != Data[fieldName].GetInnerType())
            {
                Data.Remove(fieldName);
                Data.Add(fieldName, BinFactory.New(type));
            }
            else
            {
                Data[fieldName].Clear();
            }

            Data[fieldName].Add(values); // implicit conversion
        }

        /// <summary>Initializes a new field, or overwrites an existing one. Can optionally be typed strongly.</summary>
        /// <param name="fieldName">The field to be added</param>
        /// <param name="values">IEnumerable containing </param>
        /// <param name="type"></param>
        /// <exception cref="ParseFormularException">This exception is thrown if the fieldName contains invalid characters.</exception>
        /// <exception cref="EmptyBinException">This exception is thrown if values is or contains null.</exception>
        /// <exception cref="InvalidCastException">This exception is thrown if a value is added to a bin that cannot be cast to the bin's type.</exception>
        /// <exception cref="BinTypeMismatchException">This exception is thrown if a value is added to a bin that cannot be cast to the bin's type.</exception>
        /// <exception cref="TypeNotSupportedException">This exception is thrown if the new bin's type is not registered in TypeIdentity.</exception>
        /// <exception cref="ArgumentNullException">This exception is thrown if attempt is made to add null to a the bin.</exception>
        public void AddFrom(string fieldname, IEnumerable values)
        {
            if (!Data.ContainsKey(fieldname))
            {
                AssignFrom(fieldname, values);
            }
            else
            {
                Data[fieldname].Add(values); // implicit conversion
            }
        }

        /// <summary>Removes a field from the message</summary>
        /// <param name="fieldName">The field to be added</param>
        /// <returns>False, if fieldName did not exist.</returns>
        public bool Remove(string fieldname)
        {
            var tmp = Data.Remove(fieldname);
            if (tmp) _internalChange = true;
            return tmp;
        }

        /// <summary>Initializes a new field, or overwrites an existing one. Can optionally be typed strongly.</summary>
        /// <param name="fieldName">The field to be renamed</param>
        /// <param name="newName">The new name of the field.</param>
        /// <param name="overwrite">Allow renaming, even if the new Name already exists.</param>
        /// <exception cref="ParseFormularException">This exception is thrown if the fieldName contains invalid characters.</exception>
        /// <exception cref="EmptyBinException">This exception is thrown the field does not exist.</exception>
        /// <exception cref="DuplicateFieldException">This exception is thrown if a field with newName already exists, and overwrite is set to false.</exception>
        /// <returns>success</returns>
        public bool Rename(string fieldName, string newName, bool overwrite=false)
        {
            if (!overwrite && Data.ContainsKey(newName)) throw new DuplicateFieldException("Field with the name \"" + newName+"\" already exists. Consider to allow overwriting", null, null);
            else
            {
                if (!Data.ContainsKey(fieldName)) throw new EmptyBinException("Field with the name \"" + fieldName + "\" does not exist.");
                var bin = Data[fieldName];

                if (!FormularFieldDescriptor.IsValidFieldName(newName)) throw new ParseFormularException("Invalid fieldname: \"" + newName+"\".");

                Data[newName] = bin;
                bin.IsDirty = true;

                Remove(fieldName);
            }
            return true;
        }


        #endregion

        #region Bin Access

        /// <summary>Initializes a new field, or overwrites an existing one. Can optionally be typed strongly.</summary>
        /// <param name="fieldName">The field to be renamed</param>
        /// <param name="newName">The new name of the field.</param>
        /// <param name="overwrite">Allow renaming, even if the new Name already exists.</param>
        /// <exception cref="ParseFormularException">This exception is thrown if the fieldName contains invalid characters.</exception>
        /// <exception cref="DuplicateFieldException">This exception is thrown if a field with newName already exists, and overwrite is set to false.</exception>
        /// <returns>success</returns>
        public Bin this[string fieldName]
		{
			get { 
				if (Data.ContainsKey(fieldName)) return Data[fieldName];
					else return null;				
			} 
			set {
                if (!FormularFieldDescriptor.IsValidFieldName(fieldName)) throw new ParseFormularException("\"" + fieldName + "\" is not a valid name for a Message's field. Only use alphanumerics, dots, hyphens and underscores. ");
                Data[fieldName] = (Bin) value; 
            }
		}

        #endregion

        #region Matching

        //      use simple wildcard pattern: use * for any amount of characters (including 0) or ? for exactly one character.
        public static Regex CreateWildCardRegex(string wildcardPattern)
        {
            var regexPattern = "^" + Regex.Escape(wildcardPattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return regex;
        }

        //public bool TopicMatch(Regex regex)
        //{
        //    return regex.IsMatch(Topic ?? "");
        //}

        public void InjectWith(Message message, bool allowNewFields, bool deepInspection = false)
        {
            if (this.Equals(message)) return;

            this.Topic = message.Topic;

            var keys = message.Fields;
            if (!allowNewFields) keys = keys.Intersect(this.Fields);

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

                    if (newType.IsCastableTo(oldType))
                    {
                        if (!deepInspection || !Data[name].Equals(message.Data[name]))  // inject only if it brings new data
                            Data[name].AssignFrom(message.Data[name]); // autocast.
                    }
                    else
                    {
                        throw new BinTypeMismatchException("Cannot replace Bin<" + TypeIdentity.Instance.FindAlias(oldType) +
                                            "> with Bin<" + TypeIdentity.Instance.FindAlias(newType) + "> implicitly.");
                    }
                }
            }
        }
        #endregion

        #region Change Management
        /// <summary>Indicates, if any Field or the topic has been changed since the last Sync.</summary>
        /// <returns>true, if the Message has Changed</returns>        
        public bool IsChanged
        {
            // check all bins, if at least one has changed since the last Sync.
            get {
                return _internalChange || Data.Values.Any(bin => bin.IsDirty);
            }
            
            // reset all bins, if IsChanged is forced to false.
            internal set {
                if (!value)
                {
                    foreach (var bin in Data.Values)
                        if (bin.IsDirty) bin.IsDirty = false;
                    _internalChange = value;
                }
            }
        }


        /// <summary>Sets the IsChanged flag to false, and publishes any changes. The timestamp will be updated to Now.</summary>
        /// <returns>true only if the Message had been changed.</returns>
        public bool Sync()
        {
            var changedFields = new List<string>();
            foreach (var field in Fields)
            {
                if (Data[field].IsDirty)
                {
                    changedFields.Add(field);
                    Data[field].IsDirty = false;
                }
            }
            
            if (changedFields.Count > 0 || _internalChange)
            {
                TimeStamp = Time.CurrentTime(); // timestamp always shows last Synced Change.

                if (ChangedWithDetails != null) // for all subscribers with detailled interest
                {
                    var changedMessage = new Message(this.Topic);
                    changedMessage.TimeStamp = TimeStamp;

                    foreach (var field in changedFields)
                        changedMessage.Data[field] = Data[field].Clone() as Bin;  // deep clone

                    ChangedWithDetails(this, changedMessage); // inform all subscribers of this particular Message
                }

                if (Changed != null) // for all subscribers with only superficial interest.
                {
                    Changed(this);
                }

                _internalChange = false;
                return true;
            }
            else return false;
        }



        #endregion

        #region Utils
        /// <summary>Checks if the Message contains any fields</summary>
        /// <returns>true, if the Message contains no fields.</returns>
        public bool IsEmpty
        {
            get
            {
                return Fields.Count() == 0;
            }
        }

        /// <summary>Checks if the Message contains a specific field</summary>
        /// <param name="fieldName">The field to be checked</param>
        /// <returns>true, if the Message contains the field.</returns>
        public bool Contains(string fieldName)
        {
            return Data.ContainsKey(fieldName);
        }

        /// <summary>Deep Clones the given Message. Fields of type Message will not be deep cloned.</summary>
        /// <returns>A new Message</returns>
        public object Clone() {
            // might be faster when utilizing binary serialisation.

			Message m = new Message();
			m.Topic = Topic;
			m.TimeStamp = TimeStamp;
			
			foreach (string name in Data.Keys) {
				var list = Data[name];
                var type = list.GetInnerType();
                var newList = BinFactory.New(type, list.Count);

				// deep cloning of all fields, but messages: nested messages are merely a reference by design.
                bool isPrimitiveType = type.IsPrimitive || type.IsValueType || (type == typeof(string));

                if (isPrimitiveType || type == typeof(Message))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        newList.Add(list[i]); // if primitive -> auto copy. if Message -> reference only
                    }
                }
                else
                {
                    if (typeof(ICloneable).IsAssignableFrom(type)) {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var clone = ((ICloneable)list[i]).Clone();
                            newList.Add(clone);
                        }
                    } else throw new SystemException(type.FullName + " cannot be cloned nor copied, while cloning the message "+this.Topic);
                }
                
                m[name] = newList; // add list to new Message
            }
			return m;
		}

        /// <summary>Creates a string representation of a given Message</summary>
        /// <returns>A string representing the Message</returns>
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