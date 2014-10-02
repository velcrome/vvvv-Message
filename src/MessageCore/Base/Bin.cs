using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using VVVV.Packs.Message.Core;


namespace VVVV.Pack.Game.Core
{
    [Serializable]
    [JsonConverter(typeof(BinSerializer))]
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
    [JsonConverter(typeof(BinSerializer))]
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


        public new Bin Clone()
        {
            Bin c = Bin.New(this.GetInnerType());
            c.AssignFrom(this);
            return c;
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
            
            // here is a place to start to add inheritance in TypeIdentities.
            if (TypeIdentity.Instance.ContainsKey(val.GetType())) 
            {
                return base.Add(val);
            } 

            if (val is IEnumerable ) // string should be treated differently, but that is implicit in the last 4 lines
            {
                var enumerable = (IEnumerable)val;

                Type type;
                try
                {
                    var num = enumerable.GetEnumerator();

                    num.MoveNext();
                    type = num.Current.GetType();
                }
                catch (Exception e)
                {
                    throw new Exception("Cannot add object " + enumerable.ToString() + " to Bin because cannot determine type. Maybe empty?", e);

                }
                if (this.GetInnerType() == type)
                {
                    foreach (var o in enumerable)
                    {
                        if (o.GetType() != this.GetInnerType()) throw new Exception("Cannot add object " + enumerable.ToString() + " from "+enumerable+ " to Bin because Type of inside element does not match");

                        
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
