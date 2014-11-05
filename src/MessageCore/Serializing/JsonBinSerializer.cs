#region usings
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VVVV.Packs.Message;
using VVVV.Packs.Message.Core;
//using System.Linq;

#endregion usings

namespace VVVV.Pack.Game.Core.Serializing
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

			Type type = (bin == null) || (bin.Count == 0)? typeof(string) : bin.GetInnerType();

            writer.WritePropertyName(TypeIdentity.Instance[type]);
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

		    var jT = jsonObject.Children().First();
		    var typeAlias = jT.Path;

            Type type = TypeIdentity.Instance.FindType(typeAlias);
            
            var jArray = jT.Values();

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