using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace VVVV.Packs.Messaging
{
    public class TypeIdentity : IReadOnlyDictionary<Type, TypeRecord>, IReadOnlyDictionary<string, TypeRecord>
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
                    _instance.Register();
                }
                return _instance;
            }
        }
        #endregion Singleton

        protected void Register()
        {
            // This is the only place where you need to add new datatypes.
            // type:alias is strictly 1:1 !
            // no case-sensitivity, beware clashes.
            // use of case is purely cosmetic, to reflect c# counterpart

            // when adding new datatypes, make sure to have a serialisation ready in all core serializers.
            TryAddRecord(new TypeRecord<bool>("bool", CloneBehaviour.Assign, () => false));
            TryAddRecord(new TypeRecord<int>("int", CloneBehaviour.Assign, () => 0));
            TryAddRecord(new TypeRecord<double>("double", CloneBehaviour.Assign, () => 0.0d));
            TryAddRecord(new TypeRecord<float>("float", CloneBehaviour.Assign, () => 0.0f));
            TryAddRecord(new TypeRecord<string>("string", CloneBehaviour.Assign, () => "vvvv"));

            var raw = new TypeRecord<Stream>("Raw", CloneBehaviour.Custom, () => new MemoryStream(new byte[] { 118, 118, 118, 118 })); // vvvv
            raw.CustomClone = (original) =>
            {
                var stream = original as Stream;
                stream.Seek(0, SeekOrigin.Begin);
                var clone = new MemoryStream();
                stream.CopyTo(clone);
                return clone;
            };
            TryAddRecord(raw);

            TryAddRecord(new TypeRecord<Message>("Message", CloneBehaviour.Assign, () => new Message()));
            TryAddRecord(new TypeRecord<Time.Time>("Time", CloneBehaviour.Assign, () => Time.Time.MinUTCTime())); // 1.1.0001 @ 0am


            TryAddRecord(new TypeRecord<VVVV.Utils.VColor.RGBAColor>("Color", CloneBehaviour.Assign, () => new VVVV.Utils.VColor.RGBAColor()));
            TryAddRecord(new TypeRecord<VVVV.Utils.VMath.Matrix4x4>("Transform", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Matrix4x4()));
            TryAddRecord(new TypeRecord<VVVV.Utils.VMath.Vector2D>("Vector2d", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Vector2D()));
            TryAddRecord(new TypeRecord<VVVV.Utils.VMath.Vector3D>("Vector3d", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Vector3D()));
            TryAddRecord(new TypeRecord<VVVV.Utils.VMath.Vector4D>("Vector4d", CloneBehaviour.Assign, () => new VVVV.Utils.VMath.Vector4D()));

        }

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
            var result = from tr in Data
                         where tr.Type == type
                         select tr;
            result.Concat(
                            from tr in Data
                            where tr.Type.IsSubclassOf(type)
                            select tr
                         );
            return result.FirstOrDefault()?.Type;
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
