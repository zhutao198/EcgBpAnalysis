using AnalysisSystem.Base;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace SystemSettings
{
    public partial class UserDetailForm : BaseDialogForm
    {
        /// <summary>内置管理员账号，不允许修改角色</summary>
        private const int SUPER_ADMIN_ID = 1;

        /// <summary>密码占位符：编辑时用于表示"不修改密码"，与登录界面保持一致的约定</summary>
        private const string PWD_PLACEHOLDER = "********";

        private int? _userId;

        /// <summary>签名图片的Base64内容，null表示无签名</summary>
        private string _signBase64;

        public event Action<int> OnUserInfoSaved;

        public UserDetailForm(int? userId) : base("", "新增")
        {
            InitializeComponent();

            _userId = userId;
        }

        private void UserDetailForm_Load(object sender, EventArgs e)
        {
            if (_userId.HasValue)
            {
                SetTitle(_langs["编辑"]);
            }

            init();
            loadUserInfo();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //防止重复提交
            if (!this.btnSave.Enabled) return;
            if (!verifyInput()) return;

            this.btnSave.Enabled = false;
            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                user userInfo = null;
                if (!_userId.HasValue)
                {
                    userInfo = getUserInfo(null);
                    dbEntities.User.Add(userInfo);
                }
                else
                {
                    user originalUser = dbEntities.User.FirstOrDefault(u => u.id == _userId.Value);
                    if (originalUser == null || originalUser.delFlag == 1)
                    {
                        //记录已被删除，直接关闭
                        this.DialogResult = DialogResult.Cancel;
                        this.Close();
                        return;
                    }
                    userInfo = getUserInfo(originalUser);
                }

                dbEntities.SaveChanges();
                OperateLog.logParam((_userId.HasValue ? "更新" : "新增") + "用户信息", userInfo, sw.ElapsedMilliseconds, user.Loginer?.userName);

                OnUserInfoSaved?.Invoke(userInfo.id);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
                CommonFuns.PromptError("操作失败");
            }
            finally
            {
                if (!this.IsDisposed) this.btnSave.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                dlg.Multiselect = false;

                if (dlg.ShowDialog(this.FindForm()) != DialogResult.OK) return;

                try
                {
                    byte[] bytes = File.ReadAllBytes(dlg.FileName);
                    //以内存流方式加载，避免Image.FromFile长期占用文件
                    showSignImage(bytes);
                    //图片无法解析时不保存，避免存入无法显示的Base64
                    if (this.picSign.Image != null) _signBase64 = Convert.ToBase64String(bytes);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "系统设置");
                    CommonFuns.PromptError("操作失败");
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            showSignImage((byte[])null);
            _signBase64 = null;
        }


        private void init()
        {
            CommonFuns.InitCommboBoxItems(this.cbxRole, SysConstants.RoleTypes, true, -1, defaultValue: 1);

            //编辑时密码框显示占位符，用户开始输入时自动清空占位符
            this.tbxPwd.KeyPress += (sender, e) =>
            {
                if (!char.IsControl(e.KeyChar) && this.tbxPwd.Text == PWD_PLACEHOLDER)
                {
                    this.tbxPwd.Text = string.Empty;
                }
            };

            //内置管理员账号不允许修改角色，避免误操作后失去管理入口
            if (_userId.HasValue && _userId.Value == SUPER_ADMIN_ID)
            {
                this.cbxRole.Enabled = false;
            }
        }

        private void loadUserInfo()
        {
            if (!_userId.HasValue) return;

            user userInfo = ZoncareEntities.Instance.User.AsNoTracking().FirstOrDefault(u => u.id == _userId.Value);
            if (userInfo == null) return;

            this.tbxUserName.Text = userInfo.userName;
            this.tbxEmpNo.Text = userInfo.empNo;
            this.tbxRealName.Text = userInfo.realname;
            this.cbxRole.SelectedValue = userInfo.roleId;
            this.tbxDepartment.Text = userInfo.deptName;
            this.tbxEmail.Text = userInfo.email;
            this.tbxTel.Text = userInfo.phone;

            this._signBase64 = userInfo.sign;
            showSignImage(userInfo.sign);

            //编辑时不回显密码密文，以占位符表示"不修改密码"
            this.tbxPwd.Text = PWD_PLACEHOLDER;
        }

        /// <summary>
        /// 密码框内容仍为占位符，表示本次不修改密码
        /// </summary>
        private bool isPasswordUnchanged()
        {
            return _userId.HasValue && this.tbxPwd.Text.Trim() == PWD_PLACEHOLDER;
        }

        /// <summary>
        /// 获取密码框中用户实际输入的内容(去除编辑时的占位符)
        /// </summary>
        private string getInputPassword()
        {
            string text = this.tbxPwd.Text.Trim();
            if (_userId.HasValue && text.StartsWith(PWD_PLACEHOLDER))
            {
                //兼容占位符之后直接输入或粘贴的情形
                return text.Substring(PWD_PLACEHOLDER.Length).Trim();
            }
            return text;
        }

        private bool verifyInput()
        {
            string userName = this.tbxUserName.Text.Trim();
            if (string.IsNullOrEmpty(userName))
            {
                ScreenCenterTip.Show("用户名为必填项");
                this.tbxUserName.Focus();
                return false;
            }
            if (userName.Length < 4 || userName.Length > 32)
            {
                ScreenCenterTip.Show("用户名长度为4-32个字符");
                this.tbxUserName.Focus();
                return false;
            }
            if (isUserNameExists(userName))
            {
                ScreenCenterTip.Show("用户名已存在");
                this.tbxUserName.Focus();
                return false;
            }

            string pwd = getInputPassword();
            //编辑时密码框为占位符表示不修改密码，输入了新密码才做校验
            if (!isPasswordUnchanged())
            {
                if (string.IsNullOrEmpty(pwd))
                {
                    ScreenCenterTip.Show("密码为必填项");
                    this.tbxPwd.Focus();
                    return false;
                }
                if (pwd.Length < 8 || pwd.Length > 32)
                {
                    ScreenCenterTip.Show("密码长度为8到32个字符");
                    this.tbxPwd.Focus();
                    return false;
                }
                bool hasLetter = Regex.IsMatch(pwd, "[A-Za-z]");
                bool hasDigit = Regex.IsMatch(pwd, @"\d");
                if (!hasLetter || !hasDigit)
                {
                    ScreenCenterTip.Show("密码必须包含字母数字两种及以上的字符");
                    this.tbxPwd.Focus();
                    return false;
                }
            }

            string realName = this.tbxRealName.Text.Trim();
            if (string.IsNullOrEmpty(realName))
            {
                ScreenCenterTip.Show("姓名为必填项");
                this.tbxRealName.Focus();
                return false;
            }

            if (getSelectedRoleId() <= 0)
            {
                ScreenCenterTip.Show("角色为必填项");
                this.cbxRole.Focus();
                return false;
            }

            if (!PubFuns.IsValidEmail(this.tbxEmail.Text.Trim()))
            {
                ScreenCenterTip.Show("电子邮件格式不正确");
                this.tbxEmail.Focus();
                return false;
            }

            if (!PubFuns.IsValidPhone(this.tbxTel.Text.Trim()))
            {
                ScreenCenterTip.Show("电话格式不正确");
                this.tbxTel.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取下拉框选中的角色id，未选择时返回-1
        /// </summary>
        private int getSelectedRoleId()
        {
            if (this.cbxRole.SelectedValue == null) return -1;

            int roleId;
            return int.TryParse(this.cbxRole.SelectedValue.ToString(), out roleId) ? roleId : -1;
        }

        /// <summary>
        /// 判断用户名是否已被其他用户占用
        /// </summary>
        private bool isUserNameExists(string userName)
        {
            int excludeId = _userId ?? 0;
            return ZoncareEntities.Instance.User.AsNoTracking().Any(u =>
                u.userName == userName
                && (u.delFlag == null || u.delFlag == 0)
                && u.id != excludeId);
        }

        private user getUserInfo(user source)
        {
            user userInfo = source ?? new user();

            userInfo.userName = this.tbxUserName.Text.Trim();
            //密码框仍为占位符表示不修改密码，此时保留原有密码；否则加密保存新密码
            if (!isPasswordUnchanged())
            {
                userInfo.password = AESEncrypt.Encrypt(getInputPassword());
            }
            userInfo.empNo = this.tbxEmpNo.Text.Trim();
            userInfo.realname = this.tbxRealName.Text.Trim();
            userInfo.email = this.tbxEmail.Text.Trim();
            userInfo.phone = this.tbxTel.Text.Trim();
            userInfo.roleId = getSelectedRoleId();
            userInfo.deptName = this.tbxDepartment.Text.Trim();
            userInfo.sign = string.IsNullOrEmpty(_signBase64) ? null : _signBase64;
            userInfo.delFlag = userInfo.delFlag ?? 0;

            DateTime nowTime = DateTime.Now;
            userInfo.updateTime = nowTime;
            userInfo.updateBy = user.Loginer?.userName;
            if (userInfo.createTime == null) userInfo.createTime = nowTime;
            if (string.IsNullOrEmpty(userInfo.createBy)) userInfo.createBy = user.Loginer?.userName;

            return userInfo;
        }

        /// <summary>
        /// 显示Base64格式的签名图片
        /// </summary>
        private void showSignImage(string base64)
        {
            byte[] bytes = null;
            if (!string.IsNullOrEmpty(base64))
            {
                try
                {
                    //兼容带"data:image/xxx;base64,"前缀的内容
                    string cleanBase64 = base64;
                    int index = cleanBase64.IndexOf(',');
                    if (index >= 0) cleanBase64 = cleanBase64.Substring(index + 1);

                    bytes = Convert.FromBase64String(cleanBase64);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "系统设置");
                }
            }

            showSignImage(bytes);
        }

        /// <summary>
        /// 显示指定字节内容的签名图片，bytes为空时清空图片
        /// </summary>
        private void showSignImage(byte[] bytes)
        {
            Image oldImage = this.picSign.Image;
            this.picSign.Image = null;
            oldImage?.Dispose();

            if (bytes == null || bytes.Length == 0) return;

            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    //复制为独立位图，避免图片依赖已释放的流
                    using (Image img = Image.FromStream(ms, true, true))
                    {
                        this.picSign.Image = new Bitmap(img);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
                this.picSign.Image = null;
            }
        }
    }
}
