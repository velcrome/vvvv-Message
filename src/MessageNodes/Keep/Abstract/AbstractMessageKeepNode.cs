using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractMessageKeepNode : AbstractFormularableNode
    {
        [Input("Input", Order = 0)]
        public IDiffSpread<Message> FInput;

        [Input("Reset", IsSingle = true, IsBang = true, Order = 1, Visibility = PinVisibility.Hidden)]
        public ISpread<bool> FReset;

        // IDiffSpread Formular Order = 2

        [Input("Default", IsSingle = true, Order = 3, Visibility = PinVisibility.Hidden, AutoValidate = false)]
        public ISpread<Message> FDefault;

        [Output("Output", Order = 0, AutoFlush = false)]
        public ISpread<Message> FOutput;

        public readonly MessageKeep Keep = new MessageKeep();

        
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