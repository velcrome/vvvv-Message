using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Packs.Messaging.Nodes
{
    public class PinDefinitionPanel : FlowLayoutPanel
    {


        public PinDefinitionPanel() {
            //table panel fills the whole window
            FlowDirection = FlowDirection.LeftToRight;

            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            Dock = DockStyle.Fill;

            AutoScroll = true;
            BorderStyle = BorderStyle.Fixed3D;
            //CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            AllowDrop = true;
            DragEnter += new DragEventHandler(InsideDragEnter);
            DragDrop += new DragEventHandler(InsideDragDrop);

            //			MouseDown += tableLayoutPanel1_MouseDown;

            LayoutByCount();
        }

        #region dynamic control layout

        public void LayoutByCount()
        {
            this.SuspendLayout();
            Controls.Clear();
            for (var i = 0; i < 10; i++)
            {
                var row = new RowPanel("item " + i, false);
                var c = row.Controls;

                Controls.Add(row);
            }
            this.ResumeLayout(false);
        }
        #endregion dynamic control layout

        #region drag and drop
        void InsideDragDrop(object sender, DragEventArgs e)
        {
            var data = (RowPanel)e.Data.GetData(typeof(RowPanel));
            FlowLayoutPanel _destination = (FlowLayoutPanel)sender;
            FlowLayoutPanel _source = (FlowLayoutPanel)data.Parent;

            if (_source != _destination)
            {
                // Add control to panel
                _destination.Controls.Add(data);
                data.Size = new Size(_destination.Width, 50);

                // Reorder
                Point p = _destination.PointToClient(new Point(e.X, e.Y));
                var item = _destination.GetChildAtPoint(p);
                int index = _destination.Controls.GetChildIndex(item, false);
                _destination.Controls.SetChildIndex(data, index);

                // Invalidate to paint!
                _destination.Invalidate();
                _source.Invalidate();
            }
            else
            {
                // Just add the control to the new panel.
                // No need to remove from the other panel, this changes the Control.Parent property.
                Point p = _destination.PointToClient(new Point(e.X, e.Y));
                var item = _destination.GetChildAtPoint(p);
                int index = _destination.Controls.GetChildIndex(item, false);
                _destination.Controls.SetChildIndex(data, index);
                _destination.Invalidate();
            }
        }

        void InsideDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        #endregion drag and drop
  
    }
}
