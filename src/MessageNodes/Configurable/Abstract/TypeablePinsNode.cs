using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class DynamicPinsNode : AbstractFormularableNode
    {
        #region fields & pins
        protected const string Tags = "Formular";

        [Input("Verbose", Visibility = PinVisibility.OnlyInspector, IsSingle = true, DefaultBoolean = false, Order = 2)]
        public ISpread<bool> FDevMode;

        [Import()]
        protected IIOFactory FIOFactory;

        protected Dictionary<string, IIOContainer> FPins = new Dictionary<string, IIOContainer>();
        protected Dictionary<string, Type> FTypes = new Dictionary<string, Type>();

        protected int DynPinCount = 5;
        protected MessageFormular Formular = new MessageFormular("");


        #endregion fields & pins

        #region pin management

        protected bool SyncPins()
        {
            bool changed = false;
            foreach (string name in FPins.Keys)
            {
                var pin = FPins[name].ToISpread();
                pin.Sync();
                if (pin.IsChanged) changed = true;
            }
            return changed;
        }

        protected bool CopyFromPins(Message message, int index, bool checkForChange = false)
        {
            var hasCopied = false;

            foreach (string name in FPins.Keys)
            {
                if (!checkForChange || FPins[name].ToISpread().IsChanged)
                {
                    var pinSpread = FPins[name].ToISpread();
                    if (!pinSpread.IsAnyInvalid())
                    {
                        // don't changedBinSize if data still the same
                        var bin = pinSpread[index] as IEnumerable;
                        if (!message.Fields.Contains(name) || !message[name].Equals(bin))
                        {
                            message.AssignFrom(name, bin);
                            hasCopied = true;
                        }  
                    }
                }
            }
            return hasCopied;

        }

        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
            DynPinCount = 5;
            List<string> invalidPins = FPins.Keys.ToList();
            Formular = new MessageFormular(configSpread[0]);

            foreach (string field in Formular.Fields)
            {
                bool create = false;

                if (FPins.ContainsKey(field) && FPins[field] != null)
                {
                    invalidPins.Remove(field);

                    if (FTypes.ContainsKey(field))
                    {
                        if (FTypes[field] != Formular[field].Type)
                        {
                            FPins[field].Dispose();
                            FPins[field] = null;
                            create = true;
                        }

                    }
                    else
                    {
                        // key is in FPins, but no type defined. should never happen
                        create = true;
                    }
                }
                else
                {
                    FPins.Add(field, null);
                    create = true;
                }

                if (create)
                {
                    IOAttribute attr = DefinePin(Formular[field]); // each implementation of DynamicPinsNode must create its own InputAttribute or OutputAttribute (

                    Type type = Formular[field].Type;
                    Type pinType = typeof(ISpread<>).MakeGenericType((typeof(ISpread<>)).MakeGenericType(type)); // the Pin is always a binsized one
                    FPins[field] = FIOFactory.CreateIOContainer(pinType, attr);

                    FTypes.Add(field, type);
                }
                DynPinCount += 2; // total pincount. always add two to account for data pin and binsize pin
            }
            foreach (string name in invalidPins)
            {
                FPins[name].Dispose();
                FPins.Remove(name);
                FTypes.Remove(name);
            }
        }

        #endregion pin management

        #region abstract methods
        protected abstract IOAttribute DefinePin(FormularFieldDescriptor config);
        #endregion abstract methods
    }

}
