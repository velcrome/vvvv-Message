using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using System.Windows.Forms;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Core.Logging;

using VVVV.DX11;
using FeralTic.DX11;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class TypeablePinsNode : AbstractFormularableNode, IWin32Window, ICustomQueryInterface, IDX11ResourceDataRetriever
    {
        #region fields & pins
        protected const string Tags = "Formular";

        protected Dictionary<string, IIOContainer> FPins = new Dictionary<string, IIOContainer>();
        protected int DynPinCount = 5;

        protected bool RemovePinsFirst;

        protected FormularLayoutPanel LayoutPanel = new FormularLayoutPanel();


        [Import()]
        protected IPluginHost FHost;
        #endregion fields & pins

        #region Initialisation

        [Import()]
        protected IIOFactory FIOFactory;

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            this.CustomizeFormular = true;

            var init = Formular.Clone() as MessageFormular;
            init.Require(RequireEnum.None);

            LayoutPanel.Formular = init; 
            LayoutPanel.CanEditFields = init.IsDynamic;

            FormularUpdate += (sender, formular) => UpdateWindow(formular, formular.IsDynamic);// sender is commonly MessageFormularRegistry
            FormularUpdate += (sender, formular) => TryDefinePins(sender, formular);

            LayoutPanel.Changed += ReadLayoutPanel;

        }

        private void ReadLayoutPanel(object sender, MessageFormular formular)
        {
            Formular = formular;
        }



        #endregion Initialisation

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

            foreach (string fieldName in FPins.Keys)
            {
                // don't change if pin data still the same
                if (!checkPinForChange || FPins[fieldName].ToISpread().IsChanged)
                {
                    var pinSpread = FPins[fieldName].ToISpread();

                    var invalidBin = pinSpread == null || pinSpread.SliceCount == 0 || pinSpread[0] == null;

                    if (invalidBin)
                    {
                        if (message.Fields.Contains(fieldName)) 
                        {
                            message.Remove(fieldName); // to be or not to be? 
                            hasCopied = true;
                        }
                    }
                    else
                    {
                        var bin = pinSpread[index] as IEnumerable;
                        var type = Formular[fieldName].Type;

                        // don't change if pin data equals the message data, because doing so will flag the message dirty.
                        if (!message.Fields.Contains(fieldName) || !message[fieldName].Equals(bin))
                        {
                            message.AssignFrom(fieldName, bin, type);
                            hasCopied = true;
                        }
                    }
                }
            }
            return hasCopied;

        }

        /// <summary>
        /// Creates a binsized pin according to the information in the desciptor. Ignores IsRequired. 
        /// </summary>
        /// <param name="field"></param>
        /// <exception cref="InvalidComObjectException">PluginInterface: Internal COM disconnect</exception>
        /// <returns>The container around the pin.</returns>
        protected virtual IIOContainer CreatePin(FormularFieldDescriptor field)
        {
            IOAttribute attr = SetPinAttributes(field); // each implementation of DynamicPinsNode must create its own InputAttribute or OutputAttribute (

            Type pinType = typeof(ISpread<>).MakeGenericType((typeof(ISpread<>)).MakeGenericType(field.Type)); // the Pin is always a binsized one

            IIOContainer pin = null;
            pin = FPins[field.Name] = FIOFactory.CreateIOContainer(pinType, attr);

            DynPinCount += 2; // total pincount. always add two to account for data pin and binsize pin

            return pin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>successful pin manipulation</returns>
        protected bool RetryConfig()
        {
            if (RemovePinsFirst)
            {
                TryDefinePins(this, Formular);
            }
            return !RemovePinsFirst;
        }

        protected bool HasLink(IIOContainer pinContainer)
        {
            try
            {
                var connected = pinContainer.GetPluginIO().IsConnected;

                foreach (var associated in pinContainer.AssociatedContainers)
                {
                    connected |= associated.GetPluginIO().IsConnected;
                }
                return connected;
            }
            catch (InvalidComObjectException)
            {
                // [Import]s not yet ready. try another time.
                // its safe to assume that no pins have been created yet.
                FLogger.Log(LogType.Warning, "Not fully initialized [" + this.GetType().Name + "].");
                return false; 
            }
            catch (Exception)
            {
                string nodePath = PluginHost.GetNodePath(false);
                FLogger.Log(LogType.Error, "Failed to protect a " + this.GetType().Name + " node: " + nodePath);
                return false;

            }
        }
        /// <summary>
        /// Detects link breaking action, that would occur when disposing a now unneeded pin, or resetting its type.
        /// </summary>
        /// <param name="newFormular">Includes the changes to be done.</param>
        /// <returns>True, if no linkbreaking conflict of type or linkbreaking pin elimination</returns>
        /// <remarks>Compares the existing dynamic pins with the required fields in the given Formular</remarks>
        protected bool HasEndangeredLinks(MessageFormular newFormular)
        {
            // pin removals
            var danger = from pinName in FPins.Keys
                         where HasLink(FPins[pinName])
                         where !(    newFormular.FieldNames.Contains(pinName) 
                                  && newFormular[pinName].IsRequired)
                         select pinName;

            // type changes - removal and recreate new
            var typeDanger = from pinName in FPins.Keys
                             where HasLink(FPins[pinName]) // first frame pin will not be initialized
                             where newFormular.FieldNames.Contains(pinName)
                             where newFormular[pinName].IsRequired
                             let innerType = FPins[pinName].GetTypeRecord().Type // ISpread<ISpread< ??? >>
                             where newFormular[pinName].Type != innerType // rather ask for conversion? see actual pin creation for more comments
                             select pinName;

            // ignore changes to binsize for now

            return danger.Count() > 0 || typeDanger.Count() > 0;
        }

        /// <summary>
        /// Will try to apply the formular to the current pin layout
        /// </summary>
        /// <param name="newFormular"></param>
        /// <remarks>
        /// If any pin in the new formular is already present, it will preserve the pin.
        /// If any current pin is not contained in the new Formular, but is linked in the patch, it will force the node to raise exceptions every frame, until the link is manually deleted.
        /// </remarks>
        protected void TryDefinePins(object sender, MessageFormular newFormular)
        {
            // reject formular, deferring it to application later on.
            if (HasEndangeredLinks(newFormular))
            {
                RemovePinsFirst = true;
                return;
            }
            else RemovePinsFirst = false;

            List<string> invalidPins = FPins.Keys.ToList();
            foreach (FormularFieldDescriptor field in newFormular.FieldDescriptors.Where(field => field.IsRequired))
            {
                if (FPins.ContainsKey(field.Name) && FPins[field.Name] != null)
                {
                    invalidPins.Remove(field.Name);

                    // same name, but types don't match
                    // todo: in fact eg float does match double from vvvv side, conversion useful for patching speed?
                    if (FPins[field.Name].GetTypeRecord().Type != field.Type)
                    {
                        FPins[field.Name].Dispose();
                        var pin = CreatePin(field);
                        if (pin != null) FPins[field.Name] = pin;
                    }
                }
                else
                {
                    var pin = CreatePin(field);
                    if (pin != null) FPins[field.Name] = pin;
                }
            }

            // cleanup
            foreach (string name in invalidPins)
            {
                FPins[name].Dispose();
                FPins.Remove(name);
            }

            ReOrder();
        }

        protected void ReOrder()
        {
            var names = from name in Formular.FieldNames
                        where Formular[name].IsRequired
                        select name;

            int counter = 0;
            foreach (var name in names)
            {
                if (FPins.ContainsKey(name) && FPins[name] != null)
                {
                    FPins[name].GetPluginIO().Order = counter * 2 + 5;
                    FPins[name].AssociatedContainers.First().GetPluginIO().Order = counter * 2 + 6;
                    counter++;
                }
            }
        }

        #endregion pin management

        #region dx11
        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;

        protected void InitDX11Graph()
        {
            if (this.RenderRequest != null) { RenderRequest(this, this.FHost); }
        }
        #endregion dx11

        #region window management

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newFormular"></param>
        /// <param name="append"></param>
        /// 
        private void UpdateWindow(MessageFormular newFormular, bool append = false)
        {
            if (newFormular == null) return;

            var old = LayoutPanel.Formular; // retrieve copy
            newFormular = newFormular.Clone() as MessageFormular; // act on copy

            if (append)
            {
                // overwrite all existing fields, and hand back the result
                foreach (var field in newFormular.FieldDescriptors)
                {
                    old[field.Name] = field; // hard overwrite 
                }
                old.Require(RequireEnum.NoneButBoth, newFormular);
                old.Name = newFormular.Name;
                newFormular = old; 
            }
            else
            {
                newFormular.Require(RequireEnum.NoneButBoth, old);
            }

            LayoutPanel.Changed -= ReadLayoutPanel;
            LayoutPanel.Formular = newFormular;
            LayoutPanel.Changed += ReadLayoutPanel;

            LayoutPanel.CanEditFields = newFormular.IsDynamic;
        }
        #endregion

        #region Window utils

        public IntPtr Handle
        {
            get { return LayoutPanel.Handle; }
        }

        public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
        {
            if (iid.Equals(Guid.Parse("00000112-0000-0000-c000-000000000046")))
            {
                ppv = Marshal.GetComInterfaceForObject(LayoutPanel, typeof(IOleObject));
                return CustomQueryInterfaceResult.Handled;
            }
            else if (iid.Equals(Guid.Parse("458AB8A2-A1EA-4d7b-8EBE-DEE5D3D9442C")))
            {
                ppv = Marshal.GetComInterfaceForObject(LayoutPanel, typeof(IWin32Window));
                return CustomQueryInterfaceResult.Handled;
            }
            else
            {
                FLogger.Log(LogType.Debug, "missing: " + iid.ToString());
                ppv = IntPtr.Zero;
                return CustomQueryInterfaceResult.NotHandled;
            }
        }
        #endregion window utils

        #region abstract methods
        protected abstract IOAttribute SetPinAttributes(FormularFieldDescriptor config);
        #endregion abstract methods
    }

}
