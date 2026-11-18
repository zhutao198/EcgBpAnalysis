using AnalysisSystem.Base;
using AnalysisSystem.Base.softdog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SystemSettings;
using Zoncare.Common;
using Zoncare.Component.Controls;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace EcgAnalyseStart
{
    public partial class LoginForm : BaseDialogForm
    {
        private string _systemName, _logoPath;
        private bool _isMainFormShown = false;
        private string _remPwd = string.Empty;

        public LoginForm() : base(false, true)
        {
            InitializeComponent();
            this.MaximizedBounds = Screen.PrimaryScreen.WorkingArea;
            this.lblSysName.Text = "";

            this.tlpInput.SuspendLayout();
            this.tlpInput.Visible = false;
            this.tbxUserName.Visible = false;
            this.tbxPwd.Visible = false;
            this.btnLogin.Visible = false;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            PubFuns.AsyncDoWork(checkDBContext);
            PubFuns.AsyncDoWork(initDog);

            (string sysName, string logoPath) = CommonFuns.GetSystemName();
            _systemName = sysName;
            _logoPath = logoPath;
            CommonValues.SystemName = _systemName;

            initUI();
            this.button2.Click += (o, args) =>
            {
                SystemSetting frmSystemSetting = new SystemSetting();
                frmSystemSetting.Show();
            };
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!_isMainFormShown) Application.Exit();
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            this.tlpInput.Visible = true;
            this.tlpInput.ResumeLayout(true);

            this.BeginInvoke(new Action(() =>
            {
                this.picCurtain.BackColor = Color.FromArgb(204, 226, 248);
                this.picCurtain.Image = CommonFuns.GetImageFromFile(_logoPath, true);
            }));

            PubFuns.AsyncDoWork(checkSkiaGpuAcceleration);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string errMsg = string.Empty;
            try
            {
                if (AppCode.CheckUsbDogStatus(this) == false) return;

                if (string.IsNullOrEmpty(this.tbxUserName.Text.Trim()))
                {
                    errMsg = "请输入用户名";
                    this.tbxUserName.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(this.tbxPwd.Text.Trim()))
                {
                    errMsg = "请输入密码";
                    this.tbxPwd.Focus();
                    return;
                }

                Stopwatch watcher = Stopwatch.StartNew();
                while (watcher.ElapsedMilliseconds < 5000) //5秒内未能读取数据视为数据库连接失败
                {
                    System.Threading.Thread.Sleep(1);
                    if (ZoncareEntities.IsReadyDbContext) break;
                }
                if (!ZoncareEntities.IsReadyDbContext)
                {
                    errMsg = "登录失败, 请重新输入";
                    return;
                }

                string userName = this.tbxUserName.Text;
                string pwd = _remPwd;
                if ("******" != this.tbxPwd.Text || string.IsNullOrEmpty(_remPwd))
                {
                    pwd = AESEncrypt.Encrypt(this.tbxPwd.Text);
                }

                if (user.Login(userName, pwd, this.chkRemember.Checked, out errMsg))
                {
                    this.Hide();
                    MainForm frmMain = new MainForm();
                    frmMain.OnLogout += () =>
                    {
                        // 注销时，创建新的LoginForm实例并显示
                        LoginForm newLoginForm = new LoginForm();
                        newLoginForm.Show();
                    };
                    frmMain.Show();
                    _isMainFormShown = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
            finally
            {
                if (!string.IsNullOrEmpty(errMsg))
                {
                    CommonFuns.PromptError(this, errMsg, isWarning: true);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Form2 drawingForm = new Form2();
            //drawingForm.ShowDialog();

        }


        //初始化界面显示
        private void initUI()
        {
            this.lblSysName.Text = CommonValues.SystemName;

            this.tbxUserName.Visible = true;
            this.tbxPwd.Visible = true;
            this.btnLogin.Visible = true;
            this.btnLogin.Width = this.tbxUserName.Width;
            Application.DoEvents();
        }

        //延迟片刻后允许输入以便加载记住的用户信息
        private bool setInput()
        {
            this.InvokeIfRequired(() =>
            {
                this.tbxUserName.Enabled = true;
                this.tbxPwd.Enabled = true;
                this.btnLogin.Enabled = true;
            });
            return true;
        }

        //检查是否支持 Skia GPU渲染
        private void checkSkiaGpuAcceleration()
        {
            this.InvokeIfRequired(() =>
            {
                // 创建 SKGLControl 并确保它已加载
                var skglControl = new SafeSKGLControl();
                this.Controls.Add(skglControl);

                // 确保窗体显示后触发渲染
                skglControl.Refresh();
                CommonValues.IsGpuAvailable = skglControl.IsGpuAccelerated;
                //CommonFuns.PromptMessage("gpu可用性: " + CommonValues.IsGpuAvailable.ToString());
                Console.WriteLine("gpu可用性: " + CommonValues.IsGpuAvailable.ToString());
            });
        }

        //检查加密狗验证
        private void initDog()
        {
            if (CommonValues.SystemType == "bp") return;

            if (!CommonValues.SoftDogInitialized)
            {
                ZoncareDogHandler.Init(PubFuns.GetBaseDirectory() + "\\config\\status.dat", true, true);
            }
        }

        //激活数据库连接
        private void checkDBContext()
        {
            Stopwatch watcher = Stopwatch.StartNew();
            while (!ZoncareEntities.IsReadyDbConn)
            {
                if (watcher.ElapsedMilliseconds < 3000)
                    System.Threading.Thread.Sleep(1);
                else
                    break;
            }

            bool hasRememberedUser = false;
            if (ZoncareEntities.IsReadyDbConn)
            {
                _MyEntities = ZoncareEntities.Instance;

                //user remUser = _MyEntities.User.AsNoTracking().FirstOrDefault(u => u.remFlag == 1);
                user remUser = ZqQuery.GetRemUser();
                if (remUser != null)
                    hasRememberedUser = fillAccount(remUser);
            }
            if (!hasRememberedUser) setInput();
        }

        //加载记住的用户信息
        private bool fillAccount(user remUser)
        {
            bool result = false;
            if (remUser.loginTime != null)
            {
                if (DateTime.Now.Subtract(remUser.loginTime.Value).Days <= 7)
                {
                    result = true;
                    this.InvokeIfRequired(() =>
                    {
                        this.tbxUserName.Text = remUser.userName;
                        this.tbxPwd.Text = "******";
                        this.chkRemember.Checked = true;
                        this.tlpInput.Invalidate();
                        Application.DoEvents();
                        _remPwd = remUser.password;
                        setInput();
                    }, true);
                }
            }
            return result;
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
