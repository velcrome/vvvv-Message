using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace VVVV.Packs.Message.Core.Formular
{
    public delegate void MessageFormularChangedHandler(MessageFormularRegistry sender, MessageFormularChangedEvent e);

    public class MessageFormularRegistry : Dictionary<string, MessageFormular>
    {
        private static MessageFormularRegistry _instance;
        public event MessageFormularChangedHandler TypeChanged;


        public static MessageFormularRegistry Instance
        {
            get { return _instance ?? (_instance = new MessageFormularRegistry()); }
        }

        public MessageFormular Define(string formularName, string configuration)
        {
            var form = new MessageFormular(configuration);
            this[formularName] = form;

            var e = new MessageFormularChangedEvent(form);
            TypeChanged(this, e);

            return form;
        }
  
    }
}