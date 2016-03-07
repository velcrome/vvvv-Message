using System;
using System.Collections.Generic;
using System.Linq;


namespace VVVV.Packs.Messaging
{
    public delegate void MessageFormularChangedHandler(MessageFormularRegistry sender, FormularChangedEventArgs e);

    public class MessageFormularRegistry
    {
        public const string RegistryName = "VVVV.Packs.Message.Core.Formular";
        public const string Default = MessageFormular.DYNAMIC;
        
        private static MessageFormularRegistry _instance;
        public event MessageFormularChangedHandler TypeChanged;

        protected Dictionary<string, List<MessageFormular>> Data = new Dictionary<string, List<MessageFormular>>();


        public static MessageFormularRegistry Instance
        {
            get { return _instance ?? (_instance = new MessageFormularRegistry()); }
        }

        internal MessageFormularRegistry()
        {
            Data[Default] = new List<MessageFormular>();
            var formular = new MessageFormular("", MessageFormular.DYNAMIC);
            Data[Default].Add(formular);
        }

        public MessageFormular this[string name]
        {
            get
            {
                var match = from nodeId in Data.Keys
                                from field in Data[nodeId]
                                where field.Name == name
                                select field;

                return match.FirstOrDefault();
            }
       }

        public string[] Names
        {
            get
            {
                var names = from nodeId in Data.Keys
                            from form in Data[nodeId]
                            select form.Name;
                return names.Distinct().ToArray();
            }
        }

        public MessageFormular[] FromId(string senderId)
        {
            var result = from form in Data[senderId]
                         select form;
            return result.ToArray();
        }


        public bool Define(string senderId, MessageFormular formular, bool supressEvent = false)
        {
            if (formular.Name == MessageFormular.DYNAMIC) return false;
            if (!Data.ContainsKey(senderId)) Data[senderId] = new List<MessageFormular>();


            var match = (
                            from nodeId in Data.Keys
                            where senderId != nodeId
                            from form in Data[nodeId]
                            where formular.Name == form.Name
                            select form.Name
                        ).FirstOrDefault();


            if (match != null)
            {
                throw new Exception("Cannot add the formular to the registry. Another formular with that name already exists.");
            }

            var ownFormulars = Data[senderId];
            var oldForm = (
                            from form in ownFormulars
                            where form.Name == formular.Name
                            select form
                         ).FirstOrDefault();

            if (oldForm != null) ownFormulars.Remove(oldForm);
            ownFormulars.Add(formular);

            if (!supressEvent)
            {
                var e = new FormularChangedEventArgs(formular);
                if (TypeChanged != null) TypeChanged(this, e);
            }
            return true;
        }

        

        public bool UndefineAll(string senderId)
        {
            var success = Data.Remove(senderId);
            return success;
        }

        public bool Undefine(string senderId, MessageFormular remove)
        {
            return Data[senderId].Remove(remove);
        }
    }
}