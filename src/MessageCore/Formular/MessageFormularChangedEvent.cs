using System;

namespace VVVV.Packs.Messaging
{
    public class FormularChangedEventArgs : EventArgs
    {
        public MessageFormular Formular { get; private set; }
        public string FormularName { get { return Formular.Name; }
        }

        private FormularChangedEventArgs()
        {
            
        }

        public FormularChangedEventArgs(MessageFormular formular) : base()
        {
            this.Formular = formular;
        }

    }
}
