using System;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Packs.Messaging.Nodes
{
    public class PicturePanel : Panel
    {
        public PicturePanel()
        {
            this.Dock = DockStyle.Fill;
            try
            {
                this.BackgroundImage = Image.FromFile("packs/vvvv-Message/nodes/assets/icon/icon.png");
                this.BackgroundImageLayout = ImageLayout.Zoom;
            }
            catch (Exception) {}
        }
    }
}
