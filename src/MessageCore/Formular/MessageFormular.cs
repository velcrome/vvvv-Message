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
        
        public ICollection<string> FieldNames
        {
            get { return dict.Keys; }
        }

        public ICollection<FormularFieldDescriptor> FieldDescriptors
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


/*
        #region IEnumerable Members
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var item in dict.Values.ToList())
            {
                yield return item;
            }
        }

        IEnumerator<FormularFieldDescriptor> IEnumerable<FormularFieldDescriptor>.GetEnumerator()
        {
            foreach (var field in dict.Values.ToList())
            {
                yield return field;
            }
        }
        #endregion IEnumerable Members
*/


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
            Definition = configuration;
            string[] config = configuration.Trim().Split(',');

            foreach (string binConfig in config)
            {
                const string pattern = @"^(\w*?)(\[\d*\])*\s+([\w\._-]+?)$"; // "Type[N] name"
                // Name can constitute of alphanumericals, dots, underscores and hyphens.

                try
                {
                    var binData = Regex.Match(binConfig.Trim(), pattern);

                    Type type = TypeIdentity.Instance.FindType(binData.Groups[1].ToString()); // if alias not found, it will gracefully return string.
                    string name = binData.Groups[3].ToString();

                    int count = 1;
                    if (binData.Groups[2].Length > 0) {
                        var arrayConnotation = binData.Groups[2].ToString();

                        if (arrayConnotation == "[]") 
                            count = -1;
                            else count = int.Parse(arrayConnotation.TrimStart('[').TrimEnd(']'));
                    }
                    if (!ForbiddenNames.Contains(name))
                    {
                        if (name != "") dict[name] = new FormularFieldDescriptor(type, name, count);
                    }
                    else throw new Exception(name + " is a forbidden Name for a field. Sorry, please pick a different one.");
                }
                catch (Exception e)
                {
                    throw new Exception("Could not parse \"" + binConfig + "\". Please check Formular Configuration for syntax Error.", e);


                }
            }
        }

        public string ToString(bool withCount = false)
        {
			StringBuilder sb = new StringBuilder();

            foreach (string name in dict.Keys)
            {
                Type type = dict[name].Type;
                sb.Append(", " + TypeIdentity.Instance.FindBaseAlias(type));
                if (withCount && dict[name].DefaultSize != 1)
                {

                    sb.Append("[");
                    if (dict[name].DefaultSize > 0) sb.Append(dict[name].DefaultSize);
                    sb.Append("]");
                }
                sb.Append(" " + name);
            }
            var str = sb.ToString();
            
            return str.Length>0? str.Substring(2) : ""; // clean leading ", "
        }
    }
}
