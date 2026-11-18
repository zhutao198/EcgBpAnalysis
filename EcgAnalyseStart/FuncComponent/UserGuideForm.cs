using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Utilities;

namespace EcgBpAnalysis.FuncComponent
{
    public partial class UserGuideForm : BaseDialogForm
    {
        private List<Button> _menuButtons;
        private Image _img10_12;
        private string _aboutInfo;
        private Dictionary<string, Image> _menuItemImages = new Dictionary<string, Image>();

        public UserGuideForm() : base("", "用户指南")
        {
            InitializeComponent();
            this.tabControl1.ApplyHeaderlessStyle = true;
        }

        private void CancelForm_Load(object sender, EventArgs e)
        {
            init();
            initUI();
            initAboutInfo();

            this.btnLeadTip.PerformClick();
        }

        private void picLeadDiagram1_Paint(object sender, PaintEventArgs e)
        {
            Rectangle outerRect = this.picLeadDiagram1.ClientRectangle;
            ControlPaint.DrawBorder(e.Graphics, outerRect, Color.Silver, ButtonBorderStyle.Solid);

            int margin = 30;
            Rectangle rect = new Rectangle(
                margin,                // X
                margin,                // Y
                picLeadDiagram1.Width - 2 * margin,  // 宽度
                picLeadDiagram1.Height - 2 * margin  // 高度
            );
            e.Graphics.DrawImage(_img10_12, rect);
        }

        private void lblAppInfo_Paint(object sender, PaintEventArgs e)
        {
            Label lbl = sender as Label;
            if (string.IsNullOrEmpty(_aboutInfo)) return;

            // 自定义行高-像素
            int customLineHeight = 36;

            string[] lines = _aboutInfo.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                int y = i * customLineHeight;

                TextRenderer.DrawText(
                    e.Graphics,
                    lines[i].TrimEnd('\r'),          // 去除回车符
                    lbl.Font,
                    new Point(0, y),                 // 从 (0, y) 开始绘制
                    lbl.ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.Top
                );
            }
        }


        private void init()
        {
            _menuButtons = CommonFuns.RetriveSubControls<Button>(this.tlpMenu);
            for (int i = 0; i < this.tlpMenu.RowCount; i++)
            {
                Control ctl = tlpMenu.GetControlFromPosition(0, i);
                if (ctl is Button btn)
                {
                    string imgName = btn.Name.Substring(3).ToLower();
                    string imgNameA = btn.Name.Substring(3).ToLower() + "A";
                    _menuItemImages[imgName] = CommonFuns.GetImageFromFile(PubFuns.GetAbsolutePath("images\\" + imgName + ".png"), false, false, 16, 16);
                    _menuItemImages[imgNameA] = CommonFuns.GetImageFromFile(PubFuns.GetAbsolutePath("images\\" + imgNameA + ".png"), false, false, 16, 16);
                }
            }

        }

        private void initUI()
        {
            //导航栏按钮
            for (int i = 0; i < _menuButtons.Count; i++)
            {
                Button btn = _menuButtons[i];
                string imgName = btn.Name.Substring(3).ToLower();
                string imgNameA = btn.Name.Substring(3).ToLower() + "A";
                int idx = Convert.ToInt32(i);
                btn.Click += (o, e) =>
                {
                    this.tabControl1.SelectedIndex = idx;
                    btn.BackColor = SystemColors.GradientInactiveCaption;
                    btn.ForeColor = Color.FromArgb(74, 122, 196);
                    btn.Image = _menuItemImages[imgNameA];
                    foreach (Button btn1 in _menuButtons)
                    {
                        if (btn1 != btn)
                        {
                            btn1.BackColor = Color.WhiteSmoke;
                            btn1.ForeColor = Color.Black;
                            btn1.Image = _menuItemImages[btn1.Name.Substring(3).ToLower()];
                        }
                    }
                };
            }

            this.btnDownload.Left = (this.panel2.Width - this.btnDownload.Width) / 2;
            this.btnDownload.Left = (this.panel2.Width - this.btnDownload.Width) / 2;

            _img10_12 = CommonFuns.GetImageFromFile(PubFuns.GetAbsolutePath("images\\leadDiagram\\lead10_12.png"), true);

            //暂时隐藏用户手册
            this.btnUserHelp.Visible = false;
            int rowIndex = this.tlpMenu.GetRow(btnUserHelp);
            this.tlpMenu.RowStyles[rowIndex].Height = 0;
        }

        private void initAboutInfo()
        {
            StringBuilder aboutInfo = new StringBuilder();

#if !NEUTRAL
            aboutInfo.AppendLine(_langs["生产企业名称：武汉中旗生物医疗电子有限公司"]);
            aboutInfo.AppendLine(_langs["生产注册地址：湖北省武汉市东湖新技术开发区高新二路380号"]);
            aboutInfo.AppendLine(_langs["生产地址：湖北省武汉市东湖新技术开发区高新二路380号"]);
            aboutInfo.AppendLine(_langs["产品名称："] + CommonValues.SystemName );
            aboutInfo.AppendLine();
            aboutInfo.AppendLine(_langs["武汉中旗生物医疗电子有限公司"]);
            aboutInfo.AppendLine(_langs["24小时服务热线：4000-400-499"]);
            aboutInfo.AppendLine(_langs["电子邮件：support@zoncare.cn"]);
#endif
            aboutInfo.AppendLine(_langs["完整版本号"] + ": " + "V1.0.0");
            aboutInfo.AppendLine(_langs["发布版本"] + ": " + "V1");

            _aboutInfo = aboutInfo.ToString();
            //this.lblAppInfo.Text = aboutInfo.ToString();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// 将普通文本转换为带行距的 RTF（修复版）
        /// </summary>
        /// <param name="text">多行文本</param>
        /// <param name="lineHeight">行高（单位：缇，1缇 = 1/1440 英寸，通常 240 缇约等于 4.23mm, 建议 200~400）</param>
        /// <param name="fontName">字体名称</param>
        /// <param name="fontSize">字体大小（单位：pt）</param>
        /// <returns>RTF 字符串</returns>
        public static string TextToRtf(string text, int lineHeight = 280, string fontName = "微软雅黑", int fontSize = 11)
        {
            if (string.IsNullOrEmpty(text)) return "";

            // 1. 按换行符分割
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            // 2. 构建每行内容（转义特殊字符）
            List<string> rtfLines = new List<string>();
            foreach (string line in lines)
            {
                // 转义 RTF 特殊字符：\ { }
                string escaped = line
                    .Replace("\\", "\\\\")
                    .Replace("{", "\\{")
                    .Replace("}", "\\}");

                // 每行以 \par 结束（段落结束符）
                rtfLines.Add(escaped + "\\par");
            }

            // 3. 合并所有行
            string content = string.Join("", rtfLines);  // 注意：不要用 \n 或空格分隔

            // 4. 构造 RTF 头部
            // fonttbl：字体表
            // \fs = 字号 × 2（如 11pt → \fs22）
            // \sl-280：负号表示固定行距，\slmult0 表示单位是缇
            string rtf = $@"{{\rtf1\ansi\ansicpg936\deff0
{{\fonttbl{{\f0\fnil\fcharset134 {fontName};}}}}
\viewkind4\uc1
\pard\lang2052\f0\fs{fontSize * 2}\sl-{lineHeight}\slmult0
{content}
}}";

            return rtf;
        }

 
    }
}
