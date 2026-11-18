using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace AnalysisApp.Commons
{
    /// <summary>
    /// 时长输入控件（基于 MaskedTextBox）：
    /// 显示/输入格式 "HH:mm:ss"，小时位可超过 24（如 24:00:00、36:15:00），
    /// 用于房颤/房扑持续时间等时长字段。
    /// 用 TryGetDuration 取时长，用 SetTimeText 赋值；
    /// 非法输入在 Validating 时自动恢复为上一个有效值。
    /// </summary>
    public class TimeDurationBox : MaskedTextBox
    {
        private string _lastValidText;

        /// <summary>小时上限（默认 99，与掩码的两位小时对应）</summary>
        public double MaxHours { get; set; } = 99;

        public TimeDurationBox()
        {
            Mask = "00:00:00";
            Text = "00:00:00";
            _lastValidText = Text;
            Validating += TimeDurationBox_Validating;
            Validated += (s, e) => _lastValidText = Text;
        }

        /// <summary>尝试获取当前输入对应的时长；输入非法返回 false</summary>
        public bool TryGetDuration(out TimeSpan value)
        {
            return TryParseDuration(Text, MaxHours, out value);
        }

        /// <summary>
        /// 设置显示文本并规范化：接受 "HH:mm:ss"（小时可大于 24，如 "26:13:45"）。
        /// 超出掩码可表示范围（大于 99 小时）时按 "99:59:59" 截断显示；
        /// 无法解析时恢复为 "00:00:00"。
        /// </summary>
        public void SetTimeText(string text)
        {
            if (TryParseDuration(text, double.MaxValue, out TimeSpan ts))
            {
                if (ts.TotalHours > MaxHours)
                {
                    // 掩码只有两位小时，超出可显示范围时截断
                    Text = "99:59:59";
                }
                else
                {
                    Text = $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
                }
            }
            else
            {
                Text = "00:00:00";
            }
            _lastValidText = Text;
        }

        /// <summary>
        /// 解析 "HH:mm:ss" 时长文本（小时位允许大于 24），超过 maxHours 视为非法。
        /// 注意：不能用 TimeSpan.TryParse——它把 "24:00:00" 的前段当作天数（得到 24 天 = 576 小时），
        /// 因此这里按 ':' 分段手工解析，小时、分钟、秒各归其位。
        /// </summary>
        public static bool TryParseDuration(string text, double maxHours, out TimeSpan value)
        {
            value = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            // 去掉掩码占位符与空白后按段解析
            text = text.Trim().Replace(" ", "").Replace("_", "");
            string[] parts = text.Split(':');
            if (parts.Length != 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int hours) ||
                !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int minutes) ||
                !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out int seconds))
            {
                return false;
            }

            // 分、秒必须落在 0~59，小时可为任意非负值（可大于 24）
            if (minutes > 59 || seconds > 59)
            {
                return false;
            }

            // 转成秒计算，避免 TimeSpan 构造函数越界
            long totalSeconds = (long)hours * 3600 + minutes * 60 + seconds;
            if (totalSeconds > (long)TimeSpan.MaxValue.TotalSeconds)
            {
                return false;
            }

            if (totalSeconds / 3600.0 > maxHours)
            {
                return false;
            }

            value = TimeSpan.FromSeconds(totalSeconds);
            return true;
        }

        private void TimeDurationBox_Validating(object sender, CancelEventArgs e)
        {
            if (TryGetDuration(out _))
            {
                _lastValidText = Text;
            }
            else
            {
                // 输入非法：恢复上一个有效值，不强制拦截焦点
                Text = _lastValidText;
            }
        }
    }
}
