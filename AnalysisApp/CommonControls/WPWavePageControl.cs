using AnalysisApp.Commons;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;

namespace AnalysisApp.CommonControls
{
    public partial class WPWavePageControl : BaseDrawingControl
    {
        public List<BeatInfo> SourceBeats = new List<BeatInfo>();

        public List<int> LeadIdx = new List<int>(1);
        public int BeatDuration = 0;                //每个心搏波形的持续时间ms
        public int Rpos = 0;                        //每个心搏R波的位置ms
        public int Lines = 0;                       //一小时的波形被分割的行数
        public float Scale = 1f;                    //放大倍数

        private int BeatPointCount = 0;             //每个心搏点的个数
        private int AllPointCount = 0;              //总的点数

        private List<List<float[]>> LineDatas;
        private string[] LineTime;
        private int RectBeatCount;

        private float blueVal = -100f;
        private float whiteVal = 100f;
        private float yellowVal = 500f;
        private float redVal = 800f;

        private Dictionary<SKColor, List<SKPoint>> colorGroups = new Dictionary<SKColor, List<SKPoint>>();


        private SKPaint PointPaint;
        private SKPaint RectPaint;
        private SKPaint TextPaint;
        private SKFont TextFont;

        public event Action<int> OnClickSampleChanged;

        public WPWavePageControl()
        {
            InitializeComponent();
        }

        private void WPWavePageControl_Load(object sender, EventArgs e)
        {
            initControl();
        }

        private void initControl()
        {
            fillLeadComboBox(this.cmbLead);
            fillEnlargeComboBox(this.enlarge);

            if (!DesignMode)
            {
                PointPaint = new SKPaint()
                {
                    StrokeCap = SKStrokeCap.Round,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.0f,
                };
                RectPaint = new SKPaint()
                {
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1.0f,
                    Color = SKColors.Black
                };
                TextPaint = new SKPaint()
                {
                    Color = SKColors.Black,
                    Style = SKPaintStyle.StrokeAndFill,
                    IsAntialias = true
                };

                TextFont = new SKFont()
                {
                    Size = 15f,
                    Typeface = SKTypeface.FromFamilyName("微软雅黑", SKFontStyle.Bold)
                };

                _graphicsControl.Cursor = Cursors.Hand;
                _graphicsControl.MouseDown += OnCanvasMouseDown;
                panel1.Controls.Add(_graphicsControl);
            }

        }

        private void fillEnlargeComboBox(ComboBox cbx)
        {
            this.enlarge.ValueMember = "ItemValue";
            this.enlarge.DisplayMember = "ItemText";

            List<ListBoxItem> dataFormatItems = new List<ListBoxItem>
            {
                new ListBoxItem(0.5, "0.5"),
                new ListBoxItem(1, "1"),
                new ListBoxItem(2, "2"),
                new ListBoxItem(4, "4")
            };
            this.enlarge.DataSource = dataFormatItems;
            this.enlarge.SelectedValue = 1;
        }

        public void UpdateDataRefresh()
        {
            InitData();
            RefreshWave();
        }

        public void RefreshWave()
        {
            _graphicsControl?.Invalidate();
        }

        private void GetLineData()
        {
            if (Lines == 0) return;
            BeatPointCount = Convert.ToInt32(BeatDuration / 1000f * AnaValues.holterConfig.SampleRate);
            int RPosPoint = Convert.ToInt32(Rpos / 1000f * AnaValues.holterConfig.SampleRate);
            int lastCount = BeatPointCount - RPosPoint;

            LineTime = new string[Lines];
            LineDatas = new List<List<float[]>>(Lines);
            RectBeatCount = SourceBeats.Count / Lines;
            for (int i = 0; i < Lines; i++)
            {
                LineTime[i] = AnaCode.getDataTimeFromSample(SourceBeats[i * RectBeatCount].StartIndex).ToString("HH:mm:ss");
                List<float[]> oneLineDatas = new List<float[]>(RectBeatCount);
                for (int j = 0; j < RectBeatCount; j++)
                {
                    int sample = SourceBeats[j + i * RectBeatCount].Sample;
                    int startSample = sample - RPosPoint > 0 ? sample - RPosPoint : SourceBeats[j + i * RectBeatCount].StartIndex;
                    int endSample = sample + lastCount;
                    List<WaveInfo> EcgData = AnaCode.ReadAllWaveData(LeadIdx, startSample, endSample);
                    List<float[]> waveData = EcgData.Select(o => o.WaveDataF).ToList();
                    float[] tempWaveData = waveData[LeadIdx[0]];
                    float[] leadWaveData = new float[tempWaveData.Length];
                    for (int idx = 0; idx < tempWaveData.Length; idx++)
                    {
                        leadWaveData[idx] = tempWaveData[idx] * 1000;
                    }
                    oneLineDatas.Add(leadWaveData);
                }

                LineDatas.Add(oneLineDatas);
            }
        }

        private void GetPoints()
        {
            if (Lines == 0) return;

            colorGroups.Clear();
            int canvasWidth = this.panel1.Width;
            int canvasHeight = this.panel1.Height;
            int width = canvasWidth - 2;
            float height = (float)(canvasHeight - Lines - 1) / Lines;

            AllPointCount = SourceBeats.Count * BeatPointCount;
            // 按颜色分组
            SKPoint[] allPoints = new SKPoint[AllPointCount];
            for (int line = 0; line < LineDatas.Count; line++)
            {
                List<float[]> oneLineData = LineDatas[line];

                float preWidth = (float)width / oneLineData.Count;
                float preHeight = (float)height / BeatPointCount;
                float baseY = height * line;

                for (int idx = 0; idx < oneLineData.Count; idx++)
                {
                    float[] values = oneLineData[idx];
                    for (int i = 0; i < values.Length; i++)
                    {
                        float x = preWidth * idx;
                        float y = baseY + preHeight * i;

                        allPoints[i + idx * values.Length + Lines * oneLineData.Count] = new SKPoint(x, y);
                        SKColor color = GetColorByValue(values[i] * Scale);

                        if (!colorGroups.ContainsKey(color)) colorGroups[color] = new List<SKPoint>();
                        colorGroups[color].Add(allPoints[i + idx * values.Length + Lines * oneLineData.Count]);
                    }
                }
            }
        }

        private void InitData()
        {
            GetLineData();
            GetPoints();
        }

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            canvas.Clear(SKColors.White);

            int width = imageWidth - 2;
            float height = (float)(imageHeight - Lines - 1) / Lines;

            foreach (var group in colorGroups)
            {
                PointPaint.Color = group.Key;
                canvas.DrawPoints(SKPointMode.Points, group.Value.ToArray(), PointPaint);
            }

            for (int line = 0; line < LineDatas.Count; line++)
            {
                float baseY = height * line;
                canvas.DrawRect(0, baseY, width - 1, height - 1, RectPaint);
                canvas.DrawText(LineTime[line], 12, baseY + 20, SKTextAlign.Left, TextFont, TextPaint);
            }
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int mouseX = e.X - 1;
                int mouseY = e.Y;

                int canvasWidth = this.panel1.Width;
                int canvasHeight = this.panel1.Height;
                int width = canvasWidth - 2;
                float height = (float)(canvasHeight - Lines - 1) / Lines;

                int xIndex = Convert.ToInt32((float)mouseX / width * RectBeatCount);
                int yIndex = (int)(mouseY / height);

                int sample = SourceBeats[RectBeatCount * yIndex + xIndex].Sample;
                OnClickSampleChanged?.Invoke(sample);
                //Console.WriteLine("----click sample: " + sample);
            }
        }

        /// <summary>
        /// 根据数值获取对应的颜色
        /// </summary>
        /// <param name="value">输入数值</param>
        /// <returns>对应的 SKColor 颜色</returns>
        private SKColor GetColorByValue(float value)
        {
            float virtualValue = value;
            //float virtualValue = getDiscretizedValue(value);

            // 区间1：value > 1000 → 红色
            if (virtualValue > redVal)
            {
                return SKColors.Red.WithAlpha(200);
            }

            // 区间2：500 <= value <= 1000 → 黄色到红色渐变
            if (virtualValue >= yellowVal)
            {
                float t = (virtualValue - yellowVal) / (redVal - yellowVal); // 0~1
                return InterpolateColor(SKColors.Yellow, SKColors.Red, t);
            }

            // 区间3：100 <= value < 500 → 白色到黄色渐变
            if (virtualValue >= whiteVal)
            {
                float t = (virtualValue - whiteVal) / (yellowVal - whiteVal); // 0~1
                return InterpolateColor(SKColors.White, SKColors.Yellow, t);
            }

            // 区间4：-100 <= value < 100 → 白色到蓝色渐变
            if (virtualValue >= blueVal)
            {
                float t = (virtualValue - blueVal) / (whiteVal - blueVal); // 0~1
                return InterpolateColor(SKColors.Blue, SKColors.White, t);
            }

            // 区间5：value < -100 → 蓝色
            return SKColors.Blue.WithAlpha(200);
        }

        /// <summary>
        /// 在两个颜色之间进行线性插值
        /// </summary>
        private SKColor InterpolateColor(SKColor color1, SKColor color2, float t)
        {
            t = Math.Max(0, Math.Min(1, t));

            byte r = (byte)(color1.Red + (color2.Red - color1.Red) * t);
            byte g = (byte)(color1.Green + (color2.Green - color1.Green) * t);
            byte b = (byte)(color1.Blue + (color2.Blue - color1.Blue) * t);

            return new SKColor(r, g, b, 200);
        }

        /// <summary>
        /// 将不同颜色的相关值数量限制在50以内
        /// </summary>
        /// <returns></returns>
        private float getDiscretizedValue(float value)
        {
            if (value > redVal || value < blueVal) return value;

            const int TotalColors = 50;

            // 定义数值范围边界
            float minVal = blueVal;   // 最低值（蓝色端）
            float maxVal = redVal;    // 最高值（红色端）

            // 将数值限制在范围内
            value = Math.Max(minVal, Math.Min(maxVal, value));

            // 归一化到 0~1
            float normalized = (value - minVal) / (maxVal - minVal);

            // 离散化到 0 ~ (TotalColors-1) 的索引
            int index = (int)(normalized * (TotalColors - 1) + 0.5f);
            index = Math.Max(0, Math.Min(TotalColors - 1, index));

            // 将索引映射回对应的数值，再走原逻辑
            float discretizedValue = minVal + (maxVal - minVal) * index / (TotalColors - 1);
            return discretizedValue;
        }

        public void SetBeatSource(DateTime startTime, DateTime endTime, List<BeatInfo> beats, bool fulltime = false, bool keepSelection = false)
        {
            SourceBeats = beats;

            LeadIdx.Clear();
            LeadIdx.Add(this.cmbLead.SelectedIndex);
            BeatDuration = Convert.ToInt32(this.textDuration.Text);
            Rpos = Convert.ToInt32(this.textRPos.Text);
            Lines = Convert.ToInt32(this.textLines.Text);
            Scale = Convert.ToSingle(this.enlarge.SelectedValue);
            UpdateDataRefresh();
        }

        private void cmbLead_SelectedIndexChanged(object sender, EventArgs e)
        {
            LeadIdx.Clear();
            LeadIdx.Add(this.cmbLead.SelectedIndex);
            UpdateDataRefresh();
        }

        private void textDuration_Validated(object sender, EventArgs e)
        {
            BeatDuration = Convert.ToInt32(this.textDuration.Text);
            UpdateDataRefresh();
        }

        private void textRPos_Validated(object sender, EventArgs e)
        {
            Rpos = Convert.ToInt32(this.textRPos.Text);
            UpdateDataRefresh();

        }

        private void textLines_Validated(object sender, EventArgs e)
        {
            Lines = Convert.ToInt32(this.textLines.Text);
            UpdateDataRefresh();
        }

        private void enlarge_SelectedIndexChanged(object sender, EventArgs e)
        {
            Scale = Convert.ToSingle(this.enlarge.SelectedValue);
            UpdateDataRefresh();
        }

        private void textDuration_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BeatDuration = Convert.ToInt32(this.textDuration.Text);
                UpdateDataRefresh();
            }
        }

        private void textRPos_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Rpos = Convert.ToInt32(this.textRPos.Text);
                UpdateDataRefresh();
            }
        }

        private void textLines_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Lines = Convert.ToInt32(this.textLines.Text);
                UpdateDataRefresh();
            }
        }

        private void WPWavePageControl_SizeChanged(object sender, EventArgs e)
        {
            GetPoints();
        }

        private void btnAPPly_Click(object sender, EventArgs e)
        {
            LeadIdx.Clear();
            LeadIdx.Add(this.cmbLead.SelectedIndex);
            BeatDuration = Convert.ToInt32(this.textDuration.Text);
            Rpos = Convert.ToInt32(this.textRPos.Text);
            Lines = Convert.ToInt32(this.textLines.Text);
            Scale = Convert.ToSingle(this.enlarge.SelectedValue);
            UpdateDataRefresh();
        }
    }
}
