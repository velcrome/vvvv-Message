#region usings
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using VVVV.Utils.VColor;


//using System.Linq;

#endregion usings

namespace VVVV.Packs.Messaging.Serializing
{
  	public class JsonBinSerializer : JsonConverter
	{
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
//                var jArray = serializer.Deserialize(reader) as JArray;
                var jArray = JArray.Load(reader);

                foreach (var jO in jArray.Children())
                {
                    if (jO is JValue)
                    {
                        var o = (jO as JValue).Value;
                        bin.Add(o);
                    }

                    if (jO is JObject)
                    {
                        var o = (jO as JObject).ToObject(type);
                        bin.Add(o);
                    }

                    //                 if (type is Stream)
     //                   bin.Add(GenerateStreamFromString(((string)(jO.Value))));

                }

            }
            else
            {
                var o = serializer.Deserialize(reader, type);
                if (o is Stream)
                    bin.Add(GenerateStreamFromString((string)o));
                else bin.Add(o);
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