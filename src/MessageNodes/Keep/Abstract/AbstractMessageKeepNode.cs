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

        [Output("Change Data", Order = int.MaxValue - 2, AutoFlush = false, Visibility = PinVisibility.Hidden)]
        Pin<Message> FChangeDataOut;

        [Output("Changed Keep Slice", Order = int.MaxValue - 1, AutoFlush = false, Visibility = PinVisibility.Hidden)]
        Pin<int> FChangeOut;

        [Output("Internal Count", Order = int.MaxValue)]
        ISpread<int> FCountOut;
#pragma warning restore
        public readonly MessageKeep Keep = new MessageKeep();

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            FReset.Changed += Reset;
        }

        protected virtual void Reset(IDiffSpread<bool> spread)
        {
            if (FReset[0])
            {
                Keep.Clear();
                FOutput.SliceCount = 0;
                FOutput.Flush();
            }
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

        protected virtual bool UpKeep()
        {
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

                        foreach (var index in indexes)
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
                
                FCountOut[0] = Keep.Count;

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
        }
    }
}