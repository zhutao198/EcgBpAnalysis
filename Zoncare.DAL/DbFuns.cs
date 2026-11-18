using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Zoncare.Common;

namespace Zoncare.DAL
{
    public class DbFuns
    {
        private static readonly int UserDefineCasePId = 274;

        /// <summary>
        /// 判断是否已存在相同的用户自定义病例
        /// </summary>
        /// <param name="lcid"></param>
        /// <param name="explain"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool ExistsUserDefineCase(int lcid, string explain, int id)
        {
            if (SysConstants.Locales.ContainsKey(lcid))
            {
                try
                {
                    //ZoncareEntities.Instance.Query<ListItem>().FromSql(""); //可自定义一个类型来接收查询的结果
                    string querySql;
                    if (id >= 0)
                    {
                        querySql = $"select [id],[pid],[number],[explain],[pinyin] from [{SysConstants.Locales[lcid]}] where pid={UserDefineCasePId} and explain='{explain}' and id<>{id}";
                    }
                    else
                    {
                        querySql = $"select [id],[pid],[number],[explain],[pinyin] from [{SysConstants.Locales[lcid]}] where pid={UserDefineCasePId} and explain='{explain}'";
                    }

                    DataTable dt = ZoncareEntities.RawQuery(querySql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        return true;
                    }
                }
                catch
                {

                }
            }
            return false;
        }

        /// <summary>
        /// 新增用户自定义病例
        /// </summary>
        /// <param name="lcid"></param>
        /// <param name="explain"></param>
        /// <param name="pinyin"></param>
        /// <returns></returns>
        public static bool AddUserDefineCase(int lcid, string explain, string pinyin)
        {
            if (SysConstants.Locales.ContainsKey(lcid))
            {
                try
                {
                    int number = 1;

                    string querySql = $"select [id],[pid],[number],[explain],[pinyin] from [{SysConstants.Locales[lcid]}] where pid={UserDefineCasePId} order by [number] desc limit 1";
                    DataTable dt = ZoncareEntities.RawQuery(querySql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        number = Convert.ToInt32(dt.Rows[0]["number"]) + 1;
                    }

                    string insertSql;
                    if (!string.IsNullOrEmpty(pinyin))
                    {
                        insertSql = $"insert into [{SysConstants.Locales[lcid]}] (pid,number,explain,pinyin) values ({UserDefineCasePId},{number},'{explain}','{pinyin}')";
                    }
                    else
                    {
                        insertSql = $"insert into [{SysConstants.Locales[lcid]}] (pid,number,explain) values ({UserDefineCasePId},{number},'{explain}')";
                    }
                    int cnt = ZoncareEntities.Instance.Database.ExecuteSqlCommand(insertSql);
                    if (cnt > 0)
                    {
                        return true;
                    }
                }
                catch
                {

                }
            }
            return false;
        }

        /// <summary>
        /// 更新用户自定义病例
        /// </summary>
        /// <param name="lcid"></param>
        /// <param name="explain"></param>
        /// <param name="pinyin"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool UpdateUserDefineCase(int lcid, string explain, string pinyin, int id)
        {
            if (SysConstants.Locales.ContainsKey(lcid))
            {
                try
                {
                    string updateSql;
                    if (!string.IsNullOrEmpty(pinyin))
                    {
                        updateSql = $"update [{SysConstants.Locales[lcid]}] set explain='{explain}',pinyin='{pinyin}' where id = {id}";
                    }
                    else
                    {
                        updateSql = $"update [{SysConstants.Locales[lcid]}] set explain='{explain}' where id = {id}";
                    }

                    int cnt = ZoncareEntities.Instance.Database.ExecuteSqlCommand(updateSql);
                    if (cnt > 0)
                    {
                        return true;
                    }
                }
                catch
                {

                }
            }
            return false;
        }

        /// <summary>
        /// 删除用户自定义病例
        /// </summary>
        /// <param name="lcid"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool DeleteUserDefineCase(int lcid, int id)
        {
            if (SysConstants.Locales.ContainsKey(lcid))
            {
                try
                {
                    int cnt = ZoncareEntities.Instance.Database.ExecuteSqlCommand($"delete from [{SysConstants.Locales[lcid]}] where id= {id}");
                    if (cnt > 0)
                    {
                        return true;
                    }
                }
                catch
                {

                }
            }
            return false;
        }

        /// <summary>
        /// 批量添加血压数据信息
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="updateExpression"></param>
        public static void BatchAddBpDetails(ZoncareEntities dbEntities, List<bpDetail> bpDataList)
        {
            //var bulkConfig = new BulkConfig
            //{
            //    UnderlyingConnection = (conn)=>  dbEntities.Database.GetDbConnection(),
            //    UnderlyingTransaction = (trans)=> dbEntities.Database.CurrentTransaction.GetDbTransaction()
            //};
            //dbEntities.BulkInsert(bpDataList);
        }

        /// <summary>
        /// 批量更新血压数据信息
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="updateExpression"></param>
        public static void BatchUpdateBpDetails(int caseId, Expression<Func<bpDetail, bpDetail>> updateExpression)
        {
            //this.BulkUpdate();
            //ZoncareEntities.Instance.BpDetails.Where(a => a.caseId == 1).BatchUpdate(new bpDetail { Description = "Updated" });

            //ZoncareEntities.Instance.BpDetails.Where(d => d.caseId.Equals(caseId)).BatchUpdate(o => new bpDetail() { time = EF.Functions... });

            //原生SQL
            //ZoncareEntities.Instance.BpDetails.FromSql("");
            //ZoncareEntities.Instance.Database.ExecuteSqlCommand("");
        }


        public static IQueryable<T> ApplyDynamicSorting<T>(IQueryable<T> query, string sortColumn, string sortDirection) // "asc" 或 "desc"
        {
            // 构造排序字符串，例如 "Price asc" 或 "Name desc"
            string orderByString = $"{sortColumn} {sortDirection}";

            return query.OrderBy(orderByString);
        }
    }
}
