using AnalysisApp.Commons;
using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component.Controls;
using Zoncare.Utilities;

namespace AnalysisApp
{
    public partial class LeadChangePopup : BaseDialogForm
    {
        private bool _isInitialized = false;
        private List<string> _originalLimbLeads;
        public bool LimbLeadChanged { get; private set; }
        public List<string> Limbleads { get; private set; }

        public LeadChangePopup() : base("", "导联纠错")
        {
            InitializeComponent();
        }

        private void AddSTEventForm_Load(object sender, EventArgs e)
        {
            LimbLeadChanged = false;
            _originalLimbLeads = AnaValues.holterConfig.LeadConfig.LfLatest;

            if (!_isInitialized)
            {
                init();
                _isInitialized = true;
            }

            loadData();
        }

        private void LeadChangePopup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK) return;

            e.Cancel = true;
            //btnReset_Click(sender, e);
            this.Hide();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            LimbLeadChanged = false;
            var defaultLF = AnaValues.holterConfig.LeadConfig.LfOrg;
            this.lblLimbRight.DisplayText = defaultLF[0];
            this.lblLimbLeft.DisplayText = defaultLF[1];
            this.lblLimbFoot.DisplayText = defaultLF[2];

            this.lblLimbRight.CircleColor = Color.FromArgb(74, 122, 196);
            this.lblLimbLeft.CircleColor = Color.FromArgb(74, 122, 196);
            this.lblLimbFoot.CircleColor = Color.FromArgb(74, 122, 196);
            this.limbLeadContainer.Refresh();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Limbleads = new List<string>()
            {
                this.lblLimbRight.DisplayText,
                this.lblLimbLeft.DisplayText,
                this.lblLimbFoot.DisplayText
            };

            if (Limbleads[0] != _originalLimbLeads[0] || Limbleads[1] != _originalLimbLeads[1])
            {
                LimbLeadChanged = true;
            }
            DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void init()
        {
            initUI();

            this.picChangeRL.Click += (o, args) =>
            {
                exchangeLeads(this.lblLimbRight, this.lblLimbLeft);
            };
            this.picChangeLF.Click += (o, args) =>
            {
                exchangeLeads(this.lblLimbLeft, this.lblLimbFoot);
            };
            this.picChangeRF.Click += (o, args) =>
            {
                exchangeLeads(this.lblLimbFoot, this.lblLimbRight);
            };
        }

        private void initUI()
        {
            this.btnSave.DialogResult = DialogResult.None; //让按钮设置的DialogResult为OK时，窗体不自动关闭

            this.limbLeadContainer.Left = (this.pnlLimb.Width - this.limbLeadContainer.Width) / 2;
            string limbImagePath = PubFuns.GetAbsolutePath("images\\leadDiagram\\limb-lead.png");
            if (File.Exists(limbImagePath))
            {
                this.limbLeadContainer.BackgroundImage = CommonFuns.GetImageFromFile(limbImagePath, true);
            }
        }

        private void loadData()
        {
            this.lblLimbRight.CircleColor = Color.FromArgb(74, 122, 196);
            this.lblLimbLeft.CircleColor = Color.FromArgb(74, 122, 196);
            this.lblLimbFoot.CircleColor = Color.FromArgb(74, 122, 196);

            Limbleads = AnaValues.holterConfig.LeadConfig.LfLatest;
            this.lblLimbRight.DisplayText = Limbleads[0];
            this.lblLimbLeft.DisplayText = Limbleads[1];
            this.lblLimbFoot.DisplayText = Limbleads[2];
        }

        private void exchangeLeads(RoundLabel leadLabel1, RoundLabel leadLabel2)
        {
            string limb1 = leadLabel1.DisplayText;
            leadLabel1.DisplayText = leadLabel2.DisplayText;
            leadLabel2.DisplayText = limb1;
            leadLabel1.CircleColor = Color.FromArgb(239, 130, 12);
            leadLabel2.CircleColor = Color.FromArgb(239, 130, 12);

            leadLabel1.Refresh();
            leadLabel2.Refresh();
        }

    }
}
