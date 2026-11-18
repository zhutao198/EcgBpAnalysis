using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    public partial class AutoCompleteTextBox : UserControl, ITextInputBox
    {
        /// <summary>
        /// 文本输入框
        /// </summary>
        [Description("文本输入框"), Category("自定义属性")]
        public TextBoxEx InputBox { get { return this.textBoxEx1; } }

        /// <summary>
        /// 输入的文本
        /// </summary>
        [Browsable(false)]
        public string InputText { get { return this.InputBox.Text; } }

        [Description("文本输入框内容描述占位符"), Category("自定义属性")]
        public string PlaceHolderStr
        {
            get
            {
                return this.textBoxEx1.PlaceHolderStr;
            }
            set
            {
                this.textBoxEx1.PlaceHolderStr = value;
            }
        }

        [Description("是否启用自定义输入提示"), Category("自定义属性"), DefaultValue(false)]
        public bool EnableCustomAutoComplete
        {
            get
            {
                return this.textBoxEx1.EnableCustomAutoComplete;
            }
            set
            {
                this.textBoxEx1.EnableCustomAutoComplete = value;
                if (value == false)
                {
                    this.tableLayoutPanel1.ColumnStyles[1].Width = 0;
                }
                else
                {
                    this.tableLayoutPanel1.ColumnStyles[1].Width = 16;
                }
            }
        }

        /// <summary>
        /// 当前选中的输入提示项
        /// </summary>
        [Browsable(false)]
        public ListBoxItem SelectedItem { get { return this.InputBox.SelectedItem; } }

        /// <summary>
        /// 当前输入文本匹配到自动填充项时发生
        /// </summary>
        public event ItemSelectedEventHandler OnItemSelected;
        public event KeyPressEventHandler TextBoxKeyPress;
        public AutoCompleteTextBox()
        {
            InitializeComponent();
            setSize();
        }

        private void AutoCompleteTextBox_Load(object sender, EventArgs e)
        {
            //this.transparent
            this.textBoxEx1.SizeChanged += new EventHandler((o, arg) =>
            {
                setSize();
            });

            this.textBoxEx1.OnItemSelected += new ItemSelectedEventHandler((o, arg) =>
            {
                OnItemSelected?.Invoke(o, arg);
            });
        }

        private void AutoCompleteTextBox_Paint(object sender, PaintEventArgs e)
        {
            if (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, SystemColors.HotTrack, ButtonBorderStyle.Solid);
            else
                ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, Color.SlateGray, ButtonBorderStyle.Solid);

            //ControlPaint.DrawBorder3D(e.Graphics, this.ClientRectangle, Border3DStyle.Flat);
            //ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, SystemColors.WindowFrame, ButtonBorderStyle.Solid);

            //e.Graphics.DrawRectangle(Pens.LightGray,
            //    new Rectangle(base.ClientRectangle.Location.X, base.ClientRectangle.Location.Y, base.ClientRectangle.Width - 1, base.ClientRectangle.Height - 1));
        }

        private void AutoCompleteTextBox_EnabledChanged(object sender, EventArgs e)
        {
            if (this.Enabled == false)
            {
                this.textBoxEx1.BackColor = SystemColors.ControlLight;
                this.tableLayoutPanel1.BackColor = SystemColors.ControlLight;
            }
            else
            {
                this.textBoxEx1.BackColor = Color.White;
                this.tableLayoutPanel1.BackColor = Color.White;
            }
        }

        private void textBoxEx1_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxKeyPress?.Invoke(sender, e);
        }

        private void picDown_MouseEnter(object sender, EventArgs e)
        {
            this.picDown.Invalidate();
        }

        private void picDown_MouseLeave(object sender, EventArgs e)
        {
            this.picDown.Invalidate();
        }

        private void picDown_MouseUp(object sender, MouseEventArgs e)
        {
            bool shouldShow = !this.textBoxEx1.DropdownIsShown; //始终为true ?

            if (this.textBoxEx1.HiddenInvoker != picDown.Handle)
            {
                shouldShow = true;
                picDown.Tag = "1";
            }
            else
            {
                if (picDown.Tag?.ToString() != "1")
                {
                    shouldShow = true;
                    picDown.Tag = "1";
                }
                else
                {
                    shouldShow = false;
                    picDown.Tag = "0";
                }
            }
            this.textBoxEx1.ShowAllSourceItems(shouldShow);
            this.textBoxEx1.Focus();
        }

        private void picDown_Paint(object sender, PaintEventArgs e)
        {
            //绘制聚焦框
            if (this.picDown.ClientRectangle.Contains(this.picDown.PointToClient(Cursor.Position)))
            {
                //int picW = this.picDown.Width, picH = this.picDown.Height;
                //int boundaryLen = this.picDown.Width;
                //if (picH < boundaryLen) boundaryLen = picH;
                //Rectangle rectFocus = new Rectangle((picW - boundaryLen) / 2, (picH - boundaryLen) / 2, boundaryLen, boundaryLen);

                //ControlPaint.DrawFocusRectangle(e.Graphics, this.picDown.ClientRectangle, Color.DeepSkyBlue, Color.White);
                ControlPaint.DrawFocusRectangle(e.Graphics, this.picDown.ClientRectangle);
            }
        }

        private void setSize()
        {
            int minHeight = this.textBoxEx1.Height + this.Padding.Top + this.Padding.Bottom + SystemInformation.Border3DSize.Height * 2;
            this.MinimumSize = new Size(0, minHeight);
            this.MaximumSize = new Size(1920, minHeight + 6);
            //this.Height = this.textBoxEx1.Height + this.Padding.Top + this.Padding.Bottom;

            this.textBoxEx1.DropdownPosition = new Point(
                0 - this.textBoxEx1.Margin.Left,
                this.textBoxEx1.Height + this.Height - this.textBoxEx1.Bottom - SystemInformation.Border3DSize.Height);

            if (EnableCustomAutoComplete && this.tableLayoutPanel1.ColumnStyles[1].Width < 12)
            {
                this.tableLayoutPanel1.ColumnStyles[1].Width = 12;
            }
        }
    }
}
