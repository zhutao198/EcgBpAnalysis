using AnalysisApp.Commons;
using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace AnalysisApp
{
    public partial class PatientInfo : BaseUserControl
    {
        private examCase _caseInfo;
        private DateTime _lastValidBirthDate;

        public PatientInfo(examCase caseInfo)
        {
            InitializeComponent();
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            if (resetUI)
            {
                this.dtpBirthday.CustomFormat = " ";
            }
            loadData();
        }

        private void PatientInfo_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsUiInitialized)
                {
                    init();
                };
                //initData();

                Console.WriteLine("患者信息界面已加载");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "患者信息");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string patientName = this.tbxName.Text.Trim();
            if (string.IsNullOrEmpty(patientName))
            {
                ScreenCenterTip.Show("请输入患者姓名");
                this.tbxName.Focus();
                return;
            }

            if (!PubFuns.IsValidPhone(this.tbxPhone.Text))
            {
                ScreenCenterTip.Show("手机号格式不正确，请重输入");
                this.tbxPhone.Focus();
                return;
            }

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                //ZoncareEntities.Instance.Attach(_caseInfo);
                List<TextBoxEx> textInputs = CommonFuns.RetriveSubControls<TextBoxEx>(this.tlpPatient);
                Type dataType = _caseInfo.GetType();
                foreach (PropertyInfo prop in dataType.GetProperties())
                {
                    foreach (TextBoxEx tbx in textInputs)
                    {
                        string tbxName = tbx.Name.Substring(3).ToLower();
                        if (tbxName == prop.Name.ToLower())
                        {
                            PubFuns.SetPropertyValue(_caseInfo, prop, tbx.Text.Trim());
                            break;
                        }
                    }
                }

                _caseInfo.remark = this.tbxRemark.Text;
                _caseInfo.medication = this.tbxMedication.Text;
                //_caseInfo.holterReportConclusion = this.tbxHolterReportConclusion.Text;

                _caseInfo.sex = this.cbxSex.SelectedValue.ToString();
                _caseInfo.ageUnit = this.cbxAgeUnit.SelectedValue.ToString();
                _caseInfo.applyDept = this.tbxApplyDept.Text.Trim();
                _caseInfo.checkDept = this.cbxCheckDept.SelectedValue?.ToString();

                if (_caseInfo.age.HasValue) _caseInfo.ageDays = CommonFuns.GetAgeDays(_caseInfo.age.Value, _caseInfo.ageUnit);

                if (this.dtpBirthday.Checked && this.dtpBirthday.CustomFormat != "")
                    _caseInfo.birthday = this.dtpBirthday.Value.ToString(SysConstants.DATE_FORMAT);
                else
                    _caseInfo.birthday = string.Empty;

                if (this.dtpRecordTime.Checked && this.dtpRecordTime.CustomFormat != " ")
                    _caseInfo.recordTime = this.dtpRecordTime.Value;
                else
                    _caseInfo.recordTime = null;

                //_caseInfo.appointmentTime = this.dtpCheckTime.Value;

                _caseInfo.updateTime = DateTime.Now;
                _caseInfo.updateBy = AnaValues.CurrentUser.userName;

                //ZoncareEntities.Instance.Entry(_caseInfo).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                //ZoncareEntities.Instance.Entry(dbPatient).CurrentValues.SetValues(patient);
                bool updateRecordTime = (_caseInfo.recordTime != AnaValues.CurrentCase.recordTime);
                bool result = AppCode.SaveCaseInfo(null, _caseInfo, false, DateTime.Now, null, updateRecordTime, AnaValues.CurrentCase.recordTime);
                if (result == true)
                {
                    AnaValues.CurrentCase = PubFuns.CloneObject(_caseInfo);
                    if (updateRecordTime)
                    {
                        if (File.Exists(Path.Combine(AnaValues.RootPath, "holter.info")))
                        {
                            AnaValues.holterConfig = HolterConfig.LoadFromFile(Path.Combine(AnaValues.RootPath, "holter.info"));
                            AnaCode.LoadStatisticsData();
                        }
                        AnaValues.DataSourceState = 2;
                    }
                    ScreenCenterTip.Show("保存成功");
                }
                else
                {
                    CommonFuns.PromptError("保存失败");
                }

                OperateLog.logParam("修改患者信息", _caseInfo, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
            }
            catch (Exception ex)
            {
                CommonFuns.PromptError("保存失败");
                Logger.CreateLog(ex, "患者信息");
            }
        }

        private void cbxAgeUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ("Y".Equals(this.cbxAgeUnit.SelectedValue))
            {
                this.tbxAge.MinValue = 0;
                this.tbxAge.MaxValue = 220;
            }
            else if ("M".Equals(this.cbxAgeUnit.SelectedValue))
            {
                this.tbxAge.MinValue = 1;
                this.tbxAge.MaxValue = 12;
            }
            else if ("D".Equals(this.cbxAgeUnit.SelectedValue))
            {
                this.tbxAge.MinValue = 1;
                this.tbxAge.MaxValue = 366;
            }
            this.tbxAge.MaxLength = this.tbxAge.MaxValue.ToString().Length;
            SelectNextControl(this.tbxAge, true, true, true, true);
        }


        private void init()
        {
            _caseInfo = ZoncareEntities.Instance.ExamCases.FirstOrDefault(o => o.id == AnaValues.CurrentCase.id);
            InitUI();
        }

        public override void InitUI()
        {
            base.InitUI();

            try
            {
                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat];
                this.dtpBirthday.MaxDate = DateTime.Today;
                this.dtpBirthday.Checked = false;
                this.dtpBirthday.CustomFormat = " ";
                this.dtpBirthday.ValueChanged += (o, arg) => { this.dtpBirthday.CustomFormat = dateFormat; checkBirthDate(); };
                this.dtpBirthday.MouseUp += (o, arg) => { checkBirthDate(); };
                _lastValidBirthDate = this.dtpBirthday.Value;

                this.dtpCheckTime.CustomFormat = dateFormat + " HH:mm:ss";
                this.dtpCheckTime.ValueChanged += (o, arg) => { this.dtpCheckTime.CustomFormat = dateFormat + " HH:mm:ss"; };
                this.dtpRecordTime.CustomFormat = " ";
                this.dtpRecordTime.ValueChanged += (o, arg) => { this.dtpRecordTime.CustomFormat = dateFormat + " HH:mm:ss"; };

                CommonFuns.InitCommboBoxItems(this.cbxSex, SysConstants.Genders, true, "");
                CommonFuns.InitCommboBoxItems(this.cbxAgeUnit, SysConstants.AgeUnits);
                cbxAgeUnit_SelectedIndexChanged(null, null);

                //CommonFuns.InitCommboBoxItems(this.cbxApplyDept, new PersistDictionary<string>(), true, "");
                CommonFuns.InitCommboBoxItems(this.cbxCheckDept, new PersistDictionary<string>(), true, "");

                //LeadInfo defaultLeadInfo = AppCode.GetDefaultLeadInfo(12);
                //string[] leads = defaultLeadInfo.props;
                //ChannelSetting chanelSetting = defaultLeadInfo.channelSetting;
                //this.lblMainLead.Text = leads[chanelSetting.mainChannel];
                //this.lblSecondLead1.Text = leads[chanelSetting.secondChannel1];
                //this.lblSecondLead2.Text = leads[chanelSetting.secondChannel2];

                this.cbxAgeUnit.SelectedIndexChanged += cbxAgeUnit_SelectedIndexChanged;

                Console.WriteLine("患者信息界面已初始化完成");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "患者信息");
            }
        }

        private void loadData()
        {
            this.lblMainLead.Text = "--";
            this.lblSecondLead1.Text = "--";
            this.lblSecondLead2.Text = "--";
            _caseInfo = ZoncareEntities.Instance.ExamCases.FirstOrDefault(o => o.id == AnaValues.CurrentCase.id);
            if (_caseInfo != null)
            {
                if (_caseInfo.checkItem == 1 || _caseInfo.checkItem == 3)
                {
                    this.tlpEcgInfo.Visible = true;
                    HolterConfig conf = AnaValues.holterConfig;
                    if (conf != null)
                    {
                        if (conf.StartReanalysis)
                        {
                            LeadInfo defaultLeadInfo = AppCode.GetDefaultLeadInfo(conf.Lead);
                            string[] leads = defaultLeadInfo.props;
                            AnalysisSetting analysisSets = conf.AnalysisSetting;
                            this.lblMainLead.Text = leads[analysisSets.MainChannel];
                            this.lblSecondLead1.Text = leads[analysisSets.SecondChannel1];
                            this.lblSecondLead2.Text = leads[analysisSets.SecondChannel2];
                        }
                        this.lblLeadType.Text = conf.Lead + _langs["导联"];
                        this.lblDuration.Text = getDuration(_caseInfo.checkItem, conf);
                    }
                }
                else
                {
                    this.tlpEcgInfo.Visible = false;
                }

                loadCaseInfo(_caseInfo);
                this.btnSave.Enabled = true;
            }
            else
            {
                this.btnSave.Enabled = false;
            }
        }

        private string getDuration(int checkItem, HolterConfig conf)
        {
            if (checkItem == 2) return "";

            int total = conf.Total;
            int sampleRate = conf.SampleRate;

            int H = (int)(total / sampleRate / 3600d);
            int M = (int)((total / sampleRate - H * 3600) / 60d);
            int S = (int)((double)total / sampleRate - H * 3600 - M * 60);

            if (H > 0)
            {
            }
            else if (M > 0)
            {
                H = 0;
                M = (int)(total / sampleRate / 60d);
                S = (int)((double)total / sampleRate - M * 60);
            }
            else
            {
                H = 0;
                M = 0;
                S = (int)((double)total / sampleRate);
            }

            // 格式化为两位数
            string formattedH = H < 10 ? "0" + H : H.ToString();
            string formattedM = M < 10 ? "0" + M : M.ToString();
            string formattedS = S < 10 ? "0" + S : S.ToString();

            return $"{formattedH}:{formattedM}:{formattedS}";
        }

        private void loadCaseInfo(examCase caseInfo)
        {
            List<TextBoxEx> textInputs = CommonFuns.RetriveSubControls<TextBoxEx>(this.tlpPatient);
            Type dataType = caseInfo.GetType();
            foreach (PropertyInfo prop in dataType.GetProperties())
            {
                foreach (TextBoxEx tbx in textInputs)
                {
                    string tbxName = tbx.Name.Substring(3).ToLower();
                    if (tbxName == prop.Name.ToLower())
                    {
                        tbx.Text = prop.GetValue(caseInfo)?.ToString();
                        break;
                    }
                }
            }

            this.tbxRemark.Text = caseInfo.remark;
            this.tbxMedication.Text = caseInfo.medication;
            this.tbxHolterReportConclusion.Text = caseInfo.holterReportConclusion;

            this.cbxSex.SelectedValue = caseInfo.sex;
            this.cbxAgeUnit.SelectedValue = caseInfo.ageUnit;
            //this.cbxApplyDept.SelectedValue = caseInfo.applyDept ?? "";
            this.cbxCheckDept.SelectedValue = caseInfo.checkDept ?? "";

            if (caseInfo.checkItem == 3 || caseInfo.checkItem == 2)
            {
                //处理二合一、血压报告结论
            }

            #region 日期时间
            string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat];
            if (!string.IsNullOrEmpty(caseInfo.birthday)) //生日
            {
                this.dtpBirthday.CustomFormat = dateFormat;
                this.dtpBirthday.Value = DateTime.ParseExact(caseInfo.birthday, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
            }
            else
            {
                this.dtpBirthday.CustomFormat = " "; 
            }

            if (caseInfo.appointmentTime.HasValue) //检查时间
            {
                this.dtpCheckTime.CustomFormat = dateFormat + " HH:mm:ss";
                this.dtpCheckTime.Value = caseInfo.appointmentTime.Value;
            }
            else
            {
                this.dtpCheckTime.CustomFormat = " ";
            }

            if (caseInfo.recordTime.HasValue) //记录时间
            {
                this.dtpRecordTime.CustomFormat = dateFormat + " HH:mm:ss";
                this.dtpRecordTime.Value = caseInfo.recordTime.Value;
            }
            else
            {
                this.dtpRecordTime.CustomFormat = " ";
            }

            if (caseInfo.uploadTime.HasValue) //上传时间
            {
                this.dtpUploadTime.CustomFormat = dateFormat + " HH:mm:ss";
                this.dtpUploadTime.Value = caseInfo.uploadTime.Value;
            }
            else
            {
                this.dtpUploadTime.CustomFormat = " ";
            }
            #endregion
        }

        //根据生日确定年龄
        private void checkBirthDate()
        {
            if (this.dtpBirthday.Checked)
            {
                this.tbxAge.Enabled = this.cbxAgeUnit.Enabled = false;

                DateTime dtpValue = this.dtpBirthday.Value;
                if (!string.IsNullOrWhiteSpace(this.dtpBirthday.CustomFormat))
                {
                    if (dtpValue != _lastValidBirthDate)
                    {
                        (int age, string ageUnit) = CommonFuns.ResolveAgeFromBirthDay(dtpValue);
                        this.tbxAge.Text = age.ToString();
                        this.cbxAgeUnit.SelectedValue = ageUnit;
                    }
                }
                _lastValidBirthDate = this.dtpBirthday.Value;
            }
            else
            {
                this.tbxAge.Enabled = true;
                this.cbxAgeUnit.Enabled = true;
            }
        }

    }
}
