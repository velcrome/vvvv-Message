using System.Collections.Generic;
using System;
using System.Linq;
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
        public bool ManageAllChanges
        {
            get;
            private set;
        }

        private IIOContainer Change;
        private IIOContainer ChangeIndex;
        private IIOContainer DoAllChange;

        public readonly MessageKeep Keep = new MessageKeep();

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            FAdvanced.Changed += ManageDiffOutput;

            ManageAllChanges = true;
        }

        private void ManageDiffOutput(IDiffSpread<bool> advanced)
        {
            if (FAdvanced[0])
            {
                Keep.QuickMode = false;

                var ia = new InputAttribute("Include input in Diff");
                ia.Order = 2;
                ia.IsSingle = true;
                ia.IsToggle = true;
                ia.DefaultBoolean = true;

                Type pinType = typeof(IDiffSpread<>).MakeGenericType(typeof(bool));
                DoAllChange = FIOFactory.CreateIOContainer(pinType, ia);
                var pin = DoAllChange.RawIOObject as IDiffSpread<bool>;
                pin.Changed += UpdateChangeAll;

                var attr = new OutputAttribute("Message Diff");
                attr.AutoFlush = false;
                attr.Order = 2;

                pinType = typeof(Pin<>).MakeGenericType(typeof(Message));
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

                if (DoAllChange!= null)
                {
                    DoAllChange.Dispose();
                    DoAllChange = null;
                    ManageAllChanges = true; 
                }

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

        private void UpdateChangeAll(IDiffSpread<bool> spread)
        {
            ManageAllChanges = spread[0];
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
                IEnumerable<int> indexes;
                var changes = Keep.Sync(out indexes);

                FChangeIndexOut.SliceCount = 0;
                FChangeIndexOut.AssignFrom(indexes);
                FChangeIndexOut.Flush();

                FChangeOut.SliceCount = 0;
                FChangeOut.AssignFrom(changes);
                FChangeOut.Flush();
            }

            FOutput.FlushResult(Keep);
            FCountOut.FlushInt(Keep.Count);

            return true;

        }

        public new void Dispose()
        {
            base.Dispose();
            Keep.Clear();
        }

    }
}