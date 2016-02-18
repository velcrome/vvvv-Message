using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace VVVV.Packs.Messaging.Nodes
{
        class RowPanel : TableLayoutPanel
        {
            #region fields and properties
            private CheckBox _toggle;
            private TextBox _text;

            public bool AllowDrag { get; set; }
            private bool _isDragging = false;

            private int _DDradius = 40;
            private int _mX = 0;
            private int _mY = 0;

            public bool Checked
            {
                get
                {
                    return _toggle.Checked;
                }
                set
                {
                    _toggle.Checked = value;
                }
            }

            public string Description
            {
                get
                {
                    return _text.Text;
                }
                set
                {
                    _text.Text = value;
                }
            }
            #endregion fields and properties

            public RowPanel()
            {
                this.AutoSize = true;
                this.Anchor = AnchorStyles.Left | AnchorStyles.Right; //| AnchorStyles.Right

                this.Height = 27;
                this.MinimumSize = new Size(300, 27);
                this.Dock = DockStyle.Fill;

                this.BorderStyle = BorderStyle.FixedSingle;
                this.BackColor = Color.FromArgb(230, 230, 230);

                this.ColumnCount = 2;
                this.RowCount = 1;

                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));


                _toggle = new CheckBox();
                LayoutToggle(_toggle);
                this.Controls.Add(_toggle);


                _text = new TextBox();
                LayoutText(_text);
                this.Controls.Add(_text);

                AllowDrag = true;

            }

            public RowPanel(string text, bool isChecked)
                : this()
            {
                Checked = isChecked;
                Description = text;
            }

            void LayoutToggle(CheckBox box)
            {
                box.Text = "";
                box.FlatStyle = FlatStyle.Standard;
                box.AutoCheck = true;
                box.Dock = DockStyle.Top;
                box.Height = 18;
                box.Width = 20;
            }

            void LayoutText(TextBox box)
            {
                box.ReadOnly = true;
                box.Dock = DockStyle.Top;
                box.Text = "Lorem ipsum";
                box.BorderStyle = BorderStyle.None;
                box.BackColor = Color.FromArgb(230, 230, 230);
            }

            protected override void OnGotFocus(EventArgs e)
            {
                _text.BackColor =
                this.BackColor = Color.Silver;
                base.OnGotFocus(e);
            }

            protected override void OnLostFocus(EventArgs e)
            {
                this.BackColor =
                _text.BackColor = Color.FromArgb(230, 230, 230);
                base.OnLostFocus(e);
            }

            protected override void OnClick(EventArgs e)
            {
                this.Focus();
                base.OnClick(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                this.Focus();
                base.OnMouseDown(e);
                _mX = e.X;
                _mY = e.Y;
                this._isDragging = false;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (!_isDragging)
                {
                    // This is a check to see if the mouse is moving while pressed.
                    // Without this, the DragDrop is fired directly when the control is clicked, now you have to drag a few pixels first.
                    if (e.Button == MouseButtons.Left && _DDradius > 0 && this.AllowDrag)
                    {
                        int num1 = _mX - e.X;
                        int num2 = _mY - e.Y;
                        if (((num1 * num1) + (num2 * num2)) > _DDradius)
                        {
                            DoDragDrop(this, DragDropEffects.All);
                            _isDragging = true;
                            return;
                        }
                    }
                    base.OnMouseMove(e);
                }
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                _isDragging = false;
                base.OnMouseUp(e);
            }


        }
    }

