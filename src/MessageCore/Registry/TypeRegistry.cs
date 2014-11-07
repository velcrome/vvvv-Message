using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace VVVV.Packs.Message.Core.Registry
{
    public delegate void TypeChangedHandler(TypeRegistry sender, TypeChangedEvent e);

    public class TypeRegistry : Dictionary<string, string>
    {
        private static TypeRegistry _instance;
        public event TypeChangedHandler TypeChanged;


        public static TypeRegistry Instance
        {
            get { return _instance ?? (_instance = new TypeRegistry()); }
        }

        public void Define(string typeName, string configuration)
        {
            this[typeName] = configuration;

            var e = new TypeChangedEvent(typeName, configuration);
            TypeChanged(this, e);
        }

        public Dictionary<string, Tuple<Type, int>> DefinitionFromName(string typeName)
        {
            if (!ContainsKey(typeName)) 
                return new Dictionary<string, Tuple<Type, int>>();
            else return Definition(this[typeName]);
        }


        public Dictionary<string, Tuple<Type, int>> Definition(string configuration)
        {
            var dict = new Dictionary<string, Tuple<Type, int>>();
            string[] config = configuration.Trim().Split(',');

            foreach (string binConfig in config)
            {
                const string pattern = @"^(\D*?)(\[\d+\])*\s+(\w+?)$"; // "Type[N] name"
                try
                {
                    var binData = Regex.Match(binConfig.Trim(), pattern);

                    Type type = TypeIdentity.Instance.FindType(binData.Groups[1].ToString()); // if alias not found, it will gracefully return string.
                    string name = binData.Groups[3].ToString();

                    int count = binData.Groups[2].Length > 0
                                    ? int.Parse(binData.Groups[2].ToString().TrimStart('[').TrimEnd(']'))
                                    : 1;

                    if (name != "") dict[name] = new Tuple<Type, int>(type, count);
                } catch (Exception)
                {
                    throw new Exception("Could not parse \"" + binConfig + "\". Please check Type Configuration");
                }
            }
            return dict;
        }
    }
}