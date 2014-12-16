using System.ComponentModel.Composition;

using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;

using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    [PluginInfo(Name = "Search", Category = "Message.Spread", Help = "Allows LINQ queries for Messages", Tags = "LINQ", Author = "velcrome")]
    public class MessageSearchNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private IDiffSpread<Message> FInput;

        [Input("Where", DefaultString = "foo.Equals(\"bar\")", IsSingle = true)]
        private IDiffSpread<string> FQuery;

        [Output("Message", AutoFlush = false)]
        private ISpread<Message> FOutput;

        [Output("Former Slice", AutoFlush = false)]
        private ISpread<int> FFormerSlice;

        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid())
                SpreadMax = 0;
            else SpreadMax = FInput.SliceCount;

            if (SpreadMax == 0)
            {
                if (FOutput.SliceCount != 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.Flush();
                }
                return;
            }
            
            if (!FInput.IsChanged && !FQuery.IsChanged) return;


//            var queryable = (FInput.AsEnumerable());
            var queryable = FInput;

            var query = from message in queryable
                        select message;

            
            foreach (var where in FQuery )
            {
                query = query.Where(where);
            }
            query.Select(message => message);
            
            var result = query.ToArray();


            FOutput.SliceCount = 0;
            FOutput.AssignFrom(result);

            SpreadMax = result.Length;
            FFormerSlice.SliceCount = SpreadMax;

            for (int i=0;i<SpreadMax;i++)
            {
                FFormerSlice[i] = FInput.IndexOf(result[i]);
            }

            FFormerSlice.Flush();
            FOutput.Flush();
        }
    }
}
