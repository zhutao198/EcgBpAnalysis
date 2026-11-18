using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;

namespace SystemSettings
{
    public partial class SystemSetting : BaseDialogForm
    {
        private Dictionary<Button, int> _tabMapping; //按钮与Tab页映射集合
        public event Action<SettingChangeType> OnSettingChanged;

        public SystemSetting() : base("", "设置")
        {
            InitializeComponent();

            this.Text = "设置";
            this.tabControl1.ApplyHeaderlessStyle = true;
            this.tabControl1.SwitchTabPage(0);
        }

        private void SystemSetting_Load(object sender, EventArgs e)
        {
            _tabMapping = new Dictionary<Button, int>() {
                { this.baseInfoBtn, 0 },
                { this.ecgSettingBtn, 1 },
                { this.bpSettingBtn, 2},
                { this.reportSettingBtn, 3 }
            };

            SuspendLayout();
            try
            {
                initUI();

                ResumeLayout(false);
                this.PerformLayout();

                this.baseInfo.OnBaseInfoSaved += () => OnSettingChanged(SettingChangeType.BaseInfoChanged);
                this.baseInfoBtn.PerformClick();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
            }
        }

        private void SystemSetting_Shown(object sender, EventArgs e)
        {
            ResumeLayout(true);
        }


        private void initUI()
        {
            //左侧菜单按钮
            if (CommonValues.SystemType == "ecg")
            {
                this.bpSettingBtn.Visible = false;
                this.tlpMenu.RowStyles[2].Height = 0;
            }
            else if (CommonValues.SystemType == "bp")
            {
                this.ecgSettingBtn.Visible = false;
                this.tlpMenu.RowStyles[1].Height = 0;
            }

            List<Button> menuButtons = CommonFuns.RetriveSubControls<Button>(this.tlpMenu);
            for (int i = 0; i < this.tlpMenu.RowCount; i++)
            {
                Button cellButton = menuButtons.FirstOrDefault(o => tlpMenu.GetCellPosition(o) != null && tlpMenu.GetCellPosition(o).Row == i);
                //Control ctl = this.tlpMenu.GetControlFromPosition(0, i);
                if (cellButton == null) continue;

                int idx = Convert.ToInt32(i);
                if (!cellButton.Visible) { this.tlpMenu.RowStyles[i].Height = 0; }

                cellButton.Click += (o, e) =>
                {
                    try
                    {
                        this.tabControl1.SelectedIndex = idx;
                        cellButton.BackColor = SystemColors.GradientInactiveCaption;
                        cellButton.ForeColor = Color.FromArgb(74, 122, 196);
                        foreach (Button btn in menuButtons)
                        {
                            if (btn != cellButton)
                            {
                                btn.BackColor = Color.WhiteSmoke;
                                btn.ForeColor = Color.Black;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex, "系统设置");
                    }
                };
            }

        }

        //根据左侧导航按钮显示相应的设置界面
        private void resetTabPages(List<Button> buttons, Button curBtn)
        {
            //foreach (Button btn in buttons)
            //{
            //    if (btn == curBtn)
            //        btn.BaseColor = Color.LimeGreen;
            //    else
            //        btn.BaseColor = Color.FromArgb(30, 60, 90);
            //}

            this.tabControl1.SelectedIndex = _tabMapping[curBtn];
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; //双缓存
                return cp;
            }
        }
    }
}
