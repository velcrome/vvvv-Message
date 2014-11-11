using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace VVVV.Packs.Message.Core.Formular
{
    public delegate void MessageFormularChangedHandler(MessageFormularRegistry sender, MessageFormularChangedEvent e);

    public class MessageFormularRegistry : Dictionary<string, MessageFormular>
    {
        public string RegistryName = "VVVV.Packs.Message.Core.Formular";
        
        private static MessageFormularRegistry _instance;
        public event MessageFormularChangedHandler TypeChanged;


        public static MessageFormularRegistry Instance
        {
            get { return _instance ?? (_instance = new MessageFormularRegistry()); }
        }

        internal MessageFormularRegistry()
        {
            this[MessageFormular.NONE] = new MessageFormular("");
        }

        public MessageFormular Define(string formularName, string configuration)
        {
            if (formularName == MessageFormular.NONE) return null;
            
            var form = new MessageFormular(configuration);
            this[formularName] = form;

            var e = new MessageFormularChangedEvent(form);
            TypeChanged(this, e);

            return form;
        }

  
    }
}