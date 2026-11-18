using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Zoncare.Common;
using Zoncare.Utilities;

namespace Zoncare.DAL
{
    public class ZqQuery
    {
        /// <summary>
        /// 根据基础数据字典类别获取相关词条列表
        /// </summary>
        /// <param name="dictCode">字典类别代码</param>
        /// <returns></returns>
        public static List<basedict> GetBaseDictionary(string dictCode)
        {
            List<basedict> lstDetail = null;
            var entries = ZoncareEntities.Instance.BaseDict.Where(d => d.category == dictCode);
            if (entries.Any())
            {
                lstDetail = entries.ToList();
            }
            return lstDetail;
        }

        /// <summary>
        /// 根据病历编辑字典类别获取相关词条列表
        /// </summary>
        /// <param name="lcid">当前语言对应的区域识别码</param>
        /// <returns></returns>
        public static List<caseEntry> GetCaseDictionary(int lcid)
        {
            List<caseEntry> lstDetail = null;

            if (SysConstants.Locales.ContainsKey(lcid))
            {
                try
                {
                    string querySql = $"select [id],[pid],[number],[explain],[pinyin] from [{SysConstants.Locales[lcid]}]";
                    DataTable dt = ZoncareEntities.RawQuery(querySql);
                    if (!PubFuns.DTIsNullOrEmpty(dt))
                    {
                        lstDetail = new List<caseEntry>();
                        foreach (DataRow dr in dt.Rows)
                        {
                            caseEntry entry = new caseEntry();
                            PubFuns.InstantiateModel<caseEntry>(dr, entry);
                            lstDetail.Add(entry);
                        }
                    }
                }
                catch
                {

                }
            }
            return lstDetail;
        }

        public static List<KeyValuePara> GetDiagnoseEntries(int lcid)
        {
            List<KeyValuePara> lstEntry = null;
            if (SysConstants.Locales.ContainsKey(lcid))
            {
                try
                {
                    string abbr = SysConstants.Locales[lcid];
                    if (lcid == 2052) abbr = "cn";

                    string querySql = $"select [stat] as Key, [{abbr}] as Value from [diag]";
                    DataTable dt = ZoncareEntities.RawQuery(querySql);
                    if (!PubFuns.DTIsNullOrEmpty(dt))
                    {
                        lstEntry = new List<KeyValuePara>();
                        foreach (DataRow dr in dt.Rows)
                        {
                            KeyValuePara entry = new KeyValuePara();
                            PubFuns.InstantiateModel<KeyValuePara>(dr, entry);
                            lstEntry.Add(entry);
                        }
                    }
                }
                catch (System.Exception ex)
                {

                }
            }

            return lstEntry;
        }

        public static user GetRemUser()
        {
            user remUser = null;
            try
            {
                string querySql = "select id,user_name,[password],login_time,sign from [user] where rem_flag=1";
                DataTable dt = ZoncareEntities.RawQuery(querySql);
                if (!PubFuns.DTIsNullOrEmpty(dt))
                {
                    remUser = new user();
                    foreach (DataRow dr in dt.Rows)
                    {
                        remUser.id = Convert.ToInt32(dr["id"]);
                        remUser.userName = dr["user_name"].ToString();
                        remUser.password = dr["password"].ToString();
                        remUser.sign = dr["sign"].ToString();
                        if (dr["login_time"] != DBNull.Value)
                        {
                            remUser.loginTime = Convert.ToDateTime(dr["login_time"]);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {

            }

            return remUser;
        }

        public static systemSettingValues loadSettings(int userId)
        {
            systemSettingValues setValues = new systemSettingValues();
            string errMsg = "";

            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            try
            {
                sysSetting sysSettingInfo = dbEntities.SysSettings.AsNoTracking().Where(o => o.id == 1).FirstOrDefault();
                if (sysSettingInfo != null)
                {
                    string baseInfoStr = sysSettingInfo.baseInfo;
                    string ecgInfoStr = sysSettingInfo.ecgInfo ?? "";
                    string reportInfoStr = sysSettingInfo.reportInfo;
                    baseSettingValues baseSets= JsonConvert.DeserializeObject<baseSettingValues>(baseInfoStr);
                    //ecgSettingValues ecgSets = JsonConvert.DeserializeObject<ecgSettingValues>(ecgInfoStr);
                    reportSettingValues reportSets = JsonConvert.DeserializeObject<reportSettingValues>(reportInfoStr);
                    setValues.baseSets = baseSets;
                    setValues.reportSets = reportSets;
                }

                holterSetting holterSets = dbEntities.HolterSettings.AsNoTracking().Where(o => o.userId == userId).FirstOrDefault();
                if (holterSets != null)
                {
                    ecgSettingValues ecgSets = JsonConvert.DeserializeObject<ecgSettingValues>(holterSets.ecgSetting);
                    setValues.ecgSets = ecgSets;

                    bpDiagnoseSettingValues bpDiagnoseSets = JsonConvert.DeserializeObject<bpDiagnoseSettingValues>(holterSets.bpDiagnoseSetting);
                    setValues.bpSets.bpDiagnoseSets = bpDiagnoseSets;
                }
                else {
                    holterSetting newHolterSettings = new holterSetting();
                    newHolterSettings.userId = user.Loginer.id;
                    newHolterSettings.ecgSetting = JsonConvert.SerializeObject(new ecgSettingValues());
                    newHolterSettings.bpDiagnoseSetting = JsonConvert.SerializeObject(new bpDiagnoseSettingValues());
                    newHolterSettings.updateTime = DateTime.Now;
                    dbEntities.HolterSettings.Add(newHolterSettings);
                    dbEntities.SaveChanges();
                }
            }
            catch (DataException ex)
            {
                errMsg = ex.Message;
            }

            return setValues;
        }


    }


}
