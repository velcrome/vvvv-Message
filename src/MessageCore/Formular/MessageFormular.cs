using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Messaging
{
    public class MessageFormular // : IEnumerable<FormularFieldDescriptor>, IEnumerable
    {
        public const string DYNAMIC = "None";

        public static ISet<string> ForbiddenNames = new HashSet<string> ( new[]{"", "ID", "Output", "Input", "Message", "Keep"} ); // These names are likely to be pin names
        
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


        public bool Append(FormularFieldDescriptor field, bool isRequired) 
        {
            if (!dict.ContainsKey(field.Name))
            {
                dict[field.Name] = field;
                return true;
            }
            else
            {
                if (dict[field.Name] != field)
                    throw new Exception("Cannot add new Field \"" + field.ToString() + "\" to Formular [" + this.Name + "]. Field is already defined as \"" + dict[field.Name].ToString() + "\".");
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
