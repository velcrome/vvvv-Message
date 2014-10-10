#region usings
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VVVV.Packs.Message;
using VVVV.Packs.Message.Core;

#endregion usings

namespace VVVV.Pack.Game.Core
{
  	public class JsonBinSerializer : JsonConverter
	{

        public JsonBinSerializer()
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
                if (o is Stream)
                {
                    var sr = new StreamReader((Stream)o);
                    serializer.Serialize(writer, sr.ReadToEnd());
                } else serializer.Serialize(writer, o);
			}
			writer.WriteEndArray();

			writer.WriteEndObject();
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);
            
            var jT = jsonObject.GetValue("Type");
		    var typeAlias = (string) jT.ToObject(typeof(string), serializer);

            Type type = TypeIdentity.Instance.FindType(typeAlias);
            JArray jArray = (JArray) jsonObject.GetValue("Bin");

   			Bin bin = Bin.New(type);
            
		    foreach (var o in jArray)
		    {
		        object instance = null;
                if (type == typeof(Stream))
                {
                    instance = o.ToObject(typeof(string), serializer);
                    bin.Add(GenerateStreamFromString((string)instance));

                } else {
                    instance = o.ToObject(type, serializer);
                    bin.Add(instance);
                }
            }
			return bin;
			
		}	
		
		public override bool CanConvert(Type objectType)
		{
			return typeof(Bin).IsAssignableFrom(objectType);
		}

        public Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

    }
}