#region usings
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VVVV.Packs.Message.Core;

#endregion usings

namespace VVVV.Pack.Game.Core
{
  	public class JsonSerializer : JsonConverter
	{

        public JsonSerializer()
        {
        }
		
        
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
		{
			Bin bin = value as Bin;
		    writer.WriteStartObject();
			writer.WritePropertyName("Type");
		    
			
			Type type = (bin == null) || (bin.Count == 0)? typeof(string) : bin.GetInnerType();

            writer.WriteValue(TypeIdentity.Instance[type]);
			
    		writer.WritePropertyName("Bin");
			writer.WriteStartArray();
			foreach (object o in bin) {
				serializer.Serialize(writer, o);
			}
			writer.WriteEndArray();

			writer.WriteEndObject();
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);
            
            var jT = jsonObject.GetValue("Type");
		    var typeName = (string) jT.ToObject(typeof(string), serializer);

		    Type type = TypeIdentity.Instance.FindType(typeName);
            JArray jArray = (JArray) jsonObject.GetValue("Bin");

   			Bin bin = Bin.New(type);
            
		    foreach (var o in jArray)
            {
                var instance = o.ToObject(type, serializer);
                bin.Add(instance);
            }
			return bin;
			
		}	
		
		public override bool CanConvert(Type objectType)
		{
			return typeof(Bin).IsAssignableFrom(objectType);
		}

    }
}