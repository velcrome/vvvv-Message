using System;
using System.Collections.Generic;
using System.Linq;

namespace VVVV.Packs.Messaging
{
    public class MessageFormular
    {
        public const string DYNAMIC = "None";

        public static ISet<string> ForbiddenNames = new HashSet<string> ( new[]{"", "ID", "Output", "Input", "Message", "Keep", "Topic"} ); // These names are likely to be pin names
        
        private Dictionary<string, FormularFieldDescriptor> dict = new Dictionary<string, FormularFieldDescriptor>();

        public string Name { get; set; }
        public string Configuration { 
            get {
                var fieldConfigs = from desc in dict.Values
                          where desc.IsRequired
                          select desc.ToString();
                return string.Join(", ", fieldConfigs.ToArray());            
            }
        } 
        
        public IEnumerable<string> FieldNames
        {
            get { return dict.Keys; }
        }

        public IEnumerable<FormularFieldDescriptor> FieldDescriptors
        {
            get { return dict.Values; }
        }

        public FormularFieldDescriptor this[string name]
        {
            get
            {
                if (dict.ContainsKey(name)) return dict[name];
                else return null;
            }
            set {
                if (value == null) throw new ArgumentNullException("FormularFieldDescriptor");
                if (string.IsNullOrWhiteSpace(name)) throw new IndexOutOfRangeException("Empty strings cannot identify a field in a Formular.");

                dict[name] = (FormularFieldDescriptor)value; 
            }
        }

        public bool IsDynamic { 
            get {
                return this.Name == MessageFormular.DYNAMIC;
            }
        }

        protected MessageFormular()
        {
            Name = "Formular";
        }

        public MessageFormular(Message template, bool withCount = false) : this()
        {
            foreach (string field in template.Fields)
            {
                var type = template[field].GetInnerType();
                var count = withCount ?  template[field].Count : -1;

                dict.Add(field, new FormularFieldDescriptor(type, field, count, true));
            }
        }

        public MessageFormular (string config, string name) : this()
        {
            this.Name = name;
            config = config==null? "" : config.Trim();

            if (config == "") return; // nothing to do here. hand back empty Formular

            string[] configArray = config.Trim().Split(',');

            foreach (string binConfig in configArray)
            {
                var desc = new FormularFieldDescriptor(binConfig, true);
                dict[desc.Name] = desc;
            }
        }

        public MessageFormular(IEnumerable<FormularFieldDescriptor> fields, string name) : this()
        {
            this.Name = name;
            foreach (var field in fields)
            {
//                var f = field.Clone() as FormularFieldDescriptor;
                dict[field.Name] = field;
            }
        }

        /// <summary>Appends a field to an existing Formular, with extensive type checking.</summary>
        /// <param name="field">The field to be added</param>
        /// <param name="isRequired">A bool indicating if the registry should skip informing interested parties about this change.</param>
        /// <exception cref="DuplicateFieldException">This exception is thrown if a syntax error prevents the config to be parsed.</exception>
        public bool Append(FormularFieldDescriptor field, bool isRequired) 
        {
            if (!dict.ContainsKey(field.Name))
            {
                dict[field.Name] = field;
                return true;
            }
            else
            {
                if (!dict[field.Name].Equals(field))
                    throw new DuplicateFieldException("Cannot add new Field \"" + field.ToString() + "\" to Formular [" + this.Name + "]. Field is already defined as \"" + dict[field.Name].ToString() + "\".", field, dict[field.Name]);
            }
            return false;
        }

        public override string ToString()
        {
            return Configuration;
            //return "\"" + Name + "\" " + Configuration;
        }

    }
}
