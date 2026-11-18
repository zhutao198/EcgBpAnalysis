using System.Threading.Tasks;
using Zoncare.Common;

namespace AnalysisSystem.Base
{
    public class EcgDataProcess
    {
        public static async Task<WorkProgress> FileConvert(string targetPath)
        {
            WorkProgress result = new WorkProgress();
            await Task.Delay(1);
            result.code = "200";
            return result;
        }


    }
}
