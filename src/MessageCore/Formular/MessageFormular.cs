using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Message.Core.Formular
{
    public class MessageFormular
    {
        public static string DYNAMIC = "None";
        private Dictionary<string, Tuple<Type, int>> dict = new Dictionary<string, Tuple<Type, int>>();

        public string Name  {get; set;} 
        
        public ICollection<string> Fields
        {
            get { return dict.Keys; }
        }
        

        public Type GetType(string fieldName)
        {
            return dict[fieldName].Item1;
        }
        public int GetCount(string fieldName)
        {
            return dict[fieldName].Item2;
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

                dict.Add(field, new Tuple<Type, int>(type, count));
            }
        }

        public MessageFormular (string configuration) : this()
        {
            string[] config = configuration.Trim().Split(',');

            foreach (string binConfig in config)
            {
                const string pattern = @"^(\w*?)(\[\d+\])*\s+(\w+?)$"; // "Type[N] name"
                try
                {
                    var binData = Regex.Match(binConfig.Trim(), pattern);

                    Type type = TypeIdentity.Instance.FindType(binData.Groups[1].ToString()); // if alias not found, it will gracefully return string.
                    string name = binData.Groups[3].ToString();

                    int count = binData.Groups[2].Length > 0
                                    ? int.Parse(binData.Groups[2].ToString().TrimStart('[').TrimEnd(']'))
                                    : -1;

                    if (name != "") dict[name] = new Tuple<Type, int>(type, count);
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
                Type type = dict[name].Item1;
                sb.Append(", " + TypeIdentity.Instance.FindBaseAlias(type));
                if (withCount && dict[name].Item2 > 0) sb.Append("[" + dict[name].Item2 + "]");
                sb.Append(" " + name);
            }
            var str = sb.ToString();
            
            return str.Length>0? str.Substring(2) : "";
        }
    }
}
