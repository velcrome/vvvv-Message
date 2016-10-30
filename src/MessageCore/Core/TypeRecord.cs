using System;

namespace VVVV.Packs.Messaging
{
    public enum CloneBehaviour
    {
        Clone,
        Assign,
        Null, 
        Custom
    }

    public interface TypeRecord
    {
        string Alias { get; }
        Type Type { get;  }

        CloneBehaviour Clone { get; }
        Func<object, object> CustomClone { get;  }
        Func<object> Default { get; set; }

    }

    public class TypeRecord<T> : TypeRecord
    {
        public TypeRecord(string alias, CloneBehaviour cloning, Func<object> newDefault = null)
        {
            Alias = alias;
            Clone = cloning;

            if (newDefault == null) newDefault = () => null;
            Default = newDefault;
        }

        public string Alias { get; private set; }

        public CloneBehaviour Clone { get; private set; }

        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }

        public Func<object> Default { get; set; }

        public Func<object, object> CustomClone
        {
            get;
            set;
        }
    }
}
