using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractMessageKeepNode : AbstractFormularableNode
    {
#pragma warning disable 649, 169
        [Input("Input", Order = 0)]
        public IDiffSpread<Message> FInput;

        [Input("Reset", IsSingle = true, IsBang = true, Order = 1, Visibility = PinVisibility.Hidden)]
        public IDiffSpread<bool> FReset;

        [Output("Output", Order = 0, AutoFlush = false)]
        public ISpread<Message> FOutput;

        [Output("Changed Message", Order = int.MaxValue - 2, AutoFlush = false, Visibility = PinVisibility.Hidden)]
        public Pin<int> FChangeOut;

        [Output("Message Diff", Order = int.MaxValue - 1, AutoFlush = false)]
        public Pin<Message> FChangeDataOut;

        [Output("Internal Count", Order = int.MaxValue, AutoFlush=false)]
        public ISpread<int> FCountOut;
#pragma warning restore
        public readonly MessageKeep Keep = new MessageKeep();

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
//            FReset.Changed += Reset;
        }

        protected virtual bool CheckReset()
        {
            if (!FReset.IsAnyInvalid() && FReset[0])
            {
                Keep.Clear();
                return true;
            }
            return false;

        }

        protected virtual bool UpKeep(bool full = false)
        {
            FChangeOut.SliceCount = 0;
            FChangeDataOut.SliceCount = 0;

            if (Keep.IsChanged)
            {
                if (FChangeDataOut.IsConnected || FChangeOut.IsConnected)
                {
                    IEnumerable<Message> changes;
                    if (!FChangeOut.IsConnected && !full)  
                    {
                        changes = Keep.Sync();
                    }
                    else // more expensive to get the indices as well
                    {
                        IEnumerable<int> indexes;
                        changes = Keep.Sync(out indexes);

                        foreach (var index in indexes) FChangeOut.Add(index);
                        FChangeOut.Flush();
                    }
                    FChangeDataOut.AssignFrom(changes);
                    FChangeDataOut.Flush();
                }
                else Keep.Sync();

                
                FCountOut[0] = Keep.Count;
                FCountOut.Flush();

                return true;

            }
            return false;
        }

        protected void DumpKeep(int SpreadMax)
        {
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var message = Keep[i];
                FOutput[i] = message;

            }
            
            FOutput.Flush();
        }
    }
}