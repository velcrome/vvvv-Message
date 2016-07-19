using System;
using System.Collections.Generic;
using System.Linq;

namespace VVVV.Packs.Messaging
{
    public delegate void FormularChanged(object sender, MessageFormular formular);

    public class MessageFormular : ICloneable
    {
        #region fields
        private Dictionary<string, FormularFieldDescriptor> fields = new Dictionary<string, FormularFieldDescriptor>();

        /// <summary>Name of the Formular that can only be used fully dynamically.</summary>
        public const string DYNAMIC = "None";

        /// <summary>The name of the Formular.</summary>
        /// <remarks>Has no checks for name validity yet. Trim yourself.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>Returns all names of all defined Fields</summary>
        /// <remarks>Can be used to iterate or acccess all Fields with the indexer []
        /// </remarks>
        public IEnumerable<string> FieldNames
        {
            get { return fields.Keys; }
        }

        /// <summary>Returns all Field definitions.</summary>
        public IEnumerable<FormularFieldDescriptor> FieldDescriptors
        {
            get { return fields.Values; }
        }


        /// <summary>Convenient way to ask, if the Formular is dynamic only.</summary>
        public bool IsDynamic { 
            get {
                return this.Name == MessageFormular.DYNAMIC;
            }
        }
        #endregion fields


        #region constructor

        protected MessageFormular()
        {
            Name = "Formular";
        }


        /// <summary>Constructor, that uses a given message</summary>
        /// <param name="formularName">The name of the new Formular.</param>
        /// <param name="template">The message whose data structure should be expressed with this Formular.</param>
        /// <param name="withCount">Use the messages bin's size as default size. Otherwise: -1</param>
        public MessageFormular(string formularName, Message template, bool withCount = false) : this()
        {
            if (string.IsNullOrWhiteSpace(formularName)) formularName = DYNAMIC;
            this.Name = formularName.Trim();

            if (template == null || template.IsEmpty) return; 

            foreach (string fieldName in template.Fields)
            {
                var type = template[fieldName].GetInnerType();
                var count = withCount ?  template[fieldName].Count : -1;

                fields.Add(fieldName, new FormularFieldDescriptor(type, fieldName, count, true));
            }
        }


        /// <summary>Constructor, that uses a given message</summary>
        /// <param name="formularName">The name of the new Formular.</param>
        /// <param name="config">Comma separated Configuration string defining the Formular</param>
        public MessageFormular (string formularName, string config) : this()
        {
            if (string.IsNullOrWhiteSpace(formularName)) formularName = DYNAMIC;
            this.Name = formularName.Trim();

            if (string.IsNullOrWhiteSpace(config)) return; // nothing to do here. hand back empty Formular
            string[] configArray = config.Trim().Split(',');

            foreach (string binConfig in configArray)
            {
                var field = new FormularFieldDescriptor(binConfig, true);
                fields[field.Name] = field;
            }
        }

        /// <summary>Constructor, that uses a number of field definitions.</summary>
        /// <param name="formularName">The name of the new Formular.</param>
        /// <param name="fields">The message whose data structure should be expressed with this Formular.</param>
        public MessageFormular(string formularName, IEnumerable<FormularFieldDescriptor> fields) : this()
        {
            this.Name = formularName.Trim();
            foreach (var field in fields)
            {
                if (field != null) 
                    this.fields[field.Name] = field;
            }
        }

        /// <summary>Constructor, that uses a number of field definitions.</summary>
        /// <param name="formularName">The name of the new Formular.</param>
        /// <param name="fields">The message whose data structure should be expressed with this Formular.</param>
        public MessageFormular(MessageFormular formular) : this()
        {
            this.Name = formular.Name;
            foreach (var field in formular.FieldDescriptors)
            {
                if (field != null)
                    this.fields[field.Name] = field;
            }
        }

        #endregion constructor

        #region utils
        /// <summary>Appends a field to an existing Formular, with extensive type checking.</summary>
        /// <param name="field">The field to be added</param>
        /// <param name="isRequired">A bool indicating if the registry should skip informing interested parties about this change.</param>
        /// <exception cref="DuplicateFieldException">This exception is thrown if a syntax error prevents the config to be parsed.</exception>
        /// <returns>success</returns>
        public bool Append(FormularFieldDescriptor field, bool isRequired) 
        {
            if (!fields.ContainsKey(field.Name))
            {
                fields[field.Name] = field;
                field.IsRequired = isRequired;
                return true;
            }
            else
            {
                if (!fields[field.Name].Equals(field))
                    throw new DuplicateFieldException("Cannot add new Field \"" + field.ToString() + "\" to Formular [" + this.Name + "]. Field is already defined as \"" + fields[field.Name].ToString() + "\".", field, fields[field.Name]);

                fields[field.Name].IsRequired |= isRequired;
                return true;
            }
        }

        /// <summary>Checks if a field can be appended to an existing Formular, with extensive type checking.</summary>
        /// <param name="field">The field to be added</param>
        /// <returns>true, if possible to append</returns>
        public bool CanAppend(FormularFieldDescriptor field)
        {
            return !fields.ContainsKey(field.Name) || fields[field.Name].Equals(field); 
        }

        /// <summary>Access a specific Field definition by name</summary>
        /// <param name="fieldName">The message whose data should be injected.</param>
        /// <remarks>returns null, if a Field definition by that name does not exist in the Formular
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if attempt is made to set null.</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown if the fieldName contains invalid characters.</exception>
        public FormularFieldDescriptor this[string fieldName]
        {
            get
            {
                if (fields.ContainsKey(fieldName)) return fields[fieldName];
                else return null;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("FormularFieldDescriptor");
                if (string.IsNullOrWhiteSpace(fieldName)) throw new IndexOutOfRangeException("Empty strings cannot identify a field in a Formular.");

                fields[fieldName] = (FormularFieldDescriptor)value;
            }
        }
        /// <summary>The configuration is a comma-separated list of all required Fields</summary>
        public string Configuration
        {
            get
            {
                var fieldConfigs = from desc in fields.Values
                                   where desc.IsRequired
                                   select desc.ToString();
                return string.Join(", ", fieldConfigs.ToArray());
            }
        }

        /// <summary>Creates a string representation of a given Formular</summary>
        /// <returns>A string representing the Formular</returns>
        public override string ToString()
        {
            return Configuration;
            //return "Formular[\"" + Name + "\"] = " + Configuration;
        }

        /// <summary>
        /// Deep Clone, including all Field descriptors
        /// </summary>
        /// <returns>a FormularFieldDescriptor object</returns>
        public object Clone()
        {
            var descriptors = from desc in FieldDescriptors
                              select desc.Clone() as FormularFieldDescriptor;
            return new MessageFormular(Name, descriptors);
        }

        #endregion utils
    }
}
