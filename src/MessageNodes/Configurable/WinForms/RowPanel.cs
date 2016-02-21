using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using VVVV.Utils.VMath;

namespace VVVV.Packs.Messaging.Nodes
{
        public class RowPanel : TableLayoutPanel
        {
            #region fields and properties
            public event EventHandler OnChange;

            private CheckBox FToggle;
            private TextBox FText;

            public bool AllowDrag { get; set; }

            private bool FIsDragging = false;
            private int FMinMovementRadius = 40;
            private Vector2D FClickPos;

            private FormularFieldDescriptor _descriptor;
            public FormularFieldDescriptor Descriptor
            {
                get {
                    return _descriptor;
                }
                set {
                    _descriptor = value;
                    this.Description = Descriptor == null ? "string Foo" : Descriptor.ToString();
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
                    FToggle.Checked = value;
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
                    FText.Text = value;
                }
            }

            private bool _canEdit = true;
            public bool CanEdit
            {
                get
                {
                    return _canEdit;
                }
                set
                {
                    if (value != CanEdit)
                    {
                        FText.ReadOnly = !value;
                        _canEdit = value;
                    }
                }
            }

            #endregion fields and properties

            #region constructors
            public RowPanel()
            {
                this.AutoSize = true;
                this.Anchor = AnchorStyles.Left | AnchorStyles.Right; 
                this.Height = 29;
                this.MinimumSize = new Size(375, 29);
                this.Dock = DockStyle.Fill;

                this.BorderStyle = BorderStyle.FixedSingle;
                this.BackColor = Color.FromArgb(230, 230, 230);

                this.ColumnCount = 2;
                this.RowCount = 1;

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));


                FToggle = new CheckBox();
                LayoutToggle(FToggle);
                this.Controls.Add(FToggle);

                FText = new TextBox();
                LayoutText(FText);
                this.Controls.Add(FText);

                AllowDrag = true;

            }

            public void InitializeListeners()
            {
                FToggle.CheckedChanged += (sender, e) => OnChange(this, e);
                FText.LostFocus += (sender, e) =>
                {
                    var oldDescription = _descriptor.ToString();
                    
                    // failsafe
                    if (!_canEdit)
                    {
                        Description = oldDescription;
                        return;
                    }

                    // generate new Descriptor, if (manual) description from textbox does not match
                    if (oldDescription != Description)
                    {
                        _descriptor = new FormularFieldDescriptor(Description);
                        Checked = true;
                    }

                    OnChange(this, e);
                };

                FText.KeyPress  += (sender, e) =>
                {
                    if (e.KeyChar.Equals((char)13))
                    {
                        // call your method for action on enter
                        e.Handled = true; // suppress default handling
                        this.Focus();
                    }
                };
            }

            public RowPanel(FormularFieldDescriptor descriptor, bool isChecked = false) : this()
            {
                Descriptor = descriptor;
                Checked = isChecked;
            }

            protected RowPanel(string text, bool isChecked = false)
                : this()
            {
                Description = text;
                Checked = isChecked;
            }
            #endregion constructors

            #region children layouts
            void LayoutToggle(CheckBox box)
            {
                box.Text = "";
                box.FlatStyle = FlatStyle.Standard;
                box.AutoCheck = true;
                box.Dock = DockStyle.Top;
                box.Height = 22;
                box.Width = 20;
            }

            void LayoutText(TextBox box)
            {
//                box.ReadOnly = true;
                box.Dock = DockStyle.Top;
                
                box.Text = "string Foo";
  
                box.BorderStyle = BorderStyle.None;
                box.BackColor = Color.FromArgb(230, 230, 230);
            }
            #endregion children layouts

            #region focus
            protected override void OnGotFocus(EventArgs e)
            {
                FText.BackColor =
                this.BackColor = Color.Silver;
                base.OnGotFocus(e);
            }

            protected override void OnLostFocus(EventArgs e)
            {
                this.BackColor =
                FText.BackColor = Color.FromArgb(230, 230, 230);
                base.OnLostFocus(e);
            }

            protected override void OnClick(EventArgs e)
            {
                this.Focus();
                base.OnClick(e);
            }

            #endregion focus

            #region drag and drop
            protected override void OnMouseDown(MouseEventArgs e)
            {
                this.Focus();
                base.OnMouseDown(e);
                FClickPos = new Vector2D(e.X, e.Y);
                this.FIsDragging = false;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (!FIsDragging)
                {
                    // This is a check to see if the mouse is moving while pressed.
                    // Without this, the DragDrop is fired directly when the control is clicked, now you have to drag a few pixels first.
                    if (e.Button == MouseButtons.Left && FMinMovementRadius > 0 && this.AllowDrag)
                    {
                        if (VMath.Dist(FClickPos, new Vector2D(e.X, e.Y)) > FMinMovementRadius)
                        {
                            DoDragDrop(this, DragDropEffects.All);
                            FIsDragging = true;
                            return;
                        }
                    }
                    base.OnMouseMove(e);
                }
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                FIsDragging = false;
                base.OnMouseUp(e);
            }
            #endregion drag and drop

        }
    }

