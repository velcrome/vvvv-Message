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
        public class FieldPanel : TableLayoutPanel
        {
            #region color constants
            public static Color COLOR_STANDARD = Color.FromArgb(230, 230, 230);
            public static Color COLOR_DYNAMIC = Color.FromArgb(250, 250, 250);
            public static Color COLOR_FOCUS = Color.Silver;
            public static Color COLOR_FAULTY = Color.IndianRed;
            #endregion color constants

            #region fields and properties
            public event EventHandler OnChange;

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

                    if (_isFaulty) FToggle.Checked = false;
                    FToggle.Enabled = !_isFaulty;
                }
            }

            protected CheckBox FToggle;
            protected TextBox FText;

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
                    if (value == null) throw new ArgumentNullException("Descriptor", "Use Clear() if you want to reset the Descriptor.");
                    _descriptor = value;
                    IsFaulty = false; // assume innocence
                    Description = _descriptor == null ? "string Foo" : _descriptor.ToString();
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
                    if (value == null) value = "";
                    FText.Text = value;
                }
            }

            private bool _canEdit = false
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
                        _canEdit = value;
                        Color = _canEdit ? COLOR_DYNAMIC : COLOR_STANDARD; 
                        FText.ReadOnly = !_canEdit;
                    }
                }
            }

            public Color Color
            {
                get
                {
                    return this.BackColor;
                }

                set
                {
                    this.BackColor = FText.BackColor = value;
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
                this.BackColor = COLOR_STANDARD;

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
            
            public FieldPanel(FormularFieldDescriptor descriptor, bool isChecked = false) : this()
            {
                Descriptor = descriptor;
                Checked = isChecked;
            }

            public void InitializeListeners()
            {
                FToggle.CheckedChanged += (sender, e) => OnChange(this, e);
                FText.GotFocus += (sender, e) => OnGotFocus(e);
                FText.LostFocus += UpdateDescriptor;

                FText.KeyDown  += (sender, e) =>
                {
                    if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true; // suppress default handling
                        this.Focus(); // remove Focus from textfield -> might trigger OnChange
                    }
                };

                KeyDown += (sender, e) =>
                {
                    if (CanEdit && e.KeyCode == Keys.Delete)
                    {
                        e.Handled = true; // suppress default handling
                        this.Clear();
                    }
                };

                this.MouseDown += RemoveWithRightClick;
                this.FText.MouseDown += RemoveWithRightClick;
;

                    
            }

            private void RemoveWithRightClick(object sender, MouseEventArgs e)
            {
                {
                    if (CanEdit && e.Button.IsRight())
                    {
                        this.Clear();
                    }
                }
            }

            private void UpdateDescriptor(object sender, EventArgs e)
            {
                OnLostFocus(e);

                var oldDescription = _descriptor == null? Description : _descriptor.ToString();

                // failsafe
                if (!CanEdit)
                {
                    Description = oldDescription;
                    return;
                }

                // generate new Descriptor, if (manual) description from textbox does not match
                if (oldDescription != Description)
                {
                    try
                    {
                        _descriptor = new FormularFieldDescriptor(Description);
                        Checked = true; // assume this is even a wanted pin, so autocheck
                        IsFaulty = false;
                    }
                    catch (Exception)
                    {
                        IsFaulty = true;
                    }
                }

                if (CanEdit) OnChange(this, e);

                Color = IsFaulty ? COLOR_FAULTY : Focused ? COLOR_FOCUS : CanEdit ? COLOR_DYNAMIC : COLOR_STANDARD;

            }
            #endregion constructor, gui creation and event initialisation

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
                Color = IsFaulty ? COLOR_FAULTY : COLOR_FOCUS;
                base.OnGotFocus(e);

                FText.SelectAll();
            }

            protected override void OnLostFocus(EventArgs e)
            {
                Color = IsFaulty? COLOR_FAULTY : CanEdit ? COLOR_DYNAMIC : COLOR_STANDARD;
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

            internal void Clear()
            {
                _descriptor = null;
                Description = "string Foo";
                Visible = false;
                Checked = false;
            }
        }
    }

