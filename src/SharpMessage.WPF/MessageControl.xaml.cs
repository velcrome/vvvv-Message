using System.Windows.Controls;
using VVVV.Packs.Messaging;


namespace SharpMessage.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        public MessageControl()
        {
            InitializeComponent();

            var m = new Message("Test");
//            m.Init("foo", 1, 2, 3);
//            m.Init("bar", "eins");
//            m.Init("xxx", true);

//            DataContext = m;
        }

    }
}