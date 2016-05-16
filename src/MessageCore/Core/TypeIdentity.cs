using System;
using System.Collections.Generic;
using System.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;


using System.Linq;


namespace VVVV.Packs.Messaging
{
    public class TypeIdentity : Dictionary<Type, string>
    {
        private static TypeIdentity _instance;
        /// <summary>
        /// TypeIdentity is a singleton right now. 
        /// </summary>
        public static TypeIdentity Instance
        {
            get { 
                if (_instance == null) _instance = new TypeIdentity();
                return _instance;
            }
        }

        /// <summary>
        /// Retrieve a list of all currently valid Aliases
        /// </summary>
        public string[] Aliases
        {
            get { return this.Values.ToArray(); }
        }

        public TypeIdentity()
	    {
            // This is the only place where you need to add new datatypes.

            Add(typeof(bool), "bool");
            Add(typeof(int), "int");
            Add(typeof(double), "double");
            Add(typeof(float), "float");
            Add(typeof(string), "string");

            Add(typeof(RGBAColor), "Color");
            Add(typeof(Matrix4x4), "Transform");
            Add(typeof(Vector2D), "Vector2d");
            Add(typeof(Vector3D), "Vector3d");
            Add(typeof(Vector4D), "Vector4d");

            Add(typeof(Stream), "Raw");
            Add(typeof(Time.Time), "Time");
            
            Add(typeof(Message), "Message");
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
                
                case "color": return VColor.Blue; 
                case "transform": return VMath.IdentityMatrix; 
                case "vector2d": return new Vector2D(); 
                case "vector3d": return new Vector3D(); 
                case "vector4d": return new Vector4D(); 

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
        /// <remarks>aliases are treated Case-insensitive here, even though they are case-sensitive.</remarks>
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
