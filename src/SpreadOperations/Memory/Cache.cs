#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using System.Linq;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{


    public class CacheNode<T> : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<T> FInput;

        [Input("Input ID")]
        public ISpread<int> FIndex;

        [Input("Retain Time", IsSingle = true, DefaultValue = 1.0)]
        public ISpread<double> FTime;

        [Input("Reset", IsSingle = true)]
        public ISpread<bool> FReset;

        [Output("Cached Output")]
        public ISpread<T> FOutput;

        [Output("Cached ID")]
        public ISpread<int> FCachedID;

        [Import()]
        public ILogger FLogger;

        [Import()]
        protected IHDEHost FHDEHost;

        protected Dictionary<int, T> data = new Dictionary<int, T>();
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