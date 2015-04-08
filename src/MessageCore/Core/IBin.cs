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
    public interface Bin : ICloneable, ISerializable, IEnumerable, IEquatable<Bin> 
    {
        int Count               { get; set; }
        object this[int slice]  { get; set; }
        bool IsDirty            { get; set; }
        object First            { get; set; }

//        object[] ToArray();

        Type GetInnerType();
        int Add(object item);
        void AssignFrom(IEnumerable enumerable);
        void Clear();
    }

    public interface Bin<T> : Bin, IEnumerable<T>
    {
        // this is not working like that. too bad! still pondering a better way to provide strong typing in these cases...
        
        //new T this[int slice] {get; set;}
        // T First { get; set; }
    }

    public class BinFactory {
        #region alternative factory constructor for runtime typing of the field
        public static Bin New(Type type, int initialCapacity = 1)
        {
            Type spreadType = typeof(BinList<>).MakeGenericType(type);
            if (TypeIdentity.Instance.ContainsKey(type))
            {
                var sl = Activator.CreateInstance(spreadType);
                return (Bin)sl;
            }
            else
            {
                throw new Exception(type.ToString() + " is not a supported Type in TypeIdentity.cs");
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
