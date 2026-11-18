using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp;
using AnalysisApp.Commons;

namespace AnalysisApp.CommonControls
{
    public partial class RRDiscreteWaveControl : BaseDrawingControl
    {
        private SKPaint LinePaint;
        private SKPaint RectPaint;
        private SKPaint TextPaint;
        private SKFont TextFont;

        public int Lines = 6;
        private int RectBeatCount;
        private List<SKPath> LinePaths;
        private string[] LineTime;

        private int CoorWidth = 0;
        private float CoorHeight = 0;

        public List<BeatInfo> SourceBeats = new List<BeatInfo>();

        public event Action<int> OnClickSampleChanged;

        public RRDiscreteWaveControl()
        {
            InitializeComponent();
        }

        private void RRDiscreteWaveControl_Load(object sender, EventArgs e)
        {
            initControl();
        }

        private void initControl()
        {
            if (!DesignMode)
            {
                LinePaint = new SKPaint()
                {
                    Color = new SKColor(0, 0, 0, 255),
                    StrokeWidth = 1.0f,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
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

        protected override void OnPaintSurface(SKCanvas canvas, SKImageInfo imageInfo, int imageWidth, int imageHeight)
        {
            canvas.Clear(SKColors.White);

            int width = imageWidth - 2;
            float height = (float)(imageHeight - Lines - 1) / Lines;

            //foreach (var group in colorGroups)
            //{
            //    PointPaint.Color = group.Key;
            //    canvas.DrawPoints(SKPointMode.Points, group.Value.ToArray(), PointPaint);
            //}

            for (int line = 0; line < LinePaths.Count; line++)
            {
                float baseY = height * line;
                canvas.DrawRect(0, baseY, width - 1, height - 1, RectPaint);
                canvas.DrawText(LineTime[line], 12, baseY + 20, SKTextAlign.Left, TextFont, TextPaint);
                if(LinePaths[line] != null)
                {
                    canvas.DrawPath(LinePaths[line], LinePaint);
                }
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
                int CoorWidth = canvasWidth - 2;
                float CoorHeight = (float)(canvasHeight - Lines - 1) / Lines;

                int xIndex = Convert.ToInt32((float)mouseX / CoorWidth * RectBeatCount);
                int yIndex = (int)(mouseY / CoorHeight);

                int sample = SourceBeats[RectBeatCount * yIndex + xIndex].Sample;
                OnClickSampleChanged?.Invoke(sample);
                //Console.WriteLine("----click sample: " + sample);
            }
        }

        public void SetBeatSource(DateTime startTime, DateTime endTime, List<BeatInfo> beats, bool fulltime = false, bool keepSelection = false)
        {
            SourceBeats = beats;
            UpdateDataRefresh();
        }

        private void InitData()
        {

            RectBeatCount = SourceBeats.Count / Lines;

            int canvasWidth = this.panel1.Width;
            int canvasHeight = this.panel1.Height;
            int CoorWidth = canvasWidth - 2;
            float CoorHeight = (float)(canvasHeight - Lines - 1) / Lines;

            float preWidth = CoorWidth * 1.0f / RectBeatCount;
            float preHeight = CoorHeight / 2000;

            LineTime = new string[Lines];
            LinePaths = new List<SKPath>(Lines);
            for (int i = 0; i < Lines; i++)
            {

                SKPath path = new SKPath();
                float baseY = CoorHeight * i;
                LineTime[i] = AnaCode.getDataTimeFromSample(SourceBeats[i * RectBeatCount].StartIndex).ToString("HH:mm:ss");
                for (int j = 1; j < RectBeatCount; j++)
                {
                    int RR = SourceBeats[j + i * RectBeatCount].RRInterval;
                    int lastRR = SourceBeats[j - 1 + i * RectBeatCount].RRInterval;
                    int RRDeviation = lastRR - RR;
                    float x = 1 + j * preWidth;
                    float y = baseY + CoorHeight/2 + RRDeviation * preHeight;
                    if (j == 1)
                    {
                        path.MoveTo(x, y);
                    }
                    else
                    {
                        path.LineTo(x, y);
                    }
                }
                LinePaths.Add(path);
            }

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
    }
}
