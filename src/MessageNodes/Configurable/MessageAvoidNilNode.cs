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
            Default = new Message(new MessageFormular(FFormular[0].Name, FConfig[0]));
            Default.TimeStamp = Time.Time.CurrentTime();
            
        }

        protected override void OnConfigChange(IDiffSpread<string> configSpread)
        {
            Default = null;
        }

        protected override void OnSelectFormular(IDiffSpread<EnumEntry> spread)
        {
            base.OnSelectFormular(spread);

            var window = (FWindow as FormularLayoutPanel);
            var fields = window.Controls.OfType<FieldPanel>();

            foreach (var field in fields) field.Checked = true;

            window.Locked = FFormular[0] != MessageFormular.DYNAMIC;
        }
    }
}