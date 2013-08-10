using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Messaging
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

        public TypeIdentity()
	    {
            // This is the only place where you need to add new datatypes.

            Add(typeof(bool), "bool".ToLower());
            Add(typeof(int), "int".ToLower());
            Add(typeof(double), "double".ToLower());
            Add(typeof(float), "float".ToLower());
            Add(typeof(string), "string".ToLower());

            Add(typeof(RGBAColor), "Color".ToLower());
            Add(typeof(Matrix4x4), "Transform".ToLower());
            Add(typeof(Vector2D), "Vector2D".ToLower());
            Add(typeof(Vector3D), "Vector3D".ToLower());
            Add(typeof(Vector4D), "Vector4D".ToLower());

            Add(typeof(Utils.Messaging.Message), "Message".ToLower());	        
	    }

    }
}
