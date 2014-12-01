using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Messaging.Core.Formular
{
    public class MessageFormular
    {
        public static string DYNAMIC = "None";

        public static ISet<string> ForbiddenNames = new HashSet<string> ( new[]{"ID", "Output", "Input", "Message", "Storage" } ); // These names are likely to be pin names
        
        private Dictionary<string, FormularFieldDescriptor> dict = new Dictionary<string, FormularFieldDescriptor>();

        public string Name { get; set; }
        public string Definition { get; set; } 
        
        public ICollection<string> Fields
        {
            get { return dict.Keys; }
        }


        public FormularFieldDescriptor this[string name]
        {
            get
            {
                if (dict.ContainsKey(name)) return dict[name];
                else return null;
            }
            set { dict[name] = (FormularFieldDescriptor)value; }
        }

        protected MessageFormular()
        {
            Name = "Formular";
        }

        public MessageFormular(Message template, bool withCount = false) : this()
        {
            foreach (string field in template.Attributes)
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
                const string pattern = @"^(\w*?)(\[\d*\])*\s+(\w+?)$"; // "Type[N] name"
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
