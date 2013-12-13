using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Collections;

using Newtonsoft.Json;
using VVVV.Pack.Messaging.Serializing;

namespace VVVV.Pack.Messaging.Collections
{
	/// <summary>
	/// Description of BundleOSCNode.
	/// </summary>
	[Serializable]
	[JsonConverter(typeof(SpreadListSerializer))]
	public class SpreadList : ArrayList, ISerializable
	{
        
        public Type SpreadType {
			get {
				if (this.Count == 0) return typeof(object);
					else return this[0].GetType();
			}
		}
		
		//		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			for (int i=0;i<this.Count;i++)
			{
				info.AddValue(i.ToString(CultureInfo.InvariantCulture), this[i]);
			}
		}
		
		public void AssignFrom(IEnumerable source) {
			
			foreach (object o in source) {
				this.Add(o);
			}
			
		}
		
		public new SpreadList Clone() {
			SpreadList c = new SpreadList();
			c.AssignFrom(this);
			return c;
		}
	}
}
