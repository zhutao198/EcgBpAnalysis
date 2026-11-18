using System;
using System.Windows.Forms;
using Zoncare.Common;

namespace Zoncare.Component.Controls
{
    public partial class ParameterEditor1 : UserControl
    {
        public string Value
        {
            get { return GetValue(); }
            set { txtMeasure.Text = value; }
        }

        public TextBox EditingTextBox { get { return this.txtMeasure; } }
        public event EventHandler ValueChanged;

        private bool _isFirstShow = true;

        /// <summary>
        /// 包含一个输入参数的编辑器
        /// </summary>
        /// <param name="unitText">参数单位</param>
        public ParameterEditor1(string unitText)
        {
            InitializeComponent();

            this.labUnit.Text = unitText;
            this.txtMeasure.Multiline = true;
        }


        private void MeasureEditor_Load(object sender, EventArgs e)
        {
            this.txtMeasure.TextChanged += txtMeasure_TextChanged;
            this.txtMeasure.GotFocus += txtMeasure_GotFocus;
            this.txtMeasure.LostFocus += txtMeasure_LostFocus;
        }

        private void txtMeasure_GotFocus(object sender, EventArgs e)
        {
            if (this.Visible == false)
            {
                this.BringToFront();
                this.Show();
            }
            if (_isFirstShow)
            {
                txtMeasure.SelectionStart = txtMeasure.Text.Length;
                _isFirstShow = false;
            }
        }

        private void txtMeasure_LostFocus(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void txtMeasure_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                this.Hide();
                e.Handled = true; // 禁止回车键输入
                return;
            }

            bool isSelectAll = (txtMeasure.SelectionLength == txtMeasure.Text.Length);
            if (labUnit.Text == "mv") //mv可以有小数点、负号
            {
                if (e.KeyChar != '\b' && e.KeyChar != '/' && e.KeyChar != '.' && e.KeyChar != '-' && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                    return;
                }
                if (e.KeyChar == '.')
                {
                    //小数点不能第一位，且不能多个
                    if (txtMeasure.SelectionStart == 0)
                    {
                        e.Handled = true;
                    }
                    else if (!isSelectAll)
                    {
                        int selEnd = txtMeasure.SelectionStart + txtMeasure.SelectionLength;
                        int dotPos = txtMeasure.Text.IndexOf('.');
                        if (dotPos > 0)
                        {
                            if (dotPos >= selEnd || dotPos < txtMeasure.SelectionStart)
                                e.Handled = true;
                        }
                    }
                }

                if (e.KeyChar == '-')
                {
                    if (!isSelectAll)
                    {
                        //负号只能第一位，且不能多个
                        if (txtMeasure.SelectionStart > 0) e.Handled = true;
                        if (txtMeasure.Text.IndexOf('-') >= 0) e.Handled = true;
                    }
                }
            }
            else
            {
                if (e.KeyChar != '\b' && e.KeyChar != '/' && !Char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void txtMeasure_TextChanged(object sender, EventArgs e)
        {
            // 确保只有一行文本
            if (txtMeasure.Lines.Length > 1)
            {
                string firstLine = txtMeasure.Lines[0];
                txtMeasure.Text = firstLine;
                txtMeasure.SelectionStart = firstLine.Length; // 移动光标到文本末尾
            }

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }


        public string GetValue()
        {
            if (labUnit.Text == "mv") //mv是小数
            {
                bool parsed = float.TryParse(txtMeasure.Text.Trim(), out float val);
                if (parsed)
                    return val + " " + labUnit.Text;
                else
                    return CommonValues.DefaultEmptyValueText + " " + labUnit.Text;
            }
            else
            {
                bool parsed = int.TryParse(txtMeasure.Text.Trim(), out int val);
                if (parsed)
                    return val + " " + labUnit.Text;
                else
                    return CommonValues.DefaultEmptyValueText + " " + labUnit.Text;
            }
        }

    }
}
