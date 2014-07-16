#region usings
using System.Linq;
using System.ComponentModel.Composition;
using VVVV.Nodes.Generic.Broadcast;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;
using System.Runtime.Serialization;

#endregion usings

namespace VVVV.Nodes.Generic
{


    public class RadioNode<T> : BroadCastNode<T>
    {
        #region fields & pins
#pragma warning disable 649, 169
        [Output("Output", AutoFlush = false)]
        protected Pin<T> FOutput;

        [Output("OnReceive")]
        protected ISpread<bool> FReceive;


#pragma warning restore
        #endregion fields & pins

        public override void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = 0;
            FOutput.AssignFrom(Receive);
            FOutput.Flush();

            FReceive.SliceCount = 1;
//            FReceive[0] = Receive.Any();
            FReceive[0] = FOutput.SliceCount > 0;

        }

    }


}