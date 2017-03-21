using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Core.Logging;

namespace VVVV.Packs.Messaging.Nodes.Serializing
{

    #region PluginInfo
    [PluginInfo(Name = "Reader", Category = "Message", Help = "Read a stream of Messages from file.", Version = "Streaming", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageReadFromFileNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Filename", StringType = StringType.Filename, IsSingle=true, FileMask = "Json File(*.txt, *.json, *.js)|*.txt, *.json, *.js")]
        protected IDiffSpread<string> FFile;

        [Input("Count", IsSingle=true, DefaultValue=1, MinValue = -1)]
        protected ISpread<int> FCount;

        [Input("Reset", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        protected ISpread<bool> FReset;

        [Input("Read", IsSingle = true, IsBang=true, DefaultBoolean=false)]
        protected ISpread<bool> FRead;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        [Output("Error", AutoFlush = false)]
        protected ISpread<string> FError;

        [Output("End of Stream")]
        protected ISpread<bool> FEndOfStream;

        [Import()]
        protected ILogger FLogger;

        [Import]
        protected IPluginHost2 PluginHost;

        protected Stream File;
        protected JsonReader Reader;
        protected IEnumerator<JToken> MessageEnumerator;

        public void OnImportsSatisfied()
        {
            FFile.Changed += FileChanged;
        }

        private void FileChanged(IDiffSpread<string> spread)
        {
            FError.FlushItem("Loading.");
            FEndOfStream[0] = false;

            try
            {
                if (FFile.IsAnyInvalid() || string.IsNullOrWhiteSpace(FFile[0]))
                {
                    FError.FlushItem("Nothing Loaded.");
                    UnLoad();
                    return;
                }
                else UnLoad(); // reset everything anyway

                var fileName = FFile[0];

                File = new FileStream(fileName, FileMode.Open);
                var io = new StreamReader(File);
                Reader = new JsonTextReader(io);

                // todo: allow single-entry json files as well
                MessageEnumerator = (JObject.ReadFrom(Reader) as JArray).Children().GetEnumerator();

                FError.FlushItem("OK.");
            }
            catch (Exception ex)
            {
                UnLoad();
                FError.FlushItem(ex.Message);

                FLogger.Log(LogType.Error, ex.ToString());
            }
        }

        public void Evaluate(int SpreadMax)
        {
            if (MessageEnumerator == null) return; // no file loaded yet

            if (!FReset.IsAnyInvalid() && FReset[0]) // rewind
            {
                MessageEnumerator.Reset();
            }

            if (FRead.IsAnyInvalid() || !FRead[0])
            {
                if (FOutput.SliceCount > 0) FOutput.FlushNil();
                return; // avoidnil on Read pin
            }

            FEndOfStream[0] = false; // assume innocence
            FOutput.SliceCount = 0;

            var maxCount = FCount[0]; // if -1, attempt to load all.
            if (maxCount < 0) maxCount = int.MaxValue;

            int i =  0;
            // if there are no more to load, then stop short
            for (; i < maxCount && MessageEnumerator.MoveNext(); i++)
            {
                try
                {
                    var message = MessageEnumerator.Current.ToObject<Message>(); 
                    message.Commit(null, false); // commit all changes, but do not hold on

                    FOutput.Add(message);

                    FError.FlushItem("OK.");
                }
                catch (Exception ex)
                {
                    FLogger.Log(LogType.Warning, "Json Message [Reader] @ " + PluginHost.GetNodePath(false) + " faulted. Message skipped.");
                    FLogger.Log(LogType.Warning, ex.Message);

                    FError.FlushItem(ex.Message);
                }
            }

            if (i < maxCount) EndOfFile();

            FOutput.Flush();
        }

        protected void EndOfFile()
        {
            MessageEnumerator.Reset();

            // report end of file 
            FEndOfStream[0] = true;
            FLogger.Log(LogType.Debug, "[Reader] @ " + PluginHost.GetNodePath(false) + " reached End of Stream.");
        }

        protected void UnLoad()
        {
            File?.Dispose();
            File = null;

            Reader?.Close();
            Reader = null;

            MessageEnumerator?.Dispose();
            MessageEnumerator = null;

            FOutput.FlushNil();
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
