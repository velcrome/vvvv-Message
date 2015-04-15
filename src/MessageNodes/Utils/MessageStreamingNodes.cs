using System.ComponentModel.Composition;
using Newtonsoft.Json;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using Newtonsoft.Json.Linq;
using System.IO;
using VVVV.Utils;
using System.Collections.Generic;
using System;


namespace VVVV.Packs.Messaging.Nodes.Serializing
{

    #region PluginInfo
    [PluginInfo(Name = "Reader", Category = "Message", Help = "Stream Messages", Tags = "Streaming", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageReadFromFileNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
#pragma warning disable 649, 169
        [Input("Filename", StringType = StringType.Filename, IsSingle=true)]
        IDiffSpread<string> FFile;

        [Input("Count", IsSingle=true, DefaultValue=1)]
        ISpread<int> FCount;

        [Input("Reset", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        ISpread<bool> FReset;

        [Input("Read", IsSingle = true, IsBang=true, DefaultBoolean=false)]
        ISpread<bool> FRead;

        [Output("Output", AutoFlush = false)]
        ISpread<Message> FOutput;

        [Output("End of Stream")]
        ISpread<bool> FEndOfStream;

        [Import()]
        protected ILogger FLogger;

        protected Stream File;
        protected JsonReader Reader;
        protected IEnumerator<JToken> MessageEnumerator;

#pragma warning restore

        public void OnImportsSatisfied()
        {
            FFile.Changed += FileChanged;
        }

        private void FileChanged(IDiffSpread<string> spread)
        {
            if (File != null)
            {
                Reader.Close();
                File.Dispose();
            }
            File = new FileStream(FFile[0], FileMode.Open);

            var io = new StreamReader(File);
            Reader = new JsonTextReader(io);
            MessageEnumerator = (JObject.ReadFrom(Reader) as JArray).Children().GetEnumerator();

        }

        public void Evaluate(int SpreadMax)
        {
            if (!FReset.IsAnyInvalid() && FReset[0] && MessageEnumerator != null)
            {
                MessageEnumerator.Reset();
            }
            
            if (FRead.IsAnyInvalid() || !FRead[0]) return;

            FEndOfStream[0] = false;

 
            FOutput.SliceCount = 0;            
            var maxCount = FCount[0];
            for (int i = 0; i < maxCount;i++)
            {
                try
                {
                    MessageEnumerator.MoveNext();
                    var message = MessageEnumerator.Current.ToObject<Message>();
                    FOutput.Add(message);
                }
                catch (Exception e)
                {
                    MessageEnumerator.Reset();
                    FEndOfStream[0] = true;

                    FLogger.Log(LogType.Debug, e.ToString());
                }
            }
            FOutput.Flush();
        }

        public void Dispose()
        {
            if (File != null)
            {
                Reader.Close();
                File.Dispose();
            }
        }
    }

}
