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
            this[MessageFormular.DYNAMIC] = new MessageFormular("", MessageFormular.DYNAMIC);
        }

        public MessageFormular Define(MessageFormular formular, bool supressEvent = false)
        {
            if (formular.Name == MessageFormular.DYNAMIC) return null;

            this[formular.Name] = formular;

            if (!supressEvent)
            {
                var e = new FormularChangedEventArgs(formular);
                if (TypeChanged != null) TypeChanged(this, e);
            }
            return formular;
        }

        public new void Clear() // just hides the base.Clear(), if cast to Dictionary, it will still call the base Clear
        {
            base.Clear();
            this[MessageFormular.DYNAMIC] = new MessageFormular("", MessageFormular.DYNAMIC);
        }


    }
}