using System.ComponentModel.Composition;
using Newtonsoft.Json;
using VVVV.Core.Logging;
using VVVV.Packs.Message;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes.Serializing
{
    using Message = VVVV.Packs.Message.Core.Message;

        #region PluginInfo
        [PluginInfo(Name = "AsJson", Category = "Message", Help = "Filter Messages", Tags = "Dynamic, velcrome, JSON")]
        #endregion PluginInfo
        public class MessageAsJsonStringNode : IPluginEvaluate
        {
            #pragma warning disable 649, 169
            [Input("Input")]
            IDiffSpread<Message> FInput;

            [Input("Pretty", IsSingle = true, IsToggle = true, DefaultBoolean = true)]
            IDiffSpread<bool> FPretty;

            [Output("String", AutoFlush = false)]
            ISpread<string> FOutput;

            [Import()]
            protected ILogger FLogger;

            #pragma warning restore

            public void Evaluate(int SpreadMax)
            {
                if (!FInput.IsChanged && !FPretty.IsChanged) return;

                FOutput.SliceCount = SpreadMax;
                JsonSerializer ser = new JsonSerializer();

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
        [PluginInfo(Name = "AsMessage", Category = "Message, Json", Help = "Filter Messages", Tags = "Dynamic, velcrome, JSON")]
        #endregion PluginInfo
        public class JsonStringAsMessageNode : IPluginEvaluate
        {
            #pragma warning disable 649, 169
            [Input("Input")]
            IDiffSpread<string> FInput;

            [Output("Message", AutoFlush = false)]
            ISpread<Message> FOutput;

            [Import()]
            protected ILogger FLogger;
            #pragma warning restore

            public void Evaluate(int SpreadMax)
            {
                if (!FInput.IsChanged) return;

                SpreadMax = FInput.SliceCount;
                FOutput.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {

                    FOutput[i] = JsonConvert.DeserializeObject<Message>(FInput[i]);
                }

                FOutput.Flush();
            }
        }

}
