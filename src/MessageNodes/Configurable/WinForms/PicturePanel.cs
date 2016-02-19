using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Packs.Messaging.Nodes
{
    public class PicturePanel : Panel
    {
        public PicturePanel()
        {
//            this.BackColor = Color.Black;
            this.Dock = DockStyle.Fill;
            this.BackgroundImage = Image.FromFile("packs/vvvv-Message/nodes/assets/icon/icon.png");
            this.BackgroundImageLayout = ImageLayout.Zoom;
        }
    }
}
