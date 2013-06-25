#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;

using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Xml;
using System.Collections.ObjectModel;

using VVVV.Nodes;
using VVVV.Nodes.Messaging;
using VVVV.Utils.Messaging;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Utils.Messaging {
	public class MessageResolver : DataContractResolver
	{
	    private TypeIdentity Identity; 
        public MessageResolver()
        {
            Identity = new TypeIdentity();

        }

        #region Standard Serialisation

//      These methods are necessary for standard .Net serialisation 
//      For JSon we use different means, have a look in SpreadList.cs for its custom Json.NET handler.

        public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			if (Identity.ContainsKey(dataContractType))
			{
				XmlDictionary dictionary = new XmlDictionary();
				typeName = dictionary.Add(Identity[dataContractType]);
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
				return Identity.Keys;
			
			}
			
		}
		
		public override Type ResolveName(string typeName, string typeNamespace, Type type, DataContractResolver knownTypeResolver)
		{
			Type foundType = null;
			foreach (Type t in Identity.Keys) {
				if (typeName.ToLower() == Identity[t] && typeNamespace == t.FullName) {
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