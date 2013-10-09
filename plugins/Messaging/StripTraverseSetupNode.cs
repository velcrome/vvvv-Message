#region usings
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Core.Logging;

using VVVV.Pack.Messaging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "TraverseSetup", Category = "Strip", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class StripTraverseSetupNode : IPluginEvaluate
	{
		#region fields & pins
        #pragma warning disable 649, 169

        [Input("Teensy")]
		public IDiffSpread<Message> FTeensy;

		[Input("Stripe")]
		public IDiffSpread<Message> FStripe; 
		
		[Input("Wire")]
		public IDiffSpread<Message> FWire;		
		
		[Output("Ordered Stripes", AutoFlush = false)]
		public ISpread<ISpread<Message>> FOrderedStripe;
		
		[Output("Ordered IDs", AutoFlush = false)]
		public ISpread<ISpread<string>> FID;
		

		[Import()]
		public ILogger FLogger;

        #pragma warning restore
        #endregion fields & pins

        //called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (!FTeensy.IsChanged && !FWire.IsChanged && !FStripe.IsChanged) return;
			
			SpreadMax = FTeensy.SliceCount;
			FOrderedStripe.SliceCount = FID.SliceCount = SpreadMax;

			Message m = new Message();
			for (int i = 0; i < SpreadMax; i++) {
				FID[i].SliceCount = 0;
				FOrderedStripe[i].SliceCount = 0;
				
				string id = (string)FTeensy[i]["start"][0];

				while (id != "none") {
//					FLogger.Log(LogType.Debug, id);
					FID[i].Add(id);

					var stripes = 
						from s in FStripe
						where s["ID"][0].Equals(id)
						select s;
					
					FOrderedStripe[i].Add(stripes.First());
					
					var result = 
						from w in FWire
						where w["end"][0].Equals(id) // wires are somehow reversed. sorry 
						select w;				
					
					if (result.Count() == 0) {
						id = "none";
					}
					else {
						Message next = result.First();
						id = (string) next["start"][0];
					}
				}
					
			}
			
			FID.Flush();

			
			
			
			FOrderedStripe.Flush();
			
		}
	}
}
