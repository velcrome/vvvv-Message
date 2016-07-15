using System.Linq;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;


namespace VVVV.Packs.Messaging.Nodes
{

    // should be cleaned up at one point. danger: counters the basic idea of a solid path for messages and introduces uncertainty in target patches when used in conjunction with split

    #region PluginInfo
    [PluginInfo(Name = "AvoidNil", Category = "Message", Version = "Formular", 
        Ignore = true, 
        Help = "Avoid empty spreads of Messages. Usually just above a split node.", 
        Tags = "Split", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageAvoidNilNode : AbstractFormularableNode
    {
#pragma warning disable 649, 169
        [Input("Input", Order = 0)]
        ISpread<Message> FInput;

        [Input("Topic", Order = 1, DefaultString="Event")]
        ISpread<string> FTopic;

        [Output("Output", AutoFlush = false)]
        ISpread<Message> FOutput;

        protected Message Default;
#pragma warning restore

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            FormularUpdate += formular => Default = null; // reset everytime the formular acually changes
        }


        public override void Evaluate(int SpreadMax)
        {
            if (Default != null && !FInput.IsChanged) return;

            if (FInput.IsAnyInvalid())
            {
                if (Default == null) NewDefault();

                FOutput.SliceCount = 1;

                Default.Topic = FTopic[0];
                FOutput[0] = Default;
                FOutput.Flush();
            }
            else FOutput.FlushResult(FInput);
        }

        private void NewDefault()
        {
            Default = new Message(Formular);
            Default.TimeStamp = Time.Time.CurrentTime();
            
        }


    }
}