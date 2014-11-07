using System;
using System.Collections.Generic;
using System.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Packs.Time;

using System.Linq;


namespace VVVV.Packs.Message.Core
{
    public class TypeIdentity : Dictionary<Type, string>
    {
        private static TypeIdentity _instance;
        public static TypeIdentity Instance
        {
            get { 
                if (_instance == null) _instance = new TypeIdentity();
                return _instance;
            }
            private set { throw new NotImplementedException(); }
        }

        public string[] Types
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


        public object Default(Type type)
        {
            return Default(FindAlias(type));
        }
        

        public object Default(string alias)
        {
            switch (alias)
            {
                case "bool": return false; 
                case "int": return 0; 
                case "double": return 0.0d; 
                case "float": return 0.0f; 
                case "string": return "vvvv"; 
                
                case "Color": return VColor.Blue; 
                case "Transform": return VMath.IdentityMatrix; 
                case "Vector2d": return new Vector2D(); 
                case "Vector3d": return new Vector3D(); 
                case "Vector4d": return new Vector4D(); 

                case "Stream": return new MemoryStream(new byte[]{118, 118, 118, 118}); // vvvv
                case "Time": return Time.Time.MinUTCTime(); // 1.1.0001 @ 0am
                case "Message":return new Message();
            }
            return null;
        }

        public string FindAlias(Type t)
        {
            foreach (Type key in Keys)
            {
                if (key == t) return this[key];
            }
            return null;
        }

        public Type FindBaseType(Type t)
        {
            foreach (Type key in Keys)
            {
                if (key == t) return key;
                if (t.IsSubclassOf(key)) return key;
            }
            return null;
        }


        public string FindBaseAlias(Type t)
        {
            foreach (Type key in Keys)
            {
                if (key == t) return this[key];
                if (t.IsSubclassOf(key)) return this[key];
            }
            return null;
        }

        public Type FindType(string alias)
        {
            Type type = typeof(string);
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
