using System.Linq;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using System.ComponentModel.Composition;
using System.Collections.Generic;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Hold", Category = "Message.Keep", Help = "Holds the last bunch of Messages that traveled through. Pick some or all.",
        Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageKeepHoldNode : AbstractMessageKeepNode
    {
        public enum HoldEnum
        {
            All,
            Index
        }

#pragma warning disable 649, 169

        [Input("Match Rule", DefaultEnumEntry = "All", IsSingle = true, Order = 2)]
        IDiffSpread<HoldEnum> FHold;

        [Input("Index", Order = int.MaxValue)]
        IDiffSpread<int> FIndex;

        [Output("Change Data", Order = int.MaxValue - 2, AutoFlush = false, Visibility= PinVisibility.Hidden)]
        Pin<Message> FChangeDataOut;

        [Output("Changed Keep Slice", Order = int.MaxValue - 1, AutoFlush = false, Visibility=PinVisibility.Hidden)]
        Pin<int> FChangeOut;

        [Output("Internal Count", Order = int.MaxValue)]
        ISpread<int> FCountOut;

 

#pragma warning restore

        public override void Evaluate(int SpreadMax)
        {
            var update = false;

            if (!FReset.IsAnyInvalid() && FReset[0])
            {
                Keep.Clear();
                update = true;
            }
            
            if (FInput.IsChanged && !FInput.IsAnyInvalid())
            {
                Keep.AssignFrom(FInput); 
                FCountOut[0] = Keep.Count;
                update = true;
            }

            FChangeOut.SliceCount = 0;
            FChangeDataOut.SliceCount = 0;

            if (Keep.IsChanged)
            {

                if (FChangeDataOut.IsConnected || FChangeOut.IsConnected)
                {

                    if (FChangeOut.IsConnected)  // more expensive
                    {
                        IEnumerable<int> indexes;
                        var changes = Keep.Sync(out indexes);

                        foreach(var index in indexes)
                            FChangeOut.Add(index);

                        FChangeDataOut.AssignFrom(changes);
                    }
                    else
                    {
                        var changes = Keep.Sync();
                        FChangeDataOut.AssignFrom(changes);
                    }


                }
                else Keep.Sync();

                FChangeOut.Flush();
                FChangeDataOut.Flush();

                
                update = true;

            }
            if (FHold.IsChanged ) update = true;
            if (FHold[0] == HoldEnum.Index && FIndex.IsChanged) update = true;

            if (update) 
            {
                switch (FHold[0])
                {
                    case HoldEnum.All:
                        FOutput.SliceCount = 0;
                        FOutput.AssignFrom(Keep);
                        break;
                    case HoldEnum.Index:
                        FOutput.SliceCount = FIndex.SliceCount;
                        for (int i = 0; i < FIndex.SliceCount;i++ )
                        {
                            FOutput[i] = Keep[FIndex[i]%Keep.Count];
                        }
                        break;
                }
                FOutput.Flush();
            }
         }

        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
           // do nothing.
        }
    }
}