using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractMessageKeepNode : TypeableNode
    {
        [Input("Input", Order = 0)]
        public ISpread<Message> FInput;

        [Input("Reset", IsSingle = true, Order = int.MaxValue-1, IsBang = true)]
        public ISpread<bool> FReset;

        //[Input("Replace Keep", Order = int.MaxValue, Visibility = PinVisibility.OnlyInspector)]
        //public ISpread<MessageKeep> FReplaceData;

        //[Output("Keep", Order = int.MaxValue, Visibility = PinVisibility.OnlyInspector)]
        //public ISpread<MessageKeep> FKeep;

        public readonly MessageKeep MessageMessageKeep = new MessageKeep();

        protected virtual void SortKeep()
        {
            MessageMessageKeep.Sort(
                delegate(Message x, Message y) {
                    return (x.TimeStamp > y.TimeStamp) ? 1 : 0;
                }
            );
        }

    }
}