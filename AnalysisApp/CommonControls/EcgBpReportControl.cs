namespace AnalysisApp.CommonControls
{
    public partial class EcgBpReportControl : BaseUserControl
    {
        public EcgBpReportControl()
        {
            InitializeComponent();
        }

        public void RefreshUI()
        {
            this.ecgReportControl1.RefreshUI();
            this.bpReportControl1.RefreshUI();
        }
    }
}
