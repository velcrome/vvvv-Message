using System.ComponentModel.Composition;

using System.Linq;
using System.Linq.Dynamic;

using VVVV.Core.Logging;
using VVVV.Packs.Messaging; 
using VVVV.PluginInterfaces.V2;  
using VVVV.Utils;


namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "SearchTemplate", Category = "Message.Spread", Help = "Allows LINQ queries for Messages", Tags = "LINQ", Author = "velcrome")]
    public class MessageSearchNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput; 

        [Output("Message", AutoFlush = false)]
        private ISpread<Message> FOutput;


        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
			if (Prep(out SpreadMax)) return;

        	var result = 
        					from message in FInput
        					let foo = (string)message["Foo"][0]
        					where foo != "bar"
        					select message;

             FOutput.SliceCount = 0;
            FOutput.AssignFrom(result.ToArray());
            FOutput.Flush();
        }
    	
		protected bool Prep(out int SpreadMax) {
            if (FInput.SliceCount <= 0 || FInput[0] == null)
                SpreadMax = 0;
            else SpreadMax = FInput.SliceCount;

            if (SpreadMax == 0)
            {
                if (FOutput.SliceCount != 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                return true;
            }
			if (!FInput.IsChanged) return true;
			
			return false;
		}			


    }
}
