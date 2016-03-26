using System;
using System.Collections.Generic;
using System.Linq;


namespace VVVV.Packs.Messaging
{
    public delegate void MessageFormularChangedHandler(MessageFormularRegistry sender, FormularChangedEventArgs e);

    public class MessageFormularRegistry
    {
        public const string RegistryName = "VVVV.Packs.Message.Core.Formular";
        public const string DefaultFormular = MessageFormular.DYNAMIC;
        public const string DefaultFormularName = "Event";
        public const string DefaultField = "string Foo";

        private static MessageFormularRegistry _instance;
        public event MessageFormularChangedHandler TypeChanged;

        protected Dictionary<string, List<MessageFormular>> Data = new Dictionary<string, List<MessageFormular>>();


        public static MessageFormularRegistry Instance
        {
            get { return _instance ?? (_instance = new MessageFormularRegistry()); }
        }

        internal MessageFormularRegistry()
        {
            Data[DefaultFormular] = new List<MessageFormular>();
            var formular = new MessageFormular("", MessageFormular.DYNAMIC);
            Data[DefaultFormular].Add(formular);
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


        /// <summary>Constructor that parses a configuration string.</summary>
        /// <param name="senderId">A unique string that helps to keep track, who registered a given Formular.</param>
        /// <param name="formular">A MessageFormular to be registered.</param>
        /// <param name="supressEvent">A bool indicating if the registry should skip informing interested parties about this change.</param>
        /// <exception cref="RegistryException">This exception is thrown if a syntax error prevents the config to be parsed.</exception>
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
                throw new RegistryException("Cannot add the formular to the registry. Another formular with that name already exists.");
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