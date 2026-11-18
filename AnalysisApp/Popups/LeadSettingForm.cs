using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace AnalysisApp
{
    public partial class LeadSettingForm : BaseDialogForm
    {
        private Dictionary<CheckBox, int> leadCheckBoxMap;
        public int MaxSelectedLeads = 3;
        private List<CheckBox> selectionOrder = new List<CheckBox>(); // 记录勾选顺序
        
        // 默认选中的导联索引列表，可以任意修改
        public List<int> selectedIndices = new List<int> ();

        // 定义事件，用于通知选中的导联索引发生变化
        public event Action<List<int>> OnLeadsSelected;

        public LeadSettingForm() : base("", "导联设置")
        {
            InitializeComponent();
            InitializeLeadCheckBoxMap();
        }

        private void InitializeLeadCheckBoxMap()
        {
            leadCheckBoxMap = new Dictionary<CheckBox, int>
            {
                { cbxLeadI, 0 },    // I导联 -> 索引0
                { cbxLeadII, 1 },   // II导联 -> 索引1
                { cbxLeadIII, 2 },  // III导联 -> 索引2
                { cbxLeadaVR, 3 },  // aVR导联 -> 索引3
                { cbxLeadaVL, 4 },  // aVL导联 -> 索引4
                { cbxLeadaVF, 5 },  // aVF导联 -> 索引5
                { cbxLeadV1, 6 },   // V1导联 -> 索引6
                { cbxLeadV2, 7 },   // V2导联 -> 索引7
                { cbxLeadV3, 8 },   // V3导联 -> 索引8
                { cbxLeadV4, 9 },   // V4导联 -> 索引9
                { cbxLeadV5, 10 },  // V5导联 -> 索引10
                { cbxLeadV6, 11 }   // V6导联 -> 索引11
            };

            foreach (var checkBox in leadCheckBoxMap.Keys)
            {
                checkBox.CheckedChanged += LeadCheckBox_CheckedChanged;
            }
        }

        private void SetDefaultSelections()
        {
            // 清除所有checkbox的选中状态
            foreach (var checkBox in leadCheckBoxMap.Keys)
            {
                checkBox.Checked = false;
            }
            
            // 根据selectedIndices列表设置默认选中的checkbox
            foreach (int index in selectedIndices)
            {
                var checkBox = leadCheckBoxMap.FirstOrDefault(kv => kv.Value == index).Key;
                if (checkBox != null)
                {
                    checkBox.Checked = true;
                }
            }
        }

        private void LeadCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var currentCheckBox = sender as CheckBox;
            if (currentCheckBox == null) return;

            int selectedCount = leadCheckBoxMap.Count(kv => kv.Key.Checked);

            // 如果当前是勾选操作且已选中三个导联
            if (currentCheckBox.Checked && selectedCount > MaxSelectedLeads)
            {
                // 获取所有已选中的导联（排除当前勾选的）
                var selectedCheckBoxes = leadCheckBoxMap
                    .Where(kv => kv.Key.Checked && kv.Key != currentCheckBox)
                    .OrderBy(kv => kv.Value) // 按照12导联索引排序（0-11）
                    .ToList();

                if (selectedCheckBoxes.Count > 0)
                {
                    // 找到排序顺序中最后一个（索引最大的）导联
                    var lastSelectedByIndex = selectedCheckBoxes.Last();
                    
                    // 取消排序顺序中最后一个导联
                    lastSelectedByIndex.Key.Checked = false;
                    
                    Console.WriteLine($"自动取消导联 '{lastSelectedByIndex.Key.Text}'（索引{lastSelectedByIndex.Value}），当前选中导联: {string.Join(", ", GetSelectedLeadNames())}");
                }
                else
                {
                    // 如果没有找到其他已选中的导联，取消当前勾选
                    currentCheckBox.Checked = false;
                    MessageBox.Show($"最多选择三导联", "导联提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            var selectedCheckBoxes = leadCheckBoxMap.Where(kv => kv.Key.Checked).ToList();
            if(MaxSelectedLeads == 3)
            {
                if (selectedCheckBoxes.Count < MaxSelectedLeads)
                {
                    MessageBox.Show($"请选择三导联", "导联提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (selectedCheckBoxes.Count > MaxSelectedLeads)
                {
                    MessageBox.Show($"最多选择三导联", "导联提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }


            // 获取选中的导联索引
            selectedIndices = selectedCheckBoxes.Select(kv => kv.Value).ToList();
            
            // 获取选中的导联名称
            var selectedLeadNames = selectedCheckBoxes.Select(kv => kv.Key.Text).ToList();

            // 触发事件通知刷新波形
            OnLeadsSelected?.Invoke(selectedIndices);

            this.Close();
        }

        public List<int> GetSelectedLeadIndices()
        {
            return leadCheckBoxMap.Where(kv => kv.Key.Checked)
                                 .Select(kv => kv.Value)
                                 .ToList();
        }

        public List<string> GetSelectedLeadNames()
        {
            return leadCheckBoxMap.Where(kv => kv.Key.Checked)
                                 .Select(kv => kv.Key.Text)
                                 .ToList();
        }

        private void LeadSettingForm_Load(object sender, EventArgs e)
        {
            SetDefaultSelections();
        }
    }
}
