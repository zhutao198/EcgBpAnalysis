using System;
using System.Windows.Forms;

namespace Zoncare.Component.Controls
{
    public class ZqToolStrip : ToolStrip
    {
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int MA_ACTIVATEANDEAT = 2;
        private const int MA_ACTIVATE = 1;

        public ZqToolStrip()
        {
            // 开启双缓冲, 减少工具栏刷新(如切换按钮选中态)时的闪烁
            this.DoubleBuffered = true;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // 检查消息是否为 WM_MOUSEACTIVATE，
            // 并且其返回值表明消息被“吞掉”了 (MA_ACTIVATEANDEAT)
            if (m.Msg == WM_MOUSEACTIVATE && m.Result == (IntPtr)MA_ACTIVATEANDEAT)
            {
                // 修改返回值，告诉系统正常激活窗体，但不要“吃掉”此次点击
                m.Result = (IntPtr)MA_ACTIVATE;
            }
        }
    }

}
