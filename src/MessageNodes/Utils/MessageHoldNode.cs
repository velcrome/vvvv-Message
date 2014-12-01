using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    public enum HoldEnum
    {
            First,
            Last,
            All
    }


    #region PluginInfo
    [PluginInfo(Name = "Hold", Category = "Message.Keep", Help = "Allows Feedback Loops for Messages",
        Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageHoldNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        IDiffSpread<Message> FInput;

        [Input("Match Rule", DefaultEnumEntry = "All", IsSingle = true)] 
        IDiffSpread<HoldEnum> FHold;
        
        [Output("MessageKeep", AutoFlush = false)]
        ISpread<Message> FOutput;


        [Import()]
        protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged && !FInput.IsAnyInvalid()) {

                FOutput.SliceCount = 0;

                switch (FHold[0])
                {
                    case HoldEnum.All: FOutput.AssignFrom(FInput);
                        break;
                    case HoldEnum.First: FOutput.Add(FInput.First());
                        break;
                    case HoldEnum.Last: FOutput.Add(FInput.Last());
                        break;


                }

                FOutput.Flush();
            }
         }
    }
}