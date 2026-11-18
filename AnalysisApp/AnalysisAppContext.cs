using System.Windows.Forms;

namespace AnalysisApp
{
    class AnalysisAppContext : ApplicationContext
    {
        private Form mainFormInternal;

        public AnalysisAppContext(Form mainForm)
        {
            mainFormInternal = mainForm;
            
            mainFormInternal.FormClosed += mainFormInternal_FormClosed;
        }

        private void mainFormInternal_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
