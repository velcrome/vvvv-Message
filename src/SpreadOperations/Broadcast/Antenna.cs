#region usings
using System.ComponentModel.Composition;
using VVVV.Nodes.Generic.Broadcast;
using VVVV.Pack.Message;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

#endregion usings

namespace VVVV.Nodes.Generic
{

    public class AntennaNode<T> : BroadCastNode<T>
    {
        #region fields & pins
#pragma warning disable 649, 169
        [Input("Input", AutoValidate = false)]
        protected Pin<T> FInput;

        [Input("Send", IsBang = true, DefaultBoolean = true, IsSingle = true)]
        protected ISpread<bool> FSend;
        
#pragma warning restore
        #endregion fields & pins

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            FInput.Disconnected += Disconnect;
            FInput.SliceCount = 0;
        }

        private void Disconnect(object sender, PinConnectionEventArgs args)
        {
            FInput.SliceCount =0;
        }

        public override void Evaluate(int SpreadMax)
        {
            if (FSend[0] == false) return;

            FInput.Sync();

            if (!FInput.IsAnyInvalid()) Send(FInput);
        }

    }


}