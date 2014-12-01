using System;

namespace VVVV.Packs.Messaging
{
    public class MessageFormularChangedEvent : EventArgs
    {
        public MessageFormular Formular { get; private set; }
        public string FormularName { get { return Formular.Name; }
        }

        private MessageFormularChangedEvent()
        {
            
        }

        public MessageFormularChangedEvent(MessageFormular formular) : base()
        {
            this.Formular = formular;
        }

    }
}
