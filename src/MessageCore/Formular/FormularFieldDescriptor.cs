using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Packs.Messaging.Core.Formular
{
    public class FormularFieldDescriptor
    {
        public string Name {get; set;}
        public Type Type {get;set;}
        public int DefaultSize {get; set;}

        public FormularFieldDescriptor(Type type, string name, int size = -1)
        {
            Name = name;
            Type = type;
            DefaultSize = size;
        }

    }
}
