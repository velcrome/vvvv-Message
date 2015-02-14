using System.ComponentModel.Composition;
using System.Text;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Packs.Time;

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
        ISpread<Time> FTimeStamp;

        [Output("Output", AutoFlush = false)] 
        ISpread<string> FOutput;

        [Output("Configuration", AutoFlush = false)] 
        ISpread<string> FConfigOut;

        [Import()] protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FInput.SliceCount <= 0 || FInput[0] == null) SpreadMax = 0;
            else SpreadMax = FInput.SliceCount;

            if (!FInput.IsChanged) return;

            FOutput.SliceCount = SpreadMax;
            FTimeStamp.SliceCount = SpreadMax;
            FTopic.SliceCount = SpreadMax;
            FConfigOut.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                Message m = FInput[i];
                FOutput[i] = m.ToString();
                FTopic[i] = m.Topic;
                FTimeStamp[i] = m.TimeStamp;;

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
            FTimeStamp.Flush();
            FOutput.Flush();
            FConfigOut.Flush();
        }
    }
}