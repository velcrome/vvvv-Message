using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VVVV.Pack.Messaging.Collections;

namespace VVVV.Pack.Messaging.Serializing
{
        public class SpreadListSerializer : JsonConverter
        {

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var list = value as SpreadList;
                writer.WriteStartObject();
                writer.WritePropertyName("Type");


                Type type = (list == null) || (list.Count == 0) ? typeof(string) : list.SpreadType;
                var alias = TypeIdentity.FindAlias(type);

                writer.WriteValue(TypeIdentity.FindBaseAlias(type));

                writer.WritePropertyName("Spread");
                writer.WriteStartArray();
                foreach (object o in list)
                {
                    if (alias != null)
                    {
                        serializer.Serialize(writer, o);
                    }
                    else
                    {
                        // Treat Stream differently. Everything else should be serializable as it is
                        if (TypeIdentity.FindBaseAlias(o.GetType()) == TypeIdentity.FindAlias(typeof(Stream)))
                        {
                            var stream = (Stream)o;
                            stream.Seek(0, SeekOrigin.Begin);
                            StreamReader sr = new StreamReader(stream);
                            serializer.Serialize(writer, sr.ReadToEnd());
                            stream.Seek(0, SeekOrigin.Begin);
                        } else serializer.Serialize(writer, o.ToString()); // fallback for unknown types (only for dev purposes)
                    }
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                SpreadList sl = new SpreadList();
                JObject jsonObject = JObject.Load(reader);

                var jT = jsonObject.GetValue("Type");
                string typeName = (string)jT.ToObject(typeof(string), serializer);

                Type type = TypeIdentity.FindType(typeName);
                JArray jArray = (JArray)jsonObject.GetValue("Spread");

                foreach (var o in jArray)
                {
                    if (type != typeof(Stream))  // the only non-serializable class as of now is Stream (which can be both ComStream and MemoryStream)
                    {
                        var instance = o.ToObject(type, serializer);
                        sl.Add(instance);
                    } else {
                        string str = (string)o.ToObject(typeof(string), serializer);
                        var buffer = Encoding.ASCII.GetBytes(str);
                        sl.Add(new MemoryStream(buffer));
                    }
                }
                return sl;

            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(SpreadList).IsAssignableFrom(objectType);
            }


        }
    }


