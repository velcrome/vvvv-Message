using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{

    // better than the GetSlice (Node), because it allows binning and Index Spreading
    [PluginInfo(Name = "GetSlice", Category = "Message", Help = "GetSlice Messages", Author = "velcrome")]
    public class MessageGetSliceNode : GetSlice<Message>
    {
    }

    public class GetSlice<T> : IPluginEvaluate
	{
		#region fields & pins
        [Input("Input", BinSize = 1, CheckIfChanged=true, BinName = "Bin Size")]
		protected ISpread<ISpread<T>> FInput;

		[Input("Index", DefaultValue = 0, CheckIfChanged=true)]
        protected ISpread<int> FIndex;

		[Output("Output", AutoFlush = false)]
        protected ISpread<ISpread<T>> FOutput;

        [Import]
        protected ILogger FLogger;
        #endregion fields & pins

        public void Evaluate(int SpreadMax)
		{
            SpreadMax = FInput.IsAnyInvalid() || FIndex.IsAnyInvalid()? 0 : FIndex.CombineWith(FIndex);

            if (SpreadMax == 0)
            {
               FOutput.FlushNil();
                return;
            } else {
                if (!FIndex.IsChanged && !FInput.IsChanged) return;
            }

            FOutput.SliceCount = SpreadMax;
			
			for (int i=0;i<SpreadMax;i++) {
				FOutput[i].AssignFrom(FInput[FIndex[i]]);
			}
			FOutput.Flush();
		}
	}
}