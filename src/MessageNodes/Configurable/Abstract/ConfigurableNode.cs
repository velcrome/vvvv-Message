using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Packs.Messaging.Nodes
{
    public abstract class ConfigurableNode :  UserControl, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Config("Configuration", DefaultString = "string Foo")]
        public IDiffSpread<string> FConfig;

        [Import()]
        protected ILogger FLogger;

        FlowLayoutPanel FWindow = new FlowLayoutPanel();

        protected abstract void HandleConfigChange(IDiffSpread<string> configSpread);
        public abstract void Evaluate(int SpreadMax);

        public ConfigurableNode()
        {
            InitializeWindow();
            LayoutByCount();
        }


        public virtual void OnImportsSatisfied()
        {
            FConfig.Changed += HandleConfigChange;
        }

        public void InitializeWindow() {
            //table panel fills the whole window
            FWindow.FlowDirection = FlowDirection.LeftToRight;

            FWindow.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            FWindow.Dock = DockStyle.Fill;

            FWindow.AutoScroll = true;
            FWindow.BorderStyle = BorderStyle.Fixed3D;
            //FWindow.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            FWindow.AllowDrop = true;
            FWindow.DragEnter += new DragEventHandler(InsideDragEnter);
            FWindow.DragDrop += new DragEventHandler(InsideDragDrop);

            //			FWindow.MouseDown += tableLayoutPanel1_MouseDown;

            //add the main table panel to the window
            Controls.Add(FWindow);
        }


        #region dynamic control layout

        void LayoutByCount()
        {
            var window = FWindow.Controls;

            FWindow.Parent.SuspendLayout();

            window.Clear();


            for (var i = 0; i < 10; i++)
            {
                var row = new RowPanel("item " + i, false);
                var c = row.Controls;

                window.Add(row);
            }

            FWindow.Parent.ResumeLayout(false);
            //			FMainPanel.PerformLayout();
            //FMainPanel.Refresh();

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
