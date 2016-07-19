using System;
using System.Windows.Forms;
using System.Drawing;
using VVVV.Utils.VMath;

namespace VVVV.Packs.Messaging.Nodes
{
    public class FieldPanel : TableLayoutPanel
    {
        #region fields and properties
        public event EventHandler Change;

        protected CheckBox FToggle;
        protected TextBox FText;

        public bool AllowDrag { get; set; }

        public bool IsDragging = false;
        protected int MinDragPixel = 8;
        protected Vector2D DragStartPosition;

        private FormularFieldDescriptor _descriptor;
        public FormularFieldDescriptor Descriptor
        {
            get
            {
                return _descriptor;
            }
            set
            {
                if (value == null)
                    IsEmpty = true; // vanish into the void...
                else
                {
                    _descriptor = value.Clone() as FormularFieldDescriptor; // have your own
                    IsFaulty = false; // assume innocence

                    Checked = _descriptor.IsRequired;
                    Description = _descriptor.ToString();
                    Invalidate();
                }
            }
        }

        private bool _isFaulty;
        public bool IsFaulty
        {
            get
            {
                return _isFaulty;
            }
            set
            {
                _isFaulty = value;

                if (_isFaulty)
                {
                    _descriptor = null;
                    Checked = false;
                }
                FToggle.Enabled = !_isFaulty;
            }
        }

        public bool Locked
        {
            get
            {
                return !FToggle.Enabled;
            }
            set
            {
                if (!IsFaulty) FToggle.Enabled = !value;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return _descriptor == null;
            }
            set
            {
                if (value == true)
                {
                    _descriptor = null;
                    Description = "Ø";
                    Visible = false;
                    Checked = false;

                    Invalidate();
                }
                else throw new InvalidOperationException("Can not autofill FieldPanel. Feed a new FormularFieldDescriptor to fill FieldPanel instead.");

            }
        }

        private bool _canEdit = false;
        public bool CanEdit
        {
            get
            {
                return _canEdit;
            }
            set
            {
                _canEdit = value;
                FText.ReadOnly = !_canEdit;

                if (IsFaulty && !_canEdit) IsEmpty = true;
                Invalidate();
            }
        }
        public bool Checked
        {
            get
            {
                return FToggle.Checked;
            }
            set
            {
                // freaking feedbacks, be gone
                FToggle.CheckedChanged -= ReadToggle;
                FToggle.Checked = value;
                FToggle.CheckedChanged += ReadToggle;

                if (Descriptor != null) Descriptor.IsRequired = value;

                Invalidate();
            }
        }
        public string Description
        {
            get
            {
                return FText.Text;
            }
            set
            {
                if (value == null) value = "";
                FText.Text = value;

                // do not enforce a Change 
            }
        }

        #endregion fields and properties

        #region constructor, gui creation and event initialisation
        public FieldPanel()
        {
            this.AutoSize = true;
            this.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.Height = 29;
            this.MinimumSize = new Size(370, 29);
            this.Dock = DockStyle.Fill;

            this.BorderStyle = BorderStyle.FixedSingle;

            this.ColumnCount = 2;
            this.RowCount = 1;

            this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));


            FToggle = new CheckBox();
            InitCheckBox(FToggle);
            this.Controls.Add(FToggle);

            FText = new TextBox();
            InitTextBox(FText);
            this.Controls.Add(FText);

            AllowDrag = true;
        }

        public FieldPanel(FormularFieldDescriptor descriptor) : this()
        {
            Descriptor = descriptor;
        }

        void InitCheckBox(CheckBox box)
        {
            box.Text = "";
            box.FlatStyle = FlatStyle.Standard;
            box.AutoCheck = true;
            box.Dock = DockStyle.Top;
            box.Height = 22;
            box.Width = 20;
            FToggle.CheckedChanged += ReadToggle;
        }

        private void ReadToggle(object sender, EventArgs e)
        {
            if (_descriptor != null) _descriptor.IsRequired = Checked;

            if (Change != null && !IsFaulty) Change(this, e);
            Invalidate();
        }

        void InitTextBox(TextBox box)
        {
            box.Dock = DockStyle.Top;
            box.Text = "string Foo";

            box.BorderStyle = BorderStyle.None;

            box.GotFocus += (sender, e) =>
            {
                if (CanEdit)
                {
                    box.SelectAll();
                }
                else
                {
                    Focus();
                }
                Invalidate();
            };

            box.LostFocus += (sender, e) =>
            {
                if (CanEdit) OnChange(e);
            };

            box.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
                {
                    e.Handled = true; // suppress default handling
                        Focus(); // remove Focus from textfield -> will trigger OnChange
                        Invalidate();
                }
            };
        }
        #endregion constructor, gui creation and event initialisation

        #region change
        protected virtual void OnChange(EventArgs e)
        {
            var oldDescription = _descriptor == null ? "" : _descriptor.ToString();

            // failsafe
            if (!CanEdit)
            {
                Description = oldDescription; // stick to the plan!
                Invalidate();
                return;
            }

            // generate new Descriptor, if (manual) description from textbox does not match
            if (oldDescription != Description)
            {
                try
                {
                    var desc = new FormularFieldDescriptor(Description);

                    // assume this is even a wanted pin, so autocheck
                    desc.IsRequired = true;
                    Descriptor = desc;

                    if (Change != null && !IsFaulty) Change(this, e);
                }
                catch (ParseFormularException)
                {
                    IsFaulty = true;
                }

                Invalidate();
            }

        }
        #endregion change

        #region events
        protected override void OnPaint(PaintEventArgs e)
        {
            var stdColor = Color.FromArgb(230, 230, 230);
            var editableColor = Color.FromArgb(250, 250, 250);
            var focusColor = Color.Silver;
            var faultyColor = Color.IndianRed;

            var isFocused = Focused || FText.Focused;
            BackColor = IsFaulty ? faultyColor : isFocused ? focusColor : CanEdit ? editableColor : stdColor;
            FText.BackColor = !CanEdit ? BackColor : isFocused ? editableColor : BackColor;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (CanEdit && e.KeyCode == Keys.Delete)
            {
                e.Handled = true; // suppress default handling
                this.IsEmpty = true;  // just clear and keep alive for future use
            }
        }

        protected override void OnClick(EventArgs e)
        {
            this.Focus();
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (CanEdit && e.Button.IsRight())
            {
                this.IsEmpty = true;
                return;
            }

            if (e.Button.IsLeft())
            {
                if (!(FText.Focus() && CanEdit)) this.Focus();
                DragStartPosition = new Vector2D(e.X, e.Y);
                this.IsDragging = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!AllowDrag || IsDragging) return;

            if (!e.Button.IsLeft()) return;

            var mousePos = new Vector2D(e.X, e.Y);

            // This is a check to see if the mouse is moving while pressed.
            // Without this, the DragDrop is fired directly when the control is clicked, now you have to drag a few pixels first.
            if (VMath.Dist(mousePos, DragStartPosition) > MinDragPixel)
            {
                DoDragDrop(this, DragDropEffects.All);
                IsDragging = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            IsDragging = false;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            Controls.Container.Invalidate();
        }
        #endregion events

    }
}

