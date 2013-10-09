using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Pack.Messaging;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.Messaging
{
    [PluginInfo(Name = "Store", Category = "Message", Help = "Stores Messages", Tags = "velcrome")]
    public class MessageStore : IPluginEvaluate
    {

        #pragma warning disable 649, 169

        [Input("Edit Message")]
        IDiffSpread<Message> FEdit;

        [Input("Edit Slice")]
        IDiffSpread<int> FEditIndex;

        [Input("Edit", IsSingle = true, IsBang = true)]
        ISpread<bool> FEditNow;

        [Input("Delete Slice")]
        ISpread<int> FDeleteIndex; 

        [Input("Delete", IsSingle = true, IsBang = true)]
        ISpread<bool> FDeleteNow;

        [Input("Clear", IsSingle = true, IsBang = true)]
        ISpread<bool> FClear;

        [Input("Add Message")]
        ISpread<Message> FAdd;

        [Input("Add", IsSingle = true, IsBang = true)]
        ISpread<bool> FAddNow;

    	[Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;
    	

        private Spread<Message> Store = new Spread<Message>(); 
            
    	[Import()]
        protected ILogger FLogger;

        #pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            

        	if (!FAddNow[0] && !FEditNow[0] && !FDeleteNow[0] && !FClear[0]) return;
        	if (FClear[0]) Store.SliceCount = 0;

            if (FEditNow[0])
            {
                int count = Math.Max(FEditIndex.SliceCount, FEdit.SliceCount);
                
                for (int i=0;i<count;i++)
                {
                    Store[FEditIndex[i]] = FEdit[i];
                }
            }

            if (FDeleteNow[0])
            {
                var del = FDeleteIndex.ToList();

                int size = Store.Count();

                for (int i=0;i<del.Count;i++)
                {
                    del[i] = VMath.Zmod(del[i], size);
                }
                del.Sort();

                for (int i = 0;i<del.Count;i++)
                {
                   Store.RemoveAt(del[i] - i);
                }
            }

            if (FAddNow[0])
            {
                Store.AddRange(FAdd);
            }
       	
        	
        	FOutput.SliceCount = 0;
        	FOutput.AssignFrom(Store);
        	FOutput.Flush();
        	
        }
    }
}
