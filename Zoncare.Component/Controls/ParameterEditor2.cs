using System;
using System.Windows.Forms;
using Zoncare.Common;

namespace Zoncare.Component.Controls
{
    public partial class ParameterEditor2 : UserControl
    {
        public string Value
        {
            get { return GetValue(); }
            set
            {
                int splitIdx1 = value.IndexOf('/');
                if (splitIdx1 == 0)
                {
                    txtMeasure1.Text = "/";
                    splitIdx1 = value.IndexOf('/', splitIdx1 + 1);
                }
                else
                {
                    txtMeasure1.Text = value.Substring(0, splitIdx1);
                }

                txtMeasure2.Text = value.Substring(splitIdx1 + 1);
            }
        }

        public TextBox EditingTextBox { get { return this.txtMeasure1; } }
        public event EventHandler ValueChanged;

        private bool _isFirstShow = true;

        /// <summary>
        /// 包含两个输入参数的编辑器
        /// </summary>
        /// <param name="unitText">参数单位</param>
        public ParameterEditor2(string unitText)
        {
            InitializeComponent();

            this.labUnit.Text = unitText;
            this.txtMeasure1.Multiline = true;
            this.txtMeasure2.Multiline = true;
        }


        private void ParameterEditor2_Load(object sender, EventArgs e)
        {
            this.txtMeasure1.TextChanged += txtMeasure_TextChanged;
            this.txtMeasure2.TextChanged += txtMeasure_TextChanged;
            this.txtMeasure1.GotFocus += txtMeasure_GotFocus;
            this.txtMeasure2.GotFocus += txtMeasure_GotFocus;
            this.txtMeasure1.LostFocus += txtMeasure_LostFocus;
            this.txtMeasure2.LostFocus += txtMeasure_LostFocus;
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
                txtMeasure1.SelectionStart = txtMeasure1.Text.Length;
                _isFirstShow = false;
            }
        }

        private void txtMeasure_LostFocus(object sender, EventArgs e)
        {
            if (!txtMeasure1.Focused && !txtMeasure2.Focused) this.Hide();
        }

        private void txtMeasure_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                this.Hide();
                e.Handled = true; // 禁止回车键输入
                return;
            }

            TextBox txtMeasure = sender as TextBox;
            bool isSelectAll = (txtMeasure.SelectionLength == txtMeasure.Text.Length);
            if (labUnit.Text == "mv") //mv可以有小数点、负号
            {
                if (e.KeyChar != '\b' && e.KeyChar != '/' && e.KeyChar != '.' && e.KeyChar != '-' && !Char.IsDigit(e.KeyChar))
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
            TextBox txtMeasure = sender as TextBox;
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
                float val1 = 0, val2 = 0;
                bool parsed1 = float.TryParse(txtMeasure1.Text, out val1);
                bool parsed2 = float.TryParse(txtMeasure2.Text, out val2);
                return (parsed1 ? val1.ToString() : CommonValues.DefaultEmptyValueText) + "/" +
                    (parsed2 ? val2.ToString() : CommonValues.DefaultEmptyValueText) + " " + labUnit.Text;
            }
            else
            {
                int val1 = 0, val2 = 0;
                bool parsed1 = int.TryParse(txtMeasure1.Text, out val1);
                bool parsed2 = int.TryParse(txtMeasure2.Text, out val2);
                return (parsed1 ? val1.ToString() : CommonValues.DefaultEmptyValueText) + "/" +
                    (parsed2 ? val2.ToString() : CommonValues.DefaultEmptyValueText) + " " + labUnit.Text;
            }
        }

    }
}
