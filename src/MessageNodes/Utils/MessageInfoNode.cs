using System.ComponentModel.Composition;
using System.Text;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Packs.Time;
using VVVV.Utils;

namespace VVVV.Pack.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Info", Category = "Message", Help = "Help to Debug Messages", Tags = "TTY", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageInfoNode : IPluginEvaluate
    {
#pragma warning disable 649, 169

        [Input("Input")] 
        IDiffSpread<Message> FInput;

        [Input("Print to TTY", IsToggle = true)]
        IDiffSpread<bool> FPrint;

        [Output("Topic", AutoFlush = false)] 
        ISpread<string> FTopic;

        [Output("Timestamp", AutoFlush = false)] 
        Pin<Time> FTimeStamp;

        [Output("Output", AutoFlush = false)] 
        ISpread<string> FOutput;

        [Output("Configuration", AutoFlush = false)] 
        ISpread<string> FConfigOut;

        [Import()] protected ILogger FLogger;

#pragma warning restore

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

                FConfigOut[i] = FInput[i].Formular.ToString();

                if (FPrint[i])
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("\t === \t ["+i+"]\t ===\t");
                    sb.AppendLine(FInput[i].ToString());
                    sb.AppendLine();
                    FLogger.Log(LogType.Message, sb.ToString());
                }
            }
            
            if (timeConnected) FTimeStamp.Flush();

            FTopic.Flush();
            FOutput.Flush();
            FConfigOut.Flush();
        }
    }
}