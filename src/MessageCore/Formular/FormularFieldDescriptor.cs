using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
