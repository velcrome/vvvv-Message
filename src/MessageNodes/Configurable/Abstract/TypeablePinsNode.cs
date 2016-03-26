using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Core.Logging;
using System.Reflection;


namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class DynamicPinsNode : AbstractFormularableNode
    {
        #region fields & pins
        protected const string Tags = "Formular";


        [Import()]
        protected IIOFactory FIOFactory;

        protected Dictionary<string, IIOContainer> FPins = new Dictionary<string, IIOContainer>();
        protected MessageFormular Formular = new MessageFormular("", MessageFormular.DYNAMIC);

        protected bool RemovePinsFirst;

        protected int DynPinCount = 5;
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

        protected IIOContainer CreatePin(FormularFieldDescriptor field)
        {
            IOAttribute attr = SetPinAttributes(field); // each implementation of DynamicPinsNode must create its own InputAttribute or OutputAttribute (

            Type pinType = typeof(ISpread<>).MakeGenericType((typeof(ISpread<>)).MakeGenericType(field.Type)); // the Pin is always a binsized one
            var pin = FPins[field.Name] = FIOFactory.CreateIOContainer(pinType, attr);

            DynPinCount += 2; // total pincount. always add two to account for data pin and binsize pin

            return pin;
        }

        protected bool RetryConfig()
        {
            if (RemovePinsFirst)
            {
                OnConfigChange(FConfig);
            }

            if (RemovePinsFirst)
                throw new PinConnectionException("Manually remove unneeded links first!");
            else return true;
        }

        protected bool HasLink(IIOContainer pinContainer)
        {
            try
            {
                var binContainer = pinContainer.RawIOObject.GetType().GetField("FStream", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.FlattenHierarchy).GetValue(pinContainer.RawIOObject);
                var container = binContainer.GetType().GetField("FDataContainer", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(binContainer) as IIOContainer;
                return container.GetPluginIO().IsConnected;
            }
            catch (Exception)
            {
                string nodePath = PluginHost.GetNodePath(false);
                FLogger.Log(LogType.Error, "Failed to protect a " + this.GetType().Name + " node: " + nodePath);
                return false;
            }
        }

        protected bool HasEndangeredLinks(MessageFormular newFormular)
        {
            // pin removals
            var danger = from field in Formular.FieldDescriptors
                         let fieldName = field.Name
                         where !newFormular.FieldNames.Contains(fieldName)
                         where HasLink(FPins[fieldName]) // first frame pin will not be initialized
                         select fieldName;

            // type changes - removal and recreate new
            var typeDanger= from field in Formular.FieldDescriptors
                            where newFormular.FieldNames.Contains(field.Name)
                            where newFormular[field.Name].Type != field.Type
                            where HasLink(FPins[field.Name]) // first frame pin will not be initialized
                            select field.Name;

            // ignore changes to binsize.

            return danger.Count() > 0 || typeDanger.Count() > 0;
        }

        protected override void OnConfigChange(IDiffSpread<string> configSpread)
        {
            var formularName = FFormular.IsAnyInvalid() ? MessageFormular.DYNAMIC : FFormular[0];
            var newFormular = new MessageFormular(configSpread[0], formularName);

            if (HasEndangeredLinks(newFormular))
            {
                RemovePinsFirst = true;
                return;
            }
            else RemovePinsFirst = false;

            List<string> invalidPins = FPins.Keys.ToList();
            foreach (string field in newFormular.FieldNames)
            {
                if (FPins.ContainsKey(field) && FPins[field] != null)
                {
                    invalidPins.Remove(field);

                    if (Formular.FieldNames.Contains(field))
                    {
                        // same name, but types don't match
                        // todo: in fact eg float does match double here...
                        if (Formular[field].Type != newFormular[field].Type)
                        {
                            FPins[field].Dispose();
                            FPins[field] = CreatePin(newFormular[field]);
                        }
                    }
                    else
                    {
                        // key is in FPins, but no type defined. should never happen
                        FLogger.Log(LogType.Debug, "Internal Fault in Pin Layout detected. Override with "+newFormular.ToString());
                        FPins[field] = CreatePin(newFormular[field]);
                    }
                }
                else
                {
                    FPins[field] = CreatePin(newFormular[field]);
                }
             }
            
            // cleanup
            Formular = newFormular;

            foreach (string name in invalidPins)
            {
                FPins[name].Dispose();
                FPins.Remove(name);
            }

            //// reorder - does not work right now, sdk offers only read-only access
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
        protected abstract IOAttribute SetPinAttributes(FormularFieldDescriptor config);
        #endregion abstract methods
    }

}
