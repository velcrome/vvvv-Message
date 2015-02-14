using VVVV.PluginInterfaces.V2;
using VVVV.Utils;


namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "AvoidNil", Category = "Message", Version = "Formular", Help = "Avoid empty spreads of Messages. Usually just above a split node.", Tags = "Split", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageAvoidNilNode : AbstractFormularableNode
    {
#pragma warning disable 649, 169
        [Input("Input", Order = 0)]
        ISpread<Message> FInput;

        [Input("Topic", Order = 1, DefaultString="Dummy")]
        ISpread<string> FTopic;

        [Output("Output", AutoFlush = false)]
        ISpread<Message> FOutput;

        protected Message Default;
#pragma warning restore


 
        public override void Evaluate(int SpreadMax)
        {
            if (Default != null && !FInput.IsChanged) return;
            
            if (FInput.IsAnyInvalid()) 
            {
                if (Default == null) NewDefault();

                FOutput.SliceCount = 1;

                Default.Topic = FTopic[0];
                FOutput[0] = Default;
            }
            else
            {
                FOutput.SliceCount = 0;
                FOutput.AssignFrom(FInput);
            }
            FOutput.Flush();
        }

        private void NewDefault()
        {
            Default = new Message(new MessageFormular(FConfig[0]));
            Default.TimeStamp = Time.Time.CurrentTime();
            
        }

        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
            Default = null;
        }
    }
}