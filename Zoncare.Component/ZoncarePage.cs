using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Zoncare.Component
{
    [DefaultEvent("PageIndexChanged"), DefaultProperty("RecordCount"), Description("C# WinForm下的分页用户控件"), ToolboxBitmap(typeof(ZoncarePage), "Resources.Bitmap.pager")]
    //[Designer(typeof(MyControlDesigner))]
    //[DesignTimeVisible(true)]
    public partial class ZoncarePage : UserControl
    {
        #region Members
        private int _PageSize = 20;
        private int _PageIndex = 1;
        private int _PageCount = 1;
        private int _RecordCount = 0;

        private Dictionary<string, string> _Langs;

        [Description("更改页面索引事件"), Category("自定义事件")]
        public EventHandler PageIndexChanged;

        [DefaultValue(20), Category("自定义属性"), Description("每页显示的记录条数")]
        public int PageSize
        {
            get
            {
                return this._PageSize;
            }
            set
            {
                if (value <= 1) { value = 20; }

                this._PageSize = value;
            }
        }

        [DefaultValue(1), Category("自定义属性"), Description("当前显示的页码"), Browsable(false)]
        public int PageIndex
        {
            get
            {
                return this._PageIndex;
            }
            set
            {
                this._PageIndex = value;
                setBtnEnabled(this);
            }
        }

        [Category("自定义属性"), Description("总记录条数"), Browsable(false)]
        public int RecordCount
        {
            get
            {
                return this._RecordCount;
            }
            set
            {
                this._RecordCount = value;
                this.lblTotalRecords.Text = _RecordCount.ToString();
                this._PageCount = getPageCount(_RecordCount, this._PageSize);
                if (_PageIndex > _PageCount) _PageIndex = _PageCount;
                setPageInfoText();
            }
        }

        [Category("自定义属性"), Description("总页数"), Browsable(false)]
        public int PageCount
        {
            get
            {
                return this._PageCount;
            }
        }

        [Description("与分页控件关联的数据集"), Browsable(false)]
        public object OriginalData
        {
            get; set;
        }

        [Description("语言包相关数据"), Browsable(false)]
        public Dictionary<string, string> Langs
        {
            get { return _Langs; }
            set { _Langs = value; LoadControlText(); }
        }
        #endregion

        public ZoncarePage()
        {
            InitializeComponent();
        }

        #region Methods
        protected int getPageCount(int RecordCounts, int PageSizes)
        {
            if (RecordCounts <= PageSizes) return 1;

            return (RecordCounts + (PageSizes - 1)) / PageSizes;

            //int num = 0;
            //string str = (Convert.ToDouble(RecordCounts) / Convert.ToDouble(PageSizes)).ToString(CultureInfo.InvariantCulture);
            //if (str.IndexOf(".") < 0)
            //{
            //    return Convert.ToInt32(str);
            //}
            //string[] strArray = Regex.Split(str, @"\.", RegexOptions.IgnoreCase);
            //if (!string.IsNullOrEmpty(strArray[1].ToString()))
            //{
            //    num = Convert.ToInt32(strArray[0]) + 1;
            //}
            //return num;
        }

        protected void setBtnEnabled(object sender)
        {
            if (object.Equals(sender, this.btnNext) || object.Equals(sender, this.btnLast))
                this.btnPrevious.Focus();
        }

        private void setPageInfoText()
        {
            this.tbxPageIndex.Text = _PageIndex.ToString();
            this.lblTotalPages.Text = _PageCount.ToString();
        }
        #endregion

        #region Events
        private void ZoncarePage_Load(object sender, EventArgs e)
        {
            this.cbxPageSize.Text = this._PageSize.ToString();
            this.lblTotalRecords.Text = _RecordCount.ToString();
            setPageInfoText();
            setBtnEnabled(sender);

            cbxPageSize.Dock = DockStyle.Fill;
            cbxPageSize.Dock = DockStyle.None;
            cbxPageSize.Anchor = AnchorStyles.None;
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            this._PageIndex = 1;
            this.onPagesizeChanged(sender, e);
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            this._PageIndex = this._PageCount;
            this.onPagesizeChanged(sender, e);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            try
            {
                int num = int.Parse(this.tbxPageIndex.Text.Trim());
                int num2 = Convert.ToInt32(num) - 1;
                if (num2 <= 0)
                {
                    num2 = 1;
                }
                this._PageIndex = num2;
                this.onPagesizeChanged(sender, e);
            }
            catch (Exception)
            {
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                int num = int.Parse(this.tbxPageIndex.Text.Trim());
                int num2 = Convert.ToInt32(num) + 1;
                if (num2 >= this.PageCount)
                {
                    num2 = this.PageCount;
                }
                this._PageIndex = num2;
                this.onPagesizeChanged(sender, e);
            }
            catch (Exception)
            {
            }
        }

        private void btnSkipTo_Click(object sender, EventArgs e)
        {
            string text = this.tbxPageIndex.Text;
            int num = this._PageIndex;
            if (string.IsNullOrEmpty(text))
            {
                num = 1;
                this.tbxPageIndex.Text = "1";
            }
            else
            {
                num = Convert.ToInt32(text);
                if (num > this._PageCount)
                {
                    num = this._PageCount;
                    this.tbxPageIndex.Text = this._PageCount.ToString();
                    this.tbxPageIndex.SelectionStart = this.tbxPageIndex.Text.Length;
                }
                else if (num < 1)
                {
                    num = 1;
                    this.tbxPageIndex.Text = "1";
                    this.tbxPageIndex.SelectionStart = this.tbxPageIndex.Text.Length;
                }

                this._PageIndex = num;
            }
            this.onPagesizeChanged(sender, e);
        }

        private void cbxPageSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b') //允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9')) //允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
            if (e.KeyChar == 13)
            {
                _PageSize = Convert.ToInt32(cbxPageSize.Text);
                _PageCount = getPageCount(this._RecordCount, this._PageSize);
                _PageIndex = 1;
                onPagesizeChanged(sender, e);
            }
        }

        private void tbxPageIndex_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.btnSkipTo_Click(sender, e);
            }
        }

        private void cbxPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PageIndexChanged == null) return;
            this._PageSize = Convert.ToInt32(cbxPageSize.Text);
            this._PageCount = getPageCount(this._RecordCount, this._PageSize);
            this._PageIndex = 1;
            onPagesizeChanged(sender, e);
        }

        private void onPagesizeChanged(object sender, EventArgs e)
        {
            try
            {
                this.PageIndexChanged(sender, e);
                setPageInfoText();
                setBtnEnabled(sender);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The event 'PageIndexChanged' is not implemented!");
            }
        }

        private void LoadControlText()
        {
            if (_Langs != null) //语言转换
            {
                List<Control> lstControls = RetriveSubControls<Control>(this);
                foreach (Control con in lstControls)
                {
                    if ((con is Label) || (con is CheckBox) || (con is RadioButton) || (con is Button))
                    {
                        if (_Langs.ContainsKey(con.Text))
                        {
                            con.Text = _Langs[con.Text];
                            if (con.Equals(this.lblRecord))
                            {
                                if (this.lblRecord.Text != "条") this.lblRecord.Visible = false;
                            }
                        }
                    }
                }
            }
        }

        private List<T> RetriveSubControls<T>(Control parent, bool reverted = false, bool includeSelf = false) where T : Control
        {
            List<T> list = new List<T>();
            if (includeSelf)
            {
                list.Add(parent as T);
            }
            Stack<Control> stack = new Stack<Control>();
            stack.Push(parent);
            while (stack.Count > 0)
            {
                Control control = stack.Pop();
                foreach (object obj in control.Controls)
                {
                    Control item = (Control)obj;
                    stack.Push(item);
                }
                if (control is T)
                {
                    if (!reverted)
                    {
                        list.Insert(0, control as T);
                    }
                    else
                    {
                        list.Add(control as T);
                    }
                }
            }
            return list;
        }

        #endregion
    }

    //public class MyControlDesigner : ParentControlDesigner
    //{
    //    private ZoncarePage MyControl;

    //    public override void Initialize(IComponent component)
    //    {
    //        base.Initialize(component);
    //        MyControl = (ZoncarePage)component;

    //        this.EnableDesignMode(MyControl, "ZoncarePage");
    //    }
    //}
}
