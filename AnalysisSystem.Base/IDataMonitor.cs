using System.Threading.Tasks;

namespace AnalysisSystem.Base
{
    /// <summary>
    /// 提供监测数据源变化的机制
    /// </summary>
    public interface IDataMonitor
    {
        void OnDataSourceChanged(bool reset = false);
        Task RefreshData(bool reset = false);
    }
}
