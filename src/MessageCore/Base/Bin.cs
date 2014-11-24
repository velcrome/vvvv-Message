using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using VVVV.Pack.Game.Core.Serializing;


namespace VVVV.Packs.Message.Core
{
    [Serializable]
    [JsonConverter(typeof(JsonBinSerializer))]
    public class Bin<T> : Bin, IEnumerable<T> 
    {
        public Bin()
        {
        }
        
        
        public Bin (params T[] values) 
        {
            foreach (var v in values)
            {
                Add(v);
            }

        }

        public Bin(IEnumerable<T> values)
        {
            Add(values);
        } 

        public override Type GetInnerType()
        {
            return typeof(T);
        }

        public new T[] ToArray()
        {
            return (T[])this.ToArray(typeof(T));
        }

        public static implicit operator T(Bin<T> sl)
        {
            return (T)sl.First;
        }

        /*
         * next two methods maybe unnecessary...
         * but i like the funny casts they allow 
         */
        public static explicit operator Bin<T>(T[] s)  // explicit generic array to Bin conversion operator
        {
            return new Bin<T>(s);  
        }
        public static explicit operator Bin<T>(T s)  // explicit generic value to Bin-First conversion operator
        {
            return new Bin<T>(s);  
        }

        // necessary implementation for IEnumerable<T>
        public new IEnumerator<T> GetEnumerator()
        {
            foreach (T item in this.ToArray())
            {
                yield return item;
            }
        }
    }
    
    
	[Serializable]
    [JsonConverter(typeof(JsonBinSerializer))]
	public abstract class Bin : ArrayList, ISerializable
	{
//      constructor
        #region Essentials
        
        protected Bin(params object[] values): base()
        {
            foreach (var v in values)
            {
                Add(v);
            }

        }
        
        public virtual Type GetInnerType()
        {
    		if (this.Count == 0) return typeof(object);
				else return this[0].GetType();
		}

        public virtual object First
        {
            get
            {
                if (this.Count == 0)
                {
                    var type = this.GetInnerType();
                    object o;
                    if (type == typeof (string)) o = "vvvv";
                    else if (type == typeof (Stream)) o = new MemoryStream();
                    else o = Activator.CreateInstance(type);

                    base.Add(o);
                }
                return this[0];
            }
            set
            {
                this[0] = value;
                if (!TypeIdentity.Instance.ContainsKey(value.GetType()))
                {
                    throw new Exception(value.GetType() + " is not a supported Type in TypeIdentity.cs");
                }
                if (this.GetInnerType() != value.GetType())
                {
                    throw new Exception(value.GetType().ToString() +" cannot be the first among " +this.GetInnerType());
                }
            }
        }

        public int SetCount(int newCount) 
        {
            var defaultValue = TypeIdentity.Instance.Default(TypeIdentity.Instance.FindAlias(GetInnerType()));
            for (int i = Count; i < newCount ;i++) this.Add(defaultValue);
            return Count;
        }

        public new Bin Clone()
        {
            Bin c = Bin.New(this.GetInnerType());
            c.AssignFrom(this);
            return c;
        }

        public override bool Equals(Object obj)
        {
//            if (obj == null) return false;

            Bin other = obj as Bin;
            if ((object) other == null)
            {
                return false;
            }

            if (other.Count != Count) return false;
            if (other.GetInnerType() != GetInnerType()) return false;

            for (int i = 0; i < Count; i++)
            {
                if (!other[i].Equals(this[i])) return false;
            }

            return true;
        }
        
        public static bool operator ==(Bin a, Bin other)
        {
            return a.Equals(other as Object);
        }

        public static bool operator !=(Bin a, Bin other)
        {
            return !(a.Equals(other));
        }



        public override int GetHashCode()
        {
            return base.GetHashCode() * (Count + 1);
        }

        public override string ToString()
        {
            var s = new StringWriter();
            s.Write("Bin<");
            s.Write(TypeIdentity.Instance.FindAlias(GetInnerType()));
            s.Write("> [");
            for (var i=0;i<Count;i++)
            {
                s.Write(this[i].ToString());
                if (Count-1 != i) s.Write(", ");
            }
            s.Write("]");
            return s.ToString();
        }
        #endregion

        #region [Serializable] Necessary Method implementation
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			for (int i=0;i<this.Count;i++)
			{
				info.AddValue(i.ToString(CultureInfo.InvariantCulture), this[i]);
				
			}
		}
        #endregion

        #region ISpread vs. ArrayList compromise

//      Think of this as a combined Add and AddRange with internal type checks.
        public override int Add(object val)
        {
            var index = this.Count;  //proper return as of ArrayList.Add()
            
//          Add a single instance
            Type type = TypeIdentity.Instance.FindBaseType(val.GetType());

            if (type != null)
            {
                if (val is IConvertible)
                {
                    return base.Add(Convert.ChangeType(val, this.GetInnerType()));
                }
                else return base.Add(val);
            }

//          Add a enumeration
            if (val is IEnumerable ) // string should be treated differently, but that is implicit in the lines before
            {
                var enumerable = (IEnumerable)val;
                try
                {
                    var num = enumerable.GetEnumerator();
                    num.MoveNext();
                    type = TypeIdentity.Instance.FindBaseType(num.Current.GetType());
                }
                catch (Exception e)
                {
                    throw new Exception("Cannot add object " + enumerable.ToString() + " to Bin because cannot determine type. Maybe empty?", e);
                }

                if (type != null && this.GetInnerType() == type)
                {
                    foreach (var o in enumerable)
                    {
                        if (!this.GetInnerType().IsAssignableFrom(o.GetType())) 
                            throw new Exception("Cannot add object " + enumerable.ToString() + " from "+enumerable+ " to Bin because Type of inside element does not match");
                        base.Add(o);
                    }
                    return index;
                }
            } 

            throw new Exception("Cannot add this value,"+ val.GetType() + " is neither a Enumeration of matching registered Type nor a matching Type.");
        }

        public void AssignFrom(IEnumerable enumerable) {
			this.Clear();
    		this.Add(enumerable);
            
		}

        // TODO: implement if necessary - with necessary Type checks in place
        protected new void Insert(int index, object value)
        {
            throw new NotImplementedException("Insert not implemented");
        }

        // TODO: implementation not recommended, LINQ depends on IEnumerable
        protected new void InsertRange(int index, ICollection c)
        {
            throw new NotImplementedException("InsertRange not implemented");
        }


        #endregion

        #region alternative factory constructor for runtime typing of the bin
        public static Bin New(Type type)
        {
            Type spreadType = typeof(Bin<>).MakeGenericType(type);
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

        #endregion



    }
	
}
