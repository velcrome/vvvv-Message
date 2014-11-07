using System;

namespace VVVV.Packs.Message.Core.Registry
{
    public class TypeChangedEvent : EventArgs
    {
        public string Config { get; private set; }
        public string TypeName { get; private set; }

        private TypeChangedEvent()
        {
            
        }

        public TypeChangedEvent(string typeName, string configuration) : base()
        {
            this.TypeName = typeName;
            this.Config = configuration;
        }

    }
}
