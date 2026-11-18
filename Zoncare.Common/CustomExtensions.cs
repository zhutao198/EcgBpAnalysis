using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Zoncare.Common
{
    //标识类所包含的测量参数属性的个数
    public class MeasureParaCount : Attribute
    {
        public int Value { get; set; }

        public MeasureParaCount(int cnt)
        {
            this.Value = cnt;
        }
    }

    //标识是否是采集界面
    public class ECG_SAMPLEING : Attribute
    {
        public bool Value { get; set; } = false;

        public ECG_SAMPLEING(bool Value)
        {
            this.Value = Value;
        }
    }

    //多语言通用窗口标识
    public class UniformWindow : Attribute
    {
        public bool IsSpecific { get; private set; }
        public string FontFamily { get; set; }
        public float FontSize { get; set; } = 12f;

        public UniformWindow(bool needChangeFont)
        {
            this.IsSpecific = needChangeFont;
        }
    }

    //标识1920*1080屏幕分辨率下窗口的设计时尺寸为1600*900
    public class W1600_H900 : Attribute
    {
        public bool Value { get; set; } = false;

        public W1600_H900(bool Value)
        {
            this.Value = Value;
        }
    }

    //重写Dictionary索引器
    public class PersistDictionary<TKey> : Dictionary<TKey, string>
    {
        private bool _doTranslate = false;

        /// <summary>
        /// 构造函数, 按需对键值进行翻译
        /// </summary>
        /// <param name="shouldTranslate">是否需对词典中的值进行翻译</param>
        public PersistDictionary(bool shouldTranslate = false) : base()
        {
            _doTranslate = shouldTranslate;
        }

        public new string this[TKey key]
        {
            get
            {
                if (this.ContainsKey(key))
                {
                    if (_doTranslate == true)
                        return CommonValues.Langs[base[key]];
                    else
                        return base[key];
                }
                else
                {
                    return key.ToString();
                }
            }
            set { base[key] = value; }
        }
    }

    //界面大小改变时, 处理各界面元素的次序比较器
    public class ControlResizeComparer : IComparer<Control>
    {
        private ListSortDirection direction;

        public ControlResizeComparer(ListSortDirection direction)
        {
            this.direction = direction;
        }

        public int Compare(Control x, Control y)
        {
            int returnValue = getPriority(x, y);
            if (direction == ListSortDirection.Descending) returnValue = returnValue * -1;

            return returnValue;
        }

        public int getPriority(Control x, Control y)
        {
            if (x.Equals(y)) return 0;
            if (x is Label || x is Button || x is ComboBox || x is TextBox || x is DateTimePicker) return -1;
            if (y is Label || y is Button || y is ComboBox || y is TextBox || y is DateTimePicker) return 1;

            if (x is Form) return 1;
            if (y is Form) return -1;

            if (x.Controls.Contains(y))
                return 1;
            else if (y.Controls.Contains(x))
                return -1;

            if (x is TableLayoutPanel) return 1;
            if (y is TableLayoutPanel) return -1;

            if (x is TabControl) return 1;
            if (y is TabControl) return -1;

            if (x is Panel) return 1;
            if (y is Panel) return -1;

            return 0;
        }
    }

    //日期类型字符串比较
    public class DateTimeStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (string.IsNullOrEmpty(x)) return -1;
            if (string.IsNullOrEmpty(y)) return 1;

            if (x.Equals(y)) return 0;

            DateTime.TryParse(x, out DateTime dateX);
            DateTime.TryParse(y, out DateTime dateY);

            return dateX.CompareTo(dateY);
        }
    }

    //比较绑定数据源的列表元素时要使用的比较器
    public class ObjectPropertyCompare<T> : IComparer<T>
    {
        private PropertyDescriptor property;
        private ListSortDirection direction;

        // 构造函数
        public ObjectPropertyCompare(PropertyDescriptor property, ListSortDirection direction)
        {
            this.property = property;
            this.direction = direction;
        }

        // 实现IComparer中方法
        public int Compare(T x, T y)
        {
            object xValue = x.GetType().GetProperty(property.Name).GetValue(x, null);
            object yValue = y.GetType().GetProperty(property.Name).GetValue(y, null);

            int returnValue;

            if (xValue == null && yValue == null)
            {
                returnValue = 0;
            }
            else if (xValue == null)
            {
                returnValue = -1;
            }
            else if (yValue == null)
            {
                returnValue = 1;
            }
            else if (xValue is IComparable)
            {
                returnValue = ((IComparable)xValue).CompareTo(yValue);
            }
            else if (xValue.Equals(yValue))
            {
                returnValue = 0;
            }
            else
            {
                returnValue = xValue.ToString().CompareTo(yValue.ToString());
            }

            if (direction == ListSortDirection.Ascending)
            {
                return returnValue;
            }
            else
            {
                return returnValue * -1;
            }
        }
    }

    /// <summary>
    /// 自定义绑定列表类
    /// </summary>
    /// <typeparam name="T">列表对象类型</typeparam>
    public class BindingCollection<T> : BindingList<T>
    {
        private bool isSorted;
        private PropertyDescriptor sortProperty;
        private ListSortDirection sortDirection;

        private ISynchronizeInvoke _SyncObject;
        private Action<ListChangedEventArgs> _FireEventAction;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BindingCollection()
        : base()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="list">实现IList接口的列表对象</param>
        public BindingCollection(IList<T> list) : base(list)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="list">实现IList接口的列表对象</param>
        /// <param name="syncObject">支持同步或异步执行委托的对象</param>
        public BindingCollection(IList<T> list, ISynchronizeInvoke syncObject) : base(list)
        {
            _SyncObject = syncObject;
            _FireEventAction = listChangeFireEvent;
        }

        //重写OnListChanged事件使其可跨线程执行
        protected override void OnListChanged(ListChangedEventArgs args)
        {
            if (_SyncObject == null)
            {
                listChangeFireEvent(args);
            }
            else
            {
                _SyncObject.Invoke(_FireEventAction, new object[] { args });
            }
        }

        //ListChanged事件发生时执行的方法
        private void listChangeFireEvent(ListChangedEventArgs args)
        {
            base.OnListChanged(args);
        }

        /// <summary>
        /// 自定义排序操作
        /// </summary>
        /// <param name="property"></param>
        /// <param name="direction"></param>
        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            List<T> items = this.Items as List<T>;

            if (items != null)
            {
                ObjectPropertyCompare<T> pc = new ObjectPropertyCompare<T>(property, direction);
                items.Sort(pc);
                isSorted = true;
            }
            else
            {
                isSorted = false;
            }

            sortProperty = property;
            sortDirection = direction;

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// 获取一个值，指示列表是否已排序
        /// </summary>
        protected override bool IsSortedCore
        {
            get
            {
                return isSorted;
            }
        }

        /// <summary>
        /// 获取一个值，指示列表是否支持排序
        /// </summary>
        protected override bool SupportsSortingCore
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 获取一个值，指定类别排序方向
        /// </summary>
        protected override ListSortDirection SortDirectionCore
        {
            get
            {
                return sortDirection;
            }
        }

        /// <summary>
        /// 获取排序属性说明符
        /// </summary>
        protected override PropertyDescriptor SortPropertyCore
        {
            get
            {
                return sortProperty;
            }
        }

        /// <summary>
        /// 移除默认实现的排序
        /// </summary>
        protected override void RemoveSortCore()
        {
            isSorted = false;
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
    }

    /// <summary>
    /// Wrapper around TcpListener that exposes the Active property
    /// </summary>
    public class TcpListenerEx : TcpListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.TcpListener"/> class with the specified local endpoint.
        /// </summary>
        /// <param name="localEP">An <see cref="T:System.Net.IPEndPoint"/> that represents the local endpoint to which to bind the listener <see cref="T:System.Net.Sockets.Socket"/>. </param><exception cref="T:System.ArgumentNullException"><paramref name="localEP"/> is null. </exception>
        public TcpListenerEx(IPEndPoint localEP) : base(localEP)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.TcpListener"/> class that listens for incoming connection attempts on the specified local IP address and port number.
        /// </summary>
        /// <param name="localaddr">An <see cref="T:System.Net.IPAddress"/> that represents the local IP address. </param>
        /// <param name="port">The port on which to listen for incoming connection attempts. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="localaddr"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port"/> is not between <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="F:System.Net.IPEndPoint.MaxPort"/>. </exception>
        public TcpListenerEx(IPAddress localaddr, int port) : base(localaddr, port)
        {
        }

        public new bool Active
        {
            get { return base.Active; }
        }
    }


    #region PropertyGrid相关
    /// <summary>
    /// 标识在属性列表中是否可见
    /// </summary>
    public class BrowsableEx : Attribute
    {
        private bool _browsable = true;
        public bool Browsable
        {
            get { return _browsable; }
            set { _browsable = value; }
        }
        public BrowsableEx()
        {

        }
        public BrowsableEx(bool browsable)
        {
            _browsable = browsable;
        }
    }

    public class InfCustomTypeDescriptor0 : ICustomTypeDescriptor
    {
        object instance = null;
        public InfCustomTypeDescriptor0(object source)
        {
            instance = source;
        }
        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(instance);
        }
        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(instance);
        }
        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(instance);
        }
        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(instance);
        }
        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(instance);
        }
        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(instance);
        }
        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(instance, editorBaseType);
        }
        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(instance, attributes);
        }
        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(instance);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(instance);
        }
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            List<PropertyDes> tmpPDCLst = new List<PropertyDes>();
            PropertyDescriptorCollection tmpPDs = TypeDescriptor.GetProperties(instance, attributes);
            IEnumerator tmpIe = tmpPDs.GetEnumerator();
            while (tmpIe.MoveNext())
            {
                PropertyDescriptor tmpPD = tmpIe.Current as PropertyDescriptor;
                tmpPDCLst.Add(new PropertyDes(tmpPD));
            }
            return new PropertyDescriptorCollection(tmpPDCLst.ToArray());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return instance;
        }
    }

    /// <summary>
    /// 用于适配多语言的自定义类型信息
    /// </summary>
    public class InfCustomTypeDescriptor : CustomTypeDescriptor
    {
        object instance = null;
        public InfCustomTypeDescriptor(object source)
        {
            instance = source;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            List<PropertyDes> tmpPDCLst = new List<PropertyDes>();
            PropertyDescriptorCollection tmpPDs = TypeDescriptor.GetProperties(instance, attributes);
            IEnumerator tmpIe = tmpPDs.GetEnumerator();
            while (tmpIe.MoveNext())
            {
                PropertyDescriptor tmpPD = tmpIe.Current as PropertyDescriptor;
                tmpPDCLst.Add(new PropertyDes(tmpPD));
            }
            return new PropertyDescriptorCollection(tmpPDCLst.ToArray());
        }

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            return instance;
        }
    }

    public class PropertyDes : PropertyDescriptor
    {
        private PropertyDescriptor _propDes;

        public PropertyDes(PropertyDescriptor propDes) : base(propDes)
        {
            _propDes = propDes;
            string ctg = this.Category;
            string dispName = this.DisplayName;
        }

        public override Type ComponentType
        {
            get { return _propDes.ComponentType; }
        }
        public override bool IsReadOnly
        {
            get { return _propDes.IsReadOnly; }
        }
        public override Type PropertyType
        {
            get { return _propDes.PropertyType; }
        }
        public override bool CanResetValue(object component)
        {
            return _propDes.CanResetValue(component);
        }
        public override object GetValue(object component)
        {
            return _propDes.GetValue(component);
        }
        public override void ResetValue(object component)
        {
            _propDes.ResetValue(component);
        }
        public override void SetValue(object component, object value)
        {
            _propDes.SetValue(component, value);
        }
        public override bool ShouldSerializeValue(object component)
        {
            return _propDes.ShouldSerializeValue(component);
        }

        public override string Category
        {
            get
            {
                string ctg = base.Category;
                if (!string.IsNullOrEmpty(ctg) && ctg != "Misc")
                {
                    int level = ctg.LastIndexOf('\t');
                    return new string('\t', level + 1) + CommonValues.Langs[ctg.Substring(level + 1)];
                }
                else
                {
                    return ctg;
                }
            }
        }

        public override string DisplayName
        {
            get
            {
                return CommonValues.Langs[base.DisplayName];
            }
        }
    }
    #endregion

}
