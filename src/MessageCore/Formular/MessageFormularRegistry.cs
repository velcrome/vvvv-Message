using System;
using System.Collections.Generic;
using System.Linq;


namespace VVVV.Packs.Messaging
{
    public delegate void MessageFormularChangedHandler(MessageFormularRegistry sender, FormularChangedEventArgs e);

    public class MessageFormularRegistry
    {
        public const string RegistryName = "Singleton.FormularRegistry";

        public const string DefaultFormularName = "Event";
        public const string DefaultField = "string Foo";

        /// <summary>
        /// Subscribe to be informed, if a Formular definition was changed
        /// </summary>
        public event MessageFormularChangedHandler FormularChanged;

        protected Dictionary<string, List<MessageFormular>> Data = new Dictionary<string, List<MessageFormular>>();
        private static MessageFormularRegistry _instance;

        /// <summary>
        /// The Registry for all Formulars is a singleton.
        /// </summary>
        public static MessageFormularRegistry Instance
        {
            get { return _instance ?? (_instance = new MessageFormularRegistry()); }
        }

        internal MessageFormularRegistry()
        {
            var formular = new MessageFormular(MessageFormular.DYNAMIC, null as Message); // empty message

            Data["_dyn"] = new List<MessageFormular>();
            Data["_dyn"].Add(formular);
        }

        /// <summary>
        /// Retrieve the currently defined Formular by name, regardless of the Definer
        /// </summary>
        /// <param name="formularName"></param>
        /// <returns>Null, if no match found</returns>
        public MessageFormular this[string formularName]
        {
            get
            {
                var match = from nodeId in Data.Keys
                                from field in Data[nodeId]
                                where field.Name == formularName
                                select field;

                return match.FirstOrDefault();
            }
       }

        /// <summary>
        /// Retrieve the names of all Formulars currently defined 
        /// </summary>
        public IEnumerable<string> AllFormularNames
        {
            get
            {
                var names = from nodeId in Data.Keys
                            from form in Data[nodeId]
                            select form.Name;
                return names; // already distinct
            }
        }

        /// <summary>
        /// Retrieve all currently defined Formulars of a specific sender entity
        /// </summary>
        /// <param name="definerId"></param>
        /// <returns></returns>
        public IEnumerable<MessageFormular> GetFormularsFrom(string definerId)
        {
            return 
                from form in Data[definerId]
                select form;
        }


        /// <summary>Tries to define a Formular. Each named Formular can only be defined by one sender entity</summary>
        /// <param name="definerId">A unique string that helps to keep track, who registered a given Formular.</param>
        /// <param name="formular">A MessageFormular to be registered.</param>
        /// <param name="supressEvent">A bool indicating if the registry should skip informing interested parties about this change.</param>
        /// <exception cref="ArgumentNullException">Thrown when the Formular is null.</exception>
        /// <exception cref="RegistryException">This exception is thrown if a syntax error prevents the config to be parsed.</exception>
        /// <returns>success</returns>
        /// <remarks>Setting the supressEvent parameter to true will prevent this method to inform any TypeChanged subscribers.</remarks>
        public bool Define(string definerId, MessageFormular formular, bool supressEvent = false)
        {
            if (formular == null) throw new ArgumentNullException("Formular cannot be null");
            if (formular.IsDynamic) return false;

            if (!Data.ContainsKey(definerId)) Data[definerId] = new List<MessageFormular>();

            var match = (
                            from nodeId in Data.Keys
                            where definerId != nodeId
                            from form in Data[nodeId]
                            where formular.Name == form.Name
                            select form.Name
                        ).FirstOrDefault();


            if (match != null)
            {
                throw new RegistryException("Cannot add the formular to the registry. Another formular with the name \""+ match +"\" already exists.");
            }

            var ownFormulars = Data[definerId];
            var oldForm = (
                            from form in ownFormulars
                            where form.Name == formular.Name
                            select form
                         ).FirstOrDefault();

            if (formular.Equals(oldForm)) return false;

            if (oldForm != null) ownFormulars.Remove(oldForm);
            ownFormulars.Add(formular);

            if (!supressEvent)
            {
                var e = new FormularChangedEventArgs(formular);
                if (FormularChanged != null) FormularChanged(this, e);
            }
            return true;
        }

        
        /// <summary>
        /// Unregisters all Formulars from a specific definer source
        /// </summary>
        /// <param name="definerId"></param>
        /// <returns>true, if remove had an effect</returns>
        public bool UndefineAll(string definerId)
        {
            var success = Data.Remove(definerId);
            return success;
        }

        /// <summary>
        /// Unregisters a specific Formular from a specific definer source
        /// </summary>
        /// <param name="definerId"></param>
        /// <returns>true, if undefine had an effect.</returns>
        public bool Undefine(string definerId, MessageFormular formular)
        {
            if (!Data.ContainsKey(definerId)) return false;
            return Data[definerId].Remove(formular);
        }
    }
}