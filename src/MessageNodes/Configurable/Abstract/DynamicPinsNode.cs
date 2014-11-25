using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Packs.Message.Core.Formular;
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
            var formular = new MessageFormular(configSpread[0]);

            foreach (string field in formular.Fields)
            {
                bool create = false;

                if (FPins.ContainsKey(field) && FPins[field] != null)
                {
                    invalidPins.Remove(field);

                    if (FTypes.ContainsKey(field))
                    {
                        if (FTypes[field] != formular.GetType(field))
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
                    Type type = formular.GetType(field);
                    IOAttribute attr = DefinePin(field, type, formular.GetCount(field)); // each implementation of DynamicPinsNode must create its own InputAttribute or OutputAttribute (
                    Type pinType = typeof(ISpread<>).MakeGenericType((typeof(ISpread<>)).MakeGenericType(type)); // the Pin is always a binsized one
                    FPins[field] = FIOFactory.CreateIOContainer(pinType, attr);

                    FTypes.Add(field, type);
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

        #region abstract methods
        protected abstract IOAttribute DefinePin(string name, Type type, int binSize = -1);
        #endregion abstract methods
    }

}
