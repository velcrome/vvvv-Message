using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

            AllowDrop = true;
            DragEnter += new DragEventHandler(InsideDragEnter);
            DragDrop += new DragEventHandler(InsideDragDrop);

        }

        #region Change Management
        protected bool FIsChanged;
        public bool IsChanged
        {
            get {
                var tmp = FIsChanged;
                FIsChanged = false;
                return tmp;
            }
            private set {
                FIsChanged = value;                
            }
        }

        public event EventHandler OnChange;
        
        #endregion Change Management

        #region dynamic control layout
        public bool LayoutByFormular(MessageFormular formular, bool forceChecked = false) {
            this.SuspendLayout();

            var prev =    Controls.OfType<RowPanel>().ToList();

            var remove =  (
                                from field in prev
                                where (field.Descriptor == null) || !(formular.FieldDescriptors.Contains(field.Descriptor))
                                select field
                          ).ToList();

            var current = (
                                from row in prev
                                select row.Descriptor
                          );

            var fresh =   (
                                from field in formular.FieldDescriptors
                                where !current.Contains(field)
                                select field
                          ).ToList();

            var maxCount = remove.Count();
            var counter = maxCount;

            foreach (var newEntry in fresh) 
            {
                // if lingering panels available, recycle it or else instanciate new one
                if (counter > 0)
                {
                    var overwrite = remove[maxCount - counter];
                    overwrite.Descriptor = newEntry;
                    overwrite.Checked = overwrite.Checked || forceChecked; // stay true, when likely to be a rename
                    overwrite.Visible = true;
                    counter--;
                }
                else
                {
                    var row = new RowPanel(newEntry, forceChecked);
                    Controls.Add(row);
                    row.OnChange += (sender, args) =>
                    {
                        IsChanged = true;
                        OnChange(this, args);
                    };
                }
            }
            
            // cleanup: just keep them lingering around, recycle when needed. should speed up gui
            while (counter > 0)
            {
                var row = remove[maxCount - counter];
                if (row != null)
                {

                    row.Descriptor = null;
                    row.Description = "empty";
                    row.Visible = false;
                    row.Checked = false;
                }
                counter--;
            }
            this.ResumeLayout();
            return true; // return 
        }
        #endregion dynamic control layout

        #region drag and drop
        void InsideDragDrop(object sender, DragEventArgs e)
        {
            var row = (RowPanel)e.Data.GetData(typeof(RowPanel));
            FlowLayoutPanel _destination = (FlowLayoutPanel)sender;
            FlowLayoutPanel _source = (FlowLayoutPanel)row.Parent;

            // this commented code can be used to drag'n'drop across windows.
            if (_source != _destination)
            {
                //// Move control to panel, updates Parent
                //_destination.Controls.Add(data);
                //data.Size = new Size(_destination.Width, 50);

                //// Reorder
                //Point p = _destination.PointToClient(new Point(e.X, e.Y));
                //var item = _destination.GetChildAtPoint(p);
                //int index = _destination.Controls.GetChildIndex(item, false);
                //_destination.Controls.SetChildIndex(data, index);

                //// Invalidate to paint!
                //_destination.Invalidate();
                //_source.Invalidate();
            }
            else
            {
                Point p = _destination.PointToClient(new Point(e.X, e.Y));
                var item = _destination.GetChildAtPoint(p);
                int index = _destination.Controls.GetChildIndex(item, false);
                _destination.Controls.SetChildIndex(row, index);  // move it
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
