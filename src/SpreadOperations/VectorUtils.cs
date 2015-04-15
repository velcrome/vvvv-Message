using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using System.Linq;
using System.Collections;
using VVVV.Utils.VMath;

namespace VVVV.Utils
{
    public static class VectorUtils
    {
        public static string ToString(this Vector2D v)
        {
            return "(" + v.x.ToString() + ", " + v.y.ToString() + ") ";
        }

        public static string ToString(this Vector3D v)
        {
            return "(" + v.x.ToString() + ", " + v.y.ToString() + ", "+v.z.ToString()+") ";
        }

        public static string ToString(this Vector4D v)
        {
            return "(" + v.x.ToString() + ", " + v.y.ToString() + ", " + v.z.ToString() + ", "+v.w.ToString()+") ";
        }
    }
}