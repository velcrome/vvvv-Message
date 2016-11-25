using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VVVV.Packs.Messaging
{
    public interface TypeProfile
    {
    }

    public class BaseProfile : TypeProfile
    {
        // This is one of the places where you can add new datatypes.
        // all classes implementing TypeProfile will be considered, when available for scanning.
        // type:alias is strictly 1:1 !
        // no case-sensitivity, no use for boxing anyway
        // use of case is purely cosmetic, to reflect c# counterpart
        // must mark the TypeRecord as public!

        // when adding new datatypes, make sure to have a serialisation ready in all core serializers.
        public TypeRecord<bool> @bool = new TypeRecord<bool>("bool", CloneBehaviour.Assign, () => false);
        public TypeRecord<int> @int = new TypeRecord<int>("int", CloneBehaviour.Assign, () => 0);
        public TypeRecord<double> @double = new TypeRecord<double>("double", CloneBehaviour.Assign, () => 0.0d);
        public TypeRecord<float> @float = new TypeRecord<float>("float", CloneBehaviour.Assign, () => 0.0f);
        public TypeRecord<string> @string = new TypeRecord<string>("string", CloneBehaviour.Assign, () => "vvvv");

        public TypeRecord<Stream> Stream = new TypeRecord<Stream>(
                "Raw", // alias
                CloneBehaviour.Custom, () => new MemoryStream(new byte[] { 118, 118, 118, 118 }), // default = vvvv
                (original) =>
                {
                    var stream = original as Stream;
                    stream.Seek(0, SeekOrigin.Begin);
                    var clone = new MemoryStream();
                    stream.CopyTo(clone);
                    return clone;
                } // custom clone
            );

        public TypeRecord<Message> Message = new TypeRecord<Message>("Message", CloneBehaviour.Assign, () => new Message());
        public TypeRecord<Time.Time> TimeStamp = new TypeRecord<Time.Time>("Time", CloneBehaviour.Assign, () => Time.Time.MinUTCTime()); // 1.1.0001 @ 0am
    }

    public sealed class TypeIdentity : IReadOnlyDictionary<Type, TypeRecord>, IReadOnlyDictionary<string, TypeRecord>
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
                    _instance = new TypeIdentity();
                    _instance.ScanAssemblies();
                }
                return _instance;
            }
        }
        #endregion Singleton

        #region Scan Assemblies

        /// <summary>
        /// Scans all loaded assemblies for Profile classes extending TypeIdentity.
        /// Will then proceed to attempt extracting and registering all public 
        /// </summary>
        public bool ScanAssemblies()
        {
            //            Assembly assembly = Assembly.LoadFrom("packs/vvvv-Message/nodes/plugins/VVVV.Nodes.Messaging.dll");

            var profiles = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                             from candidate in GetLoadableTypes(assembly)
                             where !candidate.IsInterface
                             where typeof(TypeProfile).IsAssignableFrom(candidate)
                             select candidate;

            foreach (var profileClass in profiles)
            {
                try
                {
                    var profile = Activator.CreateInstance(profileClass) as TypeProfile; // all fields of the profile should be fully initialized 

                    // allow basic inheritance.
                    var infos = profile.GetType().GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public).Where(declaration => typeof(TypeRecord).IsAssignableFrom(declaration.FieldType));

                    foreach (var fieldInfo in infos)
                    {
                        var name = fieldInfo.Name;
                        var record = fieldInfo.GetValue(profile) as TypeRecord;

                        var success = false;
                        if (record != null) success = TryAddRecord(record);

                        if (!success) { } // freak out
                    }
                }
                catch (Exception ex)
                {
                    var e = ex.Message;
                    // no way to log from here :(
                    // catch, so vvvv startup cycle does not freak out
                }
            }
            return true;
        }

        private IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null) return Enumerable.Empty<Type>();
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }

        }
        #endregion Scan Assemblies

        #region append fields
        /// <summary>
        /// This allows late additions of Records. Nonetheless, it could be used in subclasses constructors for alternative initialization.
        /// </summary>
        /// <param name="newRecord"></param>
        /// <returns>success, when the record type is now available across the application.</returns>
        public bool TryAddRecord(TypeRecord newRecord)
        {
            if (newRecord == null) return false;
            if (Data.Contains(newRecord)) return false;

            if (Keys.Contains(newRecord.Type)) return false;
            if (Data.Where(tr => tr.Alias == newRecord.Alias).Count() > 0) return false;

            Data.Add(newRecord);

            return true;
        }
        #endregion

        /// <summary>
        /// Retrieve a list of all currently valid Aliases
        /// </summary>
        public string[] Aliases
        {
            get { return Data.Select(tr => tr.Alias).ToArray(); }
        }


        #region base type helper

        /// <summary>
        /// Retrieve the registered type for any given inherited type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Null, if no base Type was found.</returns>
        public TypeRecord FindBaseType(Type type)
        {
            var direct = from tr in Data
                         where tr.Type == type
                         select tr;
            var inherited = direct.Concat(
                            from tr in Data
                            where type.IsSubclassOf(tr.Type)
                            select tr
                         );
            return direct.Concat(inherited).FirstOrDefault();
        }
        #endregion base type handler

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
                key = key.ToLower();
                return Data.Where(tr => tr.Alias.ToLower() == key).FirstOrDefault();
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
            alias = alias.ToLower();
            var result = Data.Where(tr => tr.Alias.ToLower() == alias).FirstOrDefault();
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
            if (this.ContainsKey(key))
            {
                value = this[key];
                return true;
            } else
            {
                value = null;
                return false;
            }
        }

        IEnumerator<KeyValuePair<string, TypeRecord>> IEnumerable<KeyValuePair<string, TypeRecord>>.GetEnumerator()
        {
            foreach (var tr in Data) yield return new KeyValuePair<string, TypeRecord>(tr.Alias, tr);
        }

        #endregion Dictionary
    }
}
