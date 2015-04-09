using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Messaging.Serializing
{
    public class JsonMessageSerializer : JsonConverter
    {
        private Type[] NativeJsonTypes = new Type[] { typeof(string), typeof(bool), typeof(double), typeof(int) };


        public JsonMessageSerializer()
        {
        }
		
        
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
		{
            var message = value as Message;

            writer.WritePropertyName(message.Topic);

            writer.WriteStartObject();

            writer.WritePropertyName("UTC");
            writer.WriteValue(message.TimeStamp.UniversalTime);

            writer.WritePropertyName("Zone");
            writer.WriteValue(message.TimeStamp.TimeZone.Id);

            foreach (var name in message.Fields)
            {
                var bin = message[name];
                string extraType = "";

//                if (!NativeJsonTypes.Contains(bin.GetInnerType()))
                    extraType = "<"+TypeIdentity.Instance.FindAlias(bin.GetInnerType())+">";

                writer.WritePropertyName(name + extraType);

                if (bin.Count != 1) writer.WriteStartArray();

                foreach (object o in bin)
                {
                    if (o is Stream)
                    {
                        var sr = new StreamReader((Stream)o);
                        serializer.Serialize(writer, sr.ReadToEnd());
                    }
                    else serializer.Serialize(writer, o);
                }
                if (bin.Count != 1) writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
		{
//            reader.SupportMultipleContent = true;
 //           var token = JObject.ReadFrom(reader);
            
            JObject jMessage= JObject.Load(reader);


            var message = new Message(jMessage.Path);
            var jBins = jMessage.Values().First();


            var utc = jBins.SelectToken("$.UTC");
            var zone = jBins.SelectToken("$.Zone");

            

            var regexp = new Regex("[^\\.](.+)<(.*)>");

            foreach (var bin in jBins)
            {
                
                
                string name;
                string ofType = null;

//                if (bin.Path == UTC) 


                var m = regexp.Match(bin.Path);

                if (m.Success)
                {
//                    name = m.Groups[0];
  //                  ofType = m.Groups[1];
                }
                else name = bin.Path;



                //object instance = null;
                //if (type == typeof(Stream))
                //{
                //    instance = o.ToObject(typeof(string), serializer);
                //    bin.Add(GenerateStreamFromString((string)instance));

                //}
                //else
                //{
                //    instance = o.ToObject(type, serializer);
                //    bin.Add(instance);
                //}
            }
			return message;
			
		}

        public override bool CanConvert(Type objectType)
        {
            return typeof(Message).IsAssignableFrom(objectType);
        }

        private Stream GenerateStreamFromString(string s)
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
