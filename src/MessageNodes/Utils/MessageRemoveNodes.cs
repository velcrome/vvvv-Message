using System.ComponentModel.Composition;
using System.Linq;

using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Remove", Category = "Message", Help = "Remove specific messages", AutoEvaluate = true, Author = "velcrome")]
    #endregion PluginInfo
    public class MessageRemoveNode : IPluginEvaluate
    {
        [Input("Input")]
        protected ISpread<Message> FInput;

        [Input("Remove")]
        protected IDiffSpread<Message> FRemove;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsAnyInvalid()) {
                if (FOutput.SliceCount > 0)
                {
                    FOutput.FlushNil();
                    return;
                } else return;
            }

            if (FRemove.IsAnyInvalid())
            {
                FOutput.FlushResult(FInput);
                return;
            }

            if ((!FInput.IsChanged && !FRemove.IsChanged)) return;

            var remains = from msg in FInput 
                          where !FRemove.Contains(msg)
                          select msg;

            FOutput.FlushResult(remains);
        }
    }


    #region PluginInfo
    [PluginInfo(Name = "RemoveEmpty", Category = "Message", Help = "Remove empty messages", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageRemoveEmptyNode : IPluginEvaluate
    {
        [Input("Input")]
        protected ISpread<Message> FInput;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;

        public void Evaluate(int SpreadMax)
        {
            if ((!FInput.IsChanged)) return;

            if (FInput.IsAnyInvalid() )
            {
                FOutput.FlushNil();
                return;
            }

            var remains = from msg in FInput
                          where msg != null
                          where !msg.IsEmpty
                          select msg;

            FOutput.FlushResult(remains);
        }
    }
}