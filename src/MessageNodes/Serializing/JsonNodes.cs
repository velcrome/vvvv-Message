using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VVVV.Packs.Messaging.Nodes.Serializing
{

        #region PluginInfo
        [PluginInfo(Name = "AsString", Category = "Message", Version="Json", Help = "Encode Messages into Json. Will used custom typed fields where applicable.", Author = "velcrome")]
        #endregion PluginInfo
        public class MessageAsJsonStringNode : IPluginEvaluate
        {
            [Input("Input")]
            protected IDiffSpread<Message> FInput;

            [Input("Pretty", IsSingle = true, IsToggle = true, DefaultBoolean = true)]
            protected IDiffSpread<bool> FPretty;

            [Output("String", AutoFlush = false)]
            protected ISpread<string> FOutput;

            [Import()]
            protected ILogger FLogger;

            public void Evaluate(int SpreadMax)
            {
                if (!FInput.IsChanged && !FPretty.IsChanged) return;

                FOutput.SliceCount = SpreadMax;

                JsonSerializerSettings settings = new JsonSerializerSettings();

                if (FPretty[0]) settings.Formatting = Formatting.Indented;
                    else settings.Formatting = Formatting.None;
                
                settings.TypeNameHandling = TypeNameHandling.None;

                for (int i = 0; i < SpreadMax; i++)
                {
                    string s = JsonConvert.SerializeObject(FInput[i], settings);

                    FOutput[i] = s ?? "";
                }
                FOutput.Flush();
            }
        }

        #region PluginInfo
        [PluginInfo(Name = "FromString", Category = "Message", Version ="Json", Help = "Decode almost any Json to Messages", Tags = "string", Author= "velcrome")]
        #endregion PluginInfo
        public class JsonStringAsMessageNode : IPluginEvaluate
        {
            [Input("Input")]
            protected IDiffSpread<string> FInput;

            [Output("Message", AutoFlush = false)]
            protected ISpread<Message> FOutput;

            [Import()]
            protected ILogger FLogger;

            public void Evaluate(int SpreadMax)
            {
                if (!FInput.IsChanged) return;

                SpreadMax = FInput.SliceCount;
                FOutput.SliceCount = 0;

                var settings = new JsonSerializerSettings();

                for (int i = 0; i < SpreadMax; i++)
                {
                    var result = JsonConvert.DeserializeObject(FInput[i]);

                    if (result is JArray)
                    {
                        foreach(var o in (result as JArray).Children()) {
                            FOutput.Add(o.ToObject<Message>());
                        }
                    }
                    if (result is JObject)
                    {
                        FOutput.Add((result as JObject).ToObject<Message>());
                    }
                }
                FOutput.Flush();
            }
        }

}
