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
        public static string DYNAMIC = "None";

        public static ISet<string> ForbiddenNames = new HashSet<string> ( new[]{"ID", "Output", "Input", "Message", "Keep"} ); // These names are likely to be pin names
        
        private Dictionary<string, FormularFieldDescriptor> dict = new Dictionary<string, FormularFieldDescriptor>();

        public string Name { get; set; }
        public string Definition { get; set; } 
        
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

                dict.Add(field, new FormularFieldDescriptor(type, field, count));
            }
            Definition = ToString();
        }

        public MessageFormular (string configuration) : this()
        {
            Definition = configuration==null? "" : configuration.Trim();
            
            if (Definition == "") return; // nothing to do here. hand back empty Formular

            string[] config = configuration.Trim().Split(',');

            foreach (string binConfig in config)
            {
                var desc = new FormularFieldDescriptor(binConfig);
                dict[desc.Name] = desc;
            }
        }

        public MessageFormular(IEnumerable<FormularFieldDescriptor> fields) : this()
        {
            foreach (var field in fields) 
                dict[field.Name] = field;
        }

        public override string ToString()
        {
            var str = dict.Values.Select(desc => desc.ToString());
            return string.Join(", ", str.ToArray());
        }
    }
}
