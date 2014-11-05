#region usings
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Xml;
using VVVV.Packs.Message.Core;

#endregion usings

namespace VVVV.Packs.Message.Core.Serializing
{
	public class MessageResolver : DataContractResolver
	{
        public MessageResolver()
        {
        }

        #region Standard Serialisation

//      These methods are necessary for standard .Net serialisation 
        //      For JSon we use different means, have a look in JsonBinSerializer for its custom Json.NET handler.

        public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
            if (TypeIdentity.Instance.ContainsKey(dataContractType))
			{
				XmlDictionary dictionary = new XmlDictionary();
                typeName = dictionary.Add(TypeIdentity.Instance[dataContractType]);
				typeNamespace = dictionary.Add(dataContractType.FullName);
				return true; // indicating that this resolver knows how to handle
			}
			else
			{
				// Defer to the known type resolver
				return knownTypeResolver.TryResolveType(dataContractType, declaredType, null, out typeName, out typeNamespace);
			}
		}
		
		public IEnumerable<Type> KnownTypes { get {
            return TypeIdentity.Instance.Keys;
			
			}
			
		}
		
		public override Type ResolveName(string typeName, string typeNamespace, Type type, DataContractResolver knownTypeResolver)
		{
			Type foundType = null;
            foreach (Type t in TypeIdentity.Instance.Keys)
            {
                if (typeName.ToLower() == TypeIdentity.Instance[t] && typeNamespace == t.FullName)
                {
					foundType = t;
				}
			}

			if (foundType != null) {
				return foundType;
			}
			else
			{
				// Defer to the known type resolver
				return knownTypeResolver.ResolveName(typeName, typeNamespace, type, null);
			}
        }
        #endregion Standard Serialisation
    }
}