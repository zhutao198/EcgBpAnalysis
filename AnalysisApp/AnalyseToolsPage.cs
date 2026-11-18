using AnalysisApp.Commons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using static AnalysisApp.Commons.ScatterToolStripButton;

namespace AnalysisApp
{
    public partial class AnalyseToolsPage : BaseUserControl
    {
        private int clickIdx = 0;
        HRVPageControl HRVPage = new HRVPageControl();
        STPageControl STPage = new STPageControl();
        WPPageControl WPPage = new WPPageControl();
        BaseUserControl funcControl = null;

        private Dictionary<int, BaseUserControl> _pageMapping; //按钮与功能页映射集

        public AnalyseToolsPage()
        {
            InitializeComponent();
            this.tabControl1.ApplyHeaderlessStyle = true;
            this.tabControl1.SelectedIndex = 0;
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            if (resetUI) getScatterTypeCount();

            if (HRVPage.IsLoaded) await HRVPage.RefreshData();
            if (STPage.IsLoaded) await STPage.RefreshData();
            if (WPPage.IsLoaded) await WPPage.RefreshData(resetUI);
        }

        private void AnalyseToolsPage_Load(object sender, EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    getScatterTypeCount();

                    if (!IsUiInitialized) InitUI();

                    Console.WriteLine("分析工具界面已加载");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "分析工具");
            }
        }

        public override void OnParentFormShown(EventArgs e)
        {
            if (DesignMode) return;
           
            base.OnParentFormShown(e);
            HRVPage.OnParentFormShown(e);
            STPage.OnParentFormShown(e);
            WPPage.OnParentFormShown(e);
        }

        public override void InitUI()
        {
            base.InitUI();

            try
            {
                _pageMapping = new Dictionary<int, BaseUserControl>();

                SwitchTabPage(clickIdx);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "分析工具");
            }
        }

        //获取心搏类型及其个数
        private void getScatterTypeCount()
        {
            var group = new List<ScatterType>();

            group.Add(new ScatterType { Title = "HRV", Code = "N", Desc = "心率变异", Count = -1, Footer = "心率变异" });
            group.Add(new ScatterType { Title = "ST", Code = "N", Desc = "ST扫描", Count = -1, Footer = "ST扫描" });
            //group.Add(new ScatterType { Title = "HRT", Code = "N", Desc = "心率震荡", Count = -1, Footer = "心率震荡" });
            //group.Add(new ScatterType { Title = "DC", Code = "N", Desc = "DC/DR", Count = -1, Footer = "DC/DR" });
            //group.Add(new ScatterType { Title = "T Wave", Code = "N", Desc = "T波电交替", Count = -1, Footer = "T波电交替" });
            //group.Add(new ScatterType { Title ="QT", Code = "N", Desc = "QT变异", Count = -1, Footer = "QT变异" });
            group.Add(new ScatterType { Title = "WT", Code = "N", Desc = "瀑布图", Count = -1, Footer = "瀑布图" });
            //group.Add(new ScatterType { Title ="SP", Code = "N", Desc = "睡眠呼吸", Count = -1, Footer = "睡眠呼吸" });

            this.toolStrip.Items.Clear();
            using (Font typeFont = new Font(this.Font.FontFamily, 12, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                for (int i = 0; i < group.Count; i++)
                {
                    ScatterType beatType = group[i];
                    ScatterToolStripButton item = new ScatterToolStripButton(beatType.Count.ToString())
                    {
                        Name = "tsBtn" + beatType.Code,
                        ToolTipText = beatType.Desc,
                        ZqScatterType = beatType,
                        AutoSize = false,
                        Size = new Size(toolStrip.Width, 60),
                        ImageScaling = ToolStripItemImageScaling.None,
                        TextAlign = ContentAlignment.MiddleCenter,
                        TextImageRelation = TextImageRelation.ImageAboveText,
                        CodeRect = new RectangleF((toolStrip.Width - 68) / 2F, 2, 68, 68),
                        TitleFontSize = 16,
                    };
                    item.Click += beatType_Click;
                    this.toolStrip.Items.Add(item);

                    if (i == clickIdx) item.UserSelected = true;
                }
            }
        }

        private void beatType_Click(object sender, EventArgs e)
        {
            int idx = 0;

            ScatterToolStripButton curItem = sender as ScatterToolStripButton;
            foreach (ToolStripItem item in this.toolStrip.Items)
            {
                ScatterToolStripButton scatterItem = item as ScatterToolStripButton;
                if (scatterItem != null)
                {
                    scatterItem.UserSelected = false;
                }
                if (scatterItem == curItem)
                {
                    clickIdx = idx;
                    scatterItem.UserSelected = true;
                }
                ++idx;
            }
            this.toolStrip.Invalidate();
            SwitchTabPage(clickIdx);
        }

        //根据工具栏按钮显示相应的设置界面
        public void SwitchTabPage(int tabIdx)
        {
            TabPage page = this.tabControl1.TabPages[tabIdx];

            for (int i = 0; i < this.toolStrip.Items.Count; i++)
            {
                ToolStripItem tsItem = this.toolStrip.Items[i];
                if (_pageMapping.ContainsKey(i)) _pageMapping[i].IsActive = false;
            }

            page.SuspendLayout();
            bool mapped = _pageMapping.TryGetValue(tabIdx, out funcControl);
            if (!mapped)
            {
                switch (tabIdx)
                {
                    case 0: //HRV
                        funcControl = _pageMapping[tabIdx] = HRVPage;
                        break;
                    case 1: //ST
                        funcControl = _pageMapping[tabIdx] = STPage;
                        break;
                    case 2: //HRT
                        funcControl = _pageMapping[tabIdx] = WPPage;
                        break;
                    //case 3: //DC
                    //    funcControl = _pageMapping[tabIdx] = new HistogramPage();
                    //    break;
                    //case 4: //T Wave
                    //    funcControl = _pageMapping[tabIdx] = new AfPage();
                    //    break;
                    //case 5: //QT
                    //    funcControl = _pageMapping[tabIdx] = new TemplatePageControl();
                    //    break;
                    //case 6: //WT
                    //    funcControl = _pageMapping[tabIdx] = new EventPageControl();
                    //    break;
                    //case 7: //SP
                    //    funcControl = _pageMapping[tabIdx] = new EcgSegmentControl();
                    //    break;
                    default:
                        break;
                }

                funcControl.Dock = DockStyle.Fill;
                page.Controls.Add(funcControl);
                funcControl.DataSourceTracked = true;
            }

            this.tabControl1.SelectedIndex = tabIdx;
            _pageMapping[tabIdx].IsActive = true;

            if (AnaValues.DataSourceState > 0)
            {
                if (!funcControl.DataSourceTracked)
                {
                    funcControl.OnDataSourceChanged(AnaValues.DataSourceState == 2 || funcControl.ShouldReset);
                    funcControl.DataSourceTracked = true;
                }
                //if (_dataSourceTracked.All(o => o.Value == true)) { AnaValues.DataSourceIsChanged = 0; }
            }

            page.ResumeLayout(true);
            AnaCode.SaveAnalyzeData();
        }

    }
}
