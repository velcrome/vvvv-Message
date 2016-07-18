using System;


namespace VVVV.Packs.Messaging
{
    /// <summary>
    /// Raised when code attempts to manipulate a pin that is linked.
    /// </summary>
    public class PinConnectionException : InvalidOperationException
    {
        public PinConnectionException(string text) : base (text){}
    }

    /// <summary>
    /// The required type is not defined in TypeIdentity, or no serializer is provided
    /// </summary>
    public class TypeNotSupportedException : NotSupportedException
    {
        public TypeNotSupportedException(string text) : base (text){ }
    }

    /// <summary>
    /// An operation tried to apply a incoompatible argument to a Bin
    /// </summary>
    public class BinTypeMismatchException : ArrayTypeMismatchException
    {
        public BinTypeMismatchException(string text) : base(text) { }
        public BinTypeMismatchException(string text, Exception e) : base(text, e) { }
    }

    /// <summary>
    /// Generic exception for problems when parsing a string to a Formular
    /// </summary>
    public class ParseFormularException : FormatException
    {
        public ParseFormularException(string text) : base(text) { }
    }

    /// <summary>
    /// Generic exception for problems when deserializing
    /// </summary>
    public class ParseMessageException : FormatException
    {
        public ParseMessageException(string text, Exception ex = null) : base(text, ex) { }
    }

    /// <summary>
    /// Raised when field definitions in Formulars clash
    /// </summary>
    public class DuplicateFieldException : ArgumentException
    {
        public FormularFieldDescriptor Fresh {
            get;
            protected set;
        }
        public FormularFieldDescriptor Old { 
            get;
            protected set;
        }

        public DuplicateFieldException(string text, FormularFieldDescriptor fresh, FormularFieldDescriptor old) : base(text)
        {
            this.Fresh = fresh;
            this.Old = old;
        }
    }

    /// <summary>
    /// Faults when adding, removing or editing within the Registry
    /// </summary>
    public class RegistryException : InvalidOperationException
    {
        public RegistryException(string text) : base(text) { }
    }

    /// <summary>
    /// Raised when an empty (but necessarily typed) bin is attempted to be created by nothing to go on. 
    /// </summary>
    /// <remarks>Try to use generic BinFactory instead.</remarks>
    public class EmptyBinException : InvalidOperationException
    {
        public EmptyBinException(string text) : base(text) { }
    }

}
