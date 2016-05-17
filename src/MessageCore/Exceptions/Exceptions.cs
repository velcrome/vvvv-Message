using System;


namespace VVVV.Packs.Messaging
{
    public class PinConnectionException : InvalidOperationException
    {
        public PinConnectionException(string message) : base (message){}
    }

    public class TypeNotSupportedException : NotSupportedException
    {
        public TypeNotSupportedException(string message) : base (message){ }
    }

    public class BinTypeMismatchException : ArrayTypeMismatchException
    {
        public BinTypeMismatchException(string message) : base(message) { }
        public BinTypeMismatchException(string message, Exception e) : base(message, e) { }
    }

    public class ParseFormularException : FormatException
    {
        public ParseFormularException(string message) : base(message) { }
    }

    public class ParseMessageException : FormatException
    {
        public ParseMessageException(string message, Exception ex = null) : base(message, ex) { }
    }

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

        public DuplicateFieldException(string message, FormularFieldDescriptor fresh, FormularFieldDescriptor old) : base(message)
        {
            this.Fresh = fresh;
            this.Old = old;
        }
    }

    public class RegistryException : InvalidOperationException
    {
        public RegistryException(string message) : base(message) { }
    }

    public class EmptyBinException : InvalidOperationException
    {
        public EmptyBinException(string message) : base(message) { }
    }

}
