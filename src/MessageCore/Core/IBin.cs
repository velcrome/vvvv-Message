using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VVVV.Packs.Messaging.Serializing;
using System.Linq;

namespace VVVV.Packs.Messaging
{
    [JsonConverter(typeof(JsonBinSerializer))]
    public interface Bin : ICloneable, ISerializable, IEnumerable, IEquatable<Bin>, IEquatable<IEnumerable> 
    {
        int Count               { get; set; }
        object this[int slice]  { get; set; }
        bool IsDirty            { get; set; }
        object First            { get; set; }

        Type GetInnerType();
        int Add(object item);
        void AssignFrom(IEnumerable enumerable);
        void Clear();
    }

    public interface Bin<T> : Bin, IEnumerable<T>
    {
        // new T [int slice] {get; set;} // does not compile, automatic up casting not possible with c#
        // T First<T> { get; set; } // available from system.ling
    }

    public class BinFactory {
        #region alternative factory constructor for runtime typing of the field
        public static Bin New(Type type, int initialCapacity = 1)
        {
            var baseType = TypeIdentity.Instance.FindBaseType(type);
            if (baseType != null)
            {
                Type genericBin = typeof(BinList<>).MakeGenericType(baseType); // downcast to allowed type, if intial type does not match directly
                var sl = Activator.CreateInstance(genericBin);
                return (Bin)sl;
            }
            else
            {
                throw new TypeNotSupportedException(type.ToString() + " is not a supported Type in vvvv-Message. See TypeIdentity.cs");
            }
        }

        public static Bin<T> New<T>(IEnumerable<T> values)
        {
            var type = typeof(T);

            var bin = New(type, values.Count()) as Bin<T>;
            bin.AssignFrom(values);
            return bin;
        }

        public static Bin<T> New<T>(params T[] values) 
        {
            var type = typeof(T);

            var bin = New(type, values.Length) as Bin<T>;
            bin.AssignFrom(values);
            return bin;
        }

        #endregion
    }



}
