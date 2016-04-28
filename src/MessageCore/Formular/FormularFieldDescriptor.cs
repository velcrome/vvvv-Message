using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Messaging
{
    public class FormularFieldDescriptor : IEquatable<FormularFieldDescriptor>, ICloneable
    {

        #region fields

        public string Name {get; set;}
        public Type Type {get; set;}
        public int DefaultSize {get; set;}
        public bool IsRequired { get; set;}
        #endregion fields

        #region constructors
        public FormularFieldDescriptor(Type type, string fieldName, int size = -1, bool isRequired=false)
        {
            if (!fieldName.IsValidFieldName())
                throw new ParseFormularException(fieldName + " is a forbidden Name for a field. Sorry, please pick a different one.");
            
            if (!TypeIdentity.Instance.ContainsKey(type))
                throw new TypeNotSupportedException(type + " is not a valid Type for a MessageFormular.");

            this.Name = fieldName;
            this.Type = type;
            this.DefaultSize = size;
            this.IsRequired = isRequired;
        }

        /// <summary>Constructor that parses a configuration string.</summary>
        /// <param name="config">A path to a directory that will be zipped.</param>
        /// <param name="isRequired">A bool indicating if all fields should be required by force.</param>
        /// <exception cref="ParseFormularException">This exception is thrown if a syntax error prevents the config to be parsed.</exception>
        public FormularFieldDescriptor(string config = "", bool isRequired = false) 
        {
                var data = MessageUtils.Parser.Match(config.Trim());

                Type type = TypeIdentity.Instance.FindType(data.Groups[1].ToString()); // if alias not found, it will return null
                string fieldName = data.Groups[3].ToString();

                int count = 1;
                if (data.Groups[2].Length > 0)
                {
                    var arrayConnotation = data.Groups[2].ToString().Trim();

                    if (arrayConnotation == "[]")
                        count = -1;
                    else count = int.Parse(arrayConnotation.TrimStart('[').TrimEnd(']').Trim());
                }
                if (!fieldName.IsValidFieldName()) 
                    throw new ParseFormularException(fieldName + " is a forbidden Name for a field. Sorry, please pick a different one.");
                
                if (type == null || fieldName == "")  
                    throw new ParseFormularException("Could not parse "+config);

               this.Type = type;
               this.Name = fieldName;
               this.DefaultSize = count;
               this.IsRequired = isRequired;
        }

        #endregion constructors

        #region utils
        public bool Equals(FormularFieldDescriptor other)
        {
            if ((object)other == null) return false;
            if (other.Name != Name) return false;
            if (other.Type != Type) return false;
            if (other.DefaultSize != DefaultSize) return false;

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
        #endregion utils
    }
}
