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

        [Input("Print Message", IsBang = true)]
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
                    FTopic.SliceCount = 
                    FTimeStamp.SliceCount = 
                    FConfigOut.SliceCount = 
                    FOutput.SliceCount = 0;
                    
                    FOutput.Flush();
                    FTimeStamp.Flush();
                    FTopic.Flush();
                    FConfigOut.Flush();
                }
                return;
            }

            if (!FInput.IsChanged) return;

            FOutput.SliceCount = SpreadMax;
            FTopic.SliceCount = SpreadMax;
            FConfigOut.SliceCount = SpreadMax;

            var timeConnected = FTimeStamp.IsConnected; // treat time a little different, because it can be quite slow.
            FTimeStamp.SliceCount = timeConnected? SpreadMax : 0;

            for (int i = 0; i < SpreadMax; i++)
            {
                Message m = FInput[i];
                FOutput[i] = m.ToString();
                FTopic[i] = m.Topic;
                if (timeConnected) FTimeStamp[i] = m.TimeStamp;;

                FConfigOut[i] = FInput[i].GetFormular().ToString(true);

                if (FPrint[i])
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("== Message " + i + " ==");
                    sb.AppendLine();

                    sb.AppendLine(FInput[i].ToString());
                    sb.Append("====\n");
                    FLogger.Log(LogType.Debug, sb.ToString());
                }
            }
            FTopic.Flush();
            if (timeConnected) FTimeStamp.Flush();
            FOutput.Flush();
            FConfigOut.Flush();
        }
    }
}