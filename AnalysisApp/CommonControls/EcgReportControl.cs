using AnalysisApp.Commons;
using AnalysisSystem.Base;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Widgets;
using Zoncare.DAL;
using Zoncare.Utilities;
using static AnalysisApp.Commons.AEcgHandler;

namespace AnalysisApp.CommonControls
{
    public partial class EcgReportControl : BaseUserControl
    {
        #region 页面级变量定义
        string SummaryFilePath = "";
        string SegmentFilePath = "";
        int TotalBeats = 0;
        string RecordTimeStr = "";
        string AnaTimeStr = "";
        string AnaLeadStr = "";
        string Tachycardia = "";
        string Bradycardia = "";

        int AvgHR = 0;
        int MaxHR = 0;
        string DateStringMaxHR = "";
        int MinHR = 0;
        string DateStringMinHR = "";

        int TotalBeatsV = 0;
        int SingleV = 0;
        int PairedV = 0;
        int BigeminyV = 0;
        int TrigeminyV = 0;
        int TachycardiaV = 0;
        int RonT = 0;

        string LongestTachycardiaV = "";
        string DateTimeLongestTachycardiaV = "";
        double LongestTachycardiaDurationV = 0f;
        int MaxHRV = 0;
        int MinHRV = 0;
        string DateTimeMaxHRV = "";
        string DateTimeMinHRV = "";
        string EscapeV = "";

        int TotalBeatsS = 0;
        int SingleS = 0;
        int PairedS = 0;
        int BigeminyS = 0;
        int TrigeminyS = 0;
        int TachycardiaS = 0;
        string LongestTachycardiaS = "";
        string DateTimeLongestTachycardiaS = "";
        double LongestTachycardiaDurationS = 0f;
        int MaxHRS = 0;
        int MinHRS = 0;
        string DateTimeMaxHRS = "";
        string DateTimeMinHRS = "";
        string EscapeS = "";

        int AsystoleCount = 0;
        double MaxAsystoleStr = 0f;
        string DateStringMaxAsystole = "";

        int LongRRCount = 0;

        double LongestRRStr = 0f;
        string DateStringLongestRR = "";

        string SDNN = "";
        string SDNNIndex = "";
        string SDANN = "";
        string RMSSD = "";
        string pNN50 = "";
        string Triindx = "";
        string TP = "";
        string ULF = "";
        string VLF = "";
        string LF = "";
        string HF = "";
        string LF_HF = "";

        int stLowCount = 0;
        string stLowest = "";
        string stLowestTime = "";
        string stLowestLead = "";
        string stLowLeadDuration = "";
        int stUpCount = 0;
        string stUpest = "";
        string stUpestTime = "";
        string stUpestLead = "";
        string stUpLeadDuration = "";
        string stTotalDuration = "";

        string AFTotalDuration = "";
        string AFPercent = "";
        string AFDuration = "";
        int AFCount = 0;
        string AFPercent2 = "";
        string AFDuration2 = "";
        int AFCount2 = 0;

        int TotalPages = 0;
        int CurrentPageIndex = 0;

        Dictionary<string, int> PageIdxs = new Dictionary<string, int>();
        ReportPrintManager ReportPrintManager = null;
        string ReportAdvice = "";
        List<List<JObject>> SummaryInfo = new List<List<JObject>>();
        List<List<JObject>> HourListInfo = new List<List<JObject>>();
        List<List<JObject>> SegmentAllPageInfo = new List<List<JObject>>();
        List<List<JObject>> HRVTimeDomainInfo = new List<List<JObject>>();
        List<List<JObject>> HRVFrequencyDomainInfo = new List<List<JObject>>();
        List<List<JObject>> HRVAnalysisInfo = new List<List<JObject>>();
        List<List<JObject>> STListInfo = new List<List<JObject>>();
        List<List<JObject>> STTrendWaveInfo = new List<List<JObject>>();
        List<List<JObject>> TTrendWaveInfo = new List<List<JObject>>();
        List<List<JObject>> AFListInfo = new List<List<JObject>>();
        List<List<JObject>> PaceListInfo = new List<List<JObject>>();

        Dictionary<string, TemplateFunctionInfo> Functions = new Dictionary<string, TemplateFunctionInfo>();
        private List<reportTemplate> ReportTemplateList;

        PrintReportPage printReportPage = new PrintReportPage();
        PrintSegmentWave SegmentWaveRenderer = new PrintSegmentWave();

        private bool conclusionChanged = false;
        private bool summarizeChanged = false;
        private string PrintPreviewType = "All";
        #endregion

        public EcgReportControl()
        {
            InitializeComponent();
            //总结/建议文本框支持右键复制粘贴
            initEditContextMenu();
            // 加载常用术语树（Dictionary\dictionary.txt）
            InitDictionaryTree();
            // 单击术语节点插入到"总结/建议"中最近编辑过的那一处的光标位置
            initDictionaryTreeEvents();
        }

        /// <summary>最近获得焦点的结论文本框（总结或建议），单击术语时插入到它的光标处</summary>
        private RichTextBox _lastFocusedEditBox;

        /// <summary>悬停在术语节点上时，浮现在该节点右侧的“添加/编辑/删除”按钮条</summary>
        private Panel _nodeButtonPanel;
        private ToolTip _nodeButtonTip;
        /// <summary>按钮条当前对应的节点</summary>
        private TreeNode _nodeButtonOwner;

        /// <summary>绑定常用术语树的单击插入事件，并记录总结/建议各自最后的光标位置</summary>
        private void initDictionaryTreeEvents()
        {
            _lastFocusedEditBox = this.summarize;
            this.summarize.Enter += (s, e) => _lastFocusedEditBox = this.summarize;
            this.advice.Enter += (s, e) => _lastFocusedEditBox = this.advice;
            this.treeView1.NodeMouseClick += treeView1_NodeMouseClick;

            // 节点文字超过 70% 宽度时用省略号截断（自绘文字），悬停显示完整内容
            this.treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;
            this.treeView1.ShowNodeToolTips = true;
            this.treeView1.DrawNode += treeView1_DrawNode;

            // 悬停到节点上时，在节点右侧显示添加/编辑/删除按钮
            initNodeHoverButtons();
        }

        /// <summary>自绘节点文字：超过 treeView1 宽度 70% 的部分用省略号代替，悬停时由 ToolTip 显示完整术语</summary>
        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null || e.Bounds.Width <= 0 || e.Bounds.Height <= 0)
            {
                return;
            }

            bool selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;

            // 文字可用宽度：最多占控件宽度的 70%，同时不能超出控件右边界
            System.Drawing.Rectangle textRect = getNodeTextRect(e.Bounds);

            // OwnerDrawText 模式下控件会给选中节点自绘系统高亮背景；
            // 用树的背景色覆盖整行，视觉上等同“无选中背景”，同时避免露出系统蓝色高亮
            if (selected)
            {
                int backWidth = treeView1.ClientSize.Width - e.Bounds.Left;
                if (backWidth < 1)
                {
                    backWidth = 1;
                }
                System.Drawing.Rectangle backRect = new System.Drawing.Rectangle(
                    e.Bounds.Left, e.Bounds.Top, backWidth, e.Bounds.Height);
                using (SolidBrush brush = new SolidBrush(treeView1.BackColor))
                {
                    e.Graphics.FillRectangle(brush, backRect);
                }
            }

            TextRenderer.DrawText(
                e.Graphics,
                e.Node.Text,
                e.Node.NodeFont ?? treeView1.Font,
                textRect,
                treeView1.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>节点文字的绘制/命中矩形：宽度最多取控件宽度的 70%，且不超出控件右边界</summary>
        private System.Drawing.Rectangle getNodeTextRect(System.Drawing.Rectangle nodeBounds)
        {
            int limit = (int)(treeView1.ClientSize.Width * 0.7);
            int available = Math.Min(limit, treeView1.ClientSize.Width - nodeBounds.Left);
            if (available < 1)
            {
                available = 1;
            }
            return new System.Drawing.Rectangle(nodeBounds.Left, nodeBounds.Top, available, nodeBounds.Height);
        }

        /// <summary>节点的悬停/点击响应区域：从文字左边界到控件右边界（文字 + 文字右侧位置）</summary>
        private System.Drawing.Rectangle getNodeHitRect(TreeNode node)
        {
            System.Drawing.Rectangle bounds = node.Bounds;
            int width = treeView1.ClientSize.Width - bounds.Left;
            if (width < 1)
            {
                width = 1;
            }
            return new System.Drawing.Rectangle(bounds.Left, bounds.Top, width, bounds.Height);
        }

        /// <summary>单击术语节点文字：把节点文本插入到总结/建议的光标处（点在展开框或空白处不插入）</summary>
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.Node == null)
            {
                return;
            }

            // 命中文字及文字右侧区域都插入；点在左侧 +/− 展开框或缩进处只作选中/展开用
            if (!getNodeHitRect(e.Node).Contains(e.X, e.Y))
            {
                return;
            }

            // 双击用于展开/折叠，只按第一次单击插入一次，避免重复插入
            if (e.Clicks > 1)
            {
                return;
            }

            try
            {
                InsertIntoEditBox(_lastFocusedEditBox ?? this.summarize, e.Node.Text + "\n");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "常用术语插入");
            }
        }

        #region 节点悬停按钮（添加 / 编辑 / 删除）

        /// <summary>创建节点右侧的“添加/编辑/删除”浮动按钮条（默认隐藏），并绑定悬停显示/离开隐藏</summary>
        private void initNodeHoverButtons()
        {
            _nodeButtonTip = new ToolTip();

            _nodeButtonPanel = new Panel();
            _nodeButtonPanel.Size = new System.Drawing.Size(68, 20);
            // 按钮条与按钮的底色都取树的背景色，视觉上等同透明（WinForms 的 Color.Transparent
            // 只会透出父容器底色，透不出树行内容，故直接用树背景色绘制）
            _nodeButtonPanel.BackColor = treeView1.BackColor;
            _nodeButtonPanel.Visible = false;

            NodeIconButton btnAdd = createNodeButton(NodeIconButton.NodeIcon.Add, "在当前节点下新增子术语", 2);
            btnAdd.Click += (s, e) => AddChildTermNode(_nodeButtonOwner);

            NodeIconButton btnEdit = createNodeButton(NodeIconButton.NodeIcon.Edit, "修改当前术语名称", 24);
            btnEdit.Click += (s, e) => EditTermNode(_nodeButtonOwner);

            NodeIconButton btnDelete = createNodeButton(NodeIconButton.NodeIcon.Delete, "删除当前术语及其子节点", 46);
            btnDelete.Click += (s, e) => DeleteTermNode(_nodeButtonOwner);

            this.Controls.Add(_nodeButtonPanel);
            _nodeButtonPanel.BringToFront();

            this.treeView1.MouseMove += treeView1_MouseMove;
            this.treeView1.MouseLeave += treeView1_MouseLeave;
            this.treeView1.MouseWheel += treeView1_MouseWheel;
            this.treeView1.AfterExpand += treeView1_AfterExpandOrCollapse;
            this.treeView1.AfterCollapse += treeView1_AfterExpandOrCollapse;
            this.treeView1.Resize += treeView1_Resize;
            _nodeButtonPanel.MouseLeave += nodeButtonPanel_MouseLeave;
        }

        private NodeIconButton createNodeButton(NodeIconButton.NodeIcon icon, string tip, int left)
        {
            NodeIconButton button = new NodeIconButton(icon, treeView1.BackColor);
            button.Location = new System.Drawing.Point(left, 0);
            _nodeButtonTip.SetToolTip(button, tip);
            _nodeButtonPanel.Controls.Add(button);
            return button;
        }

        /// <summary>鼠标在术语树内移动：悬停在节点文字及其右侧区域时显示按钮条，其余位置隐藏</summary>
        private void treeView1_MouseMove(object sender, MouseEventArgs e)
        {
            TreeNode node = treeView1.GetNodeAt(e.X, e.Y);
            if (node == null)
            {
                HideNodeButtons();
                return;
            }

            // 按钮已显示且仍停在本节点行：位置不变，直接保持
            if (_nodeButtonPanel.Visible && node == _nodeButtonOwner)
            {
                return;
            }

            // 命中文字及文字右侧区域才弹出；缩进、展开框等左侧区域不弹出
            if (!getNodeHitRect(node).Contains(e.X, e.Y))
            {
                HideNodeButtons();
                return;
            }

            ShowNodeButtons(node);
        }

        private void treeView1_MouseLeave(object sender, EventArgs e)
        {
            // 鼠标移到按钮条上不算离开，由按钮条的 MouseLeave 负责
            if (_nodeButtonPanel != null && _nodeButtonPanel.Visible &&
                _nodeButtonPanel.ClientRectangle.Contains(_nodeButtonPanel.PointToClient(Cursor.Position)))
            {
                return;
            }
            HideNodeButtons();
        }

        private void nodeButtonPanel_MouseLeave(object sender, EventArgs e)
        {
            // 从按钮条移回节点行时不隐藏，交给 treeView1_MouseMove 重新定位
            if (treeView1.ClientRectangle.Contains(treeView1.PointToClient(Cursor.Position)))
            {
                return;
            }
            HideNodeButtons();
        }

        private void treeView1_MouseWheel(object sender, MouseEventArgs e)
        {
            // 滚动后行位置变化，直接隐藏，鼠标再移动时会重新出现
            HideNodeButtons();
        }

        private void treeView1_AfterExpandOrCollapse(object sender, TreeViewEventArgs e)
        {
            HideNodeButtons();
        }

        private void treeView1_Resize(object sender, EventArgs e)
        {
            HideNodeButtons();
        }

        /// <summary>把按钮条定位到指定节点行的右侧并显示</summary>
        private void ShowNodeButtons(TreeNode node)
        {
            if (node == null || _nodeButtonPanel == null || !node.IsVisible)
            {
                return;
            }

            // 整体靠右对齐：贴住树控件客户区右边界（ClientSize 已排除竖向滚动条，不会压住滚动条）
            int x = treeView1.ClientSize.Width - _nodeButtonPanel.Width;
            if (x < 0)
            {
                x = 0;
            }
            int y = node.Bounds.Top + (node.Bounds.Height - _nodeButtonPanel.Height) / 2;
            if (y < 0)
            {
                y = 0;
            }

            System.Drawing.Point location = this.PointToClient(treeView1.PointToScreen(new System.Drawing.Point(x, y)));

            _nodeButtonOwner = node;
            _nodeButtonPanel.Location = location;
            _nodeButtonPanel.Visible = true;
            _nodeButtonPanel.BringToFront();
        }

        private void HideNodeButtons()
        {
            if (_nodeButtonPanel != null && _nodeButtonPanel.Visible)
            {
                _nodeButtonPanel.Visible = false;
            }
            _nodeButtonOwner = null;
        }

        /// <summary>在指定节点下新增子术语，并回写 dictionary.txt</summary>
        private void AddChildTermNode(TreeNode parent)
        {
            HideNodeButtons();

            if (parent == null)
            {
                return;
            }

            string text = TermInputForm.Prompt(this, "新增子术语", "在“" + parent.Text + "”下新增子术语：", "");
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (parent.Nodes.ContainsKey(text))
            {
                MessageBox.Show(this, "该节点下已存在同名术语：“" + text + "”。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TreeNode node = new TreeNode(text);
            node.Name = text;             // Name 作为去重键
            node.ToolTipText = text;      // 悬停显示完整术语

            parent.Nodes.Add(node);
            parent.Expand();              // 展开父节点，便于看到新增的子术语

            treeView1.SelectedNode = node;
            SaveDictionaryTree();
        }

        /// <summary>“新增”按钮：添加一级术语节点（根级），并回写 dictionary.txt</summary>
        private void addNode_Click(object sender, EventArgs e)
        {
            try
            {
                string text = TermInputForm.Prompt(this, "新增一级术语", "一级术语名称：", "");
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                if (treeView1.Nodes.ContainsKey(text))
                {
                    MessageBox.Show(this, "已存在同名一级术语：“" + text + "”。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                TreeNode node = new TreeNode(text);
                node.Name = text;             // Name 作为去重键
                node.ToolTipText = text;      // 悬停显示完整术语

                treeView1.Nodes.Add(node);
                treeView1.SelectedNode = node;
                SaveDictionaryTree();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "新增一级术语");
                MessageBox.Show(this, "新增一级术语失败：" + ex.Message, "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>修改当前术语名称，并回写 dictionary.txt</summary>
        private void EditTermNode(TreeNode node)
        {
            if (node == null)
            {
                return;
            }
            HideNodeButtons();

            string text = TermInputForm.Prompt(this, "编辑术语", "术语名称：", node.Text);
            if (string.IsNullOrEmpty(text) || text == node.Text)
            {
                return;
            }

            TreeNodeCollection siblings = node.Parent == null ? treeView1.Nodes : node.Parent.Nodes;
            TreeNode sameNameNode = siblings.ContainsKey(text) ? siblings.Find(text, false)[0] : null;
            if (sameNameNode != null && sameNameNode != node)
            {
                MessageBox.Show(this, "同一层级已存在同名术语：“" + text + "”。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            node.Text = text;
            node.Name = text;
            node.ToolTipText = text;
            treeView1.SelectedNode = node;
            SaveDictionaryTree();
        }

        /// <summary>删除当前术语及其子节点，并回写 dictionary.txt</summary>
        private void DeleteTermNode(TreeNode node)
        {
            if (node == null)
            {
                return;
            }
            HideNodeButtons();

            string message = node.Nodes.Count > 0
                ? "确定删除术语“" + node.Text + "”及其 " + node.Nodes.Count + " 个子节点？"
                : "确定删除术语“" + node.Text + "”？";

            if (MessageBox.Show(this, message, "删除确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            node.Remove();
            SaveDictionaryTree();
        }

        /// <summary>把整棵术语树按 Tab 缩进回写到 Dictionary\dictionary.txt（UTF-8 无 BOM）</summary>
        private void SaveDictionaryTree()
        {
            try
            {
                string dictPath = Path.Combine(Application.StartupPath, "Dictionary", "dictionary.txt");
                StringBuilder builder = new StringBuilder();
                AppendDictionaryNodes(builder, treeView1.Nodes, 0);
                File.WriteAllText(dictPath, builder.ToString(), new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "常用术语字典保存");
                MessageBox.Show(this, "常用术语保存失败：" + ex.Message, "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void AppendDictionaryNodes(StringBuilder builder, TreeNodeCollection nodes, int depth)
        {
            foreach (TreeNode node in nodes)
            {
                builder.Append('\t', depth);
                builder.AppendLine(node.Text);
                AppendDictionaryNodes(builder, node.Nodes, depth + 1);
            }
        }

        #endregion

        /// <summary>在文本框光标处插入文本（有选中内容则替换），光标停在插入内容之后</summary>
        private void InsertIntoEditBox(RichTextBox box, string text)
        {
            if (box == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            int pos = box.SelectionStart;
            if (pos < 0 || pos > box.TextLength)
            {
                pos = box.TextLength;
            }

            box.Select(pos, 0);
            box.SelectedText = text;

            box.SelectionStart = pos + text.Length;
            box.SelectionLength = 0;
            box.Focus(); // 光标回到文本框，便于继续输入（TextChanged 会置脏标记，失焦时自动保存）
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            RefreshUI();
        }

        private void EcgReportControl_Load(object sender, EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    //RefreshUI();

                    var printMenuStrip = new ContextMenuStrip();
                    printMenuStrip.ShowImageMargin = false;
                    printMenuStrip.Items.Add("HRV报告打印");
                    printMenuStrip.Click += (o, args) =>
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        initHRVPrintData();
                        printReport();
                        object para = new { AnaValues.CurrentCase.caseNo };
                        OperateLog.logParam("HRV报告打印", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                    };
                    this.btnPrint.DropDownMenu = printMenuStrip;
                    this.btnPrint.Click += (o, args) =>
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        initPrintData();
                        printReport();
                        object para = new { AnaValues.CurrentCase.caseNo };
                        OperateLog.logParam("心电报告打印", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                    };
                    this.btnPrint.ArrowClick += (o, args) => printMenuStrip.Show(this.btnPrint, this.btnPrint.Width, this.btnPrint.Height);

                    //var printPreviewMenuStrip = new ContextMenuStrip();
                    //printPreviewMenuStrip.ShowImageMargin = false;
                    //printPreviewMenuStrip.Items.Add("HRV报告打印预览");
                    //printPreviewMenuStrip.Click += (o, args) =>
                    //{
                    //    Stopwatch sw = Stopwatch.StartNew();
                    //    initHRVPrintData();
                    //    PrintPreviewType = "All";
                    //    PrintPreview();
                    //    object para = new { AnaValues.CurrentCase.caseNo };
                    //    OperateLog.logParam("HRV报告打印预览", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                    //};
                    //this.btnPrintPreview.DropDownMenu = printPreviewMenuStrip;
                    //this.btnPrintPreview.Click += (o, args) =>
                    //{
                    //    Stopwatch sw = Stopwatch.StartNew();
                    //    initPrintData();
                    //    PrintPreviewType = "All";
                    //    PrintPreview();
                    //    object para = new { AnaValues.CurrentCase.caseNo };
                    //    OperateLog.logParam("心电报告打印预览", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                    //};
                    //this.btnPrintPreview.ArrowClick += (o, args) => printPreviewMenuStrip.Show(this.btnPrintPreview, this.btnPrintPreview.Width, this.btnPrintPreview.Height);

                    //var exportPdfMenuStrip = new ContextMenuStrip();
                    //exportPdfMenuStrip.ShowImageMargin = false;
                    //exportPdfMenuStrip.Items.Add("HRV导出pdf");
                    //exportPdfMenuStrip.Click += (o, args) =>
                    //{
                    //    SaveFileDialog savefile = new SaveFileDialog();
                    //    savefile.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";    //保存文件的格式
                    //    savefile.FilterIndex = 1;
                    //    savefile.RestoreDirectory = true;
                    //    if (savefile.ShowDialog() == DialogResult.OK)
                    //    {
                    //        string filePath = savefile.FileName;
                    //        if (string.IsNullOrEmpty(filePath)) return;

                    //        Stopwatch sw = Stopwatch.StartNew();
                    //        bool exported = exportToPdf(filePath, 1);
                    //        if (exported)
                    //            ScreenCenterTip.Show("导出成功");
                    //        else
                    //            CommonFuns.PromptError("导出失败");

                    //        object para = new { AnaValues.CurrentCase.caseNo };
                    //        OperateLog.logParam("导出报告", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                    //    }
                    //};
                    //this.btnExportPdf.DropDownMenu = exportPdfMenuStrip;
                    //this.btnExportPdf.Click += (o, args) =>
                    //{
                    //    SaveFileDialog savefile = new SaveFileDialog();
                    //    savefile.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";    //保存文件的格式
                    //    savefile.FilterIndex = 1;
                    //    savefile.RestoreDirectory = true;
                    //    if (savefile.ShowDialog() == DialogResult.OK)
                    //    {
                    //        string filePath = savefile.FileName;
                    //        if (string.IsNullOrEmpty(filePath)) return;

                    //        Stopwatch sw = Stopwatch.StartNew();
                    //        bool exported = exportToPdf(filePath, 0);
                    //        if (exported)
                    //            ScreenCenterTip.Show("导出成功");
                    //        else
                    //            CommonFuns.PromptError("导出失败");

                    //        object para = new { AnaValues.CurrentCase.caseNo };
                    //        OperateLog.logParam("导出报告", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
                    //    }
                    //};
                    //this.btnExportPdf.ArrowClick += (o, args) => exportPdfMenuStrip.Show(this.btnExportPdf, this.btnExportPdf.Width, this.btnExportPdf.Height);

                    Console.WriteLine("报告界面已加载");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心电报告");
            }
        }

        #region 打印勾选状态持久化

        /// <summary>
        /// 勾选状态记录文件，以程序运行目录为基准；
        /// 本机调试时即 E:\project\EcgBpAnalysis\EcgAnalyseStart\bin\Debug\config\ecgPrintCheck.json。
        /// </summary>
        private static readonly string PrintCheckFilePath = Path.Combine(Application.StartupPath, "config", "ecgPrintCheck.json");

        /// <summary>程序化恢复勾选状态时抑制写文件，避免把设计器默认值回写</summary>
        private bool _restoringPrintCheck = false;

        /// <summary>界面上参与打印内容选择的复选框（Designer 中全部 15 个）</summary>
        private List<CheckBox> GetPrintCheckBoxes()
        {
            return new List<CheckBox>
            {
                cbxSummarize, cbxHourCountList, cbxSegment, cbxQT, cbxHRT, cbxPace,
                cbxHRVData, cbxHRVTimeDomain, cbxHRVFrequencyDomain, cbxHRVAnalysis,
                cbxSTList, cbxSTTrendWave, cbxTTrendWave, cbxAtrialList, cbxSmallSegment
            };
        }

        /// <summary>
        /// 进入界面时读取 config\ecgPrintCheck.json，按控件名恢复勾选状态；
        /// 同时绑定 CheckedChanged，用户每次勾选/取消勾选都立即写回文件。
        /// 文件不存在、为空或格式损坏时保持设计器默认状态，并补写一份默认记录。
        /// </summary>
        private void InitPrintCheckState()
        {
            List<CheckBox> checkBoxes = GetPrintCheckBoxes();
            bool restored = false;

            _restoringPrintCheck = true;
            try
            {
                if (File.Exists(PrintCheckFilePath))
                {
                    string json = File.ReadAllText(PrintCheckFilePath, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        JObject saved = JObject.Parse(json);
                        foreach (CheckBox cbx in checkBoxes)
                        {
                            JToken token = saved[cbx.Name];
                            if (token != null)
                            {
                                cbx.Checked = token.Value<bool>();
                            }
                        }
                        restored = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心电打印勾选状态读取");
            }
            finally
            {
                _restoringPrintCheck = false;
            }

            if (!restored)
            {
                SavePrintCheckState();
            }

            // 状态变化时写文件；先解绑再绑定，避免重复订阅
            foreach (CheckBox cbx in checkBoxes)
            {
                cbx.CheckedChanged -= printCheckBox_CheckedChanged;
                cbx.CheckedChanged += printCheckBox_CheckedChanged;
            }
        }

        /// <summary>把当前所有复选框状态按控件名写入 JSON 文件</summary>
        private void SavePrintCheckState()
        {
            try
            {
                JObject state = new JObject();
                foreach (CheckBox cbx in GetPrintCheckBoxes())
                {
                    state[cbx.Name] = cbx.Checked;
                }

                string dir = Path.GetDirectoryName(PrintCheckFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(PrintCheckFilePath, state.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心电打印勾选状态保存");
            }
        }

        private void printCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_restoringPrintCheck || DesignMode)
            {
                return;
            }
            SavePrintCheckState();
        }

        #endregion

        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            initPrintData();
            PrintPreviewType = "All";
            PrintPreview();
            object para = new { AnaValues.CurrentCase.caseNo };
            OperateLog.logParam("心电报告打印预览", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";    //保存文件的格式
            savefile.FilterIndex = 1;
            savefile.RestoreDirectory = true;
            if (savefile.ShowDialog() == DialogResult.OK)
            {
                string filePath = savefile.FileName;
                if (string.IsNullOrEmpty(filePath)) return;

                Stopwatch sw = Stopwatch.StartNew();
                bool exported = exportToPdf(filePath);
                if (exported)
                    ScreenCenterTip.Show("导出成功");
                else
                    CommonFuns.PromptError("导出失败");

                object para = new { AnaValues.CurrentCase.caseNo };
                OperateLog.logParam("导出报告", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
            }
        }

        public void RefreshUI()
        {
            // 从 config\ecgPrintCheck.json 恢复上次保存的打印勾选状态
            InitPrintCheckState();

            bool bPace = AnaValues.holterConfig.AnalysisSetting.PaceAnalysisEnable;
            if (bPace)
            {
                this.cbxPace.Enabled = true;
                this.picPace.Enabled = true;
                this.cbxHRVData.Enabled = false;
            }
            else
            {
                this.cbxPace.Enabled = false;
                this.picPace.Enabled = false;
                this.cbxHRVData.Enabled = true;
            }


            // 建议文本框同为 RichTextBox，同样需要统一换行符，避免每行之间多出空行
            this.advice.Text = NormalizeLineBreaks(AnaValues.CurrentCase.holterReportConclusion);

            if (!string.IsNullOrEmpty(AnaValues.RootPath))
            {
                SummaryFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SummaryFileName);
                SegmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
            }

            if (!File.Exists(SummaryFilePath))
            {
                CalcData();
            }
            else
            {
                ReadData();
            }

            Functions = GetTemplateFunctions();

            // 刷新结论区与模板单选按钮（模板编辑窗体保存后也会调用）
            RefreshSummarize();

        }

        /// <summary>
        /// 刷新结论区（summarize）与模板单选按钮。
        /// 结论取值优先级：
        /// 1) 该病例上次保存的结论 caseInfo.holterAutoReportConclusion（用户修改 summarize 后保存的值）
        /// 2) 为空时取默认报告模板(is_default == 1)的 content
        /// 3) 仍为空则自动生成
        /// 模板编辑窗体保存后重新加载模板列表并按此优先级刷新。
        /// </summary>
        private void RefreshSummarize()
        {
            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            ReportTemplateList = dbEntities.ReportTemplates.Where(o => o.languageId == CommonValues.LanguageID && o.systemType == "ecg").ToList();
            // 结论取值优先级：
            // 1) 该病例上次保存的结论 caseInfo.holterAutoReportConclusion（用户修改 summarize 后保存的值）
            // 2) 为空时取默认报告模板(is_default == 1)的 content
            // 3) 仍为空则自动生成
            string autoReportConclusion = "";
            examCase caseInfo = ZoncareEntities.Instance.ExamCases.Where(o => o.id == AnaValues.CurrentCase.id).FirstOrDefault();
            if (caseInfo != null)
            {
                autoReportConclusion = caseInfo.holterAutoReportConclusion;
            }
            if (string.IsNullOrEmpty(autoReportConclusion) && ReportTemplateList != null)
            {
                reportTemplate defaultTemplate = ReportTemplateList.FirstOrDefault(o => o.isDefault == 1);
                if (defaultTemplate != null)
                {
                    autoReportConclusion = defaultTemplate.content;
                    autoReportConclusion += ("\n请结合临床。\n");
                }
            }
            if (!string.IsNullOrEmpty(autoReportConclusion))
            {
                this.summarize.Text = NormalizeLineBreaks(ApplyTemplateFunctions(autoReportConclusion));
            }
            else
            {
                setSummaryInfo();
            }

            // 填充 4 个报告模板单选按钮
            initReportTemplateRadios();
        }
        private Dictionary<string, TemplateFunctionInfo> GetTemplateFunctions()
        {
            int totalSeconds = AnaValues.holterConfig.Total / AnaValues.holterConfig.SampleRate;
            DateTime endTime = AnaValues.holterConfig.StartDateTime.AddSeconds(totalSeconds);

            double longestRR = 0;
            string longestRRTime = "";
            if (AnaEvent.AllEvents["最长RR间期"].Count > 0)
            {
                longestRR = Math.Round((double)AnaEvent.AllEvents["最长RR间期"][0].AvgRR / 1000d, 2);
                longestRRTime = AnaEvent.AllEvents["最长RR间期"][0].StartTime.Split(' ')[1];
            }

            int longRRCount = AnaEvent.AllEvents.ContainsKey("长RR间期") ? AnaEvent.AllEvents["长RR间期"].Count : 0;
            int longRRSet = CommonValues.SystemSettings.ecgSets.customSets.longRR;

            int escapeG = AnaEvent.AllEvents.ContainsKey("交界性逸搏") ? AnaEvent.AllEvents["交界性逸搏"].Count : 0;

            string stDuration = "0";
            if (!string.IsNullOrWhiteSpace(stTotalDuration))
            {
                stDuration = stTotalDuration;
            }

            int percepCount = 0;        //感知失败
            int outputCount = 0;        //输出失败
            int captureCount = 0;       //夺获失败
            List<AEcgHandler.AEcgPacemakerEvaluation> paceMakerList = AEcgHandler.GetPacemakerEvaluationInfo();
            for (int i = 0; i < paceMakerList.Count; i++)
            {
                //0-感知失败，1-输出失败，2-夺获失败
                switch (paceMakerList[i].failureType)
                {
                    case 0:
                        ++percepCount;
                        break;
                    case 1:
                        ++outputCount;
                        break;
                    case 2:
                        ++captureCount;
                        break;
                }
            }

            Dictionary<string, TemplateFunctionInfo> functions = new Dictionary<string, TemplateFunctionInfo>();
            functions["<$TotalBeats$>"] = new TemplateFunctionInfo(CommonValues.Langs["总心搏数"], TotalBeats.ToString(), CommonValues.Langs["次"]);
            functions["<$StartTime$>"] = new TemplateFunctionInfo(CommonValues.Langs["起始时间"], AnaValues.holterConfig.StartTime, "");
            functions["<$EndTime$>"] = new TemplateFunctionInfo(CommonValues.Langs["终止时间"], endTime.ToString("yyyy-MM-dd HH:mm:ss"), "");
            functions["<$RecordingDuration$>"] = new TemplateFunctionInfo(CommonValues.Langs["记录时长"], RecordTimeStr, "");
            functions["<$AnalysisDuration$>"] = new TemplateFunctionInfo(CommonValues.Langs["分析时长"], AnaTimeStr, "");
            functions["<$AnalysisTime$>"] = new TemplateFunctionInfo(CommonValues.Langs["分析时间"], AnaValues.CurrentCase.uploadTime.Value.ToString(), "");
            functions["<$HRMax$>"] = new TemplateFunctionInfo(CommonValues.Langs["最快心率"], MaxHR.ToString(), "");
            functions["<$HRMaxat$>"] = new TemplateFunctionInfo(CommonValues.Langs["最快心率发生时间"], DateStringMaxHR, "");
            functions["<$HRMin$>"] = new TemplateFunctionInfo(CommonValues.Langs["最慢心率"], MinHR.ToString(), "");
            functions["<$HRMinat$>"] = new TemplateFunctionInfo(CommonValues.Langs["最慢心率发生时间"], DateStringMinHR, "");
            functions["<$HRAvg$>"] = new TemplateFunctionInfo(CommonValues.Langs["全程平均心率"], AvgHR.ToString(), "");
            functions["<$MAXRR$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长RR间期"], longestRR.ToString("F2"), CommonValues.Langs["秒"]);
            functions["<$MAXRRAT$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长RR间期发生时间"], longestRRTime, "");
            functions["<$LongRR.Count$>"] = new TemplateFunctionInfo(CommonValues.Langs["长RR间期次数"], longRRCount.ToString(), CommonValues.Langs["次"]);
            functions["<$LongRR.Thred$>"] = new TemplateFunctionInfo(CommonValues.Langs["长RR间期阈值"], longRRSet.ToString(), CommonValues.Langs["次"]);

            functions["<$Asystole$>"] = new TemplateFunctionInfo(CommonValues.Langs["停搏次数"], AsystoleCount.ToString(), CommonValues.Langs["次"]);
            functions["<$AsystoleDuration$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长停搏"], MaxAsystoleStr.ToString(), CommonValues.Langs["秒"]);
            functions["<$AsystoleTime$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长停搏发生时间"], DateStringMaxAsystole, "");

            functions["<$SVCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性早搏数"], TotalBeatsS.ToString(), CommonValues.Langs["次"]);
            functions["<$SVPercent$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性早搏总占比"], Math.Round((double)TotalBeatsS / TotalBeats * 100, 2).ToString(), "");
            functions["<$SVISOCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["单发房早数"], SingleS.ToString(), CommonValues.Langs["次"]);
            functions["<$SVCoupleCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["成对房早数"], PairedS.ToString(), CommonValues.Langs["阵"]);
            functions["<$SVBIGCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性二联律"], BigeminyS.ToString(), CommonValues.Langs["阵"]);
            functions["<$SVTRIGCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性三联律"], TrigeminyS.ToString(), CommonValues.Langs["阵"]);
            functions["<$SVRUNCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["房速"], TachycardiaS.ToString(), CommonValues.Langs["阵"]);
            functions["<$SVRUNLongest$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长房速个数"], LongestTachycardiaS.ToString(), CommonValues.Langs["次"]);
            functions["<$SVRUNLongestat$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长房速发生时间"], DateTimeLongestTachycardiaS, "");
            functions["<$SVRUNLongestDuration$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长房速时长"], LongestTachycardiaDurationS.ToString(), CommonValues.Langs["秒"]);

            functions["<$PVCCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性早搏数"], TotalBeatsV.ToString(), CommonValues.Langs["次"]);
            functions["<$PVCPercent$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性早搏总占比"], Math.Round((double)TotalBeatsV / TotalBeats * 100, 2).ToString(), "");
            functions["<$PVCISOCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["单发室早数"], SingleV.ToString(), CommonValues.Langs["次"]);
            functions["<$PVCCoupleCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["成对室早数"], PairedV.ToString(), CommonValues.Langs["阵"]);
            functions["<$PVCBigCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性二联律"], BigeminyV.ToString(), CommonValues.Langs["阵"]);
            functions["<$PVCTrigCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性三联律"], TrigeminyV.ToString(), CommonValues.Langs["阵"]);
            functions["<$PVCRunCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["室速"], TachycardiaV.ToString(), CommonValues.Langs["阵"]);
            functions["<$PVCRunLongest$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长室速个数"], LongestTachycardiaV.ToString(), CommonValues.Langs["次"]);
            functions["<$PVCRunLongestat$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长室速发生时间"], DateTimeLongestTachycardiaV, "");
            functions["<$PVCRunLongestDuration$>"] = new TemplateFunctionInfo(CommonValues.Langs["最长室速时长"], LongestTachycardiaDurationV.ToString(), CommonValues.Langs["秒"]);
            functions["<$RonTPrematureCount$>"] = new TemplateFunctionInfo(CommonValues.Langs["R on T早搏数"], RonT.ToString(), CommonValues.Langs["次"]);

            functions["<$AEsc$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性逸搏数"], EscapeS.ToString(), CommonValues.Langs["次"]);
            functions["<$NEsc$>"] = new TemplateFunctionInfo(CommonValues.Langs["交界性逸搏数"], escapeG.ToString(), CommonValues.Langs["次"]);
            functions["<$VEsc$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性逸搏数"], EscapeV.ToString(), CommonValues.Langs["次"]);

            functions["<$STMAX$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大抬高"], stUpest, "");
            functions["<$STMAXAT$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大抬高发生时间"], stUpestTime, "");
            functions["<$STMAXCH$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大抬高发生通道"], stUpestLead, "");
            functions["<$STMIN$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大压低"], stLowest, "");
            functions["<$STMINAT$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大压低发生时间"], stLowestTime, "");
            functions["<$STMINCH$>"] = new TemplateFunctionInfo(CommonValues.Langs["ST段最大压低发生通道"], stLowestLead, "");
            functions["<$STALLTIME$>"] = new TemplateFunctionInfo(CommonValues.Langs["异常ST段总时间"], stDuration, CommonValues.Langs["分"]);

            functions["<$AF.Keep$>"] = new TemplateFunctionInfo(CommonValues.Langs["房颤房扑持续时间"], AFTotalDuration, "");
            functions["<$AFIB.QRSPERCENT$>"] = new TemplateFunctionInfo(CommonValues.Langs["房颤负荷"], AFPercent, "%");
            functions["<$AFIB.Keep$>"] = new TemplateFunctionInfo(CommonValues.Langs["房颤持续时间"], AFDuration, "");
            functions["<$AFIB.Times$>"] = new TemplateFunctionInfo(CommonValues.Langs["房颤阵数"], AFCount.ToString(), CommonValues.Langs["阵"]);
            functions["<$AFLUT.QRSPERCENT$>"] = new TemplateFunctionInfo(CommonValues.Langs["房扑负荷"], AFPercent2, "%");
            functions["<$AFLUT.Keep$>"] = new TemplateFunctionInfo(CommonValues.Langs["房扑持续时间"], AFDuration2, "");
            functions["<$AFLUT.Times$>"] = new TemplateFunctionInfo(CommonValues.Langs["房扑阵数"], AFCount2.ToString(), CommonValues.Langs["阵"]);

            bool bPace = AnaValues.holterConfig.AnalysisSetting.PaceAnalysisEnable;
            if (bPace)
            {
                functions["<$PM.APaced$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性起搏数"], SDNNIndex, CommonValues.Langs["次"]);
                functions["<$PM.APacedPer$>"] = new TemplateFunctionInfo(CommonValues.Langs["房性起搏总占比"], ULF, "%");
                functions["<$PM.VPaced$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性起搏数"], pNN50, CommonValues.Langs["次"]);
                functions["<$PM.VPacedPer$>"] = new TemplateFunctionInfo(CommonValues.Langs["室性起搏总占比"], VLF, "%");
                functions["<$PM.AVPaced$>"] = new TemplateFunctionInfo(CommonValues.Langs["房室顺序起搏数"], RMSSD, CommonValues.Langs["次"]);
                functions["<$PM.AVPacedPer$>"] = new TemplateFunctionInfo(CommonValues.Langs["房室顺序起搏总占比"], LF, "%");
                functions["<$PM.FTS$>"] = new TemplateFunctionInfo(CommonValues.Langs["起搏感知失败次数"], percepCount.ToString(), CommonValues.Langs["次"]);
                functions["<$PM.FTO$>"] = new TemplateFunctionInfo(CommonValues.Langs["起搏输出失败次数"], outputCount.ToString(), CommonValues.Langs["次"]);
                functions["<$PM.FTC$>"] = new TemplateFunctionInfo(CommonValues.Langs["起搏夺获失败次数"], captureCount.ToString(), CommonValues.Langs["次"]);
            }
            else
            {
                functions["<$HRVSDNN$>"] = new TemplateFunctionInfo(CommonValues.Langs["R-R间期标准差"], SDNN, "ms");
                functions["<$HRVSDANN$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均R-R间期标准差"], SDANN, "ms");
                functions["<$HRVSDNNIndex$>"] = new TemplateFunctionInfo(CommonValues.Langs["5min R-R间期标准差的均值"], SDNNIndex, "ms");
                functions["<$HRVSDANN$>"] = new TemplateFunctionInfo(CommonValues.Langs["相邻R-R间期差值均方根"], RMSSD, "ms");
                functions["<$HRVPNN50$>"] = new TemplateFunctionInfo(CommonValues.Langs["相邻R-R间期差值"], pNN50, "%");
                functions["<$HRVTRIGIndex$>"] = new TemplateFunctionInfo(CommonValues.Langs["HRV三角指数"], Triindx, "");
                functions["<$HRVLF$>"] = new TemplateFunctionInfo(CommonValues.Langs["极低频功率"], VLF, "ms²");
                functions["<$HRVLFNORM$>"] = new TemplateFunctionInfo(CommonValues.Langs["低频功率"], LF, "ms²");
                functions["<$HRVHF$>"] = new TemplateFunctionInfo(CommonValues.Langs["高频功率"], HF, "ms²");
                functions["<$HRVLFHF$>"] = new TemplateFunctionInfo(CommonValues.Langs["低频功率/高频功率"], LF_HF, "");
            }

            return functions;
        }
        private void getEventInfo()
        {
            int segmentUnanalyzableTotal = 0;
            for (int i = 0; i < AnaValues.UnanalyzableSegments.Count; i++)
            {
                segmentUnanalyzableTotal += AnaValues.UnanalyzableSegments[i].EndIndex - AnaValues.UnanalyzableSegments[i].StartIndex;
            }
            int totalAnaSeconds = (AnaValues.holterConfig.Total - segmentUnanalyzableTotal) / AnaValues.holterConfig.SampleRate;
            TimeSpan AnaTime = TimeSpan.FromSeconds(totalAnaSeconds);
            AnaTimeStr = (int)AnaTime.TotalHours + CommonValues.Langs["小时"] + AnaTime.Minutes + CommonValues.Langs["分钟"];

            string mainCH = AnaValues.LeadIdxToNameMap[AnaValues.holterConfig.MainChannel];
            string secondCH1 = AnaValues.LeadIdxToNameMap[AnaValues.holterConfig.SecondChannel1];
            string secondCH2 = AnaValues.LeadIdxToNameMap[AnaValues.holterConfig.SecondChannel2];

            AnaLeadStr = $"{mainCH} {secondCH1} {secondCH2}";

            Tachycardia = AnaEvent.AllEvents["心动过速"].Count > 0 ? AnaEvent.AllEvents["心动过速"].Count.ToString() : "0";
            Bradycardia = AnaEvent.AllEvents["心动过缓"].Count > 0 ? AnaEvent.AllEvents["心动过缓"].Count.ToString() : "0";

            LongestTachycardiaV = AnaEvent.AllEvents["室速"].Count > 0 ? AnaEvent.AllEvents["室速"][0].BeatSize.ToString() : "0";
            DateTimeLongestTachycardiaV = AnaEvent.AllEvents["室速"].Count > 0 ? AnaEvent.AllEvents["室速"][0].StartTime : "0";

            LongestTachycardiaDurationV = AnaEvent.AllEvents["室速"].Count > 0 ? AnaEvent.AllEvents["室速"][0].Duration : 0f;
            if (AnaEvent.AllEvents["室速"].Count > 0)
            {
                MaxHRV = AnaEvent.AllEvents["室速"][0].AvgHr;
                MinHRV = AnaEvent.AllEvents["室速"][0].AvgHr;
                for (int i = 0; i < AnaEvent.AllEvents["室速"].Count; i++)
                {
                    MaxHRV = Math.Max(MaxHRV, AnaEvent.AllEvents["室速"][i].AvgHr);
                    MinHRV = Math.Min(MinHRV, AnaEvent.AllEvents["室速"][i].AvgHr);
                }

                int maxHRIndex = AnaEvent.AllEvents["室速"].FindIndex(h => h.AvgHr == MaxHRV);
                int minHRIndex = AnaEvent.AllEvents["室速"].FindIndex(h => h.AvgHr == MinHRV);
                DateTimeMaxHRV = AnaEvent.AllEvents["室速"][maxHRIndex].StartTime;
                DateTimeMinHRV = AnaEvent.AllEvents["室速"][minHRIndex].StartTime;

            }
            EscapeV = AnaEvent.AllEvents.ContainsKey("室性逸搏") ? AnaEvent.AllEvents["室性逸搏"].Count.ToString() : "0";
            RonT = AnaEvent.AllEvents.ContainsKey("R on T") ? AnaEvent.AllEvents["R on T"].Count : 0;

            LongestTachycardiaS = AnaEvent.AllEvents["房速"].Count > 0 ? AnaEvent.AllEvents["房速"][0].BeatSize.ToString() : "0";
            DateTimeLongestTachycardiaS = AnaEvent.AllEvents["房速"].Count > 0 ? AnaEvent.AllEvents["房速"][0].StartTime : "0";
            LongestTachycardiaDurationS = AnaEvent.AllEvents["房速"].Count > 0 ? AnaEvent.AllEvents["房速"][0].Duration : 0f;

            if (AnaEvent.AllEvents["房速"].Count > 0)
            {
                MaxHRS = AnaEvent.AllEvents["房速"][0].AvgHr;
                MinHRS = AnaEvent.AllEvents["房速"][0].AvgHr;
                for (int i = 0; i < AnaEvent.AllEvents["房速"].Count; i++)
                {
                    MaxHRS = Math.Max(MaxHRS, AnaEvent.AllEvents["房速"][i].AvgHr);
                    MinHRS = Math.Min(MinHRS, AnaEvent.AllEvents["房速"][i].AvgHr);
                }

                int maxHRIndex = AnaEvent.AllEvents["房速"].FindIndex(h => h.AvgHr == MaxHRS);
                int minHRIndex = AnaEvent.AllEvents["房速"].FindIndex(h => h.AvgHr == MinHRS);
                DateTimeMaxHRS = AnaEvent.AllEvents["房速"][maxHRIndex].StartTime;
                DateTimeMinHRS = AnaEvent.AllEvents["房速"][minHRIndex].StartTime;

            }
            EscapeS = AnaEvent.AllEvents.ContainsKey("房性逸搏") ? AnaEvent.AllEvents["房性逸搏"].Count.ToString() : "0";
        }

        private void GetPaceInfo()
        {
            List<BeatInfo> beatInfoList = AnaValues.CachedAllBeats;
            string beatSequence = AnaEvent.GetBeatSequence(beatInfoList);
            PaceReportInfo paceReportInfo = AnaCode.GetPaceReportInfo(beatSequence);
            SDNN = paceReportInfo.TotalCount.ToString();
            SDNNIndex = paceReportInfo.APCount.ToString();
            pNN50 = paceReportInfo.VPCount.ToString();
            RMSSD = paceReportInfo.AVPCount.ToString();
            Triindx = "";
            TP = paceReportInfo.TotalPercent.ToString();
            ULF = paceReportInfo.APPercent.ToString();
            VLF = paceReportInfo.VPPercent.ToString();
            LF = paceReportInfo.AVPPercent.ToString();
            HF = "";
            LF_HF = "";
        }

        private void GetHRVInfo()
        {
            HRVReportInfo hrvReportInfo = AnaCode.GetHRVReportInfo();
            if (hrvReportInfo == null) return;
            SDNN = hrvReportInfo.sdnn;
            SDNNIndex = hrvReportInfo.sdnnIndex;
            SDANN = hrvReportInfo.sdann;
            pNN50 = hrvReportInfo.pnn50;
            RMSSD = hrvReportInfo.rmssd;
            Triindx = hrvReportInfo.hrvTriangularIndex;
            TP = hrvReportInfo.total_power;
            ULF = hrvReportInfo.ulf;
            VLF = hrvReportInfo.vlf;
            LF = hrvReportInfo.lf;
            HF = hrvReportInfo.hf;
            LF_HF = Math.Round(Convert.ToDouble(hrvReportInfo.lf) / Convert.ToDouble(hrvReportInfo.hf), 1).ToString();

        }

        private void GetSTInfo()
        {
            STReportInfo stReportInfo = AnaCode.GetSTReportInfo();
            stLowCount = stReportInfo.stLowCount;
            stLowest = stReportInfo.stLowest;
            stLowestTime = stReportInfo.stLowestTime;
            stLowestLead = stReportInfo.stLowestLead;
            stLowLeadDuration = stReportInfo.stLowLeadDuration;
            stUpCount = stReportInfo.stUpCount;
            stUpest = stReportInfo.stUpest;
            stUpestTime = stReportInfo.stUpestTime;
            stUpestLead = stReportInfo.stUpestLead;
            stUpLeadDuration = stReportInfo.stUpLeadDuration;
            stTotalDuration = stReportInfo.stTotalDuration;
        }

        private void GetAFInfo()
        {
            AFReportInfo afReportInfo = AnaCode.GetAfReportInfo();
            AFTotalDuration = afReportInfo.AFTotalDuration;
            AFPercent = afReportInfo.AFPercent;
            AFDuration = afReportInfo.AFDuration;
            AFCount = afReportInfo.AFCount;
            AFPercent2 = afReportInfo.AFPercent2;
            AFDuration2 = afReportInfo.AFDuration2;
            AFCount2 = afReportInfo.AFCount2;
        }

        private void CalcData()
        {
            AnaEvent.CalcAvgHR(AnaValues.CachedAllBeats);
            TotalBeats = AnaEvent.TotalBeatCount;
            int totalSeconds = AnaValues.holterConfig.Total / AnaValues.holterConfig.SampleRate;
            TimeSpan totalTime = TimeSpan.FromSeconds(totalSeconds);
            RecordTimeStr = (int)totalTime.TotalHours + CommonValues.Langs["小时"] + totalTime.Minutes + CommonValues.Langs["分钟"];

            AvgHR = AnaEvent.TotalAvgHR;
            MaxHR = AnaEvent.AllEvents["最快心率"].Count > 0 ? AnaEvent.AllEvents["最快心率"][0].AvgHr : 0;
            DateStringMaxHR = AnaEvent.AllEvents["最快心率"].Count > 0 ? AnaEvent.AllEvents["最快心率"][0].StartTime.Split(' ')[1] : "";
            MinHR = AnaEvent.AllEvents["最慢心率"].Count > 0 ? AnaEvent.AllEvents["最慢心率"][0].AvgHr : 0;
            DateStringMinHR = AnaEvent.AllEvents["最慢心率"].Count > 0 ? AnaEvent.AllEvents["最慢心率"][0].StartTime.Split(' ')[1] : "";

            TotalBeatsV = AnaEvent.CalcBeatCount(AnaValues.CachedAllBeats, 'V') + AnaEvent.CalcBeatCount(AnaValues.CachedAllBeats, 'T');
            SingleV = AnaEvent.AllEvents.ContainsKey("单发室早") ? AnaEvent.AllEvents["单发室早"].Count : 0;
            PairedV = AnaEvent.AllEvents.ContainsKey("成对室早") ? AnaEvent.AllEvents["成对室早"].Count : 0;
            BigeminyV = AnaEvent.AllEvents.ContainsKey("室性二联律") ? AnaEvent.AllEvents["室性二联律"].Count : 0;
            TrigeminyV = AnaEvent.AllEvents.ContainsKey("室性三联律") ? AnaEvent.AllEvents["室性三联律"].Count : 0;
            TachycardiaV = AnaEvent.AllEvents.ContainsKey("室速") ? AnaEvent.AllEvents["室速"].Count : 0;
            RonT = AnaEvent.AllEvents.ContainsKey("R on T") ? AnaEvent.AllEvents["R on T"].Count : 0;

            TotalBeatsS = AnaEvent.CalcBeatCount(AnaValues.CachedAllBeats, 'S');
            SingleS = AnaEvent.AllEvents.ContainsKey("单发房早") ? AnaEvent.AllEvents["单发房早"].Count : 0;
            PairedS = AnaEvent.AllEvents.ContainsKey("成对房早") ? AnaEvent.AllEvents["成对房早"].Count : 0;
            BigeminyS = AnaEvent.AllEvents.ContainsKey("房性二联律") ? AnaEvent.AllEvents["房性二联律"].Count : 0;
            TrigeminyS = AnaEvent.AllEvents.ContainsKey("房性三联律") ? AnaEvent.AllEvents["房性三联律"].Count : 0;
            TachycardiaS = AnaEvent.AllEvents.ContainsKey("房速") ? AnaEvent.AllEvents["房速"].Count : 0;

            AsystoleCount = 0;
            MaxAsystoleStr = 0;
            DateStringMaxAsystole = "";
            if (AnaEvent.AllEvents["停搏"].Count > 0)
            {
                AsystoleCount = AnaEvent.AllEvents["停搏"].Count;
                MaxAsystoleStr = Math.Round((double)AnaEvent.AllEvents["停搏"][0].AvgRR / 1000f, 2);
                DateStringMaxAsystole = AnaEvent.AllEvents["停搏"][0].StartTime.Split(' ')[1];
            }

            LongRRCount = 0;
            if (AnaEvent.AllEvents["长RR间期"].Count > 0)
            {
                LongRRCount = AnaEvent.AllEvents["长RR间期"].Count;
            }

            LongestRRStr = 0;
            DateStringLongestRR = "";
            if (AnaEvent.AllEvents["最长RR间期"].Count > 0)
            {
                LongestRRStr = Math.Round((double)AnaEvent.AllEvents["最长RR间期"][0].AvgRR / 1000f, 2);
                DateStringLongestRR = AnaEvent.AllEvents["最长RR间期"][0].StartTime.Split(' ')[1];
            }

            getEventInfo();
            bool bPace = AnaValues.holterConfig.AnalysisSetting.PaceAnalysisEnable;
            if (bPace)
            {
                GetPaceInfo();
            }
            else
            {
                GetHRVInfo();
            }
            GetSTInfo();
            GetAFInfo();
        }
        private void GetHRVTimeDomainInfo()
        {
            HRVTimeDomainInfo.Clear();

            JObject RRPlotAllInfo = new JObject();
            List<double> AllInfo = HRVCommonMethod.GetRRPlotInfo(0);
            RRPlotAllInfo["RRPlotInfo"] = JToken.FromObject(AllInfo);

            JObject RRPlotDayInfo = new JObject();
            List<double> DayInfo = HRVCommonMethod.GetRRPlotInfo(1);
            RRPlotDayInfo["RRPlotInfo"] = JToken.FromObject(DayInfo);

            JObject RRPlotNightInfo = new JObject();
            List<double> NightInfo = HRVCommonMethod.GetRRPlotInfo(2);
            RRPlotNightInfo["RRPlotInfo"] = JToken.FromObject(NightInfo);

            JObject HRVTimeDomainListInfo = new JObject();
            AEcgHandler.CalculateHRV();
            HRVTimeDomainListInfo["ListInfo"] = JToken.FromObject(HRVCommonMethod.GetHRVTimeDomainList());

            List<JObject> infoList = new List<JObject>();
            infoList.Add(RRPlotAllInfo);
            infoList.Add(RRPlotDayInfo);
            infoList.Add(RRPlotNightInfo);
            infoList.Add(HRVTimeDomainListInfo);
            HRVTimeDomainInfo.Add(infoList);
        }
        private void GetHRVFrequencyDomainInfo()
        {
            HRVFrequencyDomainInfo.Clear();
            List<HRVFreqDomainInfo> hrvMaxMinFreqDomainInfoList = new List<HRVFreqDomainInfo>();

            List<List<AEcgHrvPsdInfo>> hrvPsdInfoList = new List<List<AEcgHrvPsdInfo>>();
            int maxIndex = 0;
            int minIndex = 0;
            AEcgHandler.CalculateHRV();
            List<HRVFreqDomainInfo> hrvFreqDomainInfoList = HRVCommonMethod.GetHrvFreqDomainList();
            (hrvPsdInfoList, maxIndex, minIndex) = HRVCommonMethod.GetHrvPsdList(hrvFreqDomainInfoList);

            hrvMaxMinFreqDomainInfoList.Add(hrvFreqDomainInfoList[maxIndex]);
            hrvMaxMinFreqDomainInfoList.Add(hrvFreqDomainInfoList[minIndex]);

            JObject hrvPsdInfoMax = new JObject();
            hrvPsdInfoMax["info"] = JToken.FromObject(hrvPsdInfoList[0]);

            JObject hrvPsdInfoMin = new JObject();
            hrvPsdInfoMin["info"] = JToken.FromObject(hrvPsdInfoList[1]);

            JObject hrvMaxMinFreqDomainInfo = new JObject();
            hrvMaxMinFreqDomainInfo["info"] = JToken.FromObject(hrvMaxMinFreqDomainInfoList);

            JObject hrvFreqDomainInfo = new JObject();
            hrvFreqDomainInfo["info"] = JToken.FromObject(hrvFreqDomainInfoList);

            List<JObject> infoList = new List<JObject>();
            infoList.Add(hrvPsdInfoMax);
            infoList.Add(hrvPsdInfoMin);
            infoList.Add(hrvMaxMinFreqDomainInfo);
            infoList.Add(hrvFreqDomainInfo);
            HRVFrequencyDomainInfo.Add(infoList);
        }
        private void GetHRVAnalysisInfo()
        {
            HRVAnalysisInfo.Clear();
            JObject hrvReportInfoObj = new JObject();
            hrvReportInfoObj["sdnn"] = SDNN;
            hrvReportInfoObj["sdann"] = SDANN;
            hrvReportInfoObj["sdnnIndex"] = SDNNIndex;
            hrvReportInfoObj["rmssd"] = RMSSD;
            hrvReportInfoObj["pnn50"] = pNN50;
            hrvReportInfoObj["hrvTriangularIndex"] = Triindx;

            hrvReportInfoObj["total_power"] = TP;
            hrvReportInfoObj["vlf"] = VLF;
            hrvReportInfoObj["lf"] = LF;
            hrvReportInfoObj["hf"] = HF;
            hrvReportInfoObj["lf_hf"] = LF_HF;

            string HrvFilePath = "";
            if (!string.IsNullOrEmpty(AnaValues.RootPath))
            {
                HrvFilePath = Path.Combine(AnaValues.RootPath, AnaValues.HRVAdviceFileName);
            }

            if (!File.Exists(HrvFilePath))
            {
                hrvReportInfoObj["advice"] = "SDNN " + SDNN + "ms\n";

                int sdnnValue = Convert.ToInt32(SDNN);
                if (sdnnValue >= 100)
                {
                    hrvReportInfoObj["advice"] += "心率变异性正常，自主神经功能状态良好\n";
                }
                else if (sdnnValue >= 50 && sdnnValue < 100)
                {
                    hrvReportInfoObj["advice"] += "心率变异性降低，自主神经功能减退\n";
                }
                else
                {
                    hrvReportInfoObj["advice"] += "心率变异性低，自主神经调节功能差，心血管存在风险\n";
                }
                hrvReportInfoObj["advice"] += "请结合临床";
            }
            else
            {
                // 读取JSON文件内容
                string jsonString = File.ReadAllText(HrvFilePath, Encoding.UTF8);
                var hrvAdviceJson = JObject.Parse(jsonString);
                if (hrvAdviceJson["advice"] != null)
                {
                    hrvReportInfoObj["advice"] = hrvAdviceJson["advice"].ToString();
                }
            }

            List<JObject> infoList = new List<JObject>();
            infoList.Add(hrvReportInfoObj);
            HRVAnalysisInfo.Add(infoList);
        }
        private void GetSTListInfo()
        {
            int dateFormat = CommonValues.SystemSettings.baseSets.dateFormat;

            STListInfo.Clear();
            List<AEcgStEventInfo> stEventInfoList = GetStEventInfo();
            if (stEventInfoList != null)
            {
                int prePageRow = 40;
                int pageCount = (stEventInfoList.Count + prePageRow - 1) / prePageRow;
                for (int i = 0; i < pageCount; i++)
                {
                    List<JObject> infoList = new List<JObject>();
                    for (int j = i * prePageRow; j < (i + 1) * prePageRow; j++)
                    {
                        if (j == stEventInfoList.Count) break;
                        JObject stInfo = new JObject();

                        if (dateFormat == 0)
                        {
                            stInfo["StartTime"] = AnaCode.getDataTimeFromSample(stEventInfoList[j].startPosition).ToString("HH:mm:ss MM-dd");
                        }
                        else
                        {
                            stInfo["StartTime"] = AnaCode.getDataTimeFromSample(stEventInfoList[j].startPosition).ToString("HH:mm:ss dd-MM");
                        }
                        stInfo["Duration"] = Math.Round((double)stEventInfoList[j].durationTime / AnaValues.holterConfig.SampleRate / 60, 2);
                        stInfo["StPointVoltage"] = ((float)Math.Round(stEventInfoList[j].stPointVoltage / 1000, 2)).ToString();
                        stInfo["STLead"] = AnaValues.LeadIdxToNameMap[stEventInfoList[j].lead];
                        infoList.Add(stInfo);
                    }
                    STListInfo.Add(infoList);
                }
            }

        }

        private async void GetSTTTrendWaveInfo(string type)
        {
            if (type == "st")
            {
                STTrendWaveInfo.Clear();
            }
            else
            {
                TTrendWaveInfo.Clear();
            }
            while (AnaCode.IsAnalysisST) { await Task.Delay(10); }

            if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
            {
                startTime = DateTime.Today;
            }
            List<BeatInfo> beats = AnaValues.CachedAllBeats;
            BeatInfo lastBeat = beats[beats.Count - 1];
            int minCount = AnaValues.minuteHR.Count;
            for (int i = 0; i < AnaValues.holterConfig.Lead; i++)
            {
                var aecgStxInfo = AEcgHandler.GetStxInfo(i);
                List<short> stBeatPosList = new List<short>();
                List<short> tBeatPosList = new List<short>();
                int beatIdx = 0;
                List<JObject> leadStxInfo = new List<JObject>();
                for (int j = 0; j < minCount; j++)
                {
                    int startSample = AnaCode.getSampleFromDateTime(startTime.AddSeconds(j * 60));
                    int endSample = AnaCode.getSampleFromDateTime(startTime.AddSeconds((j + 1) * 60));
                    if (startSample > lastBeat.StartIndex)
                    {
                        JObject minStxInfo = new JObject();
                        minStxInfo["StartTime"] = startTime.AddSeconds(j * 60);
                        minStxInfo["STAmp"] = 0;
                        minStxInfo["TAmp"] = 0;
                        leadStxInfo.Add(minStxInfo);
                        continue;
                    }

                    int stAmpCount = 0;
                    int stAmpTotal = 0;
                    int tAmpTotal = 0;
                    for (int p = beatIdx; p < aecgStxInfo.Count; p++)
                    {
                        if (aecgStxInfo[p].stBeatPos >= startSample && aecgStxInfo[p].stBeatPos < endSample)
                        {
                            stBeatPosList.Add(aecgStxInfo[p].stAmp);
                            tBeatPosList.Add(aecgStxInfo[p].tWaveAmp);

                            stAmpTotal += aecgStxInfo[p].stAmp;
                            tAmpTotal += aecgStxInfo[p].tWaveAmp;
                            ++stAmpCount;
                        }
                        else
                        {
                            if (stAmpCount != 0)
                            {
                                beatIdx = p;
                            }

                            break;
                        }
                    }

                    if (stAmpCount != 0)
                    {
                        JObject minStxInfo = new JObject();
                        minStxInfo["StartTime"] = startTime.AddSeconds(j * 60);
                        minStxInfo["STAmp"] = (double)stAmpTotal / stAmpCount;
                        minStxInfo["TAmp"] = (double)tAmpTotal / stAmpCount;
                        //minStxInfo.STAmp = AnaCode.Median(stBeatPosList);
                        //minStxInfo.TAmp = AnaCode.Median(tBeatPosList);

                        leadStxInfo.Add(minStxInfo);
                    }
                    else
                    {
                        JObject minStxInfo = new JObject();
                        minStxInfo["StartTime"] = startTime.AddSeconds(j * 60);
                        minStxInfo["STAmp"] = 0;
                        minStxInfo["TAmp"] = 0;

                        leadStxInfo.Add(minStxInfo);
                    }

                }

                if (type == "st")
                {
                    STTrendWaveInfo.Add(leadStxInfo);
                }
                else
                {
                    TTrendWaveInfo.Add(leadStxInfo);
                }
            }
        }

        private void GetAFListInfo()
        {
            int dateFormat = CommonValues.SystemSettings.baseSets.dateFormat;

            AFListInfo.Clear();
            List<AfInfo> AFInfoList = AnaCode.GetAfList();
            if (AFInfoList != null)
            {
                int prePageRow = 40;
                int pageCount = (AFInfoList.Count + prePageRow - 1) / prePageRow;
                for (int i = 0; i < pageCount; i++)
                {
                    List<JObject> infoList = new List<JObject>();
                    for (int j = i * prePageRow; j < (i + 1) * prePageRow; j++)
                    {
                        if (j == AFInfoList.Count) break;
                        JObject afInfo = new JObject();

                        if (dateFormat == 0)
                        {
                            afInfo["StartTime"] = AFInfoList[j].StartTime.ToString("HH:mm:ss MM-dd");
                        }
                        else
                        {
                            afInfo["StartTime"] = AFInfoList[j].StartTime.ToString("HH:mm:ss dd-MM");
                        }

                        TimeSpan timeSpan = TimeSpan.FromSeconds((double)AFInfoList[j].Duration);
                        afInfo["Duration"] = AnaCode.FormatTimeSpanTo24Hour(timeSpan);
                        afInfo["TypeName"] = AFInfoList[j].TypeName;

                        infoList.Add(afInfo);
                    }
                    AFListInfo.Add(infoList);
                }
            }
        }

        private void GetPaceListInfo()
        {
            PaceListInfo.Clear();
            List<JObject> infoList = new List<JObject>();

            if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
            {
                startTime = DateTime.Today;
            }
            try
            {
                int captureCountTotal = 0;       //夺获失败
                int percepCountTotal = 0;        //感知失败
                int outputCountTotal = 0;        //输出失败
                int dateFormat = CommonValues.SystemSettings.baseSets.dateFormat;

                List<AEcgHandler.AEcgPacemakerEvaluation> paceMakerList = AEcgHandler.GetPacemakerEvaluationInfo();

                int oneHourSamples = 3600 * AnaValues.holterConfig.SampleRate;
                DateTime nextHour = startTime.AddHours(1);
                double firstHourSamples = (nextHour - startTime).TotalSeconds * AnaValues.holterConfig.SampleRate;
                // 获取总时长（小时）
                int totalHours = (int)Math.Ceiling((double)(AnaValues.holterConfig.Total - firstHourSamples) / oneHourSamples) + 1;
                int month = Convert.ToInt32(startTime.ToString("MM"));
                int day = Convert.ToInt32(startTime.ToString("dd"));
                int startHour = Convert.ToInt32(startTime.ToString("HH"));
                for (int hour = 0; hour < totalHours; hour++)
                {
                    JObject hourInfo = new JObject();
                    int timeHour = (hour + startHour) - 24 * ((hour + startHour) / 24);
                    if ((hour + startHour) % 24 == 0)
                    {
                        DateTime tomorrow = startTime.AddDays(1);
                        month = Convert.ToInt32(tomorrow.ToString("MM"));
                        day = Convert.ToInt32(tomorrow.ToString("dd"));
                        if (dateFormat == 0)
                        {
                            hourInfo["Time"] = $"{timeHour:00}:00 {month:00}-{day:00}";
                        }
                        else
                        {
                            hourInfo["Time"] = $"{timeHour:00}:00 {day:00}-{month:00}";
                        }
                    }
                    else
                    {
                        if (hour == 0)
                        {
                            if (dateFormat == 0)
                            {
                                hourInfo["Time"] = $"{timeHour:00}:00 {month:00}-{day:00}";
                            }
                            else
                            {
                                hourInfo["Time"] = $"{timeHour:00}:00 {day:00}-{month:00}";
                            }
                        }
                        else
                        {
                            hourInfo["Time"] = $"{timeHour:00}:00";
                        }
                    }
                    // 计算总心搏数
                    hourInfo["TotalBeat"] = AnaEvent.TotalBeatCount;

                    List<BeatInfo> hourlyBeats = AnaEvent.AllHourlyBeats[hour];
                    // 计算当前小时的心搏数据
                    hourInfo["MinHR"] = CalcHourListInfo.GetHourFastestLowestHr(hour, "最慢心率");
                    // 计算平均心率
                    hourInfo["AvgHR"] = AnaEvent.CalcAvgHR(hourlyBeats);

                    hourInfo["MaxHR"] = CalcHourListInfo.GetHourFastestLowestHr(hour, "最快心率");

                    string beatSequenceHour = AnaEvent.GetBeatSequence(hourlyBeats);
                    PaceReportInfo paceReportInfoHour = AnaCode.GetPaceReportInfo(beatSequenceHour);
                    hourInfo["totalCount"] = paceReportInfoHour.TotalCount;

                    int captureCount = 0;       //夺获失败
                    int percepCount = 0;        //感知失败
                    int outputCount = 0;        //输出失败

                    for (int i = 0; i < paceMakerList.Count; i++)
                    {
                        if (hour == 0)
                        {
                            if (paceMakerList[i].rWavePosition < firstHourSamples)
                            {
                                //0-感知失败，1-输出失败，2-夺获失败
                                switch (paceMakerList[i].failureType)
                                {
                                    case 0:
                                        ++percepCount;
                                        ++percepCountTotal;
                                        break;
                                    case 1:
                                        ++outputCount;
                                        ++outputCountTotal;
                                        break;
                                    case 2:
                                        ++captureCount;
                                        ++captureCountTotal;
                                        break;
                                }
                            }
                        }
                        else if (hour == totalHours - 1)
                        {
                            if (paceMakerList[i].rWavePosition > firstHourSamples + (hour - 1) * oneHourSamples && paceMakerList[i].rWavePosition < AnaValues.holterConfig.Total)
                            {
                                //0-感知失败，1-输出失败，2-夺获失败
                                switch (paceMakerList[i].failureType)
                                {
                                    case 0:
                                        ++percepCount;
                                        ++percepCountTotal;

                                        break;
                                    case 1:
                                        ++outputCount;
                                        ++outputCountTotal;

                                        break;
                                    case 2:
                                        ++captureCount;
                                        ++captureCountTotal;

                                        break;
                                }
                            }
                        }
                        else
                        {
                            if (paceMakerList[i].rWavePosition > firstHourSamples + (hour - 1) * oneHourSamples && paceMakerList[i].rWavePosition < firstHourSamples + hour * oneHourSamples)
                            {
                                //0-感知失败，1-输出失败，2-夺获失败
                                switch (paceMakerList[i].failureType)
                                {
                                    case 0:
                                        ++percepCount;
                                        ++percepCountTotal;

                                        break;
                                    case 1:
                                        ++outputCount;
                                        ++outputCountTotal;

                                        break;
                                    case 2:
                                        ++captureCount;
                                        ++captureCountTotal;

                                        break;
                                }
                            }
                        }
                    }
                    hourInfo["capture"] = captureCount;
                    hourInfo["percep"] = percepCount;
                    hourInfo["output"] = outputCount;

                    hourInfo["apCount"] = paceReportInfoHour.APCount;
                    hourInfo["vpCount"] = paceReportInfoHour.VPCount;
                    hourInfo["avpCount"] = paceReportInfoHour.AVPCount;

                    hourInfo["Asystole"] = CalcHourListInfo.CalcEventCount(hour, "停搏");

                    infoList.Add(hourInfo);
                }

                int maxHR = Convert.ToInt32(infoList[0]["MaxHR"]);
                int minHR = Convert.ToInt32(infoList[0]["MinHR"]);
                JObject hourInfoTotal = new JObject();
                hourInfoTotal["Time"] = CommonValues.Langs["总数"];
                AnaEvent.CalcAvgHR(AnaValues.CachedAllBeats);
                hourInfoTotal["TotalBeat"] = AnaEvent.TotalBeatCount;
                int asystole = 0;
                for (int i = 0; i < infoList.Count; i++)
                {
                    maxHR = Math.Max(maxHR, Convert.ToInt32(infoList[i]["MaxHR"]));
                    if (Convert.ToInt32(infoList[i]["MinHR"]) > 0)
                    {
                        minHR = Math.Min(minHR, Convert.ToInt32(infoList[i]["MinHR"]));
                    }
                    asystole += Convert.ToInt32(infoList[i]["Asystole"]);
                }
                hourInfoTotal["MinHR"] = minHR;
                hourInfoTotal["AvgHR"] = AnaEvent.CalcAvgHR(AnaValues.CachedAllBeats);
                hourInfoTotal["MaxHR"] = maxHR;

                List<BeatInfo> beatInfoList = AnaValues.CachedAllBeats;
                string beatSequence = AnaEvent.GetBeatSequence(beatInfoList);
                PaceReportInfo paceReportInfo = AnaCode.GetPaceReportInfo(beatSequence);

                hourInfoTotal["totalCount"] = paceReportInfo.TotalCount;

                hourInfoTotal["capture"] = captureCountTotal;
                hourInfoTotal["percep"] = percepCountTotal;
                hourInfoTotal["output"] = outputCountTotal;

                hourInfoTotal["apCount"] = paceReportInfo.APCount;
                hourInfoTotal["vpCount"] = paceReportInfo.VPCount;
                hourInfoTotal["avpCount"] = paceReportInfo.AVPCount;


                hourInfoTotal["Asystole"] = asystole;
                infoList.Add(hourInfoTotal);
                PaceListInfo.Add(infoList);
            }
            catch (Exception e)
            {
                Logger.CreateLog(e, "心电报告");
            }
        }

        private void ReadData()
        {
            // 检查文件是否存在
            if (!File.Exists(SummaryFilePath))
            {
                return;
            }

            try
            {
                // 读取JSON文件内容
                string jsonString = File.ReadAllText(SummaryFilePath, Encoding.UTF8);
                var summaryValuesJson = JObject.Parse(jsonString);

                TotalBeats = JsonObjToInt(summaryValuesJson["tbxTotalBeats"]?.ToString());
                RecordTimeStr = summaryValuesJson["tbxRecordTime"]?.ToString();

                AvgHR = JsonObjToInt(summaryValuesJson["tbxAvgHR"]?.ToString());
                MaxHR = JsonObjToInt(summaryValuesJson["tbxMaxHR"]?.ToString());
                DateStringMaxHR = summaryValuesJson["MaxHRTime"]?.ToString().Split(' ')[1];
                MinHR = JsonObjToInt(summaryValuesJson["tbxMinHR"]?.ToString());
                DateStringMinHR = summaryValuesJson["MinHRTime"]?.ToString().Split(' ')[1];

                TotalBeatsV = JsonObjToInt(summaryValuesJson["tbxTotalBeatsV"]?.ToString());
                SingleV = JsonObjToInt(summaryValuesJson["tbxSingleV"]?.ToString());
                PairedV = JsonObjToInt(summaryValuesJson["tbxPairedV"]?.ToString());
                BigeminyV = JsonObjToInt(summaryValuesJson["tbxBigeminyV"]?.ToString());
                TrigeminyV = JsonObjToInt(summaryValuesJson["tbxTrigeminyV"]?.ToString());
                TachycardiaV = JsonObjToInt(summaryValuesJson["tbxTachycardiaV"]?.ToString());
                RonT = JsonObjToInt(summaryValuesJson["tbxRonT"]?.ToString());

                TotalBeatsS = JsonObjToInt(summaryValuesJson["tbxTotalBeatsS"]?.ToString());
                SingleS = JsonObjToInt(summaryValuesJson["tbxSingleS"]?.ToString());
                PairedS = JsonObjToInt(summaryValuesJson["tbxPairedS"]?.ToString());
                BigeminyS = JsonObjToInt(summaryValuesJson["tbxBigeminyS"]?.ToString());
                TrigeminyS = JsonObjToInt(summaryValuesJson["tbxTrigeminyS"]?.ToString());
                TachycardiaS = JsonObjToInt(summaryValuesJson["tbxTachycardiaS"]?.ToString());

                AsystoleCount = JsonObjToInt(summaryValuesJson["tbxAsystoleCount"]?.ToString());
                MaxAsystoleStr = Math.Round(JsonObjToDouble(summaryValuesJson["tbxMaxAsystole"]?.ToString()) / 1000f, 2);
                DateStringMaxAsystole = summaryValuesJson["MaxAsystoleTime"]?.ToString().Split(' ')[1];

                if (AnaEvent.AllEvents["长RR间期"].Count > 0)
                {
                    LongRRCount = AnaEvent.AllEvents["长RR间期"].Count;
                }

                LongestRRStr = JsonObjToDouble(summaryValuesJson["longestRRStr"]?.ToString());
                DateStringLongestRR = summaryValuesJson["dateStringLongestRR"]?.ToString();

                AnaTimeStr = summaryValuesJson["tbxAnaTime"]?.ToString();

                string mainCH = AnaValues.LeadIdxToNameMap[AnaValues.holterConfig.MainChannel];
                string secondCH1 = AnaValues.LeadIdxToNameMap[AnaValues.holterConfig.SecondChannel1];
                string secondCH2 = AnaValues.LeadIdxToNameMap[AnaValues.holterConfig.SecondChannel2];

                AnaLeadStr = $"{mainCH} {secondCH1} {secondCH2}";

                Tachycardia = summaryValuesJson["tbxTachycardia"]?.ToString();
                Bradycardia = summaryValuesJson["tbxBradycardia"]?.ToString();

                LongestTachycardiaV = summaryValuesJson["tbxLongestTachycardiaV"]?.ToString();
                DateTimeLongestTachycardiaV = summaryValuesJson["tbxLongestTachycardiaTimeV"]?.ToString();

                LongestTachycardiaDurationV = JsonObjToDouble(summaryValuesJson["tbxLongestTachycardiaDurationV"]?.ToString());
                MaxHRV = JsonObjToInt(summaryValuesJson["tbxMaxTachycardiaV"]?.ToString());
                MinHRV = JsonObjToInt(summaryValuesJson["tbxMinTachycardiaV"]?.ToString());


                DateTimeMaxHRV = summaryValuesJson["tbxMaxTachycardiaTimeV"]?.ToString();
                DateTimeMinHRV = summaryValuesJson["tbxMinTachycardiaTimeV"]?.ToString();

                EscapeV = summaryValuesJson["EscapeV"]?.ToString();

                LongestTachycardiaS = summaryValuesJson["tbxLongestTachycardiaS"]?.ToString();
                DateTimeLongestTachycardiaS = summaryValuesJson["tbxLongestTachycardiaTimeS"]?.ToString();
                LongestTachycardiaDurationS = JsonObjToDouble(summaryValuesJson["tbxLongestTachycardiaDurationS"]?.ToString());


                MaxHRS = JsonObjToInt(summaryValuesJson["tbxMaxTachycardiaS"]?.ToString());
                MinHRS = JsonObjToInt(summaryValuesJson["tbxMinTachycardiaS"]?.ToString());

                DateTimeMaxHRS = summaryValuesJson["tbxMaxTachycardiaTimeS"]?.ToString();
                DateTimeMinHRS = summaryValuesJson["tbxMinTachycardiaTimeS"]?.ToString();

                EscapeS = summaryValuesJson["EscapeS"]?.ToString();


                SDNN = summaryValuesJson["tbxSDNN"]?.ToString();
                SDNNIndex = summaryValuesJson["tbxSDNNindex"]?.ToString();
                SDANN = summaryValuesJson["tbxSDANN"]?.ToString();
                pNN50 = summaryValuesJson["tbxpNN50"]?.ToString();
                RMSSD = summaryValuesJson["tbxRMSSD"]?.ToString();
                Triindx = summaryValuesJson["tbxTriindex"]?.ToString();
                TP = summaryValuesJson["tbxTP"]?.ToString();
                ULF = summaryValuesJson["tbxULF"]?.ToString();
                VLF = summaryValuesJson["tbxVLF"]?.ToString();
                LF = summaryValuesJson["tbxLF"]?.ToString();
                HF = summaryValuesJson["tbxHF"]?.ToString();
                LF_HF = summaryValuesJson["tbxLFHF"]?.ToString();

                stLowCount = JsonObjToInt(summaryValuesJson["tbxSTLowCount"]?.ToString());
                stLowest = summaryValuesJson["tbxLowest"]?.ToString();
                stLowestTime = summaryValuesJson["lowestTime"]?.ToString().Split(' ')[1];
                stLowestLead = summaryValuesJson["lowestLead"]?.ToString();
                stLowLeadDuration = summaryValuesJson["tbxLowestDuration"]?.ToString();
                stUpCount = JsonObjToInt(summaryValuesJson["tbxSTUpCount"]?.ToString());
                stUpest = summaryValuesJson["tbxUpest"]?.ToString();
                stUpestTime = summaryValuesJson["upestTime"]?.ToString().Split(' ')[1];
                stUpestLead = summaryValuesJson["upestLead"]?.ToString();
                stUpLeadDuration = summaryValuesJson["tbxUpestDuration"]?.ToString();
                STReportInfo stReportInfo = AnaCode.GetSTReportInfo();
                stTotalDuration = stReportInfo.stTotalDuration;

                AFPercent = summaryValuesJson["tbxAFPrecent"]?.ToString();
                AFDuration = summaryValuesJson["tbxAFDuration"]?.ToString();
                AFCount = JsonObjToInt(summaryValuesJson["tbxAFCount"]?.ToString());
                AFPercent2 = summaryValuesJson["tbxAFLPrecent"]?.ToString();
                AFDuration2 = summaryValuesJson["tbxAFLDuration"]?.ToString();
                AFCount2 = JsonObjToInt(summaryValuesJson["tbxAFLCount"]?.ToString());
                AFTotalDuration = AnaCode.FormatSeconds(AnaCode.TryHmsToSeconds(AFDuration) + AnaCode.TryHmsToSeconds(AFDuration2));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int JsonObjToInt(string strValue)
        {
            int ret = 0;
            if (!string.IsNullOrEmpty(strValue))
            {
                int.TryParse(strValue, out ret);
            }
            return ret;
        }

        private double JsonObjToDouble(string strValue)
        {
            double ret = 0f;
            if (!string.IsNullOrEmpty(strValue))
            {
                double.TryParse(strValue, out ret);
            }
            return ret;
        }

        private void setSummaryInfo()
        {
            this.summarize.Text = "";

            int adviceIdx = 1;

            string tempText = CommonValues.Langs["总心搏数{0}个，记录时长{1}"];
            tempText = string.Format(tempText, TotalBeats, RecordTimeStr) + "\n";

            string templateInfo = tempText;

            tempText = CommonValues.Langs["窦性心律，平均心率{1}次/分，最快心率{2}次/分，发生于{3}，最慢心率{4}次/分，发生于{5}"];
            tempText = string.Format(tempText, adviceIdx, AvgHR, MaxHR, DateStringMaxHR, MinHR, DateStringMinHR);
            templateInfo += tempText;
            if (AFCount + AFCount2 > 0)
            {
                tempText = CommonValues.Langs["。房颤(房扑)发作阵次{0}阵,房颤(房扑)总时长{1}"];
                tempText = string.Format(tempText, AFCount + AFCount2, AFTotalDuration);

                templateInfo += tempText;
            }

            templateInfo += (CommonValues.Langs["。"] + "\n");

            ++adviceIdx;

            if (TotalBeatsV != 0)
            {
                tempText = CommonValues.Langs["室性早搏{1}个"];
                tempText = string.Format(tempText, adviceIdx, TotalBeatsV);
                templateInfo += tempText;
                if (SingleV != 0)
                {
                    tempText = CommonValues.Langs["，单发室早{0}个"];
                    tempText = string.Format(tempText, SingleV);
                    templateInfo += tempText;
                }
                if (PairedV != 0)
                {
                    tempText = CommonValues.Langs["，成对室早{0}阵"];
                    tempText = string.Format(tempText, PairedV);
                    templateInfo += tempText;
                }
                if (BigeminyV != 0)
                {
                    tempText = CommonValues.Langs["，室性二联律{0}阵"];
                    tempText = string.Format(tempText, BigeminyV);
                    templateInfo += tempText;
                }
                if (TrigeminyV != 0)
                {
                    tempText = CommonValues.Langs["，室性三联律{0}阵"];
                    tempText = string.Format(tempText, TrigeminyV);
                    templateInfo += tempText;
                }
                if (TachycardiaV != 0)
                {
                    tempText = CommonValues.Langs["，室速{0}阵"];
                    tempText = string.Format(tempText, TachycardiaV);
                    templateInfo += tempText;
                }
                if (RonT != 0)
                {
                    tempText = CommonValues.Langs["，R on T{0}个"];
                    tempText = string.Format(tempText, RonT);
                    templateInfo += tempText;
                }

                templateInfo += (CommonValues.Langs["。"] + "\n");

                ++adviceIdx;
            }

            if (TotalBeatsS != 0)
            {
                tempText = CommonValues.Langs["房性早搏{1}次"];
                tempText = string.Format(tempText, adviceIdx, TotalBeatsS);
                templateInfo += tempText;
                if (SingleS != 0)
                {
                    tempText = CommonValues.Langs["，单发房早{0}次"];
                    tempText = string.Format(tempText, SingleS);
                    templateInfo += tempText;
                }
                if (PairedS != 0)
                {
                    tempText = CommonValues.Langs["，成对房早{0}阵"];
                    tempText = string.Format(tempText, PairedS);
                    templateInfo += tempText;
                }
                if (BigeminyS != 0)
                {
                    tempText = CommonValues.Langs["，房性二联律{0}阵"];
                    tempText = string.Format(tempText, BigeminyS);
                    templateInfo += tempText;
                }
                if (TrigeminyS != 0)
                {
                    tempText = CommonValues.Langs["，房性三联律{0}阵"];
                    tempText = string.Format(tempText, TrigeminyS);
                    templateInfo += tempText;
                }
                if (TachycardiaS != 0)
                {
                    tempText = CommonValues.Langs["，房速{0}阵"];
                    tempText = string.Format(tempText, TachycardiaS);
                    templateInfo += tempText;
                }

                templateInfo += (CommonValues.Langs["。"] + "\n");

                ++adviceIdx;
            }


            //    tempText = CommonValues.Langs["{0}.停搏次数{1}次，长RR间期{2}个，最长RR间期{3}秒，发生于{4}。"];
            //    tempText = string.Format(tempText, adviceIdx, AsystoleCount, LongRRCount, LongestRRStr, DateStringLongestRR);
            if (LongestRRStr >= 2.0f)
            {
                tempText = CommonValues.Langs["大于等于2.0秒长RR间期有{1}次，最长RR间期{2}秒，发生于{3}。"];
                tempText = string.Format(tempText, adviceIdx, AsystoleCount, LongestRRStr, DateStringLongestRR);
            }
            else
            {
                tempText = CommonValues.Langs["大于等于2.0秒长RR间期有{1}次。"];
                tempText = string.Format(tempText, adviceIdx, AsystoleCount);
            }

            templateInfo += (tempText + "\n");
            ++adviceIdx;

            tempText = CommonValues.Langs["未见/可见ST-T异常改变。"];
            tempText = string.Format(tempText, adviceIdx);
            templateInfo += (tempText + "\n");
            ++adviceIdx;

            tempText = CommonValues.Langs["心率变异性，SDNN：{1}ms(正常参考值范围102-180ms)。"];
            tempText = string.Format(tempText, adviceIdx, SDNN);
            templateInfo += (tempText + "\n");
            ++adviceIdx;
            templateInfo += ("请结合临床。\n");
            this.summarize.AppendText(templateInfo);

        }

        private void initSummaryPrintData()
        {
            SummaryInfo.Clear();
            PageIdxs["summary"] = TotalPages;
            ++TotalPages;
            List<JObject> infoList = new List<JObject>();
            JObject info = new JObject();
            info["bPrintHrvData"] = cbxHRVData.Checked;
            info["TotalBeats"] = TotalBeats;
            info["AvgHR"] = AvgHR;
            info["MaxHR"] = MaxHR;
            info["DateStringMaxHR"] = (DateStringMaxHR != "" && DateStringMaxHR != "0") ? DateStringMaxHR : "";
            info["MinHR"] = MinHR;
            info["DateStringMinHR"] = (DateStringMinHR != "" && DateStringMinHR != "0") ? DateStringMinHR : "";

            info["AsystoleCount"] = AsystoleCount;
            info["MaxAsystoleStr"] = MaxAsystoleStr;
            info["DateStringMaxAsystole"] = (DateStringMaxAsystole != "" && DateStringMaxAsystole != "0") ? DateStringMaxAsystole : "0";
            info["Tachycardia"] = Tachycardia;
            info["Bradycardia"] = Bradycardia;

            info["RecordTimeStr"] = RecordTimeStr;
            info["AnaTimeStr"] = AnaTimeStr;
            info["AnaLeadStr"] = AnaLeadStr;

            info["TotalBeatsV"] = TotalBeatsV;
            info["SingleV"] = SingleV;
            info["PairedV"] = PairedV;
            info["BigeminyV"] = BigeminyV;
            info["TrigeminyV"] = TrigeminyV;
            info["TachycardiaV"] = TachycardiaV;
            info["LongestTachycardiaV"] = LongestTachycardiaV;
            info["DateTimeLongestTachycardiaV"] = (DateTimeLongestTachycardiaV != "" && DateTimeLongestTachycardiaV != "0") ? DateTimeLongestTachycardiaV.Split(' ')[1] : DateTimeLongestTachycardiaV;
            info["LongestTachycardiaDurationV"] = LongestTachycardiaDurationV;
            info["MaxHRV"] = MaxHRV;
            info["MinHRV"] = MinHRV;
            info["DateTimeMaxHRV"] = (DateTimeMaxHRV != "" && DateTimeMaxHRV != "0") ? DateTimeMaxHRV.Split(' ')[1] : DateTimeMaxHRV;
            info["DateTimeMinHRV"] = (DateTimeMinHRV != "" && DateTimeMinHRV != "0") ? DateTimeMinHRV.Split(' ')[1] : DateTimeMinHRV;
            info["EscapeV"] = EscapeV;
            info["RonT"] = RonT;

            info["TotalBeatsS"] = TotalBeatsS;
            info["SingleS"] = SingleS;
            info["PairedS"] = PairedS;
            info["BigeminyS"] = BigeminyS;
            info["TrigeminyS"] = TrigeminyS;
            info["TachycardiaS"] = TachycardiaS;
            info["LongestTachycardiaS"] = LongestTachycardiaS;
            info["DateTimeLongestTachycardiaS"] = (DateTimeLongestTachycardiaS != "" && DateTimeLongestTachycardiaS != "0") ? DateTimeLongestTachycardiaS.Split(' ')[1] : DateTimeLongestTachycardiaS;
            info["LongestTachycardiaDurationS"] = LongestTachycardiaDurationS;
            info["MaxHRS"] = MaxHRS;
            info["MinHRS"] = MinHRS;
            info["DateTimeMaxHRS"] = (DateTimeMaxHRS != "" && DateTimeMaxHRS != "0") ? DateTimeMaxHRS.Split(' ')[1] : DateTimeMaxHRS;
            info["DateTimeMinHRS"] = (DateTimeMinHRS != "" && DateTimeMinHRS != "0") ? DateTimeMinHRS.Split(' ')[1] : DateTimeMinHRS;
            info["EscapeS"] = EscapeS;

            info["SDNN"] = SDNN;
            info["SDANN"] = SDANN;
            info["SDNNIndx"] = SDNNIndex;
            info["pNN50"] = pNN50;
            info["RMSSD"] = RMSSD;
            info["Triindx"] = Triindx;
            info["TP"] = TP;
            info["ULF"] = ULF;
            info["VLF"] = VLF;
            info["LF"] = LF;
            info["HF"] = HF;
            info["LF_HF"] = LF_HF;

            info["stLowCount"] = stLowCount;
            info["stLowest"] = stLowest;
            info["stLowestTime"] = stLowestTime;
            info["stLowestLead"] = stLowestLead;
            info["stLowLeadDuration"] = stLowLeadDuration;
            info["stUpCount"] = stUpCount;
            info["stUpest"] = stUpest;
            info["stUpestTime"] = stUpestTime;
            info["stUpestLead"] = stUpestLead;
            info["stUpLeadDuration"] = stUpLeadDuration;
            info["stTotalDuration"] = stTotalDuration;

            info["AFPercent"] = AFPercent;
            info["AFDuration"] = AFDuration;
            info["AFCount"] = AFCount;
            info["AFPercent2"] = AFPercent2;
            info["AFDuration2"] = AFDuration2;
            info["AFCount2"] = AFCount2;

            infoList.Add(info);
            SummaryInfo.Add(infoList);

            if (CommonValues.SystemSettings.reportSets.displayConclusionType == 0)
            {
                ReportAdvice = summarize.Text;
            }
            else if (CommonValues.SystemSettings.reportSets.displayConclusionType == 1)
            {
                ReportAdvice = advice.Text;
            }
            else
            {
                ReportAdvice = summarize.Text + "\n" + advice.Text;
            }
        }

        private void hourListPrintData()
        {
            PageIdxs["hourlist"] = TotalPages;
            ++TotalPages;
        }

        private void initSegmentPrintData()
        {
            PageIdxs["segment"] = TotalPages;

            getSegmentAllPageInfo();

            TotalPages += SegmentAllPageInfo.Count;
        }

        private void initHRVTimeDomainPrintData()
        {
            PageIdxs["HRVTimeDomain"] = TotalPages;

            GetHRVTimeDomainInfo();

            ++TotalPages;
        }

        private void initHRVFrequencyDomainPrintData()
        {
            PageIdxs["HRVFrequencyDomain"] = TotalPages;

            GetHRVFrequencyDomainInfo();

            ++TotalPages;
        }

        private void initHRVAnalysisPrintData()
        {
            PageIdxs["HRVAnalysis"] = TotalPages;

            GetHRVAnalysisInfo();

            ++TotalPages;
        }

        private void initSTListPrintData()
        {
            PageIdxs["STList"] = TotalPages;

            GetSTListInfo();

            TotalPages += STListInfo.Count;
        }

        private void initSTTrendWavePrintData()
        {
            PageIdxs["STTrendWave"] = TotalPages;

            GetSTTTrendWaveInfo("st");

            ++TotalPages;
        }

        private void initTTrendWavePrintData()
        {
            PageIdxs["TTrendWave"] = TotalPages;

            GetSTTTrendWaveInfo("t");

            ++TotalPages;
        }

        private void initAFListPrintData()
        {
            PageIdxs["AFList"] = TotalPages;

            GetAFListInfo();

            TotalPages += AFListInfo.Count;
        }
        private void initPaceListPrintData()
        {
            PageIdxs["pace"] = TotalPages;
            bool bPace = AnaValues.holterConfig.AnalysisSetting.PaceAnalysisEnable;
            if (bPace)
            {
                GetPaceListInfo();
            }
            ++TotalPages;
        }

        public void initPrintData()
        {
            TotalPages = 0;
            PageIdxs.Clear();
            if (cbxSummarize.Checked)
            {
                initSummaryPrintData();
            }
            if (cbxHourCountList.Checked)
            {
                hourListPrintData();
            }
            if (cbxSegment.Checked)
            {
                initSegmentPrintData();
            }
            if (cbxHRVAnalysis.Checked)
            {
                initHRVAnalysisPrintData();
            }
            if (cbxHRVTimeDomain.Checked)
            {
                initHRVTimeDomainPrintData();
            }
            if (cbxHRVFrequencyDomain.Checked)
            {
                initHRVFrequencyDomainPrintData();
            }
            if (cbxSTList.Checked)
            {
                initSTListPrintData();
            }
            if (cbxSTTrendWave.Checked)
            {
                initSTTrendWavePrintData();
            }
            if (cbxTTrendWave.Checked)
            {
                initTTrendWavePrintData();
            }
            if (cbxAtrialList.Checked)
            {
                initAFListPrintData();
            }
            if (cbxPace.Checked)
            {
                initPaceListPrintData();
            }
        }

        public void initHRVPrintData()
        {
            TotalPages = 0;
            PageIdxs.Clear();

            initHRVAnalysisPrintData();
            initHRVTimeDomainPrintData();
            initHRVFrequencyDomainPrintData();
        }

        private void getSegmentAllPageInfo()
        {
            if (!string.IsNullOrEmpty(AnaValues.RootPath))
            {
                SegmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
            }

            List<JObject> SegmentInfoJsons = OperateSegment.getAllSegmentObj(SegmentFilePath);

            List<JObject> SegmentPageInfos = OperateSegment.getSegmentPageInfo(SegmentInfoJsons);

            SegmentAllPageInfo = OperateSegment.CalcSegmentPrintPageData(SegmentPageInfos, SegmentInfoJsons);
        }

        private void printSegmentWaves()
        {
            bool bHorizontalPrint = false;

            ReportPrintManager = new ReportPrintManager(bHorizontalPrint, TotalPages);
            ReportPrintManager.PageIdxs = PageIdxs;
            ReportPrintManager.AllPageInfo["segment"] = SegmentAllPageInfo;
            ReportPrintManager.SetOnDemandRendering();
            ReportPrintManager.ShowPrintDialog();
        }

        public void printReport()
        {
            bool bHorizontalPrint = false;

            if ((!cbxSummarize.Checked && !cbxHourCountList.Checked) && (cbxSegment.Checked && SegmentAllPageInfo.Count > 0))
            {
                int direction = Convert.ToInt32(SegmentAllPageInfo[0][0]["direction"]);
                bHorizontalPrint = direction == 0 ? true : false;
            }

            ReportPrintManager = new ReportPrintManager(bHorizontalPrint, TotalPages);
            ReportPrintManager.PageIdxs = PageIdxs;
            ReportPrintManager.AllPageInfo["summary"] = SummaryInfo;
            ReportPrintManager.AllPageInfo["hourlist"] = HourListInfo;
            ReportPrintManager.AllPageInfo["segment"] = SegmentAllPageInfo;
            ReportPrintManager.AllPageInfo["HRVAnalysis"] = HRVAnalysisInfo;
            ReportPrintManager.AllPageInfo["HRVTimeDomain"] = HRVTimeDomainInfo;
            ReportPrintManager.AllPageInfo["HRVFrequencyDomain"] = HRVFrequencyDomainInfo;
            ReportPrintManager.AllPageInfo["STList"] = STListInfo;
            ReportPrintManager.AllPageInfo["STTrendWave"] = STTrendWaveInfo;
            ReportPrintManager.AllPageInfo["TTrendWave"] = TTrendWaveInfo;
            ReportPrintManager.AllPageInfo["AFList"] = AFListInfo;
            ReportPrintManager.AllPageInfo["pace"] = PaceListInfo;
            ReportPrintManager.advice = ReportAdvice;
            ReportPrintManager.SetOnDemandRendering();
            ReportPrintManager.ShowPrintDialog();
        }

        private void PrintPreview()
        {
            try
            {
                // 创建自定义打印预览窗体
                using (var previewForm = new BaseForm())
                {
                    previewForm.Text = CommonValues.Langs["打印预览"];
                    previewForm.Size = new Size(1000, 700);
                    previewForm.ShowInTaskbar = false;
                    previewForm.StartPosition = FormStartPosition.CenterScreen;
                    previewForm.WindowState = FormWindowState.Maximized;

                    // 创建PrintDocument
                    var printDocument = new PrintDocument();
                    printDocument.PrintPage += (s, pe) =>
                    {
                        PrintDocument_PrintPageForPreview(s, pe);
                    };

                    // 创建PrintPreviewControl
                    var previewControl = new PrintPreviewControl()
                    {
                        Dock = DockStyle.Fill,
                        Document = printDocument,
                        Zoom = 0.85,
                        Rows = 1,
                        Columns = 1,
                        StartPage = 0
                    };

                    // 创建工具栏
                    var toolPanel = new Panel()
                    {
                        Dock = DockStyle.Top,
                        Height = 50,
                        BackColor = SystemColors.Control
                    };

                    // 创建导航控件
                    var lblPageInfo = new Label()
                    {
                        Text = CommonValues.Langs["第"] + "1" + CommonValues.Langs["页"] + " / " + CommonValues.Langs["共"] + TotalPages + CommonValues.Langs["页"],
                        Location = new Point(10, 15),
                        Size = new Size(150, 20),
                        Font = new System.Drawing.Font("Microsoft YaHei", 9)
                    };

                    var btnPrevPage = new Button()
                    {
                        Text = CommonValues.Langs["上一页"],
                        Location = new Point(170, 10),
                        Size = new Size(60, 30),
                        Font = new System.Drawing.Font("Microsoft YaHei", 9)
                    };

                    var btnNextPage = new Button()
                    {
                        Text = CommonValues.Langs["下一页"],
                        Location = new Point(240, 10),
                        Size = new Size(60, 30),
                        Font = new System.Drawing.Font("Microsoft YaHei", 9)
                    };

                    var btnZoomIn = new Button()
                    {
                        Text = CommonValues.Langs["放大"],
                        Location = new Point(320, 10),
                        Size = new Size(60, 30),
                        Font = new System.Drawing.Font("Microsoft YaHei", 9)
                    };

                    var btnZoomOut = new Button()
                    {
                        Text = CommonValues.Langs["缩小"],
                        Location = new Point(390, 10),
                        Size = new Size(60, 30),
                        Font = new System.Drawing.Font("Microsoft YaHei", 9)
                    };

                    var btnPrint = new Button()
                    {
                        Text = CommonValues.Langs["打印"],
                        Location = new Point(460, 10),
                        Size = new Size(80, 30),
                        Font = new System.Drawing.Font("Microsoft YaHei", 9)
                    };

                    var btnClose = new Button()
                    {
                        Text = CommonValues.Langs["关闭"],
                        Location = new Point(550, 10),
                        Size = new Size(80, 30),
                        Font = new System.Drawing.Font("Microsoft YaHei", 9)
                    };

                    // 添加控件到工具栏
                    toolPanel.Controls.AddRange(new Control[] {
                        lblPageInfo, btnPrevPage, btnNextPage,
                        btnZoomIn, btnZoomOut, btnPrint, btnClose
                    });

                    // 添加控件到窗体
                    previewForm.Controls.AddRange(new Control[] {
                        previewControl, toolPanel
                    });

                    // 绑定事件
                    btnPrevPage.Click += (s, ev) =>
                    {
                        if (previewControl.StartPage > 0)
                        {
                            previewControl.StartPage--;
                            int pageIdx = previewControl.StartPage + 1;
                            string text = CommonValues.Langs["第"] + pageIdx + CommonValues.Langs["页"] + " / " + CommonValues.Langs["共"] + TotalPages + CommonValues.Langs["页"];
                            lblPageInfo.Text = text;
                            btnPrevPage.Enabled = previewControl.StartPage > 0;
                            btnNextPage.Enabled = previewControl.StartPage < TotalPages - 1;
                        }
                    };

                    btnNextPage.Click += (s, ev) =>
                    {
                        if (previewControl.StartPage < TotalPages - 1)
                        {
                            previewControl.StartPage++;
                            int pageIdx = previewControl.StartPage + 1;
                            string text = CommonValues.Langs["第"] + pageIdx + CommonValues.Langs["页"] + " / " + CommonValues.Langs["共"] + TotalPages + CommonValues.Langs["页"];
                            lblPageInfo.Text = text;
                            btnPrevPage.Enabled = previewControl.StartPage > 0;
                            btnNextPage.Enabled = previewControl.StartPage < TotalPages - 1;
                        }
                    };

                    btnZoomIn.Click += (s, ev) =>
                    {
                        previewControl.Zoom = Math.Min(previewControl.Zoom + 0.1, 3.0);
                    };

                    btnZoomOut.Click += (s, ev) =>
                    {
                        previewControl.Zoom = Math.Max(previewControl.Zoom - 0.1, 0.1);
                    };

                    btnPrint.Click += (s, ev) =>
                    {
                        try
                        {
                            if (PrintPreviewType == "All")
                            {
                                initPrintData();
                                printReport();
                            }
                            else if (PrintPreviewType == "Segment")
                            {
                                printSegmentWaves();
                            }
                            else
                            {
                                printReport();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"打印失败: {ex.Message}", "打印错误",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    btnClose.Click += (s, ev) =>
                    {
                        CurrentPageIndex = 0;
                        previewForm.Close();
                    };

                    previewForm.FormClosed += (s, ev) =>
                    {
                        CurrentPageIndex = 0;
                        previewForm.Close();
                    };

                    previewForm.MouseWheel += (s, ev) =>
                    {
                        // 根据滚轮方向翻页
                        if (ev.Delta > 0)
                        {
                            if (previewControl.StartPage > 0)
                            {
                                previewControl.StartPage--;
                                int pageIdx = previewControl.StartPage + 1;
                                string text = CommonValues.Langs["第"] + pageIdx + CommonValues.Langs["页"] + " / " + CommonValues.Langs["共"] + TotalPages + CommonValues.Langs["页"];
                                lblPageInfo.Text = text;
                                btnPrevPage.Enabled = previewControl.StartPage > 0;
                                btnNextPage.Enabled = previewControl.StartPage < TotalPages - 1;
                            }
                        }
                        else if (ev.Delta < 0)
                        {
                            if (previewControl.StartPage < TotalPages - 1)
                            {
                                previewControl.StartPage++;
                                int pageIdx = previewControl.StartPage + 1;
                                string text = CommonValues.Langs["第"] + pageIdx + CommonValues.Langs["页"] + " / " + CommonValues.Langs["共"] + TotalPages + CommonValues.Langs["页"];
                                lblPageInfo.Text = text; btnPrevPage.Enabled = previewControl.StartPage > 0;
                                btnNextPage.Enabled = previewControl.StartPage < TotalPages - 1;
                            }
                        }
                    };

                    // 显示预览窗体
                    previewForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打印预览初始化失败: {ex.Message}", "错误",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPageForPreview(object sender, PrintPageEventArgs e)
        {
            try
            {

                // 渲染当前页
                Bitmap page = printReport(CurrentPageIndex);

                if (page != null)
                {
                    // 计算打印区域，确保图像适应页面
                    System.Drawing.Rectangle pageBounds = e.PageBounds;
                    System.Drawing.Rectangle imageBounds = new System.Drawing.Rectangle(0, 0, page.Width, page.Height);

                    // 保持宽高比进行缩放
                    float scale = Math.Min(
                        pageBounds.Width / (float)imageBounds.Width,
                        pageBounds.Height / (float)imageBounds.Height
                    );

                    int scaledWidth = (int)(imageBounds.Width * scale);
                    int scaledHeight = (int)(imageBounds.Height * scale);

                    // 居中显示
                    int x = (pageBounds.Width - scaledWidth) / 2;
                    int y = (pageBounds.Height - scaledHeight) / 2;

                    System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(x, y, scaledWidth, scaledHeight);

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    e.Graphics.DrawImage(page, destRect, imageBounds, GraphicsUnit.Pixel);

                    // 释放资源
                    page.Dispose();

                    // 更新页码并判断是否有更多页面
                    CurrentPageIndex++;
                    e.HasMorePages = CurrentPageIndex < TotalPages;
                }
                else
                {
                    e.HasMorePages = false;
                }
            }
            catch (Exception ex)
            {
                e.HasMorePages = false;
                Console.WriteLine($"渲染第{CurrentPageIndex + 1}页时发生错误: {ex.Message}",
                              "渲染错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap printReport(int pageIndex)
        {
            Bitmap page = null;
            if (pageIndex < TotalPages)
            {
                bool bHorizontalPrint = false;

                if ((!cbxSummarize.Checked && !cbxHourCountList.Checked) && (cbxSegment.Checked && SegmentAllPageInfo.Count > 0))
                {
                    int direction = Convert.ToInt32(SegmentAllPageInfo[0][0]["direction"]);
                    bHorizontalPrint = direction == 0 ? true : false;
                }

                printReportPage.TotalPages = TotalPages;
                if ((PageIdxs.ContainsKey("summary")) && (pageIndex >= PageIdxs["summary"] && pageIndex < PageIdxs["summary"] + SummaryInfo.Count))
                {
                    printReportPage.SummaryInfo = SummaryInfo;

                    printReportPage.advice = ReportAdvice;
                    page = printReportPage.RenderReportPage(pageIndex, "summary");
                }
                if ((PageIdxs.ContainsKey("hourlist")) && (pageIndex >= PageIdxs["hourlist"] && pageIndex <= PageIdxs["hourlist"] + HourListInfo.Count))
                {
                    page = printReportPage.RenderReportPage(pageIndex, "hourlist");
                }
                if ((PageIdxs.ContainsKey("segment")) && (pageIndex >= PageIdxs["segment"] && pageIndex < PageIdxs["segment"] + SegmentAllPageInfo.Count))
                {
                    SegmentWaveRenderer.TotalPages = TotalPages;
                    SegmentWaveRenderer.SegmentAllPageInfo = SegmentAllPageInfo;
                    page = SegmentWaveRenderer.RenderEcgPage(pageIndex - PageIdxs["segment"], bHorizontalPrint, pageIndex);
                }
                if ((PageIdxs.ContainsKey("HRVAnalysis")) && (pageIndex >= PageIdxs["HRVAnalysis"] && pageIndex <= PageIdxs["HRVAnalysis"] + HRVAnalysisInfo.Count))
                {
                    printReportPage.HRVAnalysisInfo = HRVAnalysisInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "HRVAnalysis");
                }
                if ((PageIdxs.ContainsKey("HRVTimeDomain")) && (pageIndex >= PageIdxs["HRVTimeDomain"] && pageIndex <= PageIdxs["HRVTimeDomain"] + HRVTimeDomainInfo.Count))
                {
                    printReportPage.HRVTimeDomainInfo = HRVTimeDomainInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "HRVTimeDomain");
                }
                if ((PageIdxs.ContainsKey("HRVFrequencyDomain")) && (pageIndex >= PageIdxs["HRVFrequencyDomain"] && pageIndex <= PageIdxs["HRVFrequencyDomain"] + HRVTimeDomainInfo.Count))
                {
                    printReportPage.HRVFrequencyDomainInfo = HRVFrequencyDomainInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "HRVFrequencyDomain");
                }
                if ((PageIdxs.ContainsKey("STList")) && (pageIndex >= PageIdxs["STList"] && pageIndex <= PageIdxs["STList"] + STListInfo.Count))
                {
                    printReportPage.STListInfo = STListInfo;
                    printReportPage.STListIdx = PageIdxs["STList"];
                    page = printReportPage.RenderReportPage(pageIndex, "STList");
                }
                if ((PageIdxs.ContainsKey("STTrendWave")) && (pageIndex >= PageIdxs["STTrendWave"] && pageIndex <= PageIdxs["STTrendWave"] + STTrendWaveInfo.Count))
                {
                    printReportPage.STTrendWaveInfo = STTrendWaveInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "STTrendWave");
                }
                if ((PageIdxs.ContainsKey("TTrendWave")) && (pageIndex >= PageIdxs["TTrendWave"] && pageIndex <= PageIdxs["TTrendWave"] + TTrendWaveInfo.Count))
                {
                    printReportPage.TTrendWaveInfo = TTrendWaveInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "TTrendWave");
                }
                if ((PageIdxs.ContainsKey("AFList")) && (pageIndex >= PageIdxs["AFList"] && pageIndex <= PageIdxs["AFList"] + AFListInfo.Count))
                {
                    printReportPage.AFListInfo = AFListInfo;
                    printReportPage.AFListIdx = PageIdxs["AFList"];
                    page = printReportPage.RenderReportPage(pageIndex, "AFList");
                }
                if ((PageIdxs.ContainsKey("pace")) && (pageIndex >= PageIdxs["pace"] && pageIndex <= PageIdxs["pace"] + PaceListInfo.Count))
                {
                    printReportPage.PaceListInfo = PaceListInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "pace");
                }
            }

            return page;
        }

        private bool exportToPdf(string outputPath)
        {
            bool result = false;
            try
            {
                using (var fs = new FileStream(outputPath, FileMode.Create))
                {
                    TotalPages = 0;

                    initPrintData();

                    var document = new Document(PageSize.A4, 0, 0, 0, 0);
                    PdfWriter.GetInstance(document, fs);
                    document.Open();

                    for (int i = 0; i < TotalPages; i++)
                    {
                        Bitmap bmp = printReport(i);
                        // 2. 转成 iTextSharp 图片并写入
                        var pdfImage = iTextSharp.text.Image.GetInstance(
                            bmp, System.Drawing.Imaging.ImageFormat.Png);

                        pdfImage.ScaleToFit(
                            document.PageSize.Width - document.LeftMargin - document.RightMargin,
                            document.PageSize.Height - document.TopMargin - document.BottomMargin);

                        document.Add(pdfImage);
                        // 3. 不是最后一页才换页，避免空白页
                        if (i < TotalPages)
                            document.NewPage();
                    }

                    document.Close();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心电报告");
            }
            return result;
        }

        private bool SaveAdvice(int? status)
        {
            examCase caseInfo = ZoncareEntities.Instance.ExamCases.Where(o => o.id == AnaValues.CurrentCase.id).FirstOrDefault();
            if (caseInfo != null)
            {
                caseInfo.holterAutoReportConclusion = this.summarize.Text;
                caseInfo.holterReportConclusion = this.advice.Text;
                caseInfo.updateTime = DateTime.Now;
                caseInfo.updateBy = AnaValues.CurrentUser.userName;
                if (status != null)
                {
                    caseInfo.diagnoseDoctor = AnaValues.CurrentUser.userName;
                    caseInfo.diagnoseTime = DateTime.Now;
                    caseInfo.status = status.Value;
                }
            }
            bool result = AppCode.SaveCaseInfo(null, caseInfo, false, DateTime.Now, null, false);
            if (result)
            {
                if (status != null)
                {
                    //上传报告到中网
                    bool exported = false, uploaded = false;
                    string errorMsg = string.Empty;
                    if (!string.IsNullOrEmpty(caseInfo.webSourceId))
                    {
                        Form ownerForm = this.FindForm();
                        AppCode.ShowProcessing("正在生成报告文件...",
                          ownerForm,
                          async (args) =>
                          {
                              LoadingForm frmLoading = null;
                              if (args != null)
                              {
                                  dynamic paraInfo = args as System.Dynamic.ExpandoObject;
                                  frmLoading = paraInfo.Form as LoadingForm;
                              }

                              string pdfFilePath = Path.Combine(AnaValues.RootPath, "report.pdf");
                              exported = exportToPdf(pdfFilePath);
                              if (exported)
                              {
                                  frmLoading?.InvokeIfRequired(() => frmLoading.Message = _langs["正在上传报告文件..."]);

                                  List<UploadFile> fileList = new List<UploadFile>()
                                  {
                                     new UploadFile() {
                                        Name="reportFile",
                                        Filename = Path.GetFileName(pdfFilePath),
                                        ContentType = "application/octet-stream",
                                        Stream = File.Open(pdfFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                                     }
                                  };
                                  NameValueCollection paras = new NameValueCollection();
                                  paras.Add("id", caseInfo.webSourceId);
                                  paras.Add("doctor", AnaValues.CurrentUser.realname);
                                  paras.Add("doctorId", AnaValues.CurrentUser.webSourceId);
                                  string resultStr = "";
                                  if (CommonValues.SystemSettings.reportSets.displayConclusionType == 0)
                                  {
                                      resultStr = summarize.Text;
                                  }
                                  else if (CommonValues.SystemSettings.reportSets.displayConclusionType == 1)
                                  {
                                      resultStr = advice.Text;
                                  }
                                  else
                                  {
                                      resultStr = summarize.Text + "\n" + advice.Text;
                                  }

                                  paras.Add("result", resultStr);
                                  uploaded = false;
                                  string resp = HttpService.UploadFiles(CommonValues.WebCaseSource + "/holter/client/patient/audit", "POST", fileList, paras, out errorMsg);
                                  if (!string.IsNullOrEmpty(resp))
                                  {
                                      JObject jo = JObject.Parse(resp);
                                      if (jo["code"]?.ToString() == "0")
                                      {
                                          uploaded = true;
                                      }
                                  }
                              }
                              await Task.Delay(200);
                          },
                          null,
                          new System.Drawing.Rectangle(PointToScreen(this.Location), this.Size));

                        if (!exported)
                        {
                            Logger.CreateLog("心电报告文件生成失败：", "心电报告");
                        }
                        else if (!uploaded)
                        {
                            Logger.CreateLog("心电报告上传失败：" + errorMsg, "心电报告");
                        }
                    }
                }
                AnaValues.CurrentCase = PubFuns.CloneObject(caseInfo);
                AnaValues.DataSourceState = 1;
            }
            return result;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            string autoReportConclusion = "";
            if (ReportTemplateList != null)
            {
                reportTemplate defaultTemplate = ReportTemplateList.FirstOrDefault(o => o.isDefault == 1);
                if (defaultTemplate != null)
                {
                    autoReportConclusion = defaultTemplate.content;
                }
            }
            if (!string.IsNullOrEmpty(autoReportConclusion))
            {
                autoReportConclusion += ("\n请结合临床。\n");
                this.summarize.Text = NormalizeLineBreaks(ApplyTemplateFunctions(autoReportConclusion));
            }
            else
            {
                setSummaryInfo();
            }

            SaveAdvice(null);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            this.advice.AppendText(this.summarize.Text);
        }

        private void btnDia_Click(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            bool result = SaveAdvice(3);
            if (result)
            {
                ScreenCenterTip.Show("诊断成功");
            }
            else
            {
                CommonFuns.PromptError("操作失败");
            }

            object para = new { AnaValues.CurrentCase.caseNo, DiagResult = this.advice.Text };
            OperateLog.logParam("诊断病例", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
        }

        private void picSummarize_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initSummaryPrintData();

            PrintPreviewType = "Summarize";
            PrintPreview();
        }

        private void picHourCountList_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            hourListPrintData();

            PrintPreviewType = "HourList";
            PrintPreview();
        }

        private void picSegment_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initSegmentPrintData();
            if (SegmentAllPageInfo.Count == 0)
            {
                ScreenCenterTip.Show(CommonValues.Langs["未保存片段图"]);
            }
            else
            {
                PrintPreviewType = "Segment";
                PrintPreview();
            }
        }

        private void picHRVTimeDomain_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initHRVTimeDomainPrintData();
            PrintPreviewType = "HRVTimeDomain";
            PrintPreview();
        }

        private void picHRVFrequencyDomain_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initHRVFrequencyDomainPrintData();
            PrintPreviewType = "HRVFrequencyDomain";
            PrintPreview();
        }

        private void picHRVAnalysis_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initHRVAnalysisPrintData();
            PrintPreviewType = "HRVAnalysis";
            PrintPreview();
        }

        private void picSTList_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initSTListPrintData();
            if (STListInfo.Count == 0)
            {
                ScreenCenterTip.Show(CommonValues.Langs["ST事件列表为空"]);
                return;
            }
            PrintPreviewType = "STList";
            PrintPreview();
        }

        private void picSTTrendWave_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initSTTrendWavePrintData();
            PrintPreviewType = "STTrendWave";
            PrintPreview();
        }

        private void picTTrendWave_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initTTrendWavePrintData();
            PrintPreviewType = "TTrendWave";
            PrintPreview();
        }

        private void picAtrialList_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initAFListPrintData();

            if (AFListInfo.Count == 0)
            {
                ScreenCenterTip.Show(CommonValues.Langs["房颤/房扑列表为空"]);
                return;
            }
            PrintPreviewType = "AFList";
            PrintPreview();
        }

        private void picQT_Click(object sender, EventArgs e)
        {

        }

        private void picHRT_Click(object sender, EventArgs e)
        {

        }

        private void picPace_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initPaceListPrintData();
            if (PaceListInfo.Count == 0)
            {
                ScreenCenterTip.Show(CommonValues.Langs["起搏列表为空"]);
                return;
            }
            PrintPreviewType = "Pace";
            PrintPreview();
        }

        private void picSmallSegment_Click(object sender, EventArgs e)
        {

        }


        #region 总结/建议文本框右键复制粘贴

        //summarize/advice 共用的右键菜单：剪切、复制、粘贴、全选
        private ContextMenuStrip _editContextMenu;
        private RichTextBox _editContextTarget;   //右键菜单当前作用的文本框

        /// <summary>
        /// 为 summarize/advice 初始化右键菜单：选中文本后可复制/剪切，支持粘贴与全选
        /// </summary>
        private void initEditContextMenu()
        {
            if (_editContextMenu != null) return;

            _editContextMenu = new ContextMenuStrip();

            ToolStripMenuItem cutItem = new ToolStripMenuItem(editMenuText("剪切", "剪切"));
            cutItem.Click += (s, e) => { getEditTargetBox()?.Cut(); };

            ToolStripMenuItem copyItem = new ToolStripMenuItem(editMenuText("复制", "复制"));
            copyItem.Click += (s, e) => { getEditTargetBox()?.Copy(); };

            ToolStripMenuItem pasteItem = new ToolStripMenuItem(editMenuText("粘贴", "粘贴"));
            pasteItem.Click += (s, e) =>
            {
                RichTextBox box = getEditTargetBox();
                if (box != null && Clipboard.ContainsText())
                {
                    box.Paste(DataFormats.GetFormat(DataFormats.UnicodeText));
                }
            };

            ToolStripMenuItem selectAllItem = new ToolStripMenuItem(editMenuText("全选", "全选"));
            selectAllItem.Click += (s, e) => { getEditTargetBox()?.SelectAll(); };

            _editContextMenu.Items.AddRange(new ToolStripItem[]
            {
                cutItem, copyItem, pasteItem, new ToolStripSeparator(), selectAllItem
            });

            //打开菜单前按当前状态刷新菜单项可用性
            _editContextMenu.Opening += (s, e) =>
            {
                RichTextBox box = getEditTargetBox();
                bool hasBox = box != null;
                bool hasSelection = hasBox && box.SelectionLength > 0;
                cutItem.Enabled = hasSelection;
                copyItem.Enabled = hasSelection;
                pasteItem.Enabled = hasBox && Clipboard.ContainsText();
                selectAllItem.Enabled = hasBox && box.TextLength > 0;
            };

            //右键时先聚焦(让光标/选区落在点击的文本框上)，并记录菜单作用的目标文本框
            this.summarize.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _editContextTarget = this.summarize;
                    this.summarize.Focus();
                }
            };
            this.advice.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _editContextTarget = this.advice;
                    this.advice.Focus();
                }
            };

            this.summarize.ContextMenuStrip = _editContextMenu;
            this.advice.ContextMenuStrip = _editContextMenu;
        }

        private RichTextBox getEditTargetBox()
        {
            if (_editContextTarget != null && !_editContextTarget.IsDisposed)
            {
                return _editContextTarget;
            }
            if (this.summarize.Focused) return this.summarize;
            if (this.advice.Focused) return this.advice;
            return null;
        }

        //多语言文案，取不到时用中文兜底
        private static string editMenuText(string key, string fallback)
        {
            try
            {
                string value = CommonValues.Langs[key];
                return string.IsNullOrEmpty(value) ? fallback : value;
            }
            catch
            {
                return fallback;
            }
        }

        #endregion

        private void advice_TextChanged(object sender, EventArgs e)
        {
            if (this.advice.ContainsFocus)
                conclusionChanged = true;
        }

        private void advice_Validated(object sender, EventArgs e)
        {
            if (conclusionChanged)
            {
                SaveAdvice(null);
                conclusionChanged = false;
            }
        }

        private void summarize_TextChanged(object sender, EventArgs e)
        {
            if (this.summarize.ContainsFocus)
                summarizeChanged = true;
        }

        private void summarize_Validated(object sender, EventArgs e)
        {
            if (summarizeChanged)
            {
                SaveAdvice(null);
                summarizeChanged = false;
            }
        }

        /// <summary>
        /// 读取 Dictionary\dictionary.txt，按缩进层级（Tab 缩进为子节点）构建常用术语树填充 treeView1。
        /// 根节点顶格，每多一个 Tab 下沉一层；同层重名节点只保留一个（文件中"束支阻滞"重复出现）。
        /// </summary>
        private void InitDictionaryTree()
        {
            try
            {
                string dictPath = Path.Combine(Application.StartupPath, "Dictionary", "dictionary.txt");
                if (!File.Exists(dictPath))
                {
                    return;
                }

                treeView1.BeginUpdate();
                treeView1.Nodes.Clear();

                // levelParents[level] 记录各层最新的父节点，支持任意缩进深度
                List<TreeNode> levelParents = new List<TreeNode>();
                string[] lines = File.ReadAllLines(dictPath, Encoding.UTF8);
                foreach (string rawLine in lines)
                {
                    if (string.IsNullOrWhiteSpace(rawLine))
                    {
                        continue;
                    }

                    string text = rawLine.Trim();
                    int depth = rawLine.Length - rawLine.TrimStart('\t').Length; // Tab 缩进层级

                    // 确定父节点：depth-1 层的最新节点；depth 超过已有深度时挂到最深的节点下
                    TreeNode parent = null;
                    int parentLevel = Math.Min(depth, levelParents.Count) - 1;
                    if (parentLevel >= 0)
                    {
                        parent = levelParents[parentLevel];
                    }

                    TreeNode node = new TreeNode(text);
                    node.Name = text; // Name 作为去重键

                    TreeNodeCollection siblings = (parent == null) ? treeView1.Nodes : parent.Nodes;
                    if (siblings.ContainsKey(text))
                    {
                        node = siblings.Find(text, false)[0]; // 重名节点不重复创建，继续沿用
                    }
                    else
                    {
                        siblings.Add(node);
                    }

                    // 悬停提示完整术语（文字被省略号截断时可查看全文）
                    node.ToolTipText = text;

                    // 记录该层的最新节点，并清掉更深层的历史记录
                    if (levelParents.Count > depth)
                    {
                        levelParents.RemoveRange(depth, levelParents.Count - depth);
                    }
                    if (levelParents.Count == depth)
                    {
                        levelParents.Add(node);
                    }
                    else
                    {
                        levelParents[depth] = node;
                    }
                }

                // 默认只显示一级节点，子节点由用户点击展开
                foreach (TreeNode root in treeView1.Nodes)
                {
                    root.Collapse(false);
                }

                treeView1.EndUpdate();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "常用术语树加载");
            }
        }

        private void btnModifyTemplate_Click(object sender, EventArgs e)
        {
            using (ReportTemplateForm reportTemplate = new ReportTemplateForm(Functions, "ecg"))
            {
                reportTemplate.ShowDialogEx(this.FindForm(), showLayer: true);
                if (reportTemplate.TemplateSaved)
                {
                    // 模板内容已保存：重新加载模板列表，按优先级刷新结论区与单选按钮
                    RefreshSummarize();
                }
            }
        }

        #region 报告模板选择（4 个 RadioButton）

        /// <summary>程序化设置选中状态时抑制 CheckedChanged，避免误写库</summary>
        private bool _templateRadioUpdating = false;
        /// <summary>界面上的模板单选按钮（按显示顺序）</summary>
        private List<RadioButton> _templateRadios;

        /// <summary>按界面顺序取 4 个模板单选按钮，并绑定选中事件（只绑定一次）</summary>
        private List<RadioButton> GetTemplateRadios()
        {
            if (_templateRadios == null)
            {
                _templateRadios = new List<RadioButton> { radioButton1, radioButton2, radioButton3, radioButton4 };
                foreach (RadioButton rb in _templateRadios)
                {
                    rb.Click += templateRadio_CheckedChanged;
                    //rb.CheckedChanged += templateRadio_CheckedChanged;
                }
            }
            return _templateRadios;
        }

        /// <summary>
        /// 用 ReportTemplateList 填充 4 个模板单选按钮：
        /// 按钮文字取 template_name，选中 is_default == 1 的那条，
        /// 并把该条记录的 content 赋给 summarize。
        /// </summary>
        private void initReportTemplateRadios()
        {
            List<RadioButton> radios = GetTemplateRadios();
            _templateRadioUpdating = true;
            try
            {
                for (int i = 0; i < radios.Count; i++)
                {
                    RadioButton rb = radios[i];
                    if (ReportTemplateList != null && i < ReportTemplateList.Count)
                    {
                        reportTemplate item = ReportTemplateList[i];
                        rb.Tag = item;
                        rb.Text = item.templateName;
                        rb.Visible = true;
                        rb.Checked = item.isDefault == 1;
                    }
                    else
                    {
                        // 模板不足 4 条时隐藏多余的按钮
                        rb.Tag = null;
                        rb.Text = "";
                        rb.Visible = false;
                        rb.Checked = false;
                    }
                }
                // 注意：默认模板 content → summarize 的赋值在 RefreshUI 中统一处理，这里不再重复
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "报告模板初始化");
            }
            finally
            {
                _templateRadioUpdating = false;
            }
        }

        /// <summary>
        /// 点击模板单选按钮：该条记录 is_default 置 1，其余置 0 并写库；
        /// 同时把该模板的 content 填入 summarize。
        /// </summary>
        private void templateRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (_templateRadioUpdating)
            {
                return;
            }

            RadioButton rb = sender as RadioButton;
            if (rb == null || !rb.Checked || ReportTemplateList == null || ReportTemplateList.Count == 0)
            {
                return;
            }

            reportTemplate selected = rb.Tag as reportTemplate;
            if (selected == null)
            {
                return;
            }

            try
            {
                bool changed = false;
                foreach (reportTemplate item in ReportTemplateList)
                {
                    int newDefault = ReferenceEquals(item, selected) ? 1 : 0;
                    if (item.isDefault != newDefault)
                    {
                        item.isDefault = newDefault;
                        changed = true;
                    }
                }
                if (changed)
                {
                    ZoncareEntities.Instance.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "报告模板默认设置");
            }

            // 选中的模板内容填入总结（先替换模板函数占位符），用户可在此基础上继续编辑
            this.summarize.Text = NormalizeLineBreaks(ApplyTemplateFunctions(selected.content)) + ("\n请结合临床。\n");

            // 模板内容属于"界面上修改了 summarize"，同步写回 caseInfo.holterAutoReportConclusion，
            // 否则下次刷新又会回到旧结论（模板内容为空时不覆盖，避免把结论清空）
            if (!string.IsNullOrEmpty(selected.content))
            {
                try
                {
                    SaveAdvice(null);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "报告模板结论保存");
                }
            }
        }

        /// <summary>
        /// 将文本中的模板函数占位符（Functions 的 Key，如 &lt;$TotalBeats$&gt;）
        /// 替换为「Value + Unit」（如 "98765" + "个" = "98765个"），
        /// 用于把模板内容渲染成实际报告文本。
        /// </summary>
        private string ApplyTemplateFunctions(string text)
        {
            if (string.IsNullOrEmpty(text) || Functions == null || Functions.Count == 0)
            {
                return text;
            }

            string result = text;
            foreach (KeyValuePair<string, TemplateFunctionInfo> kv in Functions)
            {
                if (string.IsNullOrEmpty(kv.Key))
                {
                    continue;
                }
                // 替换为「值 + 单位」：值或单位为空时按空串处理
                string replaceValue = (kv.Value.Value ?? "") + (kv.Value.Unit ?? "");
                result = result.Replace(kv.Key, replaceValue);
            }
            return result;
        }

        /// <summary>
        /// 规范化模板内容后再赋给 RichTextBox，保证换行正确且没有多余空行。
        /// 说明：RichTextBox 的换行符是 LF(\n)，若传入 CRLF(\r\n)，
        /// RichEdit 会把 CR 当作段落分隔、LF 再当作一次换行，导致每个换行中间多一个空行；
        /// 另外数据库中的 content 也可能存的是转义文本（反斜杠 + n 两个字符）。
        /// </summary>
        private static string NormalizeLineBreaks(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? "";
            }

            // 没有真实换行符时，才按转义文本处理，避免破坏正文里本就存在的反斜杠
            if (text.IndexOf('\n') < 0 && text.IndexOf('\r') < 0)
            {
                text = text.Replace("\\r\\n", "\n").Replace("\\n", "\n");
            }

            // 统一为 LF（RichTextBox 原生换行符），去掉 CR 以免产生多余空行
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // 合并多余空行：连续多个换行只保留一个
            while (text.Contains("\n\n"))
            {
                text = text.Replace("\n\n", "\n");
            }

            return text;
        }

        #endregion

        /// <summary>节点右侧的图标按钮：自绘加号/铅笔/垃圾桶图标，带 1px 边框与悬停高亮</summary>
        private sealed class NodeIconButton : Button
        {
            public enum NodeIcon
            {
                Add,
                Edit,
                Delete
            }

            private static readonly System.Drawing.Color AccentColor = System.Drawing.Color.FromArgb(74, 122, 196);
            private static readonly System.Drawing.Color BorderColor = System.Drawing.Color.FromArgb(198, 213, 234);
            private static readonly System.Drawing.Color HoverBackColor = System.Drawing.Color.FromArgb(237, 243, 252);
            private static readonly System.Drawing.Color DeleteHoverColor = System.Drawing.Color.FromArgb(214, 74, 70);

            private readonly NodeIcon _icon;
            /// <summary>常态背景色 = 树的背景色，视觉上透明融入树行</summary>
            private readonly System.Drawing.Color _normalBackColor;
            private bool _hover;

            public NodeIconButton(NodeIcon icon, System.Drawing.Color normalBackColor)
            {
                _icon = icon;
                _normalBackColor = normalBackColor;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                Size = new System.Drawing.Size(20, 20);
                BackColor = normalBackColor;
                Cursor = Cursors.Hand;
                TabStop = false;
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                Text = string.Empty;
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                _hover = true;
                Invalidate();
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                _hover = false;
                Invalidate();
                base.OnMouseLeave(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;

                // 删除按钮悬停时用红色强调
                System.Drawing.Color accent = (_icon == NodeIcon.Delete && _hover) ? DeleteHoverColor : AccentColor;
                System.Drawing.Color border = _hover ? accent : BorderColor;
                System.Drawing.Color back = _hover ? HoverBackColor : _normalBackColor;

                using (SolidBrush backBrush = new SolidBrush(back))
                {
                    g.FillRectangle(backBrush, ClientRectangle);
                }
                using (Pen borderPen = new Pen(border))
                {
                    g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
                }

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (Pen iconPen = new Pen(accent, 1.5f))
                {
                    iconPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    iconPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                    switch (_icon)
                    {
                        case NodeIcon.Add:
                            g.DrawLine(iconPen, 5, 10, 15, 10);
                            g.DrawLine(iconPen, 10, 5, 10, 15);
                            break;

                        case NodeIcon.Edit:
                            // 铅笔：笔身两条平行线 + 笔尾封口 + 笔尖
                            g.DrawLine(iconPen, 7, 13, 13, 7);
                            g.DrawLine(iconPen, 9, 15, 15, 9);
                            g.DrawLine(iconPen, 13, 7, 15, 9);
                            g.DrawLine(iconPen, 7, 13, 9, 15);
                            g.DrawLine(iconPen, 7, 13, 6, 14);
                            g.DrawLine(iconPen, 9, 15, 6, 14);
                            break;

                        case NodeIcon.Delete:
                            g.DrawLine(iconPen, 6, 6, 14, 6);    // 桶盖
                            g.DrawLine(iconPen, 8, 4, 12, 4);    // 提手
                            g.DrawLine(iconPen, 7, 6, 7, 15);    // 左壁
                            g.DrawLine(iconPen, 13, 6, 13, 15);  // 右壁
                            g.DrawLine(iconPen, 7, 15, 13, 15);  // 桶底
                            g.DrawLine(iconPen, 10, 8, 10, 13);  // 桶内竖线
                            break;
                    }
                }
            }
        }

        /// <summary>术语新增/编辑用的简易输入框（Enter 确认、Esc 取消）</summary>
        private sealed class TermInputForm : Form
        {
            private TextBox _textBox;
            private string _value;

            private TermInputForm(string title, string tip, string value)
            {
                this.Text = title;
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.StartPosition = FormStartPosition.CenterParent;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.ShowInTaskbar = false;
                this.ClientSize = new System.Drawing.Size(320, 132);
                this.Font = new System.Drawing.Font("微软雅黑", 9F);

                Label label = new Label();
                label.Text = tip;
                label.Location = new System.Drawing.Point(16, 14);
                label.Size = new System.Drawing.Size(288, 20);

                _textBox = new TextBox();
                _textBox.Location = new System.Drawing.Point(16, 40);
                _textBox.Size = new System.Drawing.Size(288, 25);
                _textBox.Text = value;
                _textBox.SelectAll();

                Button ok = new Button();
                ok.Text = "确定";
                ok.DialogResult = DialogResult.OK;
                ok.Location = new System.Drawing.Point(148, 80);
                ok.Size = new System.Drawing.Size(75, 28);
                ok.Click += (s, e) => { _value = _textBox.Text.Trim(); };

                Button cancel = new Button();
                cancel.Text = "取消";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location = new System.Drawing.Point(229, 80);
                cancel.Size = new System.Drawing.Size(75, 28);

                this.AcceptButton = ok;
                this.CancelButton = cancel;
                this.Controls.Add(label);
                this.Controls.Add(_textBox);
                this.Controls.Add(ok);
                this.Controls.Add(cancel);
            }

            /// <summary>弹出输入框，返回去掉首尾空白后的文本；取消返回 null</summary>
            public static string Prompt(IWin32Window owner, string title, string tip, string value)
            {
                using (TermInputForm form = new TermInputForm(title, tip, value))
                {
                    return form.ShowDialog(owner) == DialogResult.OK ? form._value : null;
                }
            }
        }
    }
}
