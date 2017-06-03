#region usings
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using SharpMessage.WPF;
using System.Windows.Forms.Integration;

#endregion usings

namespace VVVV.Packs.Messaging.WPF 
{
    #region PluginInfo
    [PluginInfo(Name = "WPF",
                Category = "Message",
                Tags = "Prototype",
                Author = "velcrome",
                AutoEvaluate = true)]
    #endregion PluginInfo
    public class MessageWpfNode : System.Windows.Forms.UserControl, IPluginEvaluate
    {
        #region fields & pins

        [Input("Input")]
        public IDiffSpread<Message> FInput;

        [Output("Output", AutoFlush = false)]
        public ISpread<Message> FOutput;

        [Import()]
        public ILogger FLogger;

        private MessageControl MessageControl;


        #endregion fields & pins

        #region constructor and init

        public MessageWpfNode()
        {
            //setup the gui
            InitializeComponent();
        }

        void InitializeComponent()
        {
            //clear controls in case init is called multiple times
            Controls.Clear();

            var container = new ElementHost { Dock = System.Windows.Forms.DockStyle.Fill };

            container.Child = MessageControl = new MessageControl();
            Controls.Add(container);
        }
        #endregion constructor and init


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged)
            {
                if (FInput[0] != MessageControl.DataContext)
                    MessageControl.DataContext = FInput[0];
                FOutput.AssignFrom(FInput);
                FOutput.Flush();
            }


        }
    }
}