using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Merge", Category = "Message.Spread", Help = "Removes redundancy and null",
        Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageMergeNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
#pragma warning disable 649, 169
        public Spread<IIOContainer<ISpread<Message>>> FInputs = new Spread<IIOContainer<ISpread<Message>>>();
        public Dictionary<ISpread<Message>, bool> FPinEmpty = new Dictionary<ISpread<Message>, bool>();

        [Config("Input Count", DefaultValue = 1, MinValue = 1, IsSingle = true)]
        public IDiffSpread<int> FInputCount;

        [Input("Distinct", DefaultBoolean=true, IsSingle=true, IsToggle = true)]
        private ISpread<bool> FDistinct;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;

        [Import]
        public IIOFactory FIOFactory;
#pragma warning restore

        #region pin management
        public void OnImportsSatisfied()
        {
            FInputCount.Changed += HandleInputCountChanged;
        }

        private void HandlePinCountChanged<T>(ISpread<int> countSpread, Spread<IIOContainer<T>> pinSpread, Func<int, IOAttribute> ioAttributeFactory) where T : class
        {
            pinSpread.ResizeAndDispose(
                countSpread[0],
                (i) =>
                {
                    var ioAttribute = ioAttributeFactory(i + 1);
                    return FIOFactory.CreateIOContainer<T>(ioAttribute);
                }
            );
        }

        private void HandleInputCountChanged(IDiffSpread<int> sender)
        {
            HandlePinCountChanged(sender, FInputs, (i) => new InputAttribute(
                string.Format("Input {0}", i)));

            FPinEmpty.Clear();
            foreach (var pins in FInputs)
            {
                FPinEmpty[pins.IOObject] = false;
            }
        }

        #endregion

        public void Evaluate(int SpreadMax)
        {
            var output = new List<Message>();

            var data = (from pin in FInputs
                        select !pin.IOObject.IsAnyInvalid()
                        ).Any(d => d);
            
            if (!data) {
                FOutput.FlushNil();
                return;
            }
            
            var changed = (
                            from pin in FInputs
                            let spread = pin.IOObject
                            where spread.IsChanged
                            where !(FPinEmpty[spread] && spread.IsAnyInvalid()) // if IS emtpy and WAS empty, don't bother
                            select true
                          ).Any();
            if (!changed) return;

            var doDistinct = FDistinct[0];

            for (int i = 0; i < FInputCount[0]; i++)
            {
                var inputSpread = FInputs[i].IOObject;
                FPinEmpty[inputSpread] = inputSpread.IsAnyInvalid();

                if (!inputSpread.IsAnyInvalid())
                {
                    var filtered = from m in inputSpread
                                   where m != null
                                   where !m.IsEmpty
                                   where !doDistinct || !output.Contains(m)
                                   select m;

                    output.AddRange(filtered);
                }
            }

            FOutput.FlushResult(output);
        }
    }
}