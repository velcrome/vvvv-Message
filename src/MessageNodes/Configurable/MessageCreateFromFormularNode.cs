using VVVV.PluginInterfaces.V2;
using VVVV.Utils;


namespace VVVV.Packs.Messaging.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Create", Category = "Message", Version = "Default", Help = "Create a blank Message, pureley based on a Formular.", Tags = "Default", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageCreateFromFormularNode : AbstractFormularableNode
    {
        #pragma warning disable 649, 169
        [Input("New", Order = 0, IsToggle=true)]
        ISpread<bool> FNew;

        [Input("Topic", Order = 1, DefaultString = "Event")]
        ISpread<string> FTopic;

        [Output("Output", AutoFlush = false)]
        ISpread<Message> FOutput;

        protected MessageFormular Formular;
        #pragma warning restore
        
        public override void Evaluate(int SpreadMax)
        {
            if (!FNew.IsAnyInvalid() && FNew[0])
            {
                FOutput.SliceCount = 1;

                var message = new Message(Formular);
                message.TimeStamp = Time.Time.CurrentTime();
                message.Topic = FTopic[0];
                
                FOutput[0] = message;
                FOutput.Flush();
            }
            else if (FOutput.SliceCount > 0)
            {
                FOutput.SliceCount = 0;
                FOutput.Flush();
            }
        }

        protected override void HandleConfigChange(IDiffSpread<string> configSpread)
        {
            Formular = new MessageFormular(FConfig[0]);
        }


    }
}