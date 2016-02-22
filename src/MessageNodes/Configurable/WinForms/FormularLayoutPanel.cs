using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VVVV.Packs.Messaging.Nodes
{
    public class FormularLayoutPanel : FlowLayoutPanel
    {
        
        public event EventHandler OnChange;

        public FormularLayoutPanel()
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

        public IEnumerable<FieldPanel> FieldPanels
        {
            get
            {
                // filter and return all child controls of type FieldPanel
                return Controls.OfType<FieldPanel>();
            }
        }

        public IEnumerable<FormularFieldDescriptor> CheckedDescriptors
        {
            get 
            {
                var desc = from field in FieldPanels
                           where field.Checked
                           select field.Descriptor;
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
                    foreach (var field in FieldPanels) field.CanEdit = value;
                }
            }
        }

        #region dynamic control layout
        public bool LayoutByFormular(MessageFormular formular, bool forceChecked = false) {
            this.SuspendLayout();

            var prev =    FieldPanels.ToList();

            var remove =  (
                                from field in prev
                                where (field.Descriptor == null) || !(formular.FieldDescriptors.Contains(field.Descriptor))
                                select field
                          ).ToList();

            var current = (
                                from field in prev
                                select field.Descriptor
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
                    AddNewFieldPanel(newEntry, forceChecked);
                }
            }
            
            // cleanup: just keep them lingering around, recycle when needed. should speed up gui
            while (counter > 0)
            {
                var field = remove[maxCount - counter];
                if (field != null)
                {
                    field.Clear(); 
                }
                counter--;
            }
            this.ResumeLayout();
            return true; // return 
        }

        private FieldPanel AddNewFieldPanel(FormularFieldDescriptor desc, bool isChecked)
        {
            var field = new FieldPanel(desc, isChecked);
            field.CanEdit = CanEditDescription;
            Controls.Add(field);
            field.OnChange += (sender, args) =>
            {
                OnChange(this, args);
            };
            field.InitializeListeners();
            return field;
        }
        #endregion dynamic control layout

        #region drag and drop
        void InsideDragDrop(object sender, DragEventArgs e)
        {
            var field = (FieldPanel)e.Data.GetData(typeof(FieldPanel));
            FlowLayoutPanel destination = (FlowLayoutPanel)sender;
            FlowLayoutPanel source = (FlowLayoutPanel)field.Parent;

            // this commented code can be used to drag'n'drop across windows.
            if (source != destination)
            {
                //// Move control to panel, updates Parent
                //destination.Controls.Add(field);
                //field.Size = new Size(destination.Width, 50);
                
                //destination.Controls.SetChildIndex(field, index);  // move it
                //source.Invalidate();
            }

            // Reorder
            Point p = destination.PointToClient(new Point(e.X, e.Y));
            var item = destination.GetChildAtPoint(p);
            int index = destination.Controls.GetChildIndex(item, false);
            destination.Controls.SetChildIndex(field, index);  // move it

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

            var field = AddNewFieldPanel(new FormularFieldDescriptor("string Foo"), false);
            field.CanEdit = true;
            
        }
    }
}
