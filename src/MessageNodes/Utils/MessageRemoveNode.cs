using System.Collections.Generic;
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
#pragma warning disable 649, 169
        [Input("Input")]
        private ISpread<ISpread<Message>> FInput;

        [Input("Remove")]
        private IDiffSpread<Message> FRemove;

        [Output("Output", AutoFlush = false)]
        private ISpread<ISpread<Message>> FOutput;

        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if ((!FInput.IsChanged && !FRemove.IsChanged)) return;

            if ((FRemove.IsAnyInvalid() || FInput.IsAnyInvalid())) {
                if (FOutput.SliceCount > 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.AssignFrom(FInput);
                    FOutput.Flush();
                    return;
                } else return;
            }

            SpreadMax = FInput.SliceCount;
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var remains = from msg in FInput[i] 
                              where !FRemove.Contains(msg)
                              select msg;
                
                FOutput[0].SliceCount = 0;
                FOutput[0].AssignFrom(remains);

            }
            FOutput.Flush();
        }
    }


    #region PluginInfo
    [PluginInfo(Name = "RemoveEmpty", Category = "Message", Help = "Remove empty messages", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageRemoveEmptyNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        private ISpread<Message> FInput;

        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;

        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if ((!FInput.IsChanged)) return;

            if (FInput.IsAnyInvalid() )
            {
                if (FOutput.SliceCount > 0)
                {
                    FOutput.SliceCount = 0;
                    FOutput.AssignFrom(FInput);
                    FOutput.Flush();
                    return;
                }
                else return;
            }

            SpreadMax = FInput.SliceCount;
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var remains = from msg in FInput
                              where msg != null
                              where !msg.IsEmpty
                              select msg;

                FOutput.SliceCount = 0;
                FOutput.AssignFrom(remains);

            }
            FOutput.Flush();
        }
    }
}