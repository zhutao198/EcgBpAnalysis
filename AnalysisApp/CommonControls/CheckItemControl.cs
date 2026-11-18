using System;
using System.Drawing;
using System.Windows.Forms;

namespace AnalysisApp.CommonControls
{
    public partial class CheckItemControl : UserControl
    {
        bool bChecked = true;
        Color btnCheckedColor;
        Color btnUnCheckedColor = Color.LightGray;

        public event Action<bool> OnItemChecked;


        public CheckItemControl()
        {
            InitializeComponent();
        }

        private void CheckItem_Load(object sender, EventArgs e)
        {
            this.Click += CheckItem_Click;
            this.btnItem.Click += CheckItem_Click;
            this.lblItem.Click += CheckItem_Click;
        }

        public void CheckItem_Click(object sender, EventArgs e)
        {
            bChecked = !bChecked;
            if (bChecked)
            {
                this.btnItem.BackColor = btnCheckedColor;
            }
            else
            {
                this.btnItem.BackColor = btnUnCheckedColor;
            }
            OnItemChecked?.Invoke(bChecked);
        }

        public void initCtl(Color btnColor, string lblText)
        {
            btnCheckedColor = btnColor;
            this.btnItem.BackColor = btnCheckedColor;
            this.lblItem.Text = lblText;
        }
    }
}
