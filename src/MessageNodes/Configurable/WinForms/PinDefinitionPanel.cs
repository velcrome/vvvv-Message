using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VVVV.Packs.Messaging.Nodes
{
    public class PinDefinitionPanel : FlowLayoutPanel
    {
        
        public event EventHandler OnChange;

        public PinDefinitionPanel()
        {
            //table panel fills the whole window
            FlowDirection = FlowDirection.LeftToRight;

            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            Dock = DockStyle.Fill;

            AutoScroll = true;
            BorderStyle = BorderStyle.Fixed3D;

            AllowDrop = true;
            DragEnter += new DragEventHandler(InsideDragEnter);
            DragDrop += new DragEventHandler(InsideDragDrop);
            DoubleClick += InsideDoubleClick;

        }

        public IEnumerable<RowPanel> RowPanels
        {
            get
            {
                // add type checks when needed
                return Controls.OfType<RowPanel>();
            }
        }

        public IEnumerable<FormularFieldDescriptor> CheckedDescriptors
        {
            get 
            {
                var desc = from row in RowPanels
                           where row.Checked
                           select row.Descriptor;
                return desc;
            }
        }

        private bool _canEdit = false;
        public bool CanEditDescription
        {
            get
            {
                return _canEdit;
            }
            set
            {
                if (value != CanEditDescription)
                {
                    _canEdit = value;
                    foreach (var row in RowPanels) row.CanEdit = value;
                }
            }
        }

        #region dynamic control layout
        public bool LayoutByFormular(MessageFormular formular, bool forceChecked = false) {
            this.SuspendLayout();

            var prev =    RowPanels.ToList();

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
                    AddNewRow(newEntry, forceChecked);
                }
            }
            
            // cleanup: just keep them lingering around, recycle when needed. should speed up gui
            while (counter > 0)
            {
                var row = remove[maxCount - counter];
                if (row != null)
                {
                    row.Clear(); 
                }
                counter--;
            }
            this.ResumeLayout();
            return true; // return 
        }

        private void AddNewRow(FormularFieldDescriptor desc, bool isChecked)
        {
            var row = new RowPanel(desc, isChecked);
            row.CanEdit = CanEditDescription;
            Controls.Add(row);
            row.OnChange += (sender, args) =>
            {
                OnChange(this, args);
            };
            row.InitializeListeners();

        }
        #endregion dynamic control layout

        #region drag and drop
        void InsideDragDrop(object sender, DragEventArgs e)
        {
            var row = (RowPanel)e.Data.GetData(typeof(RowPanel));
            FlowLayoutPanel destination = (FlowLayoutPanel)sender;
            FlowLayoutPanel source = (FlowLayoutPanel)row.Parent;

            // this commented code can be used to drag'n'drop across windows.
            if (source != destination)
            {
                //// Move control to panel, updates Parent
                //destination.Controls.Add(row);
                //row.Size = new Size(destination.Width, 50);
                
                //destination.Controls.SetChildIndex(row, index);  // move it
                //source.Invalidate();
            }

            // Reorder
            Point p = destination.PointToClient(new Point(e.X, e.Y));
            var item = destination.GetChildAtPoint(p);
            int index = destination.Controls.GetChildIndex(item, false);
            destination.Controls.SetChildIndex(row, index);  // move it

            OnChange(this, new EventArgs());
            destination.Invalidate();
        }

        void InsideDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        #endregion drag and drop


        private void InsideDoubleClick(object sender, EventArgs e)
        {
            if (!CanEditDescription) return;
            AddNewRow(new FormularFieldDescriptor("string Foo"), false);
            
        }
    }
}
