using AnalysisApp.Commons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisApp
{
    public partial class EventPageControl : BaseUserControl
    {
        
        public EventPageControl()
        {
            InitializeComponent();

            // 设置listView1的样式
            listView1.FullRowSelect = true;
            listView1.HideSelection = false;
            listView1.OwnerDraw = true;

            // 订阅listView1的点击事件
            listView1.MouseClick += ListView1_MouseClick;
            listView1.DrawItem += ListView1_DrawItem;
            listView1.DrawSubItem += ListView1_DrawSubItem;
            listView1.DrawColumnHeader += ListView1_DrawColumnHeader;
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            //AnaEvent.AllEvents.Clear();
            //AnaEvent.CalcEventInfo();
            //AnaEvent.ClearDeletedNca();
            UpdateUIAfterLoading();

            if (this.drawEcgCommonWave1.IsLoaded)
            {
                this.drawEcgCommonWave1.RefreshData(resetUI);

                if (eventWaveViewControl1.EventList.Count > 0)
                {
                    this.drawEcgCommonWave1.OnWaveformStartTimeChanged(eventWaveViewControl1.EventList[0].Sample);
                }
            }
        }

        private void EventPageControl_Load(object sender, EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    if (!IsUiInitialized) InitUI();

                    UpdateUIAfterLoading();

                    Console.WriteLine("心电事件界面已加载");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心电事件");
            }
        }

        public override void InitUI()
        {
            base.InitUI();

            try
            {
                this.panel1.Hide();
                DisplayInitialUI();
                drawEcgCommonWave1.OnDataChanged += (d) => { UpdateUIAfterLoading(true); };
                this.eventWaveViewControl1.OnDataChanged += (d) => { UpdateUIAfterLoading(true); };

                //this.eventWaveViewControl1.OnDataChanged += (d) => { UpdateUIAfterLoading(true); this.drawEcgCommonWave1.forceWaveformRedraw(); };
                //this.eventWaveViewControl1.OnDataChanged += (d) => { this.eventWaveViewControl1.ForceWaveformRedraw(); this.drawEcgCommonWave1.forceWaveformRedraw(); };
                this.eventWaveViewControl1.OnSampleChanged += this.drawEcgCommonWave1.OnWaveformStartTimeChanged;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心电事件");
            }
        }

        private void DisplayInitialUI()
        {
            // 设置listView1的基础样式
            listView1.Items.Clear();
            listView1.View = View.Details;
            listView1.HeaderStyle = ColumnHeaderStyle.None;

            Form parentForm = FindForm();
            if (parentForm != null)
            {
                int totalWidth = (int)(parentForm.Width * 0.14);
                int firstColumnWidth = (int)(totalWidth * 0.7);
                int secondColumnWidth = (int)(totalWidth * 0.3);

                listView1.Columns.Clear();
                _ = listView1.Columns.Add("", firstColumnWidth);

                ColumnHeader secondColumn = new ColumnHeader
                {
                    Text = "",
                    Width = secondColumnWidth,
                    TextAlign = HorizontalAlignment.Right
                };
                _ = listView1.Columns.Add(secondColumn);
            }

            // 添加加载中的提示项
            ListViewItem loadingItem = new ListViewItem(CommonValues.Langs["加载中..."]);
            _ = loadingItem.SubItems.Add("");
            _ = listView1.Items.Add(loadingItem);
        }

        private void ShowLoadingState()
        {
            // 可以在这里添加加载动画或状态提示
            listView1.Enabled = false;
        }

        public async void UpdateUIAfterLoading(bool eventClicked = false)
        {
            while (AnaEvent.IsCalculating)
            {
                await Task.Delay(10);
            }

            // 更新UI显示
            DisplayEventsInListView(eventClicked);
            listView1.Enabled = true;
        }

        private void DisplayEventsInListView(bool eventClicked)
        {
            // 使用BeginUpdate/EndUpdate来减少UI刷新次数
            listView1.BeginUpdate();

            try
            {
                // 清空listView1
                listView1.Items.Clear();

                // 设置listView1的视图模式为Details
                listView1.View = View.Details;

                // 设置不显示表头
                listView1.HeaderStyle = ColumnHeaderStyle.None;

                Form parentForm = FindForm();
                if (parentForm == null)
                {
                    return;
                }
                int totalWidth = (int)(parentForm.Width * 0.14);

                // 计算列宽：第一列70%，第二列30%
                int firstColumnWidth = (int)(totalWidth * 0.65);
                int secondColumnWidth = (int)(totalWidth * 0.35);

                // 添加两列：无标题头
                listView1.Columns.Clear();
                _ = listView1.Columns.Add("", firstColumnWidth);

                // 创建第二列并设置右对齐
                ColumnHeader secondColumn = new ColumnHeader
                {
                    Text = "",
                    Width = secondColumnWidth,
                    TextAlign = HorizontalAlignment.Right
                };
                _ = listView1.Columns.Add(secondColumn);

                // 统计各类事件的数量
                Dictionary<string, string> eventCounts = new Dictionary<string, string>();

                // 统计事件
                CountEvents(eventCounts);

                ecgSettingValues EcgSets = CommonValues.SystemSettings.ecgSets;

                // 预分配列表容量，减少内存分配
                var itemsToAdd = new List<ListViewItem>(eventCounts.Count);

                // 将事件信息添加到临时列表
                foreach (KeyValuePair<string, string> eventPair in eventCounts)
                {
                    if (string.IsNullOrWhiteSpace(eventPair.Value)) continue;
                    string title;
                    switch (eventPair.Key)
                    {
                        case "停搏":
                            title = CommonValues.Langs[eventPair.Key] + "(>=" + EcgSets.customSets.asystoleTime / 1000f + "s)";
                            break;
                        case "长RR间期":
                            title = CommonValues.Langs[eventPair.Key] + "(>=" + EcgSets.customSets.longRR / 1000f + "s)";
                            break;
                        case "单发房早":
                        case "单发室早":
                        case "单发交界早":
                        case "心动过速":
                        case "心动过缓":
                        case "室性逸搏":
                        case "房性逸搏":
                        case "交界性逸搏":
                        case "R on T":
                        case "ST":
                        case "房早未下传":
                        case "室性融合波":
                            title = CommonValues.Langs[eventPair.Key] + "(" + CommonValues.Langs["个"] + ")";
                            break;
                        case "成对房早":
                        case "房性二联律":
                        case "房性三联律":
                        case "房速":
                        case "成对室早":
                        case "室性二联律":
                        case "室性三联律":
                        case "室速":
                        case "成对交界早":
                        case "交界二联律":
                        case "交界三联律":
                        case "交界速":
                            title = CommonValues.Langs[eventPair.Key] + "(" + CommonValues.Langs["阵"] + ")";
                            break;
                        case "最长RR间期":
                            title = CommonValues.Langs[eventPair.Key] + "(s)";
                            break;
                        case "最快心率":
                        case "最慢心率":
                            title = CommonValues.Langs[eventPair.Key] + "(bpm)";
                            break;
                        default:
                            title = eventPair.Key;
                            break;
                    }
                    ListViewItem item = new ListViewItem(title);
                    item.SubItems.Add(eventPair.Value.ToString());
                    itemsToAdd.Add(item);
                }

                // 批量添加项目，减少UI刷新
                listView1.Items.AddRange(itemsToAdd.ToArray());

                // 默认选中"最快心率"行（延迟执行，确保listView1完全渲染）
                SelectFastestHrItem(eventClicked);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "心电事件");
            }
            finally
            {
                listView1.EndUpdate();
            }
        }

        private void CountEvents(Dictionary<string, string> eventCounts)
        {
            try
            {
                foreach (string key in AnaEvent.AllEvents.Keys)
                {
                    if (!eventCounts.ContainsKey(key))
                    {
                        eventCounts[key] = "";
                    }
                    //List<AnaEvent.EventInfo> unDeletedEvent = AnaEvent.AllEvents[key].Where(e => e.Deleted == false).ToList();

                    switch (key)
                    {
                        case "最长RR间期":
                            eventCounts[key] = AnaEvent.AllEvents[key].Count > 0 ? Math.Round((double)AnaEvent.AllEvents[key][0].AvgRR / 1000f, 2).ToString() : "";
                            this.eventWaveViewControl1.EventCount[key] = AnaEvent.AllEvents[key].Count;
                            break;
                        case "最快心率":
                        case "最慢心率":
                            eventCounts[key] = AnaEvent.AllEvents[key].Count > 0 ? AnaEvent.AllEvents[key][0].AvgHr.ToString() : "";
                            this.eventWaveViewControl1.EventCount[key] = AnaEvent.AllEvents[key].Count;
                            break;
                        case "停搏":
                        case "长RR间期":
                        case "心动过速":
                        case "心动过缓":
                            eventCounts[key] = AnaEvent.AllEvents[key].Count > 0 ? AnaEvent.AllEvents[key].Count.ToString() : "";
                            this.eventWaveViewControl1.EventCount[key] = AnaEvent.AllEvents[key].Count;
                            break;
                        default:
                            eventCounts[key] = AnaEvent.AllEvents[key].Count > 0 ? AnaEvent.AllEvents[key].Count.ToString() : "";
                            this.eventWaveViewControl1.EventCount[key] = AnaEvent.AllEvents[key].Count;
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.CreateLog(ex, "事件");
            }

        }

        private void ListView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 获取点击位置对应的项
                ListViewHitTestInfo hitTestInfo = listView1.HitTest(e.Location);

                if (hitTestInfo.Item != null)
                {
                    // 处理鼠标点击事件
                    listView1.SelectedItems.Clear();
                    hitTestInfo.Item.Selected = true;

                    HandleListViewItemClick(hitTestInfo.Item);

                    this.eventWaveViewControl1.GetFocus();
                }
            }

        }

        private void ListView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (e.Item != null && e.Item.Selected)
            {
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(0,120,215)))
                {
                    e.Graphics.FillRectangle(bgBrush, e.Bounds);
                }

                TextRenderer.DrawText(e.Graphics, e.Item.SubItems[0].Text, e.Item.Font, e.Bounds, Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                if (e.Item.SubItems.Count > 1)
                {
                    TextRenderer.DrawText(e.Graphics, e.Item.SubItems[1].Text, e.Item.Font, e.Bounds, Color.White,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private void ListView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item != null && e.Item.Selected)
            {
                TextFormatFlags flags;
                if (e.ColumnIndex == 0)
                {
                    flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                }
                else
                {
                    flags = TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                }
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font, e.Bounds, Color.White, flags);
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private void ListView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private async void HandleListViewItemClick(ListViewItem clickedItem)
        {
            if (clickedItem == null)
            {
                return;
            }

            // 获取点击的事件名称（去掉单位后缀）
            string eventName = clickedItem.Text;

            // 移除单位后缀
            eventName = eventName.Substring(0, eventName.IndexOf('('));

            while (AnaEvent.IsCalculating)
            {
                await Task.Delay(10);
            }

            // 更新列表视图显示
            if(UpdateListViewDisplay(eventName))
            {
                // 更新事件波形显示控件
                eventWaveViewControl1.EventName = eventName;
                eventWaveViewControl1.EventList = AnaEvent.AllEvents[eventName];
                eventWaveViewControl1.PageIdx = 0;
                eventWaveViewControl1.ClickRectIdx = 0;
                eventWaveViewControl1.resetSelect();
                eventWaveViewControl1.UpdateDataForceWaveformRedraw();

                int sample = AnaEvent.AllEvents[eventName][0].Sample;
                this.drawEcgCommonWave1.OnWaveformStartTimeChanged(sample);

            }
            else
            {
                SelectFastestHrItem(false);
            }

        }
        private void ResetBottonStatus(string eventName)
        {
            switch (eventName)
            {
                case "停搏":
                    ModifyBtnStatus(true);
                    break;
                case "长RR间期":
                    ModifyBtnStatus(true);
                    break;
                case "最长RR间期":
                    ModifyBtnStatus(false);
                    break;
                case "最快心率":
                    ModifyBtnStatus(false);
                    break;
                case "最慢心率":
                    ModifyBtnStatus(false);
                    break;
                case "心动过速":
                    ModifyBtnStatus(true);
                    break;
                case "心动过缓":
                    ModifyBtnStatus(true);
                    break;
                case "ST":
                    ModifyBtnStatus(true);
                    break;
                case "单发房早":
                case "成对房早":
                case "房性二联律":
                case "房性三联律":
                case "房速":
                    ModifyBtnStatus(true);
                    break;
                case "单发室早":
                case "成对室早":
                case "室性二联律":
                case "室性三联律":
                case "室速":
                    ModifyBtnStatus(true);
                    break;
                case "单发交界早":
                case "成对交界早":
                case "交界二联律":
                case "交界三联律":
                case "交界速":
                    ModifyBtnStatus(true);
                    break;
                case "室性逸搏":
                    ModifyBtnStatus(true);
                    break;
                case "房性逸搏":
                    ModifyBtnStatus(true);
                    break;
                case "交界性逸搏":
                    ModifyBtnStatus(true);
                    break;
                case "R on T":
                    ModifyBtnStatus(true);
                    break;
                case "房早未下传":
                    ModifyBtnStatus(true);
                    break;
                default:
                    // 对于未知事件类型，不重新加载
                    break;
            }
        }

        private void ModifyBtnStatus(bool bFlag)
        {
            this.btnDelete.Enabled = bFlag;
            this.btnDeletePage.Enabled = bFlag;
            this.btnDeleteAll.Enabled = bFlag;
        }

        private bool UpdateListViewDisplay(string clickEventName)
        {
            //foreach (string key in AnaEvent.AllEvents.Keys)
            //{
            //    Console.WriteLine("---key:"+key +" count: "+ AnaEvent.AllEvents[key].Count);
            //}
            //foreach (ListViewItem item in listView1.Items)
            //{
            //    Console.WriteLine("---item text:" + item.Text);

            //}
            ecgSettingValues EcgSets = CommonValues.SystemSettings.ecgSets;

            bool bFlag = true;
            // 重新统计事件数量并更新显示
            Dictionary<string, string> eventCounts = new Dictionary<string, string>();
            CountEvents(eventCounts);
            int eventIdx = 0;
            foreach (KeyValuePair<string, string> eventPair in eventCounts)
            {
                bool bEventExist = false;
                string eventName = eventPair.Key;
                foreach (ListViewItem item in listView1.Items)
                {
                    string itemText = item.Text;
                    string itemEventName = itemText.Substring(0, itemText.IndexOf('('));
                    if (eventName == itemEventName)
                    {
                        bEventExist = true;
                        item.SubItems[1].Text = CommonValues.Langs[eventPair.Value];
                        break;
                    }
                }
                if (!bEventExist && !string.IsNullOrWhiteSpace(eventPair.Value))
                {
                    string title;

                    switch (eventPair.Key)
                    {
                        case "停搏":
                            title = CommonValues.Langs[eventPair.Key] + "(>=" + EcgSets.customSets.asystoleTime / 1000f + "s)";
                            break;
                        case "长RR间期":
                            title = CommonValues.Langs[eventPair.Key] + "(>=" + EcgSets.customSets.longRR / 1000f + "s)";
                            break;
                        case "单发房早":
                        case "单发室早":
                        case "单发交界早":
                        case "心动过速":
                        case "心动过缓":
                        case "室性逸搏":
                        case "房性逸搏":
                        case "交界性逸搏":
                        case "R on T":
                        case "ST":
                        case "房早未下传":
                            title = CommonValues.Langs[eventPair.Key] + "(" + CommonValues.Langs["个"] + ")";
                            break;
                        case "成对房早":
                        case "房性二联律":
                        case "房性三联律":
                        case "房速":
                        case "成对室早":
                        case "室性二联律":
                        case "室性三联律":
                        case "室速":
                        case "成对交界早":
                        case "交界二联律":
                        case "交界三联律":
                        case "交界速":
                            title = CommonValues.Langs[eventPair.Key] + "(" + CommonValues.Langs["阵"] + ")";
                            break;
                        case "最长RR间期":
                            title = CommonValues.Langs[eventPair.Key] + "(s)";
                            break;
                        case "最快心率":
                        case "最慢心率":
                            title = CommonValues.Langs[eventPair.Key] + "(bpm)";
                            break;
                        default:
                            title = eventPair.Key;
                            break;
                    }
                    ListViewItem item = new ListViewItem(title);
                    item.SubItems.Add(eventPair.Value.ToString());
                    listView1.Items.Insert(eventIdx, item);
                    ++eventIdx;
                }
            }
            //// 更新列表项
            //foreach (ListViewItem item in listView1.Items)
            //{
            //    string itemText = item.Text;
            //    string eventName = itemText;

            //    // 移除单位后缀
            //    eventName = itemText.Substring(0, itemText.IndexOf('('));
            //    if(!eventCounts.ContainsKey(eventName) || eventCounts[eventName] == "0")
            //    {
            //        AnaEvent.AllEvents.Remove(eventName);
            //        item.Remove();
            //        if(clickEventName == eventName)
            //        {
            //            bFlag = false;
            //        }
            //    }
            //}

            return bFlag;
        }

        private void SelectFastestHrItem(bool eventClicked)
        {
            if (listView1.Items.Count == 0)
            {
                return;
            }

            //// 确保listView1获得焦点
            //if (!listView1.Focused)
            //{
            //    listView1.Focus();
            //}

            // 查找"最快心率"行
            ListViewItem fastestHrItem = null;
            foreach (ListViewItem item in listView1.Items)
            {
                if(eventClicked)
                {
                    string eventName = this.eventWaveViewControl1.EventName;
                    if (item.Text.StartsWith(eventName) && AnaEvent.AllEvents[eventName].Count > 0)
                    {
                        fastestHrItem = item;
                        break;
                    }
                }
                else
                {
                    if (item.Text.StartsWith(CommonValues.Langs["最快心率"]))
                    {
                        fastestHrItem = item;
                        break;
                    }
                }

            }

            // 如果没有找到"最快心率"，选择第一行
            if (fastestHrItem == null && listView1.Items.Count > 0)
            {
                fastestHrItem = listView1.Items[0];
            }

            if (fastestHrItem != null)
            {
                //// 清除之前的选择
                //foreach (ListViewItem item in listView1.Items)
                //{
                //    item.BackColor = SystemColors.Window;
                //    item.ForeColor = SystemColors.WindowText;
                //}

                //// 设置选中行的背景色为蓝色
                //fastestHrItem.BackColor = Color.Blue;
                //fastestHrItem.ForeColor = Color.White;

                // 选中该项
                fastestHrItem.Selected = true;
                fastestHrItem.Focused = true;

                //// 确保listView1有焦点
                //listView1.Focus();

                //ModifyBtnStatus(false);

                // 触发事件显示
                string eventName = fastestHrItem.Text;

                // 移除单位后缀
                eventName = eventName.Substring(0, eventName.LastIndexOf('('));
                eventWaveViewControl1.EventName = eventName;
                eventWaveViewControl1.EventList = AnaEvent.AllEvents[eventName];
                //eventWaveViewControl1.PageIdx = 0;
                //eventWaveViewControl1.ClickRectIdx = eventWaveViewControl1.LastCheckIdx;
                eventWaveViewControl1.UpdateDataForceWaveformRedraw();
                eventWaveViewControl1.UpdateCommonWave();

            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            eventWaveViewControl1.DeleteSelectEvents();
        }

        private void btnDeletePage_Click(object sender, EventArgs e)
        {
            eventWaveViewControl1.DeletePageEvents();
        }

        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            eventWaveViewControl1.DeleteAllEvents();
        }
    }
}
