using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace Zoncare.DAL
{
    public partial class user
    {
        private bool _isSelected;

        public event Action<user> OnSelectStateChanged;

        [NotMapped]
        public int DisplayIndex { get; set; }

        /// <summary>
        /// 是否被选中(用于批量操作)
        /// </summary>
        [NotMapped]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnSelectStateChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// 当前登录用户
        /// </summary>
        public static user Loginer { get; set; }

        /// <summary>
        /// 用户是否为超级管理员
        /// </summary>
        public bool IsSuperAdmin
        {
            //get { return (this.sign == 0); }
            get { return false; }
        }


        #region 公共方法
        /// <summary>
        /// 用户登录信息验证
        /// </summary>
        /// <param name="userName">登录用户名</param>
        /// <param name="pwd">用户密码</param>
        /// <param name="errMsg">返回的错误信息</param>
        /// <returns>是否成功登录</returns>
        public static bool Login(string userName, string pwd, bool rememberPwd, out string errMsg)
        {
            errMsg = "";
            bool result = false;
            Stopwatch watcher = Stopwatch.StartNew();
            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            try
            {
                List<user> userList = dbEntities.User.Where(o => o.userName == userName && o.password == pwd).ToList();
                if (userList.Count > 0)
                {
                    user foundUser = userList.Where(o => o.delFlag == 0 || o.delFlag == null).FirstOrDefault();
                    if (foundUser != null)
                    {
                        //获取权限
                        retrievePermissions(foundUser);

                        //foundUser.is_online = 1;
                        //foundUser.login_ip = PubFuns.GetLocalIP().ToString();
                        //foundUser.login_time = DateTime.Now;
                        //dbEntities.Entry(foundUser).State = System.Data.Entity.EntityState.Modified;
                        //dbEntities.SaveChanges();
                        user.Loginer = foundUser;
                        foundUser.loginTime = DateTime.Now;
                        foundUser.remFlag = (rememberPwd ? 1 : 0);

                        //将其它用户标记为不记住信息
                        if (rememberPwd)
                        {
                            List<user> otherUsers = dbEntities.User.Where(u => u.id != foundUser.id).ToList();
                            foreach (user entity in otherUsers)
                            {
                                dbEntities.Entry(entity).Property(e => e.remFlag).IsModified = true;
                                entity.remFlag = 0;
                            }
                        }

                        dbEntities.SaveChanges();

                        watcher.Stop();
                        string strJsonPara = $"username: \"{userName}\"";
                        OperateLog.logParam("用户登录", strJsonPara, "", watcher.ElapsedMilliseconds, userName, "");
                        result = true;
                    }
                    else
                    {
                        errMsg = "用户名或密码错误";
                    }
                }
                else
                {
                    errMsg = "用户名或密码错误";
                }
            }
            catch (DataException ex)
            {
                errMsg = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 用户退出系统
        /// </summary>
        public void Logout()
        {
            try
            {
                //this.last_login_time = DateTime.Now;
                //this.is_online = 0;
                //ZoncareEntities.Instance?.SaveChanges();
            }
            catch
            {

            }
        }
        #endregion

        /// <summary>
        /// 获取用户的所有权限
        /// </summary>
        /// <param name="logon">登录用户</param>
        private static void retrievePermissions(user logon)
        {

        }

    }
}
