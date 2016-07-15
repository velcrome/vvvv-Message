using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VVVV.Packs.Messaging.Nodes
{
    public class FormularLayoutPanel : FlowLayoutPanel
    {
        
        public event EventHandler<FormularChangedEventArgs> Changed;

        public FormularLayoutPanel()
        {
            //table panel fills the whole window
            FlowDirection = FlowDirection.LeftToRight;

            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            Dock = DockStyle.Fill;

            AutoScroll = true;
            BorderStyle = BorderStyle.Fixed3D;

            AllowDrop = true;
        }

        #region fields
        public IEnumerable<FieldPanel> FieldPanels
        {
            get
            {
                // filter and return all child controls of type FieldPanel
                return Controls.OfType<FieldPanel>().ToList();
            }
        }

        private bool _isLocked;
        public bool Locked
        {
            get
            {
                return _isLocked;
            }
            set
            {
                _isLocked = value;

                var all = from field in FieldPanels
                          where !field.IsEmpty
                          where !field.IsFaulty
                          select field;

                foreach (var field in all)
                {
                    field.Locked = _isLocked;
                }
            }
        }
        
        protected string _formularName = MessageFormular.DYNAMIC;
        public MessageFormular Formular
        {
            get 
            {
                var fields = from field in FieldPanels
                           where !field.IsEmpty
                           where !field.IsFaulty
                           select field;

                foreach (var field in fields)
                    field.Descriptor.IsRequired = field.Checked;

                var formular =  new MessageFormular(_formularName, fields.Select(field => field.Descriptor));
                return formular;
            }
            set
            {
                LayoutByFormular(value, value.IsDynamic);
                _formularName = value.Name;
                Invalidate();
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

        #endregion fields

        #region dynamic control layout
        protected bool LayoutByFormular(MessageFormular formular, bool forceChecked = false) {
            this.SuspendLayout();

            var prev =    FieldPanels.ToList();

            var remove =  (
                                from field in prev
                                where !(field.IsFaulty && field.CanEdit)
                                where field.IsEmpty || !(formular.FieldDescriptors.Contains(field.Descriptor))
                                select field
                          ).ToList();

            var remain = (
                                from field in prev
                                where !(field.IsFaulty && field.CanEdit) && !field.IsEmpty
                                where formular.FieldDescriptors.Contains(field.Descriptor)
                                select field
                          ).ToList();

            var currentDesc = (
                                from field in prev
                                select field.Descriptor
                          );

            var fresh =   (
                                from field in formular.FieldDescriptors
                                where !currentDesc.Contains(field)
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
                    overwrite.Checked = overwrite.Checked || forceChecked; 
//                    overwrite.Checked = overwrite.Checked || forceChecked || newEntry.IsRequired; // stay true, when likely to be a rename
                    overwrite.Visible = true;
                    overwrite.Locked = Locked;
                    counter--;
                }
                else
                {
                    AddNewFieldPanel(newEntry, forceChecked );
//                    AddNewFieldPanel(newEntry, forceChecked || newEntry.IsRequired);
                }
            }
            
            // cleanup: just keep them lingering around, recycle when needed. should speed up gui
            while (counter > 0)
            {
                var field = remove[maxCount - counter];
                if (field != null) field.IsEmpty = true;
                counter--;
            }

            // update remaining fields
            foreach (var field in remain)
            {
                var isRequired = formular[field.Name].IsRequired;
                field.Checked = field.Descriptor.IsRequired = isRequired;
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
            field.Locked = Locked;

            field.Change += (sender, args) =>
            {
                if (Changed != null) Changed(this, new FormularChangedEventArgs(Formular));
            };
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
                var copy = AddNewFieldPanel((FormularFieldDescriptor)field.Descriptor.Clone(), field.Checked);
                copy.Locked = Locked;
                Controls.SetChildIndex(copy, index);  // place it
            }
            else Controls.SetChildIndex(field, index);  // move it



            Changed(this, new FormularChangedEventArgs(Formular) );
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        #endregion drag and drop

        #region focus
        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
        }
        #endregion focus

    }
}
