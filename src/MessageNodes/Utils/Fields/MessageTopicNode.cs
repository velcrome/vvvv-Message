using System.Collections.Generic;
using System.Linq;
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
        [Input("Input")]
        protected IDiffSpread<Message> FInput;

        [Input("Topic", DefaultString = "Event")]
        protected IDiffSpread<string> FTopic;

        [Input("Update", IsToggle = true, Order = int.MaxValue, DefaultBoolean = true)]
        protected IDiffSpread<bool> FUpdate;

        [Output("Output", AutoFlush = false)]
        protected ISpread<Message> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && 
                !FTopic.IsChanged &&
                !(FUpdate.IsChanged && FUpdate.Any())
            ) return;

            SpreadMax = FInput.IsAnyInvalid() ? 0 : FInput.SliceCount;
            for (int i = 0; i < SpreadMax; i++)
            {
                // double check if topic change needs to occur, because setting it will mark the message as dirty
                if (FUpdate[i] && !FInput[i].Topic.Equals(FTopic[i]))
                    FInput[i].Topic = FTopic[i];
            }

            FOutput.FlushResult(FInput);

        }
    }
}