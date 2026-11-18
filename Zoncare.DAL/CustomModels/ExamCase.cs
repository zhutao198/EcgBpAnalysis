using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Zoncare.Common;

namespace Zoncare.DAL
{
    public partial class examCase
    {
        [NotMapped, JsonIgnore]
        public int DisplayIndex { get; set; }

        /// <summary>
        /// 是否被选中(用于批量操作)
        /// </summary>
        [NotMapped, JsonIgnore]
        public bool IsSelected { get; set; }

        /// <summary>
        /// 数据或流程处理的状态
        /// </summary>
        [NotMapped, JsonIgnore]
        public CaseProcessState ProcessState { get; set; } = new CaseProcessState();

        /// <summary>
        /// 是否正在上传或分析
        /// </summary>
        [NotMapped, JsonIgnore]
        public bool Processing { get; set; }

        public static readonly PersistDictionary<string> ColMap = new PersistDictionary<string>(true)
        {
            {"caseNo", "病历号"},
            {"name", "姓名"},
            {"idcard", "身份证号"},
            {"birthday", "出生日期"},
            {"patientNo", "患者编号"},
            {"bedNo", "病区/床号"},
            {"height", "身高(cm)"},
            {"weight", "体重(kg)"},
            {"applyDept", "申请科室"},
            {"applyDoctor", "申请医生"},
            {"applyTime", "申请时间"},
            {"checkDept", "检查科室"},
            {"checkDoctor", "检查医生"},
            {"diagnoseDoctor", "诊断医生"},
            {"diagnoseTime", "诊断时间"},
            {"auditDoctor", "审核医生"},
            {"auditTime", "审核时间"},
            {"appointmentTime", "检查日期"},
            {"recordTime", "记录时间"},
            {"uploadTime", "上传时间"},
            {"medication", "治疗用药"},
            {"clinicalDiagnosis", "临床诊断"},
            {"holterReportConclusion", "报告结论"},
            {"bpReportConclusion", "报告结论"},
            {"remark", "备注"},
            {"idTypeName", "ID类型"},
            {"sexName", "性别"},
            {"ageName", "年龄"},
            {"phone", "手机号"},
            {"checkItemName", "检查项目"},
            {"dataTypeName", "数据信息"},
            {"hasPacemakerName", "有无起搏器"},
            {"statusName", "状态"},
            {"filePath", "文件目录"},
            {"cancelReason", "删除原因"}
        };

        /// <summary>
        /// 重置当前病例的处理进度
        /// </summary>
        public void ResetProcessState()
        {
            Processing = false;
            ProcessState = new CaseProcessState();
        }

        /// <summary>
        /// 获取病例列表中显示的字段信息
        /// </summary>
        /// <returns></returns>
        public static List<DataField> GetDisplayDataFields()
        {
            List<DataField> fieldList = new List<DataField>();
            tableColumns colsInfo = ZoncareEntities.Instance.TableColumns.FirstOrDefault(o => o.userId == user.Loginer.id);
            if (colsInfo != null)
            {
                JArray arr = (JArray)JsonConvert.DeserializeObject(colsInfo.column, SysConstants.JsonSettings);
                foreach (JToken item in arr)
                {
                    DataField fld = item.ToObject<DataField>();
                    fld.DisplayName = ColMap[fld.Field];
                    if (!fld.Hide) fieldList.Add(fld);
                }
            }
            else
            {
                string[] defaultColumns = getDefaultColumns();
                foreach (string colName in defaultColumns)
                {
                    DataField fld = new DataField();
                    fld.Field = colName;
                    fld.DisplayName = ColMap[fld.Field];
                    fieldList.Add(fld);
                }
            }

            return fieldList;
        }

        private static string[] getDefaultColumns()
        {
            if (CommonValues.SystemType == "ecg")
                return new string[] { "statusName", "caseNo", "name", "checkItemName", "recordTime", "uploadTime", "holterReportConclusion", "dataTypeName" };
            else if (CommonValues.SystemType == "bp")
                return new string[] { "statusName", "caseNo", "name", "checkItemName", "recordTime", "uploadTime", "bpReportConclusion", "dataTypeName" };
            else
                return new string[] { "statusName", "caseNo", "name", "checkItemName", "recordTime", "uploadTime", "holterReportConclusion", "dataTypeName" };
        }
    }
}
