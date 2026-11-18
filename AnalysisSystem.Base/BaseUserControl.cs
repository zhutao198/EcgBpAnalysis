using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Zoncare.Common;

namespace AnalysisSystem.Base
{
    public partial class BaseUserControl : UserControl, IDataMonitor
    {
        public bool IsActive { get; set; }

        /// <summary>
        /// 是否已同步更新数据
        /// </summary>
        public bool DataSourceTracked { get; set; }

        //protected dynamic DefaultLeadInfo;
        protected PersistDictionary<string> _langs = CommonValues.Langs;


        public BaseUserControl()
        {
            InitializeComponent();
        }

        public void OnDataSourceChanged()
        {
            CommonValues.MainTreadContext.Post(state =>
            {
                this.InvokeIfRequired(() =>
                {
                    RefreshData();
                });
                DataSourceTracked = true;
            }, null);
        }

        public virtual void RefreshData()
        {

        }

        private void BaseUserControl_Load(object sender, EventArgs e)
        {

        }

        //protected virtual void RefreshData()
        //{

        //}

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
    }
}
