using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VVVV.Packs.Messaging.Serializing;
using System.Linq;

namespace VVVV.Packs.Messaging
{
    /// <summary>Non-Generic interface for a Bin.</summary>
    /// <remarks>
    /// Bin are used as named fields in a Message. On convention, ANY change to it will set the IsDirty flag to true.
    /// </remarks>    
    [JsonConverter(typeof(JsonBinSerializer))]
    public interface Bin : ICloneable, ISerializable, IEnumerable, IEquatable<Bin>, IEquatable<IEnumerable> 
    {
        /// <summary>Access to individual elements of a bin</summary>
        /// <param name="slice">The field to read or written. </param>
        /// <exception cref="ArgumentNullException">Thrown if null is to be written into a bin.</exception>
        /// <exception cref="InvalidCastException">Thrown if an item is attempting to be written to a bin without the type matching.</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown if attempt is made to read a slice that does not exist.</exception>
        object this[int slice] { get; set; }

        /// <summary>Access to the first element in the bin</summary>
        /// <exception cref="ArgumentNullException">Thrown if null is to be written into a bin.</exception>
        /// <exception cref="TypeNotSupportedException">Thrown if an item is attempting to be written to a bin without a proper registration in TypeIdentity</exception>
        /// <exception cref="InvalidCastException">Thrown if an item is attempting to be written to a bin without the type matching.</exception>
        object First { get; set; }

        /// <summary>The current number of elements in the bin</summary>
        int Count               { get; set; }

        /// <summary>Indicates if the bin has been marked for sweeping recently. </summary>
        /// <returns>If reference is not null, check if it is the Sweeper, i.e. if reference was the last Committer</returns>
        bool IsSweeping(object reference = null);

        /// <summary>
        /// Commit change, and leave a breadcrumb, so you can identify it
        /// </summary>
        /// <param name="reference">A value of null means Sweeping is done.</param>
        void Sweep(object reference = null);

        bool IsChanged  { get; set; }

        /// <summary>
        /// The runtime type of the bin.
        /// </summary>
        Type GetInnerType();

        /// <summary>
        /// Versatile Add method. 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <remarks>
        /// Can accept single items (which will be casted or autoconverted if necessary), or IEnumerable, all of which will be appended to the bin.
        /// Does not allow null in any way. If it encounters null or any other fault, it will stop right then, and throw an Exception.
        /// </remarks>
        /// <exception cref="ArgumentNullException">This exception is thrown if null is to be written into a bin.</exception>
        /// <exception cref="InvalidCastException">This exception is thrown if an item is attempting to be written to a bin without the type matching.</exception>
        /// <exception cref="BinTypeMismatchException">This exception is thrown if an item is attempting to be written to a bin without the type matching.</exception>
        int Add(object item);

        /// <summary>
        /// Overwrites any data in an existing bin. 
        /// </summary>
        /// <param name="item"></param>
        /// <remarks>Does not allow null in any way.  If it encounters null or any other fault, it will stop right then, and throw an Exception.</remarks>
        /// <exception cref="ArgumentNullException">Thrown if null is to be written into a bin.</exception>
        /// <exception cref="InvalidCastException">Thrown if an item is attempting to be written to a bin without the type matching.</exception>
        /// <exception cref="BinTypeMismatchException">Thrown if an item is attempting to be written to a bin without the type matching.</exception>
        void AssignFrom(IEnumerable enumerable);

        /// <summary>
        /// Removes all data from the bin. 
        /// </summary>
        void Clear();
    }

    /// <summary>Generic interface for a Bin.</summary>
    /// <remarks>
    /// Bin are used as named fields in a Message. On convention, ANY change to it will set the IsDirty flag to true.
    /// </remarks>    
    public interface Bin<T> : Bin, IEnumerable<T>
    {
        /// <summary>Access to individual elements of a bin</summary>
        /// <exception cref="ArgumentNullException">Thrown if null is to be written into a bin.</exception>
        /// <exception cref="TypeNotSupportedException">Thrown if an item is attempting to be written to a bin without a proper registration in TypeIdentity</exception>
        /// <exception cref="InvalidCastException">Thrown if an item is attempting to be written to a bin without the type matching.</exception>
        new T this[int slice] {get; set;}

        /// <summary>Access to the first element in the bin</summary>
        /// <exception cref="ArgumentNullException">Thrown if null is to be written into a bin.</exception>
        /// <exception cref="TypeNotSupportedException">Thrown if an item is attempting to be written to a bin without a proper registration in TypeIdentity</exception>
        /// <exception cref="InvalidCastException">Thrown if an item is attempting to be written to a bin without the type matching.</exception>
        new T First { get; set; }
    }

    /// <summary>
    /// This Factory class provides alternative constructors for design-time and runtime typing of a Bin</summary>
    /// <remarks>
    /// Bin are used as fields in a Message. Internally, Bins are usually BinLists, which should only be used through either the generic or non-generic Bin interface.
    /// </remarks>    
    public class BinFactory {

        /// <summary>Constructs and initializes a new object with a non-generic Bin interface.</summary>
        /// <typeparam name = "T" >Any registered, valid type, as of TypeIdentity.</typeparam>
        /// <exception cref="TypeNotSupportedException">Thrown if the wanted type is not registered, nor any of its base types.</exception>
        public static Bin New(Type type, int initialCapacity = 1)
        {
            var baseType = TypeIdentity.Instance.FindBaseType(type)?.Type;
            if (baseType != null)
            {
                Type genericBin = typeof(BinList<>).MakeGenericType(baseType); // downcast to allowed type, if intial type does not match directly
                var bin = Activator.CreateInstance(genericBin);
                return (Bin)bin;
            }
            else
            {
                throw new TypeNotSupportedException(type.ToString() + " is not a supported Type in vvvv-Message. See TypeIdentity.cs");
            }
        }

        /// <summary>Constructs and initializes a new object with a generic Bin interface.</summary>
        /// <typeparam name = "T" >Any </typeparam>
        /// <exception cref="TypeNotSupportedException">Thrown if the wanted type is not registered, nor any of its base types.</exception>
        public static Bin<T> New<T>(IEnumerable<T> values)
        {
            var type = typeof(T);

            var bin = New(type, values.Count()) as Bin<T>;
            bin.AssignFrom(values);
            return bin;
        }

        /// <summary>Constructs and initializes a new object with a generic Bin interface.</summary>
        /// <typeparam name = "T" >Any type </typeparam>
        /// <returns>true, if the Message has Changed</returns>        
        /// <remarks>Use like this: var b = BinFactory.New(1, 23, 42);
        /// The first parameter will define T, unless explicitly specified.
        /// </remarks>
        /// <exception cref="TypeNotSupportedException">Thrown if the wanted type is not registered, nor any of its base types.</exception>
        public static Bin<T> New<T>(params T[] values) 
        {
            var type = typeof(T);

            var bin = New(type, values.Length) as Bin<T>;
            bin.AssignFrom(values);
            return bin;
        }
    }



}
