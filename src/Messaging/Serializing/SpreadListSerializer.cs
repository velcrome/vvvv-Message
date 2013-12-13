using System;
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

                writer.WriteValue(TypeIdentity.Instance[type]);

                writer.WritePropertyName("Spread");
                writer.WriteStartArray();
                foreach (object o in list)
                {
                    try
                    {
                        serializer.Serialize(writer, o);
                    }
                    finally
                    {
                        serializer.Serialize(writer, o.ToString());
                    }
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                SpreadList sl = new SpreadList();
                JObject jsonObject = JObject.Load(reader);
                string typeName = "string";

                var jT = jsonObject.GetValue("Type");
                typeName = (string)jT.ToObject(typeof(string), serializer);

                Type type = typeof(string);
                foreach (Type key in TypeIdentity.Instance.Keys)
                {
                    if (TypeIdentity.Instance[key] == typeName)
                    {
                        type = key;
                    }
                }

                JArray jArray = (JArray)jsonObject.GetValue("Spread");

                foreach (var o in jArray)
                {
                    var instance = o.ToObject(type, serializer);
                    sl.Add(instance);
                }
                return sl;

            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(SpreadList).IsAssignableFrom(objectType);
            }


        }
    }


