using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VVVV.Packs.Messaging.Nodes
{
    public class FormularLayoutPanel : FlowLayoutPanel
    {
        public event FormularChanged Changed;

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
                var panels = from panel in FieldPanels
                           where !panel.IsEmpty
                           where !panel.IsFaulty
                           select panel;

                // might not be necessary...
                //foreach (var panel in panels)
                //    panel.Descriptor.IsRequired = panel.Checked;

                var formular =  new MessageFormular(_formularName, panels.Select(field => field.Descriptor));
                return formular;
            }
            set
            {
                _formularName = value.Name;
                LayoutByFormular(value, value.IsDynamic);  // will cause Change events, if not prevented
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
        protected bool LayoutByFormular(MessageFormular formular, bool keepUnused) {
            this.SuspendLayout();

            var empty = Enumerable.Empty<FieldPanel>();
            var prev =    FieldPanels.ToList();

            var unused = (
                                from panel in prev
                                where panel.IsFaulty && panel.CanEdit
                                select panel
                          ).Concat(
                                from panel in prev
                                where !panel.IsFaulty && !panel.IsEmpty
                                where !formular.FieldNames.Contains(panel.Descriptor.Name)
                                select panel
                          ).ToList();

            var remove =  (
                                from panel in prev
                                where !panel.IsFaulty
                                where panel.IsEmpty || !formular.FieldNames.Contains(panel.Descriptor.Name)
                                select panel
                          ).Concat(keepUnused? empty : unused).ToList();

            // keep all that will 
            var remain = (
                                from panel in prev
                                where !panel.IsFaulty
                                where !panel.IsEmpty
                                where formular.FieldNames.Contains(panel.Descriptor.Name)
                                select panel
                          ).Concat(keepUnused? unused : empty).ToList();

            var currentDesc = (
                                from panel in prev
                                where !panel.IsEmpty
                                select panel.Descriptor.Name
                          );

            var fresh =   (
                                from field in formular.FieldDescriptors
                                where !currentDesc.Contains(field.Name)
                                select field
                          ).ToList();

            var maxCount = remove.Count();
            var counter = maxCount;

            foreach (var descriptor in fresh) 
            {
                // if lingering panels available, recycle it or else instanciate new one
                if (counter > 0)
                {
                    FieldPanel revive = remove[maxCount - counter];
                    revive.Descriptor = descriptor; // will modify the checkbox and description text

                    revive.Visible = true;
                    revive.Locked = Locked;
                    counter--;
                }
                else
                {
                    AddNewFieldPanel(descriptor);
                }
            }
            
            // cleanup: just keep them lingering around, recycle when needed. should speed up gui
            while (counter > 0)
            {
                FieldPanel panel = remove[maxCount - counter];
                if (panel != null && !(panel.IsFaulty && keepUnused)) panel.IsEmpty = true; // removes Checked as well
                counter--;
            }

            // update remaining fields
            foreach (FieldPanel panel in remain)
            {
                if (!panel.IsFaulty) // don't touch faulty panels, user might want to fix them later.
                {
                    var desc = formular[panel.Descriptor.Name]; 
                    desc.IsRequired = panel.Checked;
                    panel.Descriptor = desc; // overwrite to stay in sync with the node's central formular
                }
            }

            this.ResumeLayout();
            return true; 
        }
        #endregion dynamic control layout

        #region new field panel
        private FieldPanel AddNewFieldPanel(FormularFieldDescriptor desc)
        {
            var field = new FieldPanel(desc);
            field.CanEdit = CanEditFields;
            Controls.Add(field);
            field.Locked = Locked;

            field.Change += (sender, args) =>
            {
                if (Changed != null) Changed(this, Formular);
            };
            return field;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (!CanEditFields) return;

            var desc = new FormularFieldDescriptor("string Foo");
            desc.IsRequired = false;
            var field = AddNewFieldPanel(desc);
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
                var desc = field.Descriptor.Clone() as FormularFieldDescriptor;
                var copy = AddNewFieldPanel(desc);
                copy.Locked = Locked;
                Controls.SetChildIndex(copy, index);  // place it
            }
            else Controls.SetChildIndex(field, index);  // move it

            Changed(this, Formular);
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
