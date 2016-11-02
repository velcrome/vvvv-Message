using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VVVV.Packs.Messaging
{
    public class BaseProfile : TypeIdentity
    {
        public override void Register(TypeIdentity target = null)
        {
            if (target == null) target = this;


            // This is the only place where you need to add new datatypes.
            // type:alias is strictly 1:1 !
            // no case-sensitivity, beware clashes.
            // use of case is purely cosmetic, to reflect c# counterpart

            // when adding new datatypes, make sure to have a serialisation ready in all core serializers.
            target.TryAddRecord(new TypeRecord<bool>("bool", CloneBehaviour.Assign, () => false));
            target.TryAddRecord(new TypeRecord<int>("int", CloneBehaviour.Assign, () => 0));
            target.TryAddRecord(new TypeRecord<double>("double", CloneBehaviour.Assign, () => 0.0d));
            target.TryAddRecord(new TypeRecord<float>("float", CloneBehaviour.Assign, () => 0.0f));
            target.TryAddRecord(new TypeRecord<string>("string", CloneBehaviour.Assign, () => "vvvv"));

            var raw = new TypeRecord<Stream>("Raw", CloneBehaviour.Custom, () => new MemoryStream(new byte[] { 118, 118, 118, 118 })); // vvvv
            raw.CustomClone = (original) =>
            {
                var stream = original as Stream;
                stream.Seek(0, SeekOrigin.Begin);
                var clone = new MemoryStream();
                stream.CopyTo(clone);
                return clone;
            };
            target.TryAddRecord(raw);

            target.TryAddRecord(new TypeRecord<Message>("Message", CloneBehaviour.Assign, () => new Message()));
            target.TryAddRecord(new TypeRecord<Time.Time>("Time", CloneBehaviour.Assign, () => Time.Time.MinUTCTime())); // 1.1.0001 @ 0am
        }
    }


    public abstract class TypeIdentity : IReadOnlyDictionary<Type, TypeRecord>, IReadOnlyDictionary<string, TypeRecord>
    {
        private List<TypeRecord> Data = new List<TypeRecord>();

        #region Singleton
        private static TypeIdentity _instance;
        /// <summary>
        /// TypeIdentity is a singleton right now. 
        /// </summary>
        public static TypeIdentity Instance
        {
            get {
                if (_instance == null)
                {
                    _instance = new BaseProfile();
                    _instance.Register();

                    _instance.FetchAll();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Scans all loaded assemblies for Profile classes extending TypeIdentity.
        /// Will then proceed to call Register of each 
        /// </summary>
        private void FetchAll()
        {
            //            Assembly assembly = Assembly.LoadFrom("packs/vvvv-Message/nodes/plugins/VVVV.Nodes.Messaging.dll");

            var profiles = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                             from types in assembly.GetTypes()
                             where types.IsSubclassOf(typeof(TypeIdentity))
                             where types != typeof(BaseProfile)
                             select types;

            foreach (var profileClass in profiles)
            {
                var profile = Activator.CreateInstance(profileClass) as TypeIdentity;
                profile.Register(Instance);
            }



        }
        #endregion Singleton

        public abstract void Register(TypeIdentity target = null);

        public bool TryAddRecord(TypeRecord newRecord)
        {
            if (newRecord == null) return false;
            if (Data.Contains(newRecord)) return false;

            if (Keys.Contains(newRecord.Type)) return false;
            if (Data.Where(tr => tr.Alias == newRecord.Alias).Count() > 0) return false;

            Data.Add(newRecord);

            return true;
        }

        /// <summary>
        /// Retrieve a list of all currently valid Aliases
        /// </summary>
        public string[] Aliases
        {
            get { return Data.Select(tr => tr.Alias).ToArray(); }
        }


        #region base type helper

        /// <summary>
        /// Retrieve the alias for a type or one of its base classes
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string FindBaseAlias(Type type)
        {
            var result =    from tr in Data
                            where tr.Type == type
                            select tr;
            result.Concat(
                            from tr in Data
                            where tr.Type.IsSubclassOf(type)
                            select tr
                         );
            return result.FirstOrDefault()?.Alias;
        }

        /// <summary>
        /// Retrieve the registered type for any given inherited type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Null, if no base Type was found.</returns>
        public Type FindBaseType(Type type)
        {
            var direct = from tr in Data
                         where tr.Type == type
                         select tr;
            var inherited = direct.Concat(
                            from tr in Data
                            where type.IsSubclassOf(tr.Type)
                            select tr
                         );
            return direct.Concat(inherited).FirstOrDefault()?.Type;
        }
        #endregion base type handler

        #region Obselete

        [Obsolete("Please use default method in TyePecord instead.")]
        public object NewDefault(Type type)
        {
            return Default(FindBaseAlias(type));
        }

        [Obsolete("Please use default method in TyePecord instead.")]
        public object Default(string alias)
        {
            alias = alias.ToLower();
            return Data.Where(tr => tr.Alias == alias).FirstOrDefault()?.Default();
        }

        /// <summary>
        /// Retrieve a registered type by its alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns>Null, if no Type by that alias was found.</returns>
        /// <remarks>aliases are treated Case-insensitive</remarks>
        [Obsolete("Please use indexer instead.")]
        public Type FindType(string alias)
        {
            alias = alias.ToLower();
            return Data.Where(tr => tr.Alias.ToLower() == alias).FirstOrDefault()?.Type;
        }


        /// <summary>
        /// Retrieve the alias for a specific type. 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>will return null, if no exact match for type has been found.</returns>
        [Obsolete("Please use indexcer instead.")]
        public string FindAlias(Type type)
        {
            return Data.Where(tr => tr.Type == type).FirstOrDefault()?.Alias;
        }

        #endregion Obselete

        #region Dictionary members
        public IEnumerable<TypeRecord> Values
        {
            get
            {
                foreach (var tr in Data) yield return tr;
            }
        }

        public int Count
        {
            get
            {
                return Data.Count;
            }
        }

        public IEnumerable<Type> Keys
        {
            get
            {
                foreach (var tr in Data) yield return tr.Type;
            }
        }

        IEnumerable<string> IReadOnlyDictionary<string, TypeRecord>.Keys
        {
            get
            {
                foreach (var tr in Data) yield return tr.Alias;
            }
        }

        public TypeRecord this[string key]
        {
            get
            {
                return Data.Where(tr => tr.Alias == key).FirstOrDefault();
            }
        }

        public TypeRecord this[Type key]
        {
            get
            {
                return Data.Where(tr => tr.Type == key).FirstOrDefault();
            }
        }


        public bool ContainsKey(Type key)
        {
            var result = Data.Where(tr => tr.Type == key).FirstOrDefault();
            return result != null;
        }

        public bool ContainsKey(string alias)
        {
            var result = Data.Where(tr => tr.Alias == alias).FirstOrDefault();
            return result != null;
        }

        public bool TryGetValue(Type key, out TypeRecord value)
        {
            var result = Data.Where(tr => tr.Type == key).FirstOrDefault();
            value = result;
            return result != null;

        }

        public IEnumerator<KeyValuePair<Type, TypeRecord>> GetEnumerator()
        {
            foreach (var tr in Data) yield return new KeyValuePair<Type, TypeRecord>(tr.Type, tr);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var tr in Data) yield return new KeyValuePair<Type, TypeRecord>(tr.Type, tr);
        }


        public bool TryGetValue(string key, out TypeRecord value)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, TypeRecord>> IEnumerable<KeyValuePair<string, TypeRecord>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion Dictionary
    }
}
