using System.Collections.Generic;
using System;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using System.ComponentModel.Composition;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class AbstractMessageKeepNode : AbstractFormularableNode
    {
#pragma warning disable 649, 169
        [Config("Diff Output", IsSingle=true, IsToggle=true)]
        public IDiffSpread<bool> FAdvanced;

        [Input("Input", Order = 0)]
        public IDiffSpread<Message> FInput;

        [Input("Reset", IsSingle = true, IsBang = true, Order = 1, Visibility = PinVisibility.Hidden)]
        public IDiffSpread<bool> FReset;

        [Input("Remove", Order = 2, Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<Message> FRemove;

        [Output("Output", Order = 0, AutoFlush = false)]
        public Pin<Message> FOutput;

        [Output("Internal Count", Order = int.MaxValue, AutoFlush = false)]
        public ISpread<int> FCountOut;

#pragma warning restore

        [Import()]
        protected IIOFactory FIOFactory;

        public Pin<Message> FChangeOut;
        public Pin<int> FChangeIndexOut;

        public IIOContainer Change;
        public IIOContainer ChangeIndex;

        public readonly MessageKeep Keep = new MessageKeep();

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            FAdvanced.Changed += ManageDiffOutput;
        }

        private void ManageDiffOutput(IDiffSpread<bool> advanced)
        {
            if (FAdvanced[0])
            {
                Keep.QuickMode = false;

                var attr = new OutputAttribute("Message Diff");
                attr.AutoFlush = false;
                attr.Order = 2;
                
                Type pinType = typeof(Pin<>).MakeGenericType(typeof(Message)); 
                Change = FIOFactory.CreateIOContainer(pinType, attr);

                FChangeOut = Change.RawIOObject as Pin<Message>;
                attr = new OutputAttribute("Changed Message Index");
                attr.AutoFlush = false;
                attr.Order = 3;

                pinType = typeof(ISpread<>).MakeGenericType(typeof(int)); 
                ChangeIndex = FIOFactory.CreateIOContainer(pinType, attr);

                FChangeIndexOut = ChangeIndex.RawIOObject as Pin<int>;
            }
            else
            {
                Keep.QuickMode = true;

                if (ChangeIndex != null)
                {
                    ChangeIndex.Dispose();
                    ChangeIndex = null;
                    FChangeIndexOut = null;
                }

                if (FChangeOut != null)
                {
                    Change.Dispose();
                    Change = null;
                    FChangeOut = null;
                }

            }
        }

        protected virtual bool CheckRemove()
        {
            var anyChange = false;
            if (!FRemove.IsAnyInvalid())
            {
                foreach (var message in FRemove)
                {
                    anyChange |= Keep.Remove(message); // Remove will return false, if not in contained in Keep
                }
            }
            return anyChange;
        }
        
        
        protected virtual bool CheckReset()
        {
            var anyChange = CheckRemove();

            if (!FReset.IsAnyInvalid() && FReset[0])
            {
                Keep.Clear();
                anyChange = true;
            }
            return anyChange;

        }

        protected virtual bool UpKeep(bool force = false)
        {
            if (!force && !Keep.IsChanged)
            {
                if (!Keep.QuickMode && FChangeOut.SliceCount != 0)
                {
                    FChangeOut.SliceCount = 0;
                    FChangeOut.Flush();
                    FChangeIndexOut.SliceCount = 0;
                    FChangeIndexOut.Flush();
                }

                return false;
            }

            if (Keep.QuickMode)
            {
                Keep.Sync();
            }
            else
            {
                IEnumerable<Message> changes;
                IEnumerable<int> indexes;
                changes = Keep.Sync(out indexes);

                FChangeIndexOut.SliceCount = 0;
                FChangeIndexOut.AssignFrom(indexes);
                FChangeIndexOut.Flush();

                FChangeOut.SliceCount = 0;
                FChangeOut.AssignFrom(changes);
                FChangeOut.Flush();
            }

            FOutput.SliceCount = 0;
            FOutput.AssignFrom(Keep);
            FOutput.Flush();

            FCountOut[0] = Keep.Count;
            FCountOut.Flush();

            return true;

        }

    }
}