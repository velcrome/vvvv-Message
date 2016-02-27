using System.Collections.Generic;


namespace VVVV.Packs.Messaging
{
    public delegate void MessageFormularChangedHandler(MessageFormularRegistry sender, FormularChangedEventArgs e);

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
            this[MessageFormular.DYNAMIC] = new MessageFormular("");
        }

        public MessageFormular Define(string formularName, string configuration, bool supressEvent = false)
        {
            if (formularName == MessageFormular.DYNAMIC) return null; 
            
            var form = new MessageFormular(configuration);
            form.Name = formularName;
            this[formularName] = form;

            if (!supressEvent)
            {
                var e = new FormularChangedEventArgs(form);
                if (TypeChanged != null) TypeChanged(this, e);
            }

            return form;
        }

  
    }
}