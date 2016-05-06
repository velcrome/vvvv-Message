using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Messaging
{
    /// <summary>
    /// This class is a simple descriptor of structural data about a formulars field.
    /// </summary>
    /// <remarks>
    /// While this functionality could also be done with .net reflections, this is a lot more light weight 
    /// </remarks>
    public class FormularFieldDescriptor : IEquatable<FormularFieldDescriptor>, ICloneable
    {

        #region fields

        public string Name {get; set;}
        public Type Type {get; set;}
        public int DefaultSize {get; set;}
        public bool IsRequired { get; set;}
        #endregion fields

        #region constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        /// <param name="size"></param>
        /// <param name="isRequired"></param>
        /// <exception cref="ParseFormularException">Thrown if the fieldName is not valid.</exception>
        /// <exception cref="TypeNotSupportedException">Thrown if the required type is not supported by TypeIdentity.</exception>
        public FormularFieldDescriptor(Type type, string fieldName, int size = -1, bool isRequired=false)
        {
            if (!fieldName.IsValidFieldName())
                throw new ParseFormularException(fieldName + " is not a valid Name for a field. Sorry, please pick a different one.");

            var baseType = TypeIdentity.Instance.FindBaseType(type);
            if (baseType == null)
                throw new TypeNotSupportedException(type + " is not a valid Type for a Formular field.");

            this.Name = fieldName;
            this.Type = baseType;
            this.DefaultSize = size;
            this.IsRequired = isRequired;
        }

        /// <summary>Constructor that parses a configuration string.</summary>
        /// <param name="config">A path to a directory that will be zipped.</param>
        /// <param name="isRequired">A bool indicating if all fields should be required by force.</param>
        /// <exception cref="ParseFormularException">This exception is thrown if a syntax error prevents the config to be parsed.</exception>
        public FormularFieldDescriptor(string config = "", bool isRequired = false) 
        {
                var data = MessageUtils.ConfigurationParser.Match(config.Trim());

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
        /// <summary>
        /// Checks Equality against another field descriptor
        /// </summary>
        /// <remarks>Does also check, if DefaultSize is equal.</remarks>
        /// <param name="other"></param>
        /// <returns>true if Equal</returns>
        public bool Equals(FormularFieldDescriptor other)
        {
            if ((object)other == null) return false;
            if (other.Name != Name) return false;
            if (other.Type != Type) return false;
            if (other.DefaultSize != DefaultSize) return false;

            return true;
        }

        /// <summary>
        /// Create a string representing the Configuration of the field
        /// </summary>
        /// <param name="withCount">If set to false, indication of the length in the configuration string will be omitted.</param>
        /// <returns>The configuration string. </returns>
        public string ToString(bool withCount = true)
        {
            var sb = new StringBuilder();

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

        /// <summary>
        /// Get a deep copy of a formular field descriptor.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new FormularFieldDescriptor(this.Type, this.Name, this.DefaultSize, this.IsRequired);
        }
        #endregion utils
    }
}
