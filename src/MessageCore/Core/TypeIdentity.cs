using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;


namespace VVVV.Packs.Messaging
{
    public class TypeIdentity : ReadOnlyDictionary<Type, string>
    {

        internal TypeIdentity(IDictionary<Type, string> dictionary) : base(dictionary)
        {}

        private static TypeIdentity _instance;
        /// <summary>
        /// TypeIdentity is a singleton right now. 
        /// </summary>
        public static TypeIdentity Instance
        {
            get {
                if (_instance == null)
                {
                    _instance = NewInstance();
                }
                return _instance;
            }
        }

        internal static TypeIdentity NewInstance()
        {
            // This is the only place where you need to add new datatypes.
            // type:alias is strictly 1:1 !
            // no case-sensitivity, beware clashes.
            // use of case is purely cosmetic, to reflect c# counterpart

            // when adding new datatypes, make sure to have a serialisation ready in all core serializers.

            var registry = new Dictionary<Type, string>();

            registry.Add(typeof(bool), "bool");
            registry.Add(typeof(int), "int");
            registry.Add(typeof(double), "double");
            registry.Add(typeof(float), "float");
            registry.Add(typeof(string), "string");

            registry.Add(typeof(VVVV.Utils.VColor.RGBAColor), "Color");
            registry.Add(typeof(VVVV.Utils.VMath.Matrix4x4), "Transform");
            registry.Add(typeof(VVVV.Utils.VMath.Vector2D), "Vector2d");
            registry.Add(typeof(VVVV.Utils.VMath.Vector3D), "Vector3d");
            registry.Add(typeof(VVVV.Utils.VMath.Vector4D), "Vector4d");

            registry.Add(typeof(Stream), "Raw");
            registry.Add(typeof(Time.Time), "Time");

            registry.Add(typeof(Message), "Message");


            registry.Add(typeof(DX11.DX11Resource<VVVV.DX11.DX11Layer>), "Layer"); 
            registry.Add(typeof(DX11.DX11Resource<FeralTic.DX11.Resources.DX11Texture2D>), "Texture2d");
            registry.Add(typeof(DX11.DX11Resource<FeralTic.DX11.Resources.IDX11Geometry>), "Geometry");

            return new TypeIdentity(registry);
        }

        /// <summary>
        /// Retrieve a list of all currently valid Aliases
        /// </summary>
        public string[] Aliases
        {
            get { return this.Values.ToArray(); }
        }


        public object NewDefault(Type type)
        {
            return Default(FindBaseAlias(type));
        }
        

        public object Default(string alias)
        {
            alias = alias.ToLower();
            switch (alias)
            {
                case "bool": return false; 
                case "int": return 0; 
                case "double": return 0.0d; 
                case "float": return 0.0f; 
                case "string": return "vvvv"; 
                
                case "color": return VVVV.Utils.VColor.VColor.Blue; 
                case "transform": return VVVV.Utils.VMath.VMath.IdentityMatrix; 
                case "vector2d": return new VVVV.Utils.VMath.Vector2D(); 
                case "vector3d": return new VVVV.Utils.VMath.Vector3D(); 
                case "vector4d": return new VVVV.Utils.VMath.Vector4D(); 

                case "raw": return new MemoryStream(new byte[]{118, 118, 118, 118}); // vvvv
                case "time": return Time.Time.MinUTCTime(); // 1.1.0001 @ 0am
                case "message":return new Message();
            }
            return null;
        }

        /// <summary>
        /// Retrieve the alias for a specific type. 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>will return null, if no exact match for type has been found.</returns>
        public string FindAlias(Type type)
        {
            if (this.ContainsKey(type))
                return this[type];
                else return null;
        }

        /// <summary>
        /// Retrieve the alias for a type or one of its base classes
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string FindBaseAlias(Type type)
        {
            foreach (Type key in Keys)
            {
                if (key == type) return this[key];
                if (type.IsSubclassOf(key)) return this[key];
            }
            return null;
        }

        /// <summary>
        /// Retrieve the registered type for any given inherited type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Null, if no base Type was found.</returns>
        public Type FindBaseType(Type type)
        {
            foreach (Type key in Keys)
            {
                if (key == type) return key;
                if (type.IsSubclassOf(key)) return key;
            }
            return null;
        }

        /// <summary>
        /// Retrieve a registered type by its alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns>Null, if no Type by that alias was found.</returns>
        /// <remarks>aliases are treated Case-insensitive</remarks>
        public Type FindType(string alias)
        {
            Type type = null;
            foreach (Type key in this.Keys)
            {
                if (this[key].ToLower() == alias.ToLower())
                {
                    type = key;
                }
            }
            return type;
        }


    }
}
