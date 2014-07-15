using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.PluginInterfaces.V2;

using VVVV.Core.Logging;


namespace VVVV.Packs.Message.Nodes
{
    using Message = VVVV.Packs.Message.Core.Message;
    
    [PluginInfo(Name = "Cache", Category = "Message", Help = "Stores Messages", Tags = "velcrome")]
    public class CacheMessageNode : IPluginEvaluate
    {

#pragma warning disable 649, 169

        [Input("Message")]
        IDiffSpread<Message> FMessage;

        [Input("ID")]
        IDiffSpread<string> FID;

        [Input("Retrieve ID")]
        IDiffSpread<string> FRetrieveID;

        [Input("Clean", IsSingle = true, IsBang = true)]
        ISpread<bool> FCleanNow;

        [Input("Clear", IsSingle = true, IsBang = true)]
        ISpread<bool> FClear;

        [Output("Output", AutoFlush = false)]
        private ISpread<ISpread<Message>> FOutput;

        [Output("AllMessages", AutoFlush = false)]
        private ISpread<Message> FAll;

        [Output("CacheCount", AutoFlush = false)]
        private ISpread<int> FCount;

        private Dictionary<string, Message> Cache = new Dictionary<string, Message>();

        [Import()]
        protected ILogger FLogger;

        #pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FClear.SliceCount > 0 && FClear[0]) Cache.Clear();

            if (FCleanNow.SliceCount > 0 && FCleanNow[0])
            {
                var newDict = new Dictionary<string, Message>();
                
                for (int i=0;i<FRetrieveID.SliceCount;i++)
                {
                    if (Cache.ContainsKey(FRetrieveID[i]))
                        newDict[FRetrieveID[i]] = Cache[FRetrieveID[i]];
                }
                Cache.Clear();
                Cache = newDict;
            }

            if ((FMessage.SliceCount > 0) && (FMessage[0] != null))
            {
                for (int i = 0; i < FID.SliceCount; i++)
                {
                    Cache[FID[i]] = FMessage[i];
                }
            }

            FCount.SliceCount = 1;
            FCount[0] = Cache.Count;
            FCount.Flush();

            SpreadMax = FRetrieveID.SliceCount;
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var key = FRetrieveID[i];
                
                if (Cache.ContainsKey(key))
                {
                    FOutput[i].SliceCount = 1;
                    FOutput[i][0] = Cache[key];
                }
                else FOutput[i].SliceCount = 0;
            }
            FOutput.Flush();

            FAll.AssignFrom(Cache.Values);
            FAll.Flush();



        }
    }
}
