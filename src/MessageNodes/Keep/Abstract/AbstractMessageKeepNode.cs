using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractMessageKeepNode : AbstractFormularableNode
    {
        [Input("Input", Order = 0)]
        public IDiffSpread<Message> FInput;

        [Input("Reset", IsSingle = true, IsBang = true, Order = 1, Visibility = PinVisibility.Hidden)]
        public IDiffSpread<bool> FReset;

        [Output("Output", Order = 0, AutoFlush = false)]
        public ISpread<Message> FOutput;

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

        protected virtual void SortKeep()
        {
            Keep.Sort(
                delegate(Message x, Message y) {
                    return (x.TimeStamp > y.TimeStamp) ? 1 : 0;
                }
            );
        }

    }
}