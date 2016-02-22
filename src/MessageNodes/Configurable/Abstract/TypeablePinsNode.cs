using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
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

        protected bool CopyFromPins(Message message, int index, bool checkPinForChange = false)
        {
            var hasCopied = false;

            foreach (string name in FPins.Keys)
            {
                // don't change if pin data still the same
                if (!checkPinForChange || FPins[name].ToISpread().IsChanged)
                {
                    var pinSpread = FPins[name].ToISpread();
                    if (!pinSpread.IsAnyInvalid())
                    {
                        var bin = pinSpread[index] as IEnumerable;

                        // don't change if pin data equals the message data
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
            Formular = new MessageFormular(configSpread[0]);

            // pin removals
            var danger = from pinName in FPins.Keys
                         where !Formular.FieldNames.Contains(pinName)
                         let pin = FPins[pinName].GetPluginIO()
                         where pin == null || pin.IsConnected // first frame pin will not be initialized
                         select pinName;

            // type changes
            danger.Concat(
                            from desc in Formular.FieldDescriptors
                            where FPins.Keys.Contains(desc.Name)
                            where FTypes[desc.Name] == desc.Type
                            let pin = FPins[desc.Name].GetPluginIO()
                            where pin == null || pin.IsConnected // first frame pin will not be initialized
                            select desc.Name
                         );

            if (danger.Count() > 0)
            {
            
            }

            List<string> invalidPins = FPins.Keys.ToList();
            foreach (string field in Formular.FieldNames)
            {
                bool create = false;

                if (FPins.ContainsKey(field) && FPins[field] != null)
                {
                    invalidPins.Remove(field);

                    if (FTypes.ContainsKey(field))
                    {
                        // same name, but types don't match
                        // todo: in fact eg float does match double here...
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
            
            // cleanup
            foreach (string name in invalidPins)
            {
                FPins[name].Dispose();
                FPins.Remove(name);
                FTypes.Remove(name);
            }

            //// reorder - does not work right now
            //var names = formular.FieldNames.ToArray();
            //for (int i = 0; i < formular.FieldNames.Count; i++)
            //{
            //    var name = names[i];
            //    var pin = FPins[name].GetPluginIO();
            //    pin.Order = i * 2 + 5;
            //}
        }

        #endregion pin management

        #region abstract methods
        protected abstract IOAttribute DefinePin(FormularFieldDescriptor config);
        #endregion abstract methods
    }

}
