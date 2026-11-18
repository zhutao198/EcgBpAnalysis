using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalysisApp.CommonControls
{
    public partial class EcgBpSummaryControl : UserControl
    {
        public EcgBpSummaryControl()
        {
            InitializeComponent();
        }

        public void GetEcgBpData()
        {
            this.ecgDataSummaryControl1.initDataAsync();
            this.bpSummaryControl1.GetBPSummaryData();
        }
    }
}
