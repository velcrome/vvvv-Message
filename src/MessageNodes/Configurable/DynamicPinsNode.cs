using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Message.Core.Registry;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Message.Nodes
{
    public abstract class DynamicPinsNode : TypeableNode
    {
        #region fields & pins

        [Input("Verbose", Visibility = PinVisibility.OnlyInspector, IsSingle = true, DefaultBoolean = false)]
        public ISpread<bool> FVerbose;

        [Import()]
        protected IIOFactory FIOFactory;

        protected Dictionary<string, IIOContainer> FPins = new Dictionary<string, IIOContainer>();
        protected Dictionary<string, Type> FTypes = new Dictionary<string, Type>();

        protected int FCount = 2;

        #endregion fields & pins

        #region pin management

        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
            FCount = 0;
            List<string> invalidPins = FPins.Keys.ToList();
            var pins = TypeRegistry.Instance.Definition(configSpread[0]);

            foreach (string name in pins.Keys)
            {
                bool create = false;

                if (FPins.ContainsKey(name) && FPins[name] != null)
                {
                    invalidPins.Remove(name);

                    if (FTypes.ContainsKey(name))
                    {
                        if (FTypes[name] != pins[name].Item1)
                        {
                            FPins[name].Dispose();
                            FPins[name] = null;
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
                    FPins.Add(name, null);
                    create = true;
                }

                if (create)
                {
                    Type type = pins[name].Item1;
                    IOAttribute attr = DefinePin(name, type, pins[name].Item2); // each implementation of DynamicPinsNode must create its own InputAttribute or OutputAttribute (
                    Type pinType = typeof(ISpread<>).MakeGenericType((typeof(ISpread<>)).MakeGenericType(type)); // the Pin is always a binsized one
                    FPins[name] = FIOFactory.CreateIOContainer(pinType, attr);

                    FTypes.Add(name, type);
                }
                FCount += 2; // total pincount. always add two to account for data pin and binsize pin
            }
            foreach (string name in invalidPins)
            {
                FPins[name].Dispose();
                FPins.Remove(name);
                FTypes.Remove(name);
            }
        }

        #endregion pin management

        #region tools

        protected VVVV.PluginInterfaces.V2.NonGeneric.ISpread ToISpread(IIOContainer pin)
        {
            return (VVVV.PluginInterfaces.V2.NonGeneric.ISpread)(pin.RawIOObject);
        }

        protected VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread ToIDiffSpread(IIOContainer pin)
        {
            return (VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread)(pin.RawIOObject);
        }
        protected VVVV.PluginInterfaces.V2.ISpread<T> ToGenericISpread<T>(IIOContainer pin)
        {
            return (VVVV.PluginInterfaces.V2.ISpread<T>)(pin.RawIOObject);
        }

        #endregion tools

        #region abstract methods
        protected abstract IOAttribute DefinePin(string name, Type type, int binSize = -1);
        #endregion abstract methods
    }

}
