using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;

namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Topic", AutoEvaluate = true, Category = "Message", Help = "Change the topic of Messages",
        Tags = "velcrome")]
    #endregion PluginInfo
    public class MessageTopicNode : IPluginEvaluate
    {
#pragma warning disable 649, 169
        [Input("Input")]
        IDiffSpread<Message> FInput;

        [Input("Topic")]
        IDiffSpread<string> FTopic;

        [Input("Update", IsToggle = true, Order = int.MaxValue, DefaultBoolean = true)]
        IDiffSpread<bool> FUpdate;


        [Output("Output", AutoFlush = false)]
        private ISpread<Message> FOutput;

        //[Import()]
        //protected ILogger FLogger;
#pragma warning restore

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && 
                !FTopic.IsChanged &&
                !(FUpdate.IsChanged && FUpdate.Any(x => x))
            ) return;

            FOutput.SliceCount = 0;
            FOutput.AssignFrom(FInput);
            FOutput.Flush();

            //if (!FUpdate.Any()) return;

            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;

            for (int i = 0; i < SpreadMax; i++)
            {
                // check if relevant change occurred
                if (FUpdate[i] && !FInput[i].Topic.Equals(FTopic[i]))
                    FInput[i].Topic = FTopic[i];
            }

        }
    }
}