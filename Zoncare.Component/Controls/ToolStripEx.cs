using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Zoncare.Utilities;

namespace Zoncare.Component.Controls
{
    public class ToolStripEx : ToolStrip
    {
        /// <summary>
        /// 被禁用的菜单元素索引列表
        /// </summary>
        [Browsable(false)]
        public ObservableCollection<string> DisabledItems { get; set; }

        [Browsable(false)]
        public Dictionary<string, Image> OriginalImages { get; set; }
        [Browsable(false)]
        public Dictionary<string, Image> DisabledImages { get; set; }

        /// <summary>
        /// 设置工具条项的原始图像及禁用效果图像
        /// </summary>
        /// <param name="item"></param>
        public void ResetImages(ToolStripItem item = null)
        {
            if (OriginalImages == null || DisabledImages == null) return;

            if (item == null)
            {
                foreach (string key in DisabledImages.Keys) { DisabledImages[key]?.Dispose(); }
                DisabledImages.Clear();
                foreach (ToolStripItem tsItem in this.Items)
                {
                    if (!OriginalImages.ContainsKey(tsItem.Name)) OriginalImages[tsItem.Name] = tsItem.Image;

                    if (tsItem.Image != null)
                    {
                        Bitmap srcBmp = new Bitmap(tsItem.Image);
                        DisabledImages[tsItem.Name] = PubFuns.GetGrayScaleImage(srcBmp);
                        srcBmp.Dispose();
                        srcBmp = null;
                    }
                    else
                    {
                        DisabledImages[tsItem.Name]?.Dispose();
                        DisabledImages[tsItem.Name] = null;
                    }
                }
            }
            else
            {
                OriginalImages[item.Name] = item.Image;

                if (item.Image != null)
                {
                    Bitmap srcBmp = new Bitmap(item.Image);
                    DisabledImages[item.Name] = PubFuns.GetGrayScaleImage(srcBmp);
                    srcBmp.Dispose();
                    srcBmp = null;
                }
                else
                {
                    DisabledImages[item.Name]?.Dispose();
                    DisabledImages[item.Name] = null;
                }
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            OriginalImages = new Dictionary<string, Image>();
            DisabledImages = new Dictionary<string, Image>();

            DisabledItems = new ObservableCollection<string>();
            DisabledItems.CollectionChanged += DisabledItems_CollectionChanged;

            this.Renderer = new ToolStripBackcolorRenderer();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, this.Width - 2, this.Height - 2);
            e.Graphics.SetClip(rect);

            base.OnPaint(e);
            e.Graphics.ResetClip();
        }

        //protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        //{
        //    if (this.DisabledItems.Contains(e.ClickedItem.Name)) //被禁用
        //    {
        //        return;
        //    }

        //    base.OnItemClicked(e);
        //}

        protected override void OnMouseDown(MouseEventArgs mea)
        {
            ToolStripItem tsItem = this.GetItemAt(mea.X, mea.Y);
            if (tsItem != null)
            {
                if (this.DisabledItems.Contains(tsItem.Name)) //被禁用
                {
                    return;
                }
            }

            base.OnMouseDown(mea);
        }

        private void DisabledItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //对启用了[禁用项]的toolstrip进行图像设置
            foreach (ToolStripItem tsItem in this.Items)
            {
                if (DisabledItems.Contains(tsItem.Name))
                {
                    if (tsItem.ForeColor != Color.Silver)
                    {
                        tsItem.ForeColor = Color.Silver;
                        tsItem.Image = DisabledImages[tsItem.Name];
                    }
                }
                else
                {
                    if (tsItem.ForeColor != Color.White)
                    {
                        tsItem.ForeColor = Color.White;
                        tsItem.Image = OriginalImages[tsItem.Name];
                    }
                }
            }
        }
    }

}
