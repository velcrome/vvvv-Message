using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using Newtonsoft.Json;
using VVVV.Packs.Timing;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Info", Category = "Message", Help = "Help to Debug Messages", Tags = "TTY", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageInfoNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("Print to TTY", IsToggle = true)]
        protected IDiffSpread<bool> FPrint;

        [Output("Topic", AutoFlush = false)]
        protected ISpread<string> FTopic;

        [Output("Timestamp", AutoFlush = false)]
        protected Pin<Time> FTimeStamp;

        [Output("Output", AutoFlush = false)]
        protected ISpread<string> FOutput;

        [Output("Configuration", AutoFlush = false)] 
        protected ISpread<string> FConfigOut;

        [Import()]
        protected ILogger FLogger;

        protected JsonSerializerSettings FJsonSettings = new JsonSerializerSettings();


        public void OnImportsSatisfied()
        {
            FJsonSettings.Formatting = Formatting.Indented;
            FJsonSettings.TypeNameHandling = TypeNameHandling.None;
        }


        public void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;

            if (SpreadMax == 0)
            {
                if (FOutput.SliceCount != 0)
                {
                    FOutput.FlushNil();
                    FTimeStamp.FlushNil();
                    FTopic.FlushNil();
                    FConfigOut.FlushNil();
                }
                return;
            }

            if (!FInput.IsChanged) return;

            FOutput.SliceCount      = 
            FTopic.SliceCount       = 
            FConfigOut.SliceCount   = SpreadMax;

            var timeConnected = FTimeStamp.IsConnected; // treat the time pin a little different, because it can be quite slow. time type is a struct that gets copied within the pin
            FTimeStamp.SliceCount = timeConnected? SpreadMax : 0;

            for (int i = 0; i < SpreadMax; i++)
            {
                Message m = FInput[i];
                FOutput[i] = m.ToString();
                FTopic[i] = m.Topic;
                if (timeConnected) FTimeStamp[i] = m.TimeStamp;;

                FConfigOut[i] = FInput[i].Formular.Configuration;

                if (FPrint[i])
                {
                    //StringBuilder sb = new StringBuilder();
                    //sb.AppendLine("\t === \t ["+i+"]\t ===");
                    //sb.AppendLine(FInput[i].ToString());
                    //sb.AppendLine();
                    //FLogger.Log(LogType.Message, sb.ToString());

                    FLogger.Log(LogType.Message, "\t === \t ["+i+"]\t ===");
                    FLogger.Log(LogType.Message, JsonConvert.SerializeObject(FInput[i], FJsonSettings));
                }
            }
            
            if (timeConnected) FTimeStamp.Flush();

            FTopic.Flush();
            FOutput.Flush();
            FConfigOut.Flush();
        }
    }
}