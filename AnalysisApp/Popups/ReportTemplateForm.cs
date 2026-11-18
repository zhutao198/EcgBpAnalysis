using AnalysisApp.Commons;
using AnalysisSystem.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.DAL;

namespace AnalysisApp
{
    public partial class ReportTemplateForm : BaseDialogForm
    {
        private Dictionary<string, TemplateFunctionInfo> Functions;
        private string CheckItemType = "ecg";
        private List<reportTemplate> ReportTemplateList;
        /// <summary>本次会话是否成功保存过模板内容（供调用方判断是否需要刷新界面）</summary>
        public bool TemplateSaved { get; private set; }
        public ReportTemplateForm(Dictionary<string, TemplateFunctionInfo> functions, string checkItemType) : base("", "综述模板编辑器")
        {
            InitializeComponent();
            Functions = functions;
            CheckItemType = checkItemType;
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            SaveTemplateContent();
            Close();
        }

        /// <summary>
        /// 把编辑区文本保存到当前选中模板记录的 content 字段。
        /// ReportTemplateList 是 AsNoTracking 加载的，需按 id 重新查出跟踪实体再更新。
        /// </summary>
        private void SaveTemplateContent()
        {
            try
            {
                reportTemplate selected = GetSelectedTemplate();
                if (selected == null)
                {
                    return;
                }

                // 编辑区文本写回（换行统一为 \r\n 入库，兼容各端读取）
                string content = this.edit.Text.Replace("\n", "\r\n");

                ZoncareEntities dbEntities = ZoncareEntities.Instance;
                reportTemplate dbTemplate = dbEntities.ReportTemplates.Where(o => o.id == selected.id).FirstOrDefault();
                if (dbTemplate == null)
                {
                    return;
                }
                dbTemplate.templateName = this.templateName.Text;
                dbTemplate.content = content;
                dbTemplate.updateTime = DateTime.Now;
                dbTemplate.updateBy = AnaValues.CurrentUser.userName;
                dbEntities.SaveChanges();

                // 同步内存列表，保持界面数据一致；并标记已保存供调用方刷新
                selected.content = content;
                TemplateSaved = true;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "模板内容保存");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ReportTemplateForm_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                initTable();
                // 编辑区文本变化时刷新预览区（含 initData 首次回填）
                this.edit.TextChanged += edit_TextChanged;
                // 重置按钮：Designer 未绑定事件，此处订阅
                this.btnRefresh.Click += btnRefresh_Click;
                initData();
            }
        }

        /// <summary>
        /// 重置：把数据库中当前模板的 original_expression（原始表达式）显示到编辑区。
        /// 只改编辑区、不立即入库；用户点"确定"时才通过 SaveTemplateContent 写回 content。
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                reportTemplate selected = GetSelectedTemplate();
                if (selected == null)
                {
                    return;
                }

                // 从数据库重新查询，保证取到最新的 original_expression
                ZoncareEntities dbEntities = ZoncareEntities.Instance;
                reportTemplate dbTemplate = dbEntities.ReportTemplates.AsNoTracking().Where(o => o.id == selected.id).FirstOrDefault();
                if (dbTemplate == null || string.IsNullOrEmpty(dbTemplate.originalExpression))
                {
                    return; // 无原始表达式时不重置，避免把编辑区清空
                }

                // 只更新编辑区（edit.TextChanged 会自动刷新预览区），入库由"确定"按钮完成
                this.edit.Text = NormalizeLineBreaks(dbTemplate.originalExpression);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "模板内容重置");
            }
        }

        private void edit_TextChanged(object sender, EventArgs e)
        {
            UpdateReview();
        }

        /// <summary>
        /// 把编辑区文本赋给 template，逐个匹配 Functions 的 Key（如 &lt;$TotalBeats$&gt;），
        /// 命中的子串替换为对应模板函数的 Value + Unit，结果作为预览显示到 review。
        /// </summary>
        private void UpdateReview()
        {
            string template = this.edit.Text;
            if (!string.IsNullOrEmpty(template) && Functions != null)
            {
                foreach (KeyValuePair<string, TemplateFunctionInfo> kv in Functions)
                {
                    if (string.IsNullOrEmpty(kv.Key))
                    {
                        continue;
                    }
                    // 替换为「值 + 单位」，值或单位为空时按空串处理
                    template = template.Replace(kv.Key, (kv.Value.Value ?? "") + (kv.Value.Unit ?? ""));
                }
            }
            this.review.Text = template;
        }

        private void initTable()
        {
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvData, true);

            // 列随表格宽度自动拉伸铺满，避免右侧留白
            this.dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                // FillWeight 控制各列的宽度比例；MinimumWidth 保证窗口较窄时函数名不换行挤压
                new DataGridViewTextBoxColumn { Name="function", HeaderText=CommonValues.Langs["函数名字"], DataPropertyName="Function", FillWeight=45, MinimumWidth=170 },
                new DataGridViewTextBoxColumn { Name="lable", HeaderText=CommonValues.Langs["注释说明"], DataPropertyName="Label", FillWeight=55 },
                new DataGridViewTextBoxColumn { Name="value", HeaderText=CommonValues.Langs["值"], DataPropertyName="Value", Visible=false},
                new DataGridViewTextBoxColumn { Name="unit", HeaderText=CommonValues.Langs["单位"], DataPropertyName="Unit", Visible=false},


            };
            foreach (DataGridViewColumn col in cols) { col.SortMode = DataGridViewColumnSortMode.NotSortable; }

            this.dgvData.Columns.AddRange(cols);
            this.dgvData.CellMouseClick += dgvData_CellContentClick;
            // 单击某行记录时，把该行的 值+单位 插入到编辑区光标处
            this.dgvData.CellClick += dgvData_CellClick;
        }

        /// <summary>
        /// 单击模板函数表格某行时，把该条记录的 Value + Unit 插入到编辑区当前光标位置。
        /// 编辑区的 SelectionStart 在失去焦点后依然保留，可直接作为插入点。
        /// </summary>
        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= this.dgvData.Rows.Count)
            {
                return;
            }
            try
            {
                FunctionRow row = this.dgvData.Rows[e.RowIndex].DataBoundItem as FunctionRow;
                if (row == null)
                {
                    return;
                }

                //string insertText = (row.Value ?? "") + (row.Unit ?? "");
                string insertText = row.Function ?? "";
                if (string.IsNullOrEmpty(insertText))
                {
                    return;
                }

                InsertIntoEdit(insertText);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "模板函数插入");
            }
        }

        /// <summary>在编辑区光标处插入文本（若当前有选中内容则替换选中内容），并保持光标在插入内容之后</summary>
        private void InsertIntoEdit(string insertText)
        {
            int pos = this.edit.SelectionStart;
            if (pos < 0 || pos > this.edit.TextLength)
            {
                pos = this.edit.TextLength;
            }

            this.edit.Select(pos, 0);
            this.edit.SelectedText = insertText;   // 在光标处插入

            this.edit.SelectionStart = pos + insertText.Length;
            this.edit.SelectionLength = 0;
            this.edit.Focus();                      // 光标回到编辑区，便于继续输入
        }

        private void dgvData_CellContentClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }
            try
            {
                DataGridView dgv = sender as DataGridView;
                DataGridViewRow clickedRow = dgv.Rows[e.RowIndex];
                DataGridViewCell curCell = clickedRow.Cells[0];
                if (curCell is DataGridViewCheckBoxCell)
                {
                    if (curCell.Value == null)
                    {
                        curCell.Value = true;
                    }
                    else
                    {
                        bool isChecked = (bool)curCell.Value;
                        if (isChecked)
                        {
                            curCell.Value = false;
                            dgv.Columns[0].HeaderCell.Value = false;
                        }
                        else
                        {
                            curCell.Value = true;
                        }

                    }
                    bool bSelected = (bool)curCell.Value;

                    dgv.InvalidateCell(e.ColumnIndex, e.RowIndex);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "模板函数列表");
            }
        }

        private void initData()
        {
            try
            {
                // TemplateFunctionInfo 是 struct，且字典的 Key（如 <$TotalBeats$>）才是函数名，
                // 不能直接绑定 Dictionary，先转换为带 Function 字段的行对象列表
                List<FunctionRow> rows = new List<FunctionRow>();
                if (Functions != null)
                {
                    foreach (KeyValuePair<string, TemplateFunctionInfo> kv in Functions)
                    {
                        rows.Add(new FunctionRow
                        {
                            Function = kv.Key,
                            Label = kv.Value.Label,
                            Value = kv.Value.Value,
                            Unit = kv.Value.Unit
                        });
                    }
                }
                this.dgvData.DataSource = rows;
                ZoncareEntities dbEntities = ZoncareEntities.Instance;
                ReportTemplateList = dbEntities.ReportTemplates.AsNoTracking().Where(o => o.languageId == CommonValues.LanguageID && o.systemType == CheckItemType).ToList();

                // 填充模板名称下拉框并选中默认模板
                initTemplateCombo();

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "模板函数列表初始化");
            }
        }

        /// <summary>
        /// 用 ReportTemplateList 的 template_name 填充下拉框，
        /// 选中 is_default == 1 的记录并把其 content 显示在编辑区。
        /// 注意：ZoncareComboBox 在默认绘制模式下直接用 Items[i].ToString() 画文字，
        /// 不能绑定实体对象，因此下拉项存模板名字符串，选中时按名字反查记录。
        /// </summary>
        private void initTemplateCombo()
        {
            //this.templateName.DropDownStyle = ComboBoxStyle.DropDownList; // 只允许从列表选择，不允许手动输入
            this.templateName.Items.Clear();
            if (ReportTemplateList != null)
            {
                foreach (reportTemplate item in ReportTemplateList)
                {
                    this.templateName.Items.Add(item.templateName);
                }
            }

            // 先订阅事件再设置选中项：SelectedIndexChanged 会触发 UpdateEditContent 完成首次回填
            this.templateName.SelectedIndexChanged -= templateName_SelectedIndexChanged;
            this.templateName.SelectedIndexChanged += templateName_SelectedIndexChanged;

            // 默认选中 is_default == 1 的记录；没有默认模板时选第一条
            reportTemplate defaultTemplate = null;
            if (ReportTemplateList != null && ReportTemplateList.Count > 0)
            {
                defaultTemplate = ReportTemplateList.FirstOrDefault(o => o.isDefault == 1)
                                  ?? ReportTemplateList[0];
            }

            if (defaultTemplate != null)
            {
                this.lblName.Text = defaultTemplate.templateName;
                this.templateName.SelectedItem = defaultTemplate.templateName;
                // 若默认项因重名等原因未触发选中事件，直接回填一次
                UpdateEditContent();
            }
            else
            {
                UpdateEditContent(); // 无模板时清空编辑区
            }
        }

        private void templateName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string name = this.templateName.SelectedItem as string;
            this.lblName.Text = name;

            UpdateEditContent();
        }

        /// <summary>获取当前选中的模板记录</summary>
        private reportTemplate GetSelectedTemplate()
        {
            //string name = this.templateName.SelectedItem as string;
            //if (string.IsNullOrEmpty(name) || ReportTemplateList == null)
            //{
            //    return null;
            //}

            return ReportTemplateList.FirstOrDefault(o => o.templateName == this.lblName.Text);
        }

        /// <summary>把当前选中模板的 content 显示到编辑区</summary>
        private void UpdateEditContent()
        {
            reportTemplate selected = GetSelectedTemplate();
            this.edit.Text = selected != null ? NormalizeLineBreaks(selected.content) : "";
        }

        /// <summary>
        /// 规范化换行符，保证 content 的换行在 RichTextBox 中正确显示。
        /// 数据库中的 content 可能用 LF(\n)、CR(\r) 或 CRLF(\r\n)，
        /// 也可能是被转义成字面量的 "\r\n"、"\n"（反斜杠 + n 两个字符）。
        /// RichTextBox 的原生换行符是 LF；若传入 CRLF，CR 会被当成段落分隔，
        /// 一个换行渲染成两行（每行之间多出空行），因此统一转成 LF。
        /// </summary>
        private static string NormalizeLineBreaks(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? "";
            }

            // 完全没有真实换行符时，才按转义文本处理，避免破坏正文里本就存在的反斜杠（如路径 D:\new）
            if (text.IndexOf('\n') < 0 && text.IndexOf('\r') < 0)
            {
                text = text.Replace("\\r\\n", "\n").Replace("\\n", "\n");
            }

            // 统一为 LF（RichTextBox 原生换行符），去掉 CR 以免产生多余空行
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // 去掉换行之间的空行：连续多个换行只保留一个
            while (text.Contains("\n\n"))
            {
                text = text.Replace("\n\n", "\n");
            }
            return text;
        }

        /// <summary>模板函数表格行数据：与 initTable 中各列的 DataPropertyName 对应</summary>
        private class FunctionRow
        {
            /// <summary>函数名（字典 Key，如 "&lt;$TotalBeats$&gt;"）</summary>
            public string Function { get; set; }
            /// <summary>注释说明</summary>
            public string Label { get; set; }
            /// <summary>当前值</summary>
            public string Value { get; set; }
            /// <summary>单位</summary>
            public string Unit { get; set; }
        }

    }
}
