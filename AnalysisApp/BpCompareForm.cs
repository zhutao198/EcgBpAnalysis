using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.DAL;

namespace AnalysisApp
{
    public partial class BpCompareForm : BaseDialogForm
    {
        public static BpCompareForm Instance { get; private set; }

        public BpCompareForm() : base("", "比较")
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
            Instance = this;
        }

        private void BpCompareForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        public async void Show(string name, IWin32Window owner = null)
        {
            if (owner != null)
            {
                if (this.Owner != null) Show();
                else Show(owner);
            }
            else
            {
                Show();
            }
            this.bpCompareControl1.InitData(name);

            while (this.bpCompareControl1.IsShown)
            {
                await Task.Delay(1);
            }
            this.bpCompareControl2.InitData(name);

            while (this.bpCompareControl2.IsShown)
            {
                await Task.Delay(1);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Hide();
                return true; // 表示已处理
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
