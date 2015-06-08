using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;

using Newtonsoft.Json;
using VVVV.Packs.Messaging.Serializing;


namespace VVVV.Packs.Messaging
{
    [Serializable]
    [JsonConverter(typeof(JsonBinSerializer))]
    internal class BinList<T> : Bin<T>
    {
        #region Fields
        internal IList<T> Data = new List<T>(1);

        public bool IsDirty { get; set; }

        #endregion

        public BinList()
        {
            IsDirty = true;
        }
 

        #region Essentials
        public int Count
        {
            get { return Data.Count; }
            set 
            {
                // somewhat dirty cast
                var list = Data as List<T>;
                if (list.Capacity < value) list.Capacity = value;

                // trim
                if (value < list.Count)
                {
                    list.RemoveRange(value, list.Count - value);
                    IsDirty = true;
                }

                // or add
                for (int i = Count; i < value; i++)
                {
                    var defaultValue = TypeIdentity.Instance.Default(this.GetInnerType());
                    this.Add(defaultValue);
                    IsDirty = true;
                }
            }
        }

        public void Clear()
        {
            Data.Clear();
            IsDirty = true;
        }

        public object this[int slice]
        {
            get
            {
                if (Data.Count > slice) return Data[slice];
                else return null;
            }
            set
            {
                if (value == null) value = TypeIdentity.Instance.Default(this.GetInnerType());
                var tmp = (T)value;
                Data[slice] = tmp;
                IsDirty = true;
            }
        }

        public override string ToString()
        {
            var s = new StringWriter();
            s.Write("Bin<");
            s.Write(TypeIdentity.Instance.FindAlias(GetInnerType()));
            s.Write("> [");
            for (var i = 0; i < Count; i++)
            {
                s.Write(this[i].ToString());
                if (Count - 1 != i) s.Write(", ");
            }
            s.Write("]");
            return s.ToString();
        }

        // returns a default in any case
        public object First
        {
            get
            {
                if (Data.Count == 0)
                {
                    var type = this.GetInnerType();
                    var o = (T)TypeIdentity.Instance.Default(type);  // if no default defined -> return null
                    Data.Add(o);
                }
                return Data[0];
            }
            set
            {
                if (!TypeIdentity.Instance.ContainsKey(value.GetType()))
                {
                    throw new Exception(value.GetType() + " is not a supported Type in TypeIdentity.cs");
                }
                if (this.GetInnerType() != value.GetType())
                {
                    throw new Exception("Bin " + value.ToString() + " of type "+ value.GetType().ToString() +" cannot be the first among " +this.GetInnerType());
                }
                Data[0] = (T)value;
                IsDirty = true;
            }
        }

        #endregion Essentials

        #region Methods for filling a Bin

//      Think of this as a combined Add and AddRange with internal type checks. it will NOT accept null!
        public int Add(object val)  
        {
            try
            {
                var index = this.Count;  //proper return as of ArrayList.Add()

                if (val == null) return index;
                // throw new Exception("Cannot Add null to "+this.ToString()+".");

                // if type is in the Registry of allowed basetypes, add this single instance
                Type type = TypeIdentity.Instance.FindBaseType(val.GetType());
                if (type != null)
                {
                    if (val is T)
                    {
                        Data.Add((T)val);
                    }
                    else Data.Add((T)Convert.ChangeType(val, this.GetInnerType())); // close, but no match. so convert.

                    IsDirty = true;
                    return Count;
                }

                //          Add a enumeration
                if (val is IEnumerable) // string should be treated differently, but that is implicit in the lines before
                {
                    foreach (var o in (IEnumerable)val)
                    {
                        if (o is T)
                        {
                            Data.Add((T)o);
                        }
                        else Data.Add((T)Convert.ChangeType(o, typeof(T))); // close, but no match. so convert.
                    }

                    IsDirty = true;
                    return index;
                }

            }
            catch (Exception e)
            {
                throw new Exception("Cannot add (" + val + ") of Type " + val.GetType() + ". It is not convertible to "+this.GetType());
            }

            throw new Exception("Cannot add this value ("+val+"). "+ val.GetType() + " is neither a Enumeration of matching registered Type nor a matching Type.");
        }

        public void AssignFrom(IEnumerable enumerable) {
			this.Clear();
    		this.Add(enumerable);
		}

        #endregion

        #region Generic Casting Utils
        public Type GetInnerType()
        {
            return typeof(T);
        }

//        public object[] ToArray()
//        {
//            var tmp = new T[this.Count];
//            ((ICollection)Data).CopyTo(tmp, 0);

////            Object[] tmp = new Object[this.Count];
////            Array.Copy(Data.ToArray(), tmp, this.Count);

//            return tmp;
//        }


        //public T[] ToArray()
        //{
            
        //    return Data.ToArray();
        //}

        public static implicit operator T(BinList<T> sl)
        {
            return (T)sl.First;
        }

        /*
         * next two methods maybe unnecessary...
         * but i like the funny casts they allow when using them through a DynamicObject wrapper
         */
        public static explicit operator BinList<T>(T[] s)  // explicit generic array to Bin conversion operator
        {
            var bin = new BinList<T>();
            bin.AssignFrom(s);
            return bin;
        }
        public static explicit operator BinList<T>(T s)  // explicit generic value to Bin-First conversion operator
        {
            var bin = new BinList<T>();
            bin.Add(s);
            return bin;
        }
        #endregion Generic Casting Utils

        #region Equality
        public bool Equals(Bin other)
        {
            if ((object)other == null) return false;

            //          check for the obvious structural matches
            if (other.Count != Count) return false;
            if (other.GetInnerType() != GetInnerType()) return false;

            //          deep check each element
            for (int i = 0; i < Count; i++)
            {
                if (!other[i].Equals(this[i])) return false;
            }
            return true;
        }

        public bool Equals(IEnumerable other)
        {
            if (other == null) return false;

            try
            {
                // full typematch?
                var othersTyped = other.Cast<T>().ToList();
                
                // same size?
                if (othersTyped.Count != Count) return false;

                //  deep check each element
                for (int i = 0; i < Count; i++)
                {
                    if (!othersTyped[i].Equals(this[i])) return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(Object obj)
        {
            //          check for an obvious Type match
            Bin other = obj as Bin;
            if ((object)other == null)
            {
                return false;
            }

            //          check for the obvious structural matches
            if (other.Count != Count) return false;
            if (other.GetInnerType() != GetInnerType()) return false;

            //          deep check each element
            for (int i = 0; i < Count; i++)
            {
                if (!other[i].Equals(this[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode() * (Count + 1);
        }


        #endregion

        #region ISerializable Members
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            for (int i = 0; i < this.Count; i++)
            {
                info.AddValue(i.ToString(CultureInfo.InvariantCulture), this[i]);
            }
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (T item in Data.ToArray())
            {
                yield return item;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (T item in Data.ToArray())
            {
                yield return item;
            }
        }
        #endregion IEnumerable Members

        #region ICloneable Member
        public object Clone()
        {
            return BinFactory.New(this as IEnumerable<T>);
        }
        #endregion ICloneable Members
    }
    
   
}
