using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VVVV.Packs.Messaging.Serializing
{
    using Time = VVVV.Packs.Time.Time;

    public class JsonMessageSerializer : JsonConverter
    {
        private Type[] NativeJsonTypes = new Type[] { typeof(string), typeof(bool), typeof(double), typeof(int), typeof(Message) };

        private Regex NameRegex = new Regex(@"(.+?)<(\w*)>|(.+?)$"); // name<specificType>


        public JsonMessageSerializer()
        {
        }
		
        
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
		{
            var message = value as Message;
            writer.WriteStartObject();

            writer.WritePropertyName("Topic");
            writer.WriteValue(message.Topic);


            foreach (var name in message.Fields)
            {
                var bin = message[name];
                string extraType = "";

                if (!NativeJsonTypes.Contains(bin.GetInnerType()))
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
                    else serializer.Serialize(writer, o, bin.GetInnerType());
                }
                if (bin.Count != 1) writer.WriteEndArray();
            }

            writer.WritePropertyName("Stamp");
            serializer.Serialize(writer, message.TimeStamp, typeof(Time));

            JObject.FromObject(message.TimeStamp, serializer);

            writer.WriteEndObject();
        }
		
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
            var message = new Message();

            JObject jMessage = JObject.Load(reader);

            var topic = jMessage.SelectToken("Topic");
            if (topic != null) message.Topic = (topic as JValue).Value as string;

            var time = jMessage.SelectToken("Stamp");
            if (time != null) message.TimeStamp = (time as JObject).ToObject<Time>();

            foreach (JProperty bin in jMessage.Children())
            {

                string name = bin.Name;

                if (name == "Topic" || name == "Stamp") continue;

                var m = NameRegex.Match(name);

                // explicit typing?
                if (m.Success && m.Groups[2].Success)
                {
                    name = m.Groups[1].Value as string;
                    var alias = m.Groups[2].Value as string;
                    var baseType = TypeIdentity.Instance.FindType(alias);
                    
                    var binType = typeof(Bin<>).MakeGenericType(baseType);

                    if (bin.Value.Type != JTokenType.Null)
                    {
                        var content = bin.Value.ToObject(binType) as Bin;
                        message[name] = content;
                    }
                    else
                    {
                        //skip Null
                    }
                }

                // try to infer type
                else
                {
                    JTokenType jType = JTokenType.Null;

                    if (bin.Value is JArray)
                    {
                        if (bin.Value.Count() > 0)
                            jType = (bin.Value as JArray).First.Type;
                    }
                    else
                    {
                        jType = bin.Value.Type;
                    }

                    Type itemType = null;
                    switch (jType)
                    {
                        case JTokenType.Boolean: itemType = typeof(bool); break;
                        case JTokenType.Float: itemType = typeof(double); break; // promote to double, as this is the default for vvvv
                        case JTokenType.Integer: itemType = typeof(int); break;
                        case JTokenType.String: itemType = typeof(string); break;

                        case JTokenType.Guid: itemType = typeof(string); break;
                        case JTokenType.Uri: itemType = typeof(string); break;

                        case JTokenType.Object: itemType = typeof(Message); break; // any non-descriptant objects will be included as nested Messages

                    }

                    if (itemType != null) // e.g. JTokenType.Null will be skipped
                    {
//                        var content = BinFactory.New(itemType);
                        var binType = typeof(Bin<>).MakeGenericType(itemType);
                        var content = bin.Value.ToObject(binType) as Bin;
                        message[name] = content;
                    }
                }

            
            
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
