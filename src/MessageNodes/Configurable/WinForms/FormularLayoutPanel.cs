using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VVVV.Packs.Messaging.Nodes
{
    public class FormularLayoutPanel : FlowLayoutPanel
    {
        
        public event EventHandler Change;

        public FormularLayoutPanel()
        {
            //table panel fills the whole window
            FlowDirection = FlowDirection.LeftToRight;

            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            Dock = DockStyle.Fill;

            AutoScroll = true;
            BorderStyle = BorderStyle.Fixed3D;

            AllowDrop = true;
//            DragEnter += new DragEventHandler(InsideDragEnter);
//            DragDrop += new DragEventHandler(InsideDragDrop);
//            DoubleClick += InsideDoubleClick;

        }

        public IEnumerable<FieldPanel> FieldPanels
        {
            get
            {
                // filter and return all child controls of type FieldPanel
                return Controls.OfType<FieldPanel>().ToList();
            }
        }

        public MessageFormular Formular
        {
            get 
            {
                var fields = from field in FieldPanels
                           where field.Checked
                           where field.IsEmpty == false
                           select field.Descriptor;
                return new MessageFormular(fields.ToList());
            }
            set
            {
                LayoutByFormular(value, false);
            }
        }

        private bool _canEdit = false;
        public bool CanEditFields
        {
            get
            {
                return _canEdit;
            }
            set
            {
                if (value != CanEditFields)
                {
                    _canEdit = value;
                    foreach (var field in FieldPanels) field.CanEdit = value;
                    Invalidate();
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
                if (field != null) field.IsEmpty = true;
                counter--;
            }
            this.ResumeLayout();
            return true; // return 
        }
        #endregion dynamic control layout

        #region new field panel
        private FieldPanel AddNewFieldPanel(FormularFieldDescriptor desc, bool isChecked)
        {
            var field = new FieldPanel(desc, isChecked);
            field.CanEdit = CanEditFields;
            Controls.Add(field);
            field.Change += (sender, args) => Change(this, args);
            
            return field;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (!CanEditFields) return;

            var field = AddNewFieldPanel(new FormularFieldDescriptor("string Foo"), false);
            field.CanEdit = true;

        }
        #endregion new field panel

        #region drag and drop

        protected override void OnDragDrop(DragEventArgs e)
        {
            var field = (FieldPanel)e.Data.GetData(typeof(FieldPanel));
            var source = field.Parent;

            Point p = PointToClient(new Point(e.X, e.Y));
            var item = GetChildAtPoint(p);
            int index = Controls.GetChildIndex(item, false);

            // drag'n'drop even across windows, but only when allowed.
            if (source != this && CanEditFields)
            {
                // Copy control to panel
                var copy = new FieldPanel((FormularFieldDescriptor)field.Descriptor.Clone(), field.Checked); 
                copy.CanEdit = true;

                Controls.Add(copy);
                Controls.SetChildIndex(copy, index);  // place it
            }
            else Controls.SetChildIndex(field, index);  // move it

            Change(this, new EventArgs());
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        #endregion drag and drop


        protected override void OnLostFocus(EventArgs e)
        {
            Refresh();
        }
    }
}
