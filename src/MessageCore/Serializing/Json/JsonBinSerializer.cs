#region usings
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

#endregion usings

namespace VVVV.Packs.Messaging.Serializing
{
  	public class JsonBinSerializer : JsonConverter
	{
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Bin bin = value as Bin;
			
            if (bin.Count != 1) writer.WriteStartArray();

			foreach (object o in bin) {
                if (o is Stream)
                {
                    string data = GenerateStringFromStream(o as Stream);
                    serializer.Serialize(writer, data, typeof(string));
                } else serializer.Serialize(writer, o);
			}
            if (bin.Count != 1) writer.WriteEndArray();
		}
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{

            Type type;
            try 
            {
                type = objectType.GetGenericArguments()[0];
            } 
            catch (Exception) 
            {
                type = typeof(string);
            }
            Bin bin = BinFactory.New(type);

            //IEnumerable jArray = null;
            if (reader.TokenType == JsonToken.StartArray)
            {
                var jArray = JArray.Load(reader);

                foreach (var jO in jArray.Children())
                {
                    if (jO is JValue)
                    {
                        var o = (jO as JValue).Value;
                        if (type == typeof(Stream))
                        {
                            var raw = GenerateStreamFromString(o as string);
                            bin.Add(raw);
                        } else bin.Add(Convert.ChangeType(o, type));
                    }

                    if (jO is JObject)
                    {
                        var o = (jO as JObject).ToObject(type);
                        bin.Add(o);
                    }
                }

            }
            else
            {
                if (typeof(Stream).IsAssignableFrom(type))
                {
                    var o = serializer.Deserialize(reader, typeof(string)) as string;
                    bin.Add(GenerateStreamFromString(o));
                }
                else
                {
                    try
                    {
                        var o = serializer.Deserialize(reader, type);
                        bin.Add(o);
                    }
                    catch
                    {
                        // casting errors
                    }
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
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(System.Text.Encoding.UTF8.GetBytes (s));
            writer.Flush();
            ms.Position = 0;
            return ms;
        }

        public string GenerateStringFromStream(Stream input)
        {
            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return System.Text.Encoding.Default.GetString(ms.ToArray());
        }

    }
}