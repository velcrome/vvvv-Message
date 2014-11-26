using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Message.Core.Formular
{
    public class MessageFormular
    {
        public static string DYNAMIC = "None";
        private Dictionary<string, FormularFieldDescriptor> dict = new Dictionary<string, FormularFieldDescriptor>();

        public string Name  {get; set;} 
        
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
        }

        public MessageFormular (string configuration) : this()
        {
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
                    if (name != "") dict[name] = new FormularFieldDescriptor(type, name, count);
                }
                catch (Exception)
                {
                    throw new Exception("Could not parse \"" + binConfig + "\". Please check Formular Configuration");
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
                if (withCount && dict[name].DefaultSize > 0) sb.Append("[" + dict[name].DefaultSize + "]");
                sb.Append(" " + name);
            }
            var str = sb.ToString();
            
            return str.Length>0? str.Substring(2) : "";
        }
    }
}
