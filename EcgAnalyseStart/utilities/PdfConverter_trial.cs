using System;
using System.Collections.Concurrent;
using System.Drawing.Printing;
using System.IO;
using System.Web.Hosting;
using TuesPechkin;

namespace EcgAnalyseStart
{
    public class PdfConverter
    {
        /// <summary>
        /// 将网页转为pdf文件
        /// </summary>
        /// <param name="filePath">文件路径/目录</param>
        /// <param name="fileName">文件名</param>
        /// <param name="url">网页url</param>
        public static bool ConvertWebpageToPDF(string htmlString, string fileName, string url)
        {
            try
            {
                HtmlToPdfDocument htmlToPdfDocument = new HtmlToPdfDocument();
                GlobalSettings globalSettings = new GlobalSettings();
                globalSettings.ProduceOutline = new bool?(true);

                globalSettings.PaperSize = PaperKind.A4;

                //设置pdf文档四周空白边距
                globalSettings.Margins.All = 0;
                globalSettings.Margins.Unit = Unit.Centimeters;
                string pageUrl = url;
                ObjectSettings objectSettings = new ObjectSettings();

                double value = 1.5;
                objectSettings.HtmlText = htmlString;
                //objectSettings.PageUrl = pageUrl;
                objectSettings.LoadSettings.BlockLocalFileAccess = true;
                objectSettings.LoadSettings.ZoomFactor = new double?(value);
                objectSettings.WebSettings.PrintMediaType = new bool?(true);
                objectSettings.WebSettings.PrintBackground = new bool?(false);
                objectSettings.WebSettings.DefaultEncoding = "utf-8";
                objectSettings.WebSettings.LoadImages = true;
                htmlToPdfDocument.GlobalSettings = globalSettings;
                htmlToPdfDocument.Objects.Add(objectSettings);

                IConverter converter = GetConverter(); //new StandardConverter(new PdfToolset(new TempFolderDeployment()));
                byte[] dataBytes = converter.Convert(htmlToPdfDocument);


                using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                {
                    fileStream.Write(dataBytes, 0, dataBytes.Length);
                    fileStream.Flush();
                    fileStream.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        ////Select.HtmlToPdf (宽度/高度不协调)
        //[Obsolete]
        //public static bool ConvertHtmlToPdf(string htmlString, string pdfPath, string baseUrl = "")
        //{
        //    bool result = false;

        //    try
        //    {
        //        var pageSize = SelectPdf.PdfPageSize.A4;

        //        // instantiate a html to pdf converter object
        //        HtmlToPdf converter = new HtmlToPdf();

        //        // set converter options
        //        converter.Options.PdfPageSize = pageSize;
        //        converter.Options.PdfPageOrientation = SelectPdf.PdfPageOrientation.Portrait;
        //        //converter.Options.WebPageWidth = 1024;
        //        //converter.Options.WebPageHeight = 1000;
        //        converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.AutoFit;
        //        converter.Options.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;

        //        converter.Options.MarginLeft = 10; //单位: pt
        //        converter.Options.MarginTop = 10;
        //        converter.Options.MarginRight = 10;
        //        converter.Options.MarginBottom = 10;

        //        // create a new pdf document converting an url
        //        SelectPdf.PdfDocument doc = converter.ConvertHtmlString(htmlString, baseUrl);

        //        // save pdf document
        //        doc.Save(pdfPath);

        //        // close pdf document
        //        doc.Close();

        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return result;
        //}

        ////itexsharp 5.5.13.2 (格式有问题)
        //[Obsolete]
        //public static bool ConvertHtmlToPdf_itexsharp(string htmlContent, string pdfPath)
        //{
        //    bool result = false;
        //    try
        //    {
        //        Document doc = new Document();
        //        try
        //        {
        //            byte[] data = Encoding.UTF8.GetBytes(htmlContent);
        //            using (MemoryStream msInput = new MemoryStream(data))
        //            {
        //                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(pdfPath, FileMode.Create));
        //                doc.Open();

        //                XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msInput, null, Encoding.UTF8, new UnicodeFontFactory());

        //                ////指定默认缩放比例为100%
        //                //var pdfDest = new iTextSharp.text.pdf.PdfDestination(iTextSharp.text.pdf.PdfDestination.XYZ, 0, doc.PageSize.Height, 1f);
        //                ////将默认设置写入pdf
        //                //var action = iTextSharp.text.pdf.PdfAction.GotoLocalPage(1, pdfDest, writer);
        //                //writer.SetOpenAction(action);

        //                doc.Close();
        //                writer.Close();
        //                msInput.Close();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Error during creation of PDF document: " + ex.Message);
        //        }

        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error during creation of PDF document: " + ex.Message);
        //    }

        //    return result;
        //}

        ////FreeSpire.PDF 8.6.0 格式有问题
        //[Obsolete]
        //public static bool ConvertHtmlToPdf_Spire(string htmlString, string pdfPath, string baseUrl = "")
        //{
        //    bool result = false;

        //    try
        //    {
        //        var pdf = new Spire.Pdf.PdfDocument();
        //        //HtmlConverter.conv
        //        PdfHtmlLayoutFormat htmlLayoutFormat = new PdfHtmlLayoutFormat();
        //        //webBrowser load html whether Waiting
        //        htmlLayoutFormat.IsWaiting = false;
        //        //page setting
        //        PdfPageSettings setting = new PdfPageSettings();
        //        setting.Size = Spire.Pdf.PdfPageSize.A4;

        //        Thread thread = new Thread(() =>
        //        {
        //           pdf.LoadFromHTML(htmlString, false, setting, htmlLayoutFormat);
        //        });
        //        thread.SetApartmentState(ApartmentState.STA);
        //        thread.Start();
        //        thread.Join();

        //        pdf.SaveToFile(pdfPath);

        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return result;
        //}


        /// <summary>
        /// //官方要求在程序生命周期内，只能实例化一次
        /// </summary>
        /// <returns></returns>
        public static IConverter GetConverter()
        {
            //var tempFolderDeployment = new TempFolderDeployment();
            //var winAnyCpuEmbeddedDeployment = new WinAnyCPUEmbeddedDeployment(tempFolderDeployment);
            //IToolset toolSet;
            //if (HostingEnvironment.IsHosted)
            //{
            //    toolSet = new RemotingToolset<PdfToolset>(winAnyCpuEmbeddedDeployment);
            //}
            //else
            //{
            //    toolSet = new PdfToolset(winAnyCpuEmbeddedDeployment);
            //}

            var tempFolderDeployment = new ThreadSafeTempFolderDeploymentWrapper();
            var toolSet = new PdfToolset(tempFolderDeployment);

            return new ThreadSafeConverter(toolSet);
        }


        public class ThreadSafeTempFolderDeploymentWrapper : IDeployment
        {
            private readonly ThreadSafeTempFolderDeployment _tempFolderManager;

            public ThreadSafeTempFolderDeploymentWrapper()
            {
                _tempFolderManager = new ThreadSafeTempFolderDeployment();
            }

            public string GetTemporaryFolder(string prefix = null)
            {
                return _tempFolderManager.GetTemporaryFolder(prefix);
            }

            public void DeleteTemporaryFolder(string path)
            {
                // 假设 ThreadSafeTempFolderDeployment 有一个方法来安全地删除文件夹  
                // 如果没有，你可能需要在这里添加逻辑来检查路径是否有效并安全地删除它  
                _tempFolderManager.ReleaseTemporaryFolder(path); // 假设这是删除文件夹的方法  

                // 或者，如果你只是简单地想删除文件夹（但请注意，这可能不是线程安全的）：  
                // if (Directory.Exists(path))  
                // {  
                //     Directory.Delete(path, true);  
                // }  
            }

            // 如果 IDeployment 接口有其他方法，你也需要在这里实现它们  
            // ...  

            public string Path
            {
                get
                {
                    return GetTemporaryFolder();
                }
            }
        }

        public class ThreadSafeTempFolderDeployment
        {
            private readonly ConcurrentDictionary<string, string> tempFolders = new ConcurrentDictionary<string, string>();

            public string GetTemporaryFolder(string prefix = null)
            {
                string tempPath;
                do
                {
                    tempPath = Path.Combine(Path.GetTempPath(), prefix ?? Guid.NewGuid().ToString());
                } while (!tempFolders.TryAdd(tempPath, tempPath));

                Directory.CreateDirectory(tempPath);
                return tempPath;
            }

            public void ReleaseTemporaryFolder(string tempPath)
            {
                if (tempFolders.TryRemove(tempPath, out _))
                {
                    try
                    {
                        Directory.Delete(tempPath, true);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle exception  
                    }
                }
            }
        }

        //public class UnicodeFontFactory : FontFactoryImp
        //{
        //    public override Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color, bool cached)
        //    {
        //        //使用微软雅黑字体解决中文乱码的问题，因为雅黑字体为字体集合所以需要使用,0来指定具体的字体。
        //        var chineseFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "msyh.ttc,0");
        //        //宋体
        //        //BaseFont baseFont = BaseFont.CreateFont(@"c:\Windows\Fonts\simsun.ttc,0", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
        //        //黑体
        //        //BaseFont baseFont = BaseFont.CreateFont(@"c:\Windows\Fonts\SIMHEI.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
        //        var baseFont = BaseFont.CreateFont(chineseFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        //        return new Font(baseFont, size, style, color);
        //    }
        //}
    }
}
