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

    /// <summary>
    /// Message is a base object to handle strongly typed data in a dynamic way.</summary>
    /// <remarks>
    /// see http://www.github.com/velcrome/vvvv-Message for details
    /// </remarks>    
    [JsonConverter(typeof(JsonMessageSerializer))]
    public class Message : ICloneable //, ISerializable
    {
        #region Properties and FieldNames
        internal Dictionary<string, Bin> Data = new Dictionary<string, Bin>();

        /// <summary>Provides access to the names of all Fields</summary>
        public IEnumerable<string> Fields
        {
            get { return Data.Keys; }
        }

        private string _topic;
        private bool _internalChange = false;

        /// <summary>The topic is a brief identifier of the Message. It can be used to sift or sort Messages quickly.</summary>
        /// <remarks>A topic is best used with a dot-separated Namespace (e.g. MyCompany.Project.Input.Touch)
        /// Only actually changing the topic will mark the message with IsChanged = true
        /// </remarks>
        public string Topic{
			get {
                return _topic;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("Topic cannot be null, empty or clear.");
                if (value.Trim() == _topic) return;

                // todo: validate topic to be namespace-syntax?
                _internalChange = true;
                _topic = value.Trim();
            }
		}

        /// <summary>Returns the last geo-localized timestamp of the Message.</summary>
        /// <remarks>Will be automatically updated with every Sync
        /// Needs the vvvv-Time contribution or nuget.
        /// </remarks>
        public Time TimeStamp
        {
            get;
            set;
        }

        /// <summary>Returns an ad-hoc Formular describing all fields of the Message, or ensures that all fields from a given formular are created in the Message</summary>
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

        /// <summary>Get all changes when Message is Sync'ed</summary>
        public event MessageChangedWithDetails ChangedWithDetails;

        /// <summary>Returns the last geo-localized timestamp of the Message.</summary>
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
        /// <param name="fieldName">The field to be added</param>
        /// <param name="values"></param>
        public void Init(string fieldName, params object[] values)
        {
            AssignFrom(fieldName, values);
        }

        public void Add(string name, params object[] values)
        {
            AddFrom(name, values);
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
        public void AssignFrom(string fieldName, IEnumerable values, Type type = null)
        {
            if (!fieldName.IsValidFieldName()) throw new ParseFormularException("\"" + fieldName + "\" is not a valid name for a Message's field. Only use alphanumerics, dots, hyphens and underscores. ");

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
        public void AddFrom(string fieldName, IEnumerable values)
        {
            if (!Data.ContainsKey(fieldName))
            {
                AssignFrom(fieldName, values);
            }
            else
            {
                Data[fieldName].Add(values); // implicit conversion
            }
        }

        /// <summary>Removes a field from the message</summary>
        /// <param name="fieldName">The field to be added</param>
        /// <returns>False, if fieldName did not exist.</returns>
        public bool Remove(string fieldName)
        {
            var tmp = Data.Remove(fieldName);
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

                if (!newName.IsValidFieldName()) throw new ParseFormularException("Invalid fieldname: \"" + newName+"\".");

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
                if (!fieldName.IsValidFieldName()) throw new ParseFormularException("\"" + fieldName + "\" is not a valid name for a Message's field. Only use alphanumerics, dots, hyphens and underscores. ");
                Data[fieldName] = (Bin) value; 
            }
		}

        #endregion

        #region Matching

        /// <summary>Attempts to conjoin data from another message.</summary>
        /// <param name="message">The message whose data should be injected.</param>
        /// <param name="deepInspection">Flag, whether Fields should be compared for actual change before insertion.</param>
        /// <remarks>The message will update its Topic too, if different.</remarks>
        /// <exception cref="ArgumentNullException">This exception is thrown if message is null.</exception>
        /// <exception cref="InvalidCastException">This exception is thrown if a value is added to a bin that cannot be cast to the bin's type.</exception>
        public void InjectWith(Message message, bool deepInspection)
        {
            if (message == null) throw new ArgumentNullException("Cannot Inject a null Message.");

            if (this.Equals(message)) return; // nothing to do

            if (Topic != message.Topic) Topic = message.Topic; // update Topic only if different

            var fieldNames = message.Fields;

//            bool allowNewFields = true;
//            if (!allowNewFields) fieldNames = fieldNames.Intersect(this.Fields);

            foreach (var name in fieldNames)
            {
                var bin = message[name];

                if (!this.Fields.Contains(name))
                {
                    this.AssignFrom(name, bin); // new bin
                }
                else
                {
                    if (!deepInspection || !this[name].Equals(message[name]))  // inject only if it brings new data
                            this[name].AssignFrom(message.Data[name]); // autocast.
                }
            }
        }
        #endregion

        #region Change Management
        /// <summary>Indicates, if any Field or the topic has been changed since the last Sync.</summary>
        /// <returns>true, if the Message has Changed</returns>        
        /// <remarks></remarks>
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