namespace AnalysisApp.Commons
{
    public interface IScatterToolStripItem 
    {
        ScatterType ZqScatterType { get; set; }
        int TitleFontSize { get; set; }
        bool UserSelected { get; set; }
    }
}
