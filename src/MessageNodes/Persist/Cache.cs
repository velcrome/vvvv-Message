
#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using System.Linq;
using VVVV.Packs.Message.Core;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Packs.Message.Nodes
{
    using Message = VVVV.Packs.Message.Core.Message;


    [PluginInfo(Name = "Cache",
        Category = "Message",
        Help = "Stores Messages",
        Author = "velcrome")]
    public class CacheNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<Message> FInput;

        [Input("Input ID")]
        public ISpread<int> FIndex;

        [Input("Retain Time", IsSingle = true, DefaultValue = 1.0)]
        public ISpread<double> FTime;

        [Input("Reset", IsSingle = true)]
        public ISpread<bool> FReset;

        [Output("Cached Output")]
        public ISpread<Message> FOutput;

        [Output("Cached ID")]
        public ISpread<int> FCachedID;

        [Import()]
        public ILogger FLogger;

        [Import()]
        protected IHDEHost FHDEHost;

        protected Dictionary<int, Message> data = new Dictionary<int, Message>();
        protected Dictionary<int, double> timestamp = new Dictionary<int, double>();

        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FReset[0])
            {
                data.Clear();
                timestamp.Clear();
            }

            SpreadMax = FInput.CombineWith(FIndex);

            if (FInput.SliceCount > 0 && FIndex.SliceCount > 0)
                for (int i = 0; i < SpreadMax; i++)
                {
                    data[FIndex[i]] = FInput[i];
                    timestamp[FIndex[i]] = FHDEHost.FrameTime;
                }

            var validTime = FHDEHost.FrameTime - FTime[0];

            var clear = from id in timestamp.Keys
                        where timestamp[id] < validTime
                        select id;

            foreach (var id in clear.ToArray())
            {
                data.Remove(id);
                timestamp.Remove(id);
            }

            FOutput.SliceCount =
            FCachedID.SliceCount = 0;
            FOutput.AssignFrom(data.Values);
            FCachedID.AssignFrom(data.Keys);

        }
    }


}
