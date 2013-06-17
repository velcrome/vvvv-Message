using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Collections;

using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Utils.Collections
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	[Serializable]
	public class SpreadList : ArrayList, ISerializable
	{
		public SpreadList() : base()
		{
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
