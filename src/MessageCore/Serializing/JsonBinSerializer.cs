#region usings
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


//using System.Linq;

#endregion usings

namespace VVVV.Packs.Messaging.Serializing
{
  	public class JsonBinSerializer : JsonConverter
	{

        public JsonBinSerializer()
        {
        }
		
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Bin bin = value as Bin;
//		    writer.WriteStartObject();

//			Type type = (bin == null) || (bin.Count == 0)? typeof(string) : bin.GetInnerType();

//            writer.WritePropertyName(TypeIdentity.Instance[type]);
			
            if (bin.Count != 1) writer.WriteStartArray();

			foreach (object o in bin) {
                if (o is Stream)
                {
                    var sr = new StreamReader((Stream)o);
                    serializer.Serialize(writer, sr.ReadToEnd());
                } else serializer.Serialize(writer, o);
			}
            if (bin.Count != 1) writer.WriteEndArray();

//			writer.WriteEndObject();
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
		{
			JObject jsonObject = JObject.Load(reader);

		    var jT = jsonObject.Children().First();
		    var typeAlias = jT.Path;

            Type type = TypeIdentity.Instance.FindType(typeAlias);
            
            var jArray = jT.Values();

   			Bin bin = BinFactory.New(type);
            
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