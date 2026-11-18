using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using Zoncare.Common;
using Zoncare.DAL;

namespace EcgBpAnalysis.FuncComponent
{
    public partial class CancelForm : BaseDialogForm
    {
        private List<int> _caseIds;
        public Action OnCaseCancelled;

        public CancelForm(List<int> caseIds) : base("", "删除原因")
        {
            InitializeComponent();

            _caseIds = caseIds;
        }

        private void CancelForm_Load(object sender, EventArgs e)
        {
            this.tbxReason.Focus();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                List<examCase> cases = _MyEntities.ExamCases.Where(o => _caseIds.Contains(o.id)).ToList();
                foreach(examCase caseInfo in cases)
                {
                    caseInfo.delFlag = 1;
                    caseInfo.cancelReason = this.tbxReason.Text.Trim();
                }
                _MyEntities.SaveChanges();
                OnCaseCancelled?.Invoke();

                object para = new { caseId = _caseIds };
                OperateLog.logParam("病例删除", para, 0, user.Loginer.userName);
                Close();
            }
            catch (Exception ex)
            {
                CommonFuns.PromptError("操作失败");
                Logger.CreateLog(ex, "病例管理");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
