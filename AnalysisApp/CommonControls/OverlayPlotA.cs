using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnalysisApp.Commons;

namespace AnalysisApp.CommonControls
{
    public partial class OverlayPlotA : UserControl
    {
        public OverlayPlotA()
        {
            InitializeComponent();
        }

        private void OverlayPlot_Load(object sender, EventArgs e)
        {
            this.panel2.BackColor = Color.FromArgb(64, 13, 19, 92);
        }

        public void OnBeatSourceChanged(List<BeatInfo> source, int overlayIndex = 0)
        {

        }

    }
}
