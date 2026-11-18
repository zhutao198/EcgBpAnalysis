using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Utilities;

namespace EcgAnalyseStart
{
    public class ExcelHelper
    {
        private static List<string> lstColumn = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private static HSSFWorkbook _workBook;

        /// <summary>
        /// 使用NPOI进行导出
        /// </summary>
        /// <param name="strCaption">Excel文件中的标题</param>
        /// <param name="lstColumnName">每一列的名称：需要与数据表的列数相同</param>
        /// <param name="myDt">数据表</param>
        /// <param name="excelSavePath">保存路径</param>
        /// <returns>true:成功/false:失败</returns>
        public static bool NpoiToEcel(string strCaption, List<ExportItem> lstColumnName, DataGridView myDGV, string excelSavePath)
        {
            bool result = false;
            try
            {
                //创建HSSFWorkbook对象
                HSSFWorkbook wb = new HSSFWorkbook();
                //创建sheet　并指定sheet的名字
                ISheet sheet1 = wb.CreateSheet(strCaption);


                IRow row0 = sheet1.CreateRow(0);

                ICellStyle style0 = wb.CreateCellStyle();//样式
                style0.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                IFont font0 = wb.CreateFont();//字体
                font0.FontName = "微软雅黑";
                font0.FontHeightInPoints = 20;
                font0.Boldweight = (short)FontBoldWeight.Bold;
                style0.SetFont(font0);//样式里的字体设置具体的字体样式

                //合并单元格
                /**
                  第一个参数：从第几行开始合并
                  第二个参数：到第几行结束合并
                  第三个参数：从第几列开始合并
                  第四个参数：到第几列结束合并
              **/
                CellRangeAddress region = new CellRangeAddress(0, 0, 0, lstColumnName.Count - 1);
                row0.CreateCell(0).SetCellValue(strCaption);
                row0.GetCell(0).CellStyle = style0;
                sheet1.AddMergedRegion(region);

                IRow row1 = sheet1.CreateRow(1);

                //新建style对象并设置样式属性
                ICellStyle style1 = wb.CreateCellStyle();//样式
                style1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                IFont font1 = wb.CreateFont();//字体
                font1.FontName = "微软雅黑";
                font1.FontHeightInPoints = 14;
                font1.Boldweight = (short)FontBoldWeight.Bold;
                style1.SetFont(font1);//样式里的字体设置具体的字体样式

                for (int i = 0; i < lstColumnName.Count; i++)
                {
                    row1.CreateCell(i).SetCellValue(lstColumnName[i].HeaderText);
                    row1.GetCell(i).CellStyle = style1;
                    sheet1.SetColumnWidth(i, 256 * lstColumnName[i].HeaderText.Length * 5);
                }

                ICellStyle style2 = wb.CreateCellStyle();//样式
                style2.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                IFont font2 = wb.CreateFont();//字体
                font2.FontName = "微软雅黑";
                font2.FontHeightInPoints = 10;
                font2.Boldweight = (short)FontBoldWeight.Normal;
                style2.SetFont(font2);//样式里的字体设置具体的字体样式

                int index = 0;
                foreach (DataGridViewRow dr in myDGV.Rows)
                {
                    //获取行中某个字段（列）的数据
                    int num = 0;
                    IRow rowi = sheet1.CreateRow(index + 2);

                    foreach (ExportItem item in lstColumnName)
                    {
                        string name = item.ColName;
                        if (null != dr.Cells[name])
                        {
                            rowi.CreateCell(num).SetCellValue(dr.Cells[name].Value.ToString());
                        }
                        else
                        {
                            rowi.CreateCell(num).SetCellValue("");
                        }
                        rowi.GetCell(num).CellStyle = style2;
                        num++;
                    }
                    index++;
                }
                //将内存中的数据写入磁盘
                using (FileStream filestream = new FileStream(excelSavePath, FileMode.Create))
                {
                    wb.Write(filestream);
                    filestream.Close();
                }
                result = true;

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
            return result;
        }

        /// <summary>
        /// 使用NPOI进行导出
        /// </summary>
        /// <param name="strCaption">Excel文件中的标题</param>
        /// <param name="lstColumnName">每一列的名称：需要与数据表的列数相同</param>
        /// <param name="myDt">数据表</param>
        /// <param name="excelSavePath">保存路径</param>
        /// <returns>true:成功/false:失败</returns>
        public static bool NpoiToEcel(string strCaption, string[] lstColumnName, List<string[]> myDt, string excelSavePath)
        {
            bool result = false;
            try
            {
                //创建HSSFWorkbook对象
                HSSFWorkbook wb = new HSSFWorkbook();
                //创建sheet　并指定sheet的名字
                ISheet sheet1 = wb.CreateSheet(strCaption);


                IRow row0 = sheet1.CreateRow(0);

                ICellStyle style0 = wb.CreateCellStyle();//样式
                style0.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                IFont font0 = wb.CreateFont();//字体
                font0.FontName = "微软雅黑";
                font0.FontHeightInPoints = 20;
                font0.Boldweight = (short)FontBoldWeight.Bold;
                style0.SetFont(font0);//样式里的字体设置具体的字体样式

                //合并单元格
                /**
                  第一个参数：从第几行开始合并
                  第二个参数：到第几行结束合并
                  第三个参数：从第几列开始合并
                  第四个参数：到第几列结束合并
              **/
                CellRangeAddress region = new CellRangeAddress(0, 0, 0, lstColumnName.Length - 1);
                row0.CreateCell(0).SetCellValue(strCaption);
                row0.GetCell(0).CellStyle = style0;
                sheet1.AddMergedRegion(region);

                IRow row1 = sheet1.CreateRow(1);

                //新建style对象并设置样式属性
                ICellStyle style1 = wb.CreateCellStyle();//样式
                style1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                IFont font1 = wb.CreateFont();//字体
                font1.FontName = "微软雅黑";
                font1.FontHeightInPoints = 14;
                font1.Boldweight = (short)FontBoldWeight.Bold;
                style1.SetFont(font1);//样式里的字体设置具体的字体样式

                for (int i = 0; i < lstColumnName.Length; i++)
                {
                    row1.CreateCell(i).SetCellValue(lstColumnName[i]);
                    row1.GetCell(i).CellStyle = style1;
                    sheet1.SetColumnWidth(i, 256 * lstColumnName[i].Length * 5);
                }

                ICellStyle style2 = wb.CreateCellStyle();//样式
                style2.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                IFont font2 = wb.CreateFont();//字体
                font2.FontName = "微软雅黑";
                font2.FontHeightInPoints = 10;
                font2.Boldweight = (short)FontBoldWeight.Normal;
                style2.SetFont(font2);//样式里的字体设置具体的字体样式

                for (int r = 0; r < myDt.Count; r++)
                {
                    string[] arrRow = myDt[r];
                    IRow rowi = sheet1.CreateRow(r + 2);

                    for (int i = 0; i < lstColumnName.Length; i++)
                    {
                        rowi.CreateCell(i).SetCellValue(arrRow[i] ?? "");
                        rowi.GetCell(i).CellStyle = style2;
                    }
                }

                //将内存中的数据写入磁盘
                using (FileStream filestream = new FileStream(excelSavePath, FileMode.Create))
                {
                    wb.Write(filestream);
                    filestream.Close();
                }
                result = true;

            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "通用底层库");
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strCaption"></param>
        /// <param name="myDGV"></param>
        /// <param name="excelSavePath"></param>
        /// <returns></returns>
        public static int ExportToExcel(string strCaption, DataGridView myDGV, string excelSavePath)
        {
            int result = 0;
            try
            {
                // 列索引，行索引，总列数，总行数
                int ColIndex = 0;
                int RowIndex = 0;
                int ColCount = 0;
                int RowCount = myDGV.RowCount;
                foreach (DataGridViewColumn col in myDGV.Columns)
                {
                    if (col.Visible == true) ColCount++;
                }

                // 创建Excel对象
                XSSFWorkbook workbook = new XSSFWorkbook();
                XSSFSheet sheet = (XSSFSheet)workbook.CreateSheet(strCaption);

                #region 鼠标右键点击文件 属性信息
                {
                    DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
                    dsi.Company = "zoncare";
                    // workbook.DocumentSummaryInformation = dsi;

                    SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
                    si.Author = "文件作者信息"; //填加xls文件作者信息
                    si.ApplicationName = "创建程序信息"; //填加xls文件创建程序信息
                    si.LastAuthor = "最后保存者信息"; //填加xls文件最后保存者信息
                    si.Comments = "作者信息"; //填加xls文件作者信息
                    si.Title = "标题信息"; //填加xls文件标题信息
                    si.Subject = "主题信息";//填加文件主题信息
                    si.CreateDateTime = System.DateTime.Now;
                    // workbook.SummaryInformation = si;
                }
                #endregion

                XSSFCellStyle dateStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                XSSFDataFormat format = (XSSFDataFormat)workbook.CreateDataFormat();
                dateStyle.DataFormat = format.GetFormat("yyyy-MM-dd hh:mm:ss");

                //获取列标题
                XSSFRow headerRow = (XSSFRow)sheet.CreateRow(0);
                XSSFCellStyle headStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                //headStyle.Alignment = CellHorizontalAlignment.CENTER;
                XSSFFont font = (XSSFFont)workbook.CreateFont();
                font.FontHeightInPoints = 10;
                font.Boldweight = 700;
                headStyle.SetFont(font);
                foreach (DataGridViewColumn col in myDGV.Columns)
                {
                    if (!col.Visible) continue;
                    headerRow.CreateCell(ColIndex).SetCellValue(col.HeaderText);
                    headerRow.GetCell(ColIndex).CellStyle = headStyle;

                    //设置列宽
                    //sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256);
                    ColIndex++;
                }
                //填充数据
                for (RowIndex = 0; RowIndex < RowCount; RowIndex++)
                {
                    XSSFRow dataRow = (XSSFRow)sheet.CreateRow(RowIndex + 1);
                    ColIndex = 0;
                    foreach (DataGridViewColumn col in myDGV.Columns)
                    {
                        if (col.Visible)
                        {
                            XSSFCell newCell = (XSSFCell)dataRow.CreateCell(ColIndex);
                            Type dataType = PubFuns.GetBasicType(myDGV[ColIndex, RowIndex].ValueType);

                            string drValue = myDGV[ColIndex, RowIndex].Value?.ToString();
                            if (drValue == null)
                            {
                                newCell.SetCellValue("");
                                continue;
                            }

                            switch (dataType.ToString())
                            {
                                case "System.String"://字符串类型
                                    newCell.SetCellValue(drValue);
                                    break;
                                case "System.DateTime"://日期类型
                                    DateTime dateV;
                                    DateTime.TryParse(drValue, out dateV);
                                    newCell.SetCellValue(dateV);
                                    newCell.CellStyle = dateStyle;//格式化显示
                                    break;
                                case "System.Boolean"://布尔型
                                    bool boolV = false;
                                    bool.TryParse(drValue, out boolV);
                                    newCell.SetCellValue(boolV);
                                    break;
                                case "System.Int16"://整型
                                case "System.Int32":
                                case "System.Int64":
                                case "System.Byte":
                                    int intV = 0;
                                    int.TryParse(drValue, out intV);
                                    newCell.SetCellValue(intV);
                                    break;
                                case "System.Decimal"://浮点型
                                case "System.Double":
                                    double doubV = 0;
                                    double.TryParse(drValue, out doubV);
                                    newCell.SetCellValue(doubV);
                                    break;
                                case "System.DBNull"://空值处理
                                    newCell.SetCellValue("");
                                    break;
                                default:
                                    newCell.SetCellValue("");
                                    break;
                            }
                        }
                        ColIndex++;
                    }
                }

                using (FileStream fs = new FileStream(excelSavePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fs);
                    //byte[] data = ms.ToArray();
                    //fs.Write(data, 0, data.Length);
                    //fs.Flush();
                }
            }
            catch (Exception ex)
            {
                result = 1;
                Logger.CreateLog(ex, "通用底层库");
            }
            return result;
        }

        /// <summary>
        /// DataSet导出到Excel的MemoryStream
        /// </summary>
        /// <param name="dtSource">源DataSet</param>
        public static MemoryStream DataSetToExcel(DataSet ds)
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            for (int k = 0; k < ds.Tables.Count; k++)
            {
                XSSFSheet sheet = (XSSFSheet)workbook.CreateSheet(ds.Tables[k].TableName.ToString());

                #region 右击文件 属性信息
                {
                    DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
                    dsi.Company = "NPOI";

                    // workbook.DocumentSummaryInformation = dsi;

                    SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
                    si.Author = "文件作者信息"; //填加xls文件作者信息
                    si.ApplicationName = "创建程序信息"; //填加xls文件创建程序信息
                    si.LastAuthor = "最后保存者信息"; //填加xls文件最后保存者信息
                    si.Comments = "作者信息"; //填加xls文件作者信息
                    si.Title = "标题信息"; //填加xls文件标题信息
                    si.Subject = "主题信息";//填加文件主题信息
                    si.CreateDateTime = System.DateTime.Now;
                    // workbook.SummaryInformation = si;
                }
                #endregion

                XSSFCellStyle dateStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                XSSFDataFormat format = (XSSFDataFormat)workbook.CreateDataFormat();
                dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");

                //取得列宽
                /*
                int[] arrColWidth = new int[dtSource.Columns.Count];
                foreach (DataColumn item in dtSource.Columns)
                {
                    arrColWidth[item.Ordinal] = Encoding.GetEncoding(936).GetBytes(item.ColumnName.ToString()).Length;
                }
                for (int i = 0; i < dtSource.Rows.Count; i++)
                {
                    for (int j = 0; j < dtSource.Columns.Count; j++)
                    {
                        int intTemp = Encoding.GetEncoding(936).GetBytes(dtSource.Rows[i][j].ToString()).Length;
                        if (intTemp > arrColWidth[j])
                        {
                            arrColWidth[j] = intTemp;
                        }
                    }
                }
                 */
                int rowIndex = 0;
                foreach (DataRow row in ds.Tables[k].Rows)
                {
                    #region 新建表，填充表头，填充列头，样式
                    if (rowIndex == 0)
                    {
                        //if (rowIndex != 0)
                        //{
                        //    sheet = (XSSFSheet)workbook.CreateSheet();
                        //}

                        #region 表头及样式
                        {
                            //XSSFRow headerRow = (XSSFRow)sheet.CreateRow(0);
                            //headerRow.HeightInPoints = 25;
                            //headerRow.CreateCell(0).SetCellValue(strHeaderText);

                            //HSSFCellStyle headStyle = (HSSFCellStyle)workbook.CreateCellStyle();
                            ////  headStyle.Alignment = CellHorizontalAlignment.CENTER;
                            //HSSFFont font = (HSSFFont)workbook.CreateFont();
                            //font.FontHeightInPoints = 20;
                            //font.Boldweight = 700;
                            //headStyle.SetFont(font);
                            //headerRow.GetCell(0).CellStyle = headStyle;
                            //// sheet.AddMergedRegion(new Region(0, 0, 0, dtSource.Columns.Count - 1));
                            ////headerRow.Dispose();
                        }
                        #endregion

                        #region 列表头及样式
                        {
                            XSSFRow headerRow = (XSSFRow)sheet.CreateRow(0);
                            XSSFCellStyle headStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                            //headStyle.Alignment = CellHorizontalAlignment.CENTER;
                            XSSFFont font = (XSSFFont)workbook.CreateFont();
                            font.FontHeightInPoints = 10;
                            font.Boldweight = 700;
                            headStyle.SetFont(font);
                            foreach (DataColumn column in ds.Tables[k].Columns)
                            {
                                headerRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
                                headerRow.GetCell(column.Ordinal).CellStyle = headStyle;

                                //设置列宽
                                //sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256);
                            }
                            // headerRow.Dispose();
                        }
                        #endregion

                        rowIndex = 1;
                    }
                    #endregion

                    #region 填充内容
                    XSSFRow dataRow = (XSSFRow)sheet.CreateRow(rowIndex);
                    foreach (DataColumn column in ds.Tables[k].Columns)
                    {
                        XSSFCell newCell = (XSSFCell)dataRow.CreateCell(column.Ordinal);

                        string drValue = row[column].ToString();

                        switch (column.DataType.ToString())
                        {
                            case "System.String"://字符串类型
                                newCell.SetCellValue(drValue);
                                break;
                            case "System.DateTime"://日期类型
                                System.DateTime dateV;
                                System.DateTime.TryParse(drValue, out dateV);
                                newCell.SetCellValue(dateV);
                                newCell.CellStyle = dateStyle;//格式化显示
                                break;
                            case "System.Boolean"://布尔型
                                bool boolV = false;
                                bool.TryParse(drValue, out boolV);
                                newCell.SetCellValue(boolV);
                                break;
                            case "System.Int16"://整型
                            case "System.Int32":
                            case "System.Int64":
                            case "System.Byte":
                                int intV = 0;
                                int.TryParse(drValue, out intV);
                                newCell.SetCellValue(intV);
                                break;
                            case "System.Decimal"://浮点型
                            case "System.Double":
                                double doubV = 0;
                                double.TryParse(drValue, out doubV);
                                newCell.SetCellValue(doubV);
                                break;
                            case "System.DBNull"://空值处理
                                newCell.SetCellValue("");
                                break;
                            default:
                                newCell.SetCellValue("");
                                break;
                        }

                    }
                    #endregion

                    rowIndex++;
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                ms.Flush();
                return ms;
            }
        }

        /// <summary>
        /// 在工作簿中生成新的工作表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <param name="cols"></param>
        /// <param name="myDt"></param>
        public static void GenerateMultipleSheets<T>(string title, List<ExportItem> cols, List<T> myDt, int[] autosizeCols = null)
        {
            if (_workBook == null) { _workBook = new HSSFWorkbook(); }
            if (_workBook.GetSheet(title) != null) return;
            ISheet sheet = _workBook.CreateSheet(title);

            IRow row0 = sheet.CreateRow(0);

            ICellStyle style0 = _workBook.CreateCellStyle();//标题样式
            style0.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            IFont font0 = _workBook.CreateFont();//字体
            font0.FontName = "微软雅黑";
            font0.FontHeightInPoints = 20;
            font0.IsBold = true;
            style0.SetFont(font0);//样式里的字体设置具体的字体样式

            CellRangeAddress region = new CellRangeAddress(0, 0, 0, cols.Count - 1);
            row0.CreateCell(0).SetCellValue(title);
            row0.GetCell(0).CellStyle = style0;
            sheet.AddMergedRegion(region);

            IRow row1 = sheet.CreateRow(1);

            //新建style对象并设置样式属性
            ICellStyle style1 = _workBook.CreateCellStyle();//列表头样式
            style1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            IFont font1 = _workBook.CreateFont();//字体
            font1.FontName = "微软雅黑";
            font1.FontHeightInPoints = 14;
            font1.IsBold = true;
            style1.SetFont(font1);//样式里的字体设置具体的字体样式

            for (int i = 0; i < cols.Count; i++)
            {
                row1.CreateCell(i).SetCellValue(cols[i].HeaderText);
                row1.GetCell(i).CellStyle = style1;
                sheet.SetColumnWidth(i, 256 * cols[i].HeaderText.Length * 5);
            }

            ICellStyle style2 = _workBook.CreateCellStyle();//列表行样式
            style2.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            IFont font2 = _workBook.CreateFont();//字体
            font2.FontName = "微软雅黑";
            font2.FontHeightInPoints = 10;
            font2.IsBold = false;
            style2.SetFont(font2);//样式里的字体设置具体的字体样式

            foreach (T dataObj in myDt)
            {
                int item_index = myDt.IndexOf(dataObj);
                if (null == dataObj) continue;

                PropertyInfo[] lstPropertyInfo = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                if (null != lstPropertyInfo && lstPropertyInfo.Length > 0)
                {
                    IRow rowI = sheet.CreateRow(item_index + 2);
                    for (int i = 0; i < cols.Count; i++)
                    {
                        PropertyInfo prop = lstPropertyInfo.FirstOrDefault(o => o.Name == cols[i].ColName);
                        if (prop != null)
                        {
                            object val = prop.GetValue(dataObj, null);
                            if (null == val)
                                rowI.CreateCell(i).SetCellValue("");
                            else
                                rowI.CreateCell(i).SetCellValue(val.ToString());

                            rowI.GetCell(i).CellStyle = style2;
                        }
                    }
                }
            }
            if (autosizeCols != null)
            {
                foreach (int col in autosizeCols)
                {
                    sheet.AutoSizeColumn(col);
                }
            }
        }

        /// <summary>
        /// 重置工作簿
        /// </summary>
        public static void Reset()
        {
            _workBook = null;
        }

        /// <summary>
        /// 保存工作簿到指定存储路径
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool SaveMultipleSheetsToExcel(string filepath)
        {
            bool result = false;
            if (_workBook != null)
            {
                try
                {
                    using (FileStream filestream = new FileStream(filepath, FileMode.Create))
                    {
                        _workBook.Write(filestream);
                        filestream.Close();
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "通用底层库");
                }
            }
            _workBook = null;
            return result;
        }
    }

    /// <summary>
    /// 用于导出列数据的名称映射
    /// </summary>
    public class ExportItem
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string ColName { get; set; }
        /// <summary>
        /// 表头名称
        /// </summary>
        public string HeaderText { get; set; }
    }
}
