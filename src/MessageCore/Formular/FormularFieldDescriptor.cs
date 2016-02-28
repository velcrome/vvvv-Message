using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Messaging
{
    public class FormularFieldDescriptor : IEquatable<FormularFieldDescriptor>, ICloneable
    {
 
        public string Name {get; set;}
        public Type Type {get; set;}
        public int DefaultSize {get; set;}
        public bool IsRequired { get; set;}

        public FormularFieldDescriptor(Type type, string name, int size = -1, bool isRequired=false)
        {
            if (MessageFormular.ForbiddenNames.Contains(name))
                throw new Exception(name + " is a forbidden Name for a field. Sorry, please pick a different one.");
            
            if (!TypeIdentity.Instance.ContainsKey(type))
                throw new Exception(type + " is not a valid Type for a MessageFormular.");

            this.Name = name;
            this.Type = type;
            this.DefaultSize = size;
            this.IsRequired = isRequired;
        }

        public FormularFieldDescriptor(string config = "", bool isRequired = false) 
        {

            const string pattern = @"^(\w*?)(\[\d*\])*\s+([\w\._-]+?)$"; // "Type[N] name"
            // Name can constitute of alphanumericals, dots, underscores and hyphens.

            try
            {
                var data = Regex.Match(config.Trim(), pattern);

                Type type = TypeIdentity.Instance.FindType(data.Groups[1].ToString()); // if alias not found, it will return null
                string name = data.Groups[3].ToString();

                int count = 1;
                if (data.Groups[2].Length > 0)
                {
                    var arrayConnotation = data.Groups[2].ToString();

                    if (arrayConnotation == "[]")
                        count = -1;
                    else count = int.Parse(arrayConnotation.TrimStart('[').TrimEnd(']'));
                }
                if (MessageFormular.ForbiddenNames.Contains(name)) 
                    throw new Exception(name + " is a forbidden Name for a field. Sorry, please pick a different one.");
                
                if (type == null || name == "")  
                    throw new Exception("Could not parse "+config);

               this.Type = type;
               this.Name = name;
               this.DefaultSize = count;
               this.IsRequired = isRequired;
            }
            catch (Exception e)
            {
                throw new Exception("Could not parse \"" + config + "\". Please check Formular Configuration for syntax Error.", e);
            }
        }

        public bool Equals(FormularFieldDescriptor other)
        {
            if ((object)other == null) return false;
            if (other.Name != Name) return false;
            if (other.Type != Type) return false;

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var withCount = true;

            sb.Append(TypeIdentity.Instance.FindAlias(Type));
            if (withCount && DefaultSize != 1)
            {

                sb.Append("[");
                if (DefaultSize > 0) sb.Append(DefaultSize);
                sb.Append("]");
            }
            sb.Append(" " + Name);
            return sb.ToString();
        }

        public object Clone()
        {
            var c = new FormularFieldDescriptor(this.Type, this.Name, this.DefaultSize, this.IsRequired);
            return c;
        }
    }
}
