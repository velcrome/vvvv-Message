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
        internal IList<T> Data = new List<T>(1);

        #region Change Management
        /// <summary>Indicates, if a bin has been changed. Will be set to true internally, when the bin was changed</summary>
        public bool IsChanged { get; set; } = true;

        protected object Sweeper {get; set;}

        public bool IsSweeping(object reference = null)
        {
            if (reference == null)
                return Sweeper != null;
            else return Sweeper == reference;
        }

        public void Sweep(object reference = null)
        {
            Sweeper = reference;
        }

        #endregion Change Management


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
                    IsChanged = true;
                }

                // or add
                for (int i = Count; i < value; i++)
                {
                    var defaultValue = TypeIdentity.Instance.NewDefault(this.GetInnerType());
                    this.Add(defaultValue);
                    IsChanged = true;
                }
            }
        }

        /// <summary>Empties the bin</summary>
        public void Clear()
        {
            Data.Clear();
            IsChanged = true;
        }



        /// <summary>Allows slicewise and generic access to the bin. </summary>
        /// <param name="slice">The field to read or written. </param>
        /// <exception cref="ArgumentNullException">This exception is thrown if null is to be written into a bin.</exception>
        /// <exception cref="InvalidCastException">This exception is thrown if an item is attempting to be written to a bin without the type matching.</exception>
        /// <exception cref="IndexOutOfRangeException">This exception is thrown if attempt is made to read a slice that does not exist.</exception>
        public T this[int slice]
        {
            get
            {
                if (Data.Count > slice && slice >= 0) return Data[slice];
                else throw new IndexOutOfRangeException("Accessing bin[" + slice + "] of a Bin with " + Data.Count + " elements is illegal.");
            }
            set
            {
                if (value == null) throw new ArgumentNullException("Cannot set \"null\" as First element of Bin<" + this.GetInnerType().Name + ">.");

                var type = TypeIdentity.Instance.FindBaseType(value.GetType());
                if (type == null)
                    throw new TypeNotSupportedException(value.GetType() + " is not a supported Type in TypeIdentity.cs");

                //if (!typeof(T).IsCastableTo(type)) // as of now: too slow.
                //{
                //    throw new BinTypeMismatchException("Bin " + value.ToString() + " of type " + value.GetType().ToString() + " cannot be the first among " + this.GetInnerType());
                //}

                var tmp = (T)value;
                if (slice > Count || slice < 0) throw new IndexOutOfRangeException("Bin<" + this.GetInnerType().Name + "> has only " + Count + " items. Cannot edit #" + slice);

                if (slice == Count)
                    this.Add(tmp);
                else Data[slice] = tmp;

                IsChanged = true;
            }
        }

        /// <summary>Allows slicewise and generic access to the bin. </summary>
        /// <param name="slice">The field to read or written. </param>
        /// <exception cref="ArgumentNullException">This exception is thrown if null is to be written into a bin.</exception>
        /// <exception cref="InvalidCastException">This exception is thrown if an item is attempting to be written to a bin without the type matching.</exception>
        /// <exception cref="IndexOutOfRangeException">This exception is thrown if attempt is made to read a slice that does not exist.</exception>
        object Bin.this[int slice]
        {
            get { return this[slice]; }
            set {
                if (value == null) throw new ArgumentNullException("Cannot set \"null\" as First element of Bin<" + this.GetInnerType().Name + ">.");
                this[slice] = (T)value;
            }
        }


        /// <summary>Make a string representation of the Message.</summary>
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

        /// <summary>Allows access to the first element of the bin. </summary>
        /// <exception cref="ArgumentNullException">This exception is thrown if null is to be written into a bin.</exception>
        /// <exception cref="TypeNotSupportedException">This exception is thrown if an item is attempting to be written to a bin without a proper registration in TypeIdentity</exception>
        /// <exception cref="InvalidCastException">This exception is thrown if an item is attempting to be written to a bin without the type matching.</exception>
        public T  First
        {
            get
            {
                if (Data.Count == 0)
                {
                    throw new EmptyBinException("Cannot retrieve first element, because Bin<" + this.GetInnerType().Name + "> is empty.");
                } else return Data[0];
            }
            set
            {
                if (value == null) throw new ArgumentNullException("Cannot set \"null\" as First element of Bin<" + this.GetInnerType().Name + ">.");

                var type = TypeIdentity.Instance.FindBaseType(value.GetType());
                if (type == null)
                {
                    throw new TypeNotSupportedException(value.GetType() + " is not a supported Type in TypeIdentity.cs");
                }
                Data[0] = (T)value; // might not work -> InvalidCastException
                IsChanged = true;
            }
        }

        object Bin.First
        {
            get { return this.First; }
            set {
                if (value == null) throw new ArgumentNullException("Cannot set \"null\" as First element of Bin<" + this.GetInnerType().Name + ">.");

                var type = TypeIdentity.Instance.FindBaseType(value.GetType());
                if (type == null)
                {
                    throw new TypeNotSupportedException(value.GetType() + " is not a supported Type in TypeIdentity.cs");
                }

                Data[0] = (T)value; // might not work -> InvalidCastException
                IsChanged = true;
            }
        }

        public Type GetInnerType()
        {
            return typeof(T);
        }

        #endregion Essentials

        #region Methods for filling a Bin
        /// <summary>Think of this as a combined Add and AddRange with internal type checks. it will NOT accept null, and stop adding right then!</summary>
        /// <exception cref="ArgumentNullException">This exception is thrown if null is to be written into a bin.</exception>
        /// <exception cref="InvalidCastException">This exception is thrown if an item is attempting to be written to a bin without the type matching.</exception>
        /// <exception cref="BinTypeMismatchException">This exception is thrown if an item is attempting to be written to a bin without the type matching.</exception>
        public int Add(object val)
        {
            var index = this.Count;  //proper return as of ArrayList.Add()
            if (val == null) throw new ArgumentNullException("Cannot add \"null\" to Bin<" + this.GetInnerType().Name + ">.");

            // if type is in the Registry of allowed basetypes, add this single instance
            Type type = TypeIdentity.Instance.FindBaseType(val.GetType());
            if (type != null)
            {
                try
                {
                    if (val is T)
                    {
                        Data.Add((T)val);
                    }
                    else Data.Add((T)Convert.ChangeType(val, this.GetInnerType())); // close, but no match. so convert.
                }
                catch (Exception e)
                {
                    throw new BinTypeMismatchException("Cannot add (" + val + ") of Type " + val.GetType() + ". It is not convertible to " + this.GetType(), e);
                }

                IsChanged = true;
                return Count;
            }

            //          Add a enumeration
            if (val is IEnumerable) // string should be treated differently, but that is implicit in the lines before
            {
                foreach (var o in (IEnumerable)val)
                {
                    if (o == null) throw new ArgumentNullException("Cannot add \"null\" to a Bin<" + this.GetInnerType().Name + ">.");

                    try
                    {
                        if (o is T)
                        {
                            Data.Add((T)o);
                        }
                        else Data.Add((T)Convert.ChangeType(o, typeof(T))); // close, but no match. so try to convert.

                        IsChanged = true;
                    }
                    catch (Exception e)
                    {
                        throw new BinTypeMismatchException("Cannot add (" + val + ") of Type " + val.GetType() + ". It is not convertible to " + this.GetType(), e);
                    }
                }
                return index;
            }

            throw new TypeNotSupportedException("Cannot add this value (" + val + "). " + val.GetType() + " is neither a Enumeration of matching registered Type nor a matching Type.");
        }

        /// <summary>Replaces the contents of the bin with new data.</summary>
        /// <exception cref="ArgumentNullException">This exception is thrown if null is to be written into a bin.</exception>
        /// <exception cref="InvalidCastException">This exception is thrown if an item is attempting to be written to a bin without the type matching.</exception>
        /// <exception cref="BinTypeMismatchException">This exception is thrown if an item is attempting to be written to a bin without the type matching.</exception>
        public void AssignFrom(IEnumerable enumerable) {
			this.Clear();
    		this.Add(enumerable);
		}

        #endregion

        #region Generic Casting Utils

        /*
         * next three operators are hardly accessible, and maybe even unnecessary...
         * but i like the funny casts they allow when using them through a DynamicObject wrapper
         */

        /*

        public static implicit operator T(BinList<T> bin)
        {
            return (T)bin.First;
        }

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

        */

        #endregion Generic Casting Utils

        #region Equality
        /// <summary>Checks for type- and slicewise equality.</summary>
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

        /// <summary>Checks for type- and slicewise equality against an enumerable.</summary>
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
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }

            return true;
        }

        /// <summary>Checks for type- and slicewise equality against an arbitrary object.</summary>
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
