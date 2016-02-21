using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Messaging
{
    public class FormularFieldDescriptor : IEquatable<FormularFieldDescriptor>
    {
 
        public string Name {get; set;}
        public Type Type {get;set;}
        public int DefaultSize {get; set;}

        public FormularFieldDescriptor(Type type, string name, int size = -1)
        {
            Name = name;
            Type = type;
            DefaultSize = size;
        }

        public FormularFieldDescriptor(string config = "") 
        {

            const string pattern = @"^(\w*?)(\[\d*\])*\s+([\w\._-]+?)$"; // "Type[N] name"
            // Name can constitute of alphanumericals, dots, underscores and hyphens.

            try
            {
                var data = Regex.Match(config.Trim(), pattern);

                Type type = TypeIdentity.Instance.FindType(data.Groups[1].ToString()); // if alias not found, it will gracefully return string.
                string name = data.Groups[3].ToString();

                int count = 1;
                if (data.Groups[2].Length > 0)
                {
                    var arrayConnotation = data.Groups[2].ToString();

                    if (arrayConnotation == "[]")
                        count = -1;
                    else count = int.Parse(arrayConnotation.TrimStart('[').TrimEnd(']'));
                }
                if (!MessageFormular.ForbiddenNames.Contains(name))
                {
                    if (name != "")
                    {
                        this.Type = type;
                        this.Name = name;
                        this.DefaultSize = count;
                    }
                }
                else throw new Exception(name + " is a forbidden Name for a field. Sorry, please pick a different one.");
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
    }
}
