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
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Utils.Message {
	public class MessageResolver : DataContractResolver
	{
		
		public Dictionary<Type, string> Identity = new Dictionary<Type, string>();
		
		public MessageResolver() {
			
			// add types to DynamicNode.cs in Line 144 as well. 
			
			Identity.Add(typeof(bool), "bool".ToLower());
			Identity.Add(typeof(int), "int".ToLower());
			Identity.Add(typeof(double), "double".ToLower());
			Identity.Add(typeof(float), "float".ToLower());
			Identity.Add(typeof(string), "string".ToLower());
			
			Identity.Add(typeof(RGBAColor), "Color".ToLower());
			Identity.Add(typeof(Matrix4x4), "Transform".ToLower());
			Identity.Add(typeof(Vector2D), "Vector2D".ToLower());
			Identity.Add(typeof(Vector3D), "Vector3D".ToLower());
			Identity.Add(typeof(Vector4D), "Vector4D".ToLower());

			Identity.Add(typeof(Message), "Message".ToLower());
		}
		
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
	}
	}