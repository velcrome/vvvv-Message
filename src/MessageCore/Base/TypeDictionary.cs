using System.Collections.Generic;

namespace VVVV.Packs.Message.Core
{
    public class TypeDictionary : Dictionary<string, string>
    {
        private static TypeDictionary instance;

        public static bool IsChanged
        {
            get;
            set;
        }

        public static TypeDictionary Instance
        {
            get
            {
                if (instance == null) instance = new TypeDictionary();
                return instance;
            }
        }
    }
}