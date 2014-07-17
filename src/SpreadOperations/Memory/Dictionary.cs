
using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Pack.Message;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Generic.Memory
{
    public class DictionaryNode<K, T> : IPluginEvaluate
    {

#pragma warning disable 649, 169

        [Input("Input")]
        IDiffSpread<T> FValue;

        [Input("ID")]
        IDiffSpread<K> FID;

        [Input("Retrieve ID")]
        IDiffSpread<K> FRetrieveID;

        [Input("Clean", IsSingle = true, IsBang = true)]
        ISpread<bool> FDoCleanNow;

        [Input("Reset", IsSingle = true, IsBang = true)]
        ISpread<bool> FDoClear;

        [Input("Unique", IsSingle = true, IsToggle = true)]
        ISpread<bool> FIsUnique;

        [Output("Output", AutoFlush = false)]
        private ISpread<ISpread<T>> FOutput;

        [Output("All Objects", AutoFlush = false)]
        private Pin<T> FAll;

        [Output("CacheCount", AutoFlush = false)]
        private ISpread<int> FCount;

        private Dictionary<K, T> Dict = new Dictionary<K, T>();

        [Import()]
        protected ILogger FLogger;

#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FDoClear.SliceCount > 0 && FDoClear[0]) Dict.Clear();

            if (FDoCleanNow.SliceCount > 0 && FDoCleanNow[0])
            {
                var newDict = new Dictionary<K, T>();

                for (int i = 0; i < FRetrieveID.SliceCount; i++)
                {
                    if (Dict.ContainsKey(FRetrieveID[i]))
                        newDict[FRetrieveID[i]] = Dict[FRetrieveID[i]];
                }
                Dict.Clear();
                Dict = newDict;
            }

            if (!FValue.IsAnyInvalid())
            {
                for (int i = 0; i < FID.SliceCount; i++)
                {
                    if (FIsUnique[0])
                    {
                        if (!Dict.ContainsValue(FValue[i])) Dict[FID[i]] = FValue[i];
                    }
                    else Dict[FID[i]] = FValue[i];
                }
            }

            FCount.SliceCount = 1;
            FCount[0] = Dict.Count;
            FCount.Flush();

            SpreadMax = FRetrieveID.SliceCount;
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var key = FRetrieveID[i];

                if (Dict.ContainsKey(key))
                {
                    FOutput[i].SliceCount = 1;
                    FOutput[i][0] = Dict[key];
                }
                else FOutput[i].SliceCount = 0;
            }
            FOutput.Flush();

            if (FAll.IsConnected)
            {
                FAll.SliceCount = 0;
                FAll.AssignFrom(Dict.Values);
                FAll.Flush();
            }


        }
    }
}
