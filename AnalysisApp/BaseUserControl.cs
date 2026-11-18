using AnalysisApp.CommonControls;
using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;

namespace AnalysisApp
{
    public partial class BaseUserControl : UserControl, IDataMonitor
    {
        public bool IsActive { get; set; }

        /// <summary>
        /// 是否已同步更新数据
        /// </summary>
        public bool DataSourceTracked { get; set; }

        /// <summary>
        /// 是否需要重置界面显示内容(主要用于重分析或切换病例后更新显示)
        /// </summary>
        public bool ShouldReset { get; set; }

        //protected dynamic DefaultLeadInfo;
        protected PersistDictionary<string> _langs = CommonValues.Langs;

        [Browsable(false), DefaultValue(false), Description("当前控件是否已加载")]
        public bool IsLoaded { get; private set; }

        [Browsable(false), DefaultValue(false), Description("当前控件是否已显示")]
        public bool IsShown { get; private set; } = false;

        public bool IsUiInitialized = false;

        public BaseUserControl()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.UpdateStyles();

            this.SuspendLayout();
        }

        private void BaseUserControl_Load(object sender, EventArgs e)
        {
            if (IsLoaded) return;

            //加载绘图组件
            //var graphicControls = CommonFuns.RetriveSubControls<BaseDrawingControl>(this);
            //graphicControls.ForEach((drawer) =>
            //{
            //    if (!drawer.IsLoaded)
            //    {
            //        typeof(BaseDrawingControl).InvokeMember(
            //            "OnLoad",
            //            System.Reflection.BindingFlags.NonPublic |
            //            System.Reflection.BindingFlags.Instance |
            //            System.Reflection.BindingFlags.InvokeMethod,
            //            null, drawer, new object[] { EventArgs.Empty });
            //    }
            //});


            if (!DesignMode)
            {
                //控件文本加载
                List<Control> controls = CommonFuns.RetriveSubControls<Control>(this, false, true);
                controls = controls.FindAll(o => !(o is BaseUserControl));
                controls = controls.FindAll(o => !(o is BaseDrawingControl));
                UIFuns.LoadControlsText(controls);

                //自适应操作系统分辨率
                if (CommonValues.WidthScale != 1 || CommonValues.HeightScale != 1)
                {
                    UIFuns.SetControls(this, adjustParent: true);
                }
            }
            IsLoaded = true;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        public virtual void OnParentFormShown(EventArgs e)
        {
            IsShown = true;
            Console.WriteLine(this.Name + " is shown");
        }

        /// <summary>
        /// 当数据源发生改变时发生
        /// </summary>
        /// <param name="resetUI">是否需要重置UI显示(e.g. 滚动条回到起始位置)</param>
        public void OnDataSourceChanged(bool resetUI = false)
        {
            //CommonValues.MainTreadContext.Post(state =>
            //{
                this.InvokeIfRequired(async () =>
                {
                    try
                    {
                        var graphicControls = CommonFuns.RetriveSubControls<BaseDrawingControl>(this);
                        graphicControls.ForEach((drawer) => { drawer.Init(); }); //刷新基础数据

                        if (IsLoaded)
                        {
                            await RefreshData(resetUI);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex, "分析模块");
                    }
                }, true);
                DataSourceTracked = true;
            //}, null);
        }


        public virtual void InitUI()
        {
            if (IsUiInitialized) return;

            IsUiInitialized = true;
        }

        public virtual async Task RefreshData(bool resetUI = false)
        {
            while (IsShown == false)
            {
                await Task.Delay(1);
            }
        }

        protected void fillLeadComboBox(ComboBox cbx, int leadCount, int? defaultIndex = null)
        {
            cbx.ValueMember = "Value";
            cbx.DisplayMember = "Text";

            List<ListItem> items = new List<ListItem>();
            string leads = SysConstants.ECG_LT_STANDARD[1];
            if (leadCount == 3) leads = SysConstants.ECG_LT_STANDARD[5];
            string[] arrLead = leads.Split(',');
            for (int i = 0; i < arrLead.Length; i++)
            {
                items.Add(new ListItem(i, arrLead[i]));
            };
            cbx.DataSource = items;

            if (defaultIndex.HasValue) cbx.SelectedValue = defaultIndex;
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        // 移除 WS_CLIPCHILDREN 样式
        //        // 这会允许父控件在子控件的区域上进行绘制，从而避免出现“空洞”
        //        cp.Style &= ~0x02000000;
        //        return cp;
        //    }
        //}
    }
}
