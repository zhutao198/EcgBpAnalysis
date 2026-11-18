using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;

namespace Zoncare.Utilities
{
    public class PubFuns
    {
        private static readonly long Jan1st1970ns = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        #region 数据处理相关
        public static object GetValueFromDB(object dbValue, Type type)
        {
            if (dbValue == null || dbValue == DBNull.Value) { return null; }

            if (type.IsGenericType &&
                type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (dbValue == null || dbValue.ToString().Length == 0) { return null; }

                NullableConverter nullableConverter = new NullableConverter(type);
                type = nullableConverter.UnderlyingType;
            }

            byte byteVal = 0;
            decimal decVal = 0;
            double douVal = 0;
            object val = null;

            switch (type.Name)
            {
                case "Int16":
                    val = Convert.ToInt16(dbValue);
                    break;
                case "Int32":
                    val = Convert.ToInt32(dbValue);
                    break;
                case "Int64":
                    val = Convert.ToInt64(dbValue);
                    break;
                case "Byte":
                    val = byte.TryParse(dbValue.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out byteVal) ? byteVal : 0;
                    break;
                case "Decimal":
                    val = decimal.TryParse(dbValue.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out decVal) ? decVal : 0;
                    break;
                case "Double":
                    //val = double.TryParse(dbValue.ToString(), NumberStyles.Any, CultureInfo.GetCultureInfo("en-AU"), out douVal) ? douVal : 0;
                    val = double.TryParse(dbValue.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out douVal) ? douVal : 0;
                    break;
                case "String":
                    val = dbValue.ToString();
                    break;
                case "DateTime":
                    val = Convert.ToDateTime(dbValue);
                    break;
                case "Boolean":
                    val = (Convert.ToInt32(dbValue) != 0);
                    break;
                default:
                    val = null;
                    break;
            }
            return val;
        }

        public static bool? GetBoolValueFromDB(object dbValue, bool? defaultValue = false)
        {
            if (dbValue == DBNull.Value) { return defaultValue; }
            if (dbValue is Boolean) return Convert.ToBoolean(dbValue);

            int intVal = 0;
            Int32.TryParse(dbValue.ToString(), out intVal);
            return (intVal != 0);
        }

        public static int? GetIntValueFromDB(object dbValue, int? defaultValue = 0)
        {
            if (dbValue == DBNull.Value) { return defaultValue; }
            int intVal = 0;
            return Int32.TryParse(dbValue.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out intVal) ? intVal : defaultValue;
        }

        public static decimal? GetDecValueFromDB(object dbValue, decimal? defaultValue = 0)
        {
            if (dbValue == DBNull.Value) { return defaultValue; }
            decimal decVal = 0;
            return decimal.TryParse(dbValue.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out decVal) ? decVal : defaultValue;
        }

        public static DateTime? GetDateTime(object objDateTime, DateTime? defaultDate = null)
        {
            if (objDateTime == null || objDateTime == DBNull.Value || string.IsNullOrEmpty(objDateTime.ToString()))
                return defaultDate;
            else
                return Convert.ToDateTime(objDateTime);
        }

        public static int? GetIntValue(object obj, int? defaultValue = null)
        {
            if (obj == null) { return defaultValue; }
            int intVal = 0;
            return Int32.TryParse(obj.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out intVal) ? intVal : defaultValue;
        }

        public static string GetDateTimeString(object objDateTime, bool forDBUsing = false)
        {
            if (objDateTime == null || objDateTime == DBNull.Value)
                return "null";
            else
            {
                if (forDBUsing)
                    return "'" + Convert.ToDateTime(objDateTime).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "'";
                else
                    return Convert.ToDateTime(objDateTime).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
        }

        public static void SetPropertyValue(object obj, PropertyInfo propInfo, string inputText, object defaultValue = null)
        {
            bool isNullable = false;
            bool isEmpty = string.IsNullOrWhiteSpace(inputText);
            Type type = propInfo.PropertyType;
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                isNullable = true;
                NullableConverter nullableConverter = new NullableConverter(type);
                type = nullableConverter.UnderlyingType;
            }

            switch (type.Name)
            {
                case "Byte":
                case "Int16":
                case "Int64":
                case "Int32":
                    if (!isEmpty)
                        propInfo.SetValue(obj, Convert.ToInt32(inputText));
                    else if (!isNullable)
                        propInfo.SetValue(obj, defaultValue);
                    else
                        propInfo.SetValue(obj, null);
                    break;
                case "Decimal":
                case "Double":
                    if (!isEmpty)
                    {
                        bool doubleParsed = double.TryParse(inputText, NumberStyles.Any, CultureInfo.InvariantCulture, out double douVal);
                        propInfo.SetValue(obj, douVal);
                    }
                    else if (!isNullable)
                        propInfo.SetValue(obj, defaultValue);
                    else
                        propInfo.SetValue(obj, null);
                    break;
                case "String":
                    propInfo.SetValue(obj, inputText);
                    break;
                case "DateTime":
                    if (!isEmpty)
                    {
                        bool timeParsed = DateTime.TryParse(inputText, out DateTime dtVal);
                        propInfo.SetValue(obj, dtVal);
                    }
                    else if (!isNullable)
                        propInfo.SetValue(obj, defaultValue);
                    else
                        propInfo.SetValue(obj, null);
                    break;
                case "Boolean":
                    if (!isEmpty)
                    {
                        bool.TryParse(inputText, out bool blnVal);
                        propInfo.SetValue(obj, blnVal);
                    }
                    else if (!isNullable)
                        propInfo.SetValue(obj, defaultValue);
                    else
                        propInfo.SetValue(obj, null);
                    break;
                default:
                    propInfo.SetValue(obj, inputText);
                    break;
            }
        }


        public static string GetSQLFieldString(object objFieldValue, bool forDBUsing = false)
        {
            if (objFieldValue == null || objFieldValue == DBNull.Value)
                return "null";
            else
            {
                if (forDBUsing)
                    return "'" + objFieldValue.ToString() + "'";
                else
                    return objFieldValue.ToString();
            }
        }

        public static string GetQueryParametersString(string fields)
        {
            string strKeyValue = string.Empty;

            bool bIsFirstProp = true;
            foreach (string fieldName in fields.Split(','))
            {
                if (!bIsFirstProp)
                {
                    strKeyValue += " and ";
                }
                strKeyValue += "[" + fieldName + "] = @" + fieldName;
                bIsFirstProp = false;
            }

            return strKeyValue;
        }

        public static object ConvertToDbValue(object srcVal)
        {
            if (srcVal == null)
                return DBNull.Value;
            else
            {
                if (srcVal.GetType().Name == "DateTime")
                    return Convert.ToDateTime(srcVal).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                else
                    return srcVal;
            }
        }

        public static Nullable<decimal> ConvertToDecimal(object obj)
        {
            decimal? dec = null;
            if (obj != null && obj != DBNull.Value)
            {
                decimal d;
                bool blnParsed = decimal.TryParse(obj.ToString(), out d);
                if (blnParsed) dec = d;
            }
            return dec;
        }

        public static bool DSIsNullOrEmpty(DataSet ds)
        {
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                return false;
            }
            return true;
        }

        public static bool DTIsNullOrEmpty(DataTable dt)
        {
            if (dt != null && dt.Rows.Count > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// get the row's data and fill into the corresponding model/class
        /// </summary>
        /// <param name="hasCommonFields">inclue common fields like 'ID', 'CreateTime' and so on</param>
        public static void InstantiateModel<T>(DataRow dr, T source, bool hasCommonFields = true) where T : class
        {
            Type type = source.GetType();
            PropertyInfo[] propInfos = type.GetProperties();
            int i = 0;
            foreach (PropertyInfo propInfo in propInfos)
            {
                if (!hasCommonFields) //ignore property 'Id','CreateTime' etc.
                {
                    if (propInfo.Name.ToUpper() == "ID")
                    {
                        //i++;
                        continue;
                    }
                }

                if (dr.Table.Columns.Contains(propInfo.Name))
                {
                    if (propInfo.CanWrite)
                    {
                        propInfo.SetValue(source, dr.IsNull(propInfo.Name) ? null : GetValueFromDB(dr[propInfo.Name], propInfo.PropertyType), null);
                    }
                    //i++;
                }
            }
        }

        /// <summary>
        /// fill a DataRow of the specified DataTable with data in an instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="source"></param>
        public static void GenerateDataRow<T>(DataTable dt, DataRow dr, T source, bool convertTimeToString = false, string timeFormat = "yyyy-MM-dd HH:mm:ss") where T : class
        {
            if (dt == null) return;

            Type type = source.GetType();
            PropertyInfo[] propInfos = type.GetProperties();
            foreach (PropertyInfo propInfo in propInfos)
            {
                Type basicType = GetBasicType(propInfo.PropertyType);
                Type dataType = basicType;
                if (basicType != typeof(string) && propInfo.PropertyType.GetInterface("IEnumerable", false) != null) continue;

                if (basicType.Name == "DateTime" && convertTimeToString == true)
                {
                    dataType = typeof(string);
                }
                if (!dt.Columns.Contains(propInfo.Name))
                {
                    dt.Columns.Add(propInfo.Name, dataType);
                }

                object propValue = null;
                if (basicType.Name == "DateTime" && convertTimeToString == true)
                {
                    propValue = propInfo.GetValue(source, null);
                    if (propValue != null) propValue = (propValue as Nullable<DateTime>).Value.ToString(timeFormat, CultureInfo.InvariantCulture);
                }
                else
                    propValue = propInfo.GetValue(source, null);

                dr[propInfo.Name] = GetDbValue(dataType, propValue);
            }
        }

        /// <summary>
        /// get query string of Access database for paging function
        /// </summary>
        /// <param name="strTable"></param>
        /// <param name="strFileds"></param>
        /// <param name="strWhere"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public static string GetPageQueryString(string strTable, string strFileds, string strWhere, string strOrderBy, int pageSize, int pageIndex)
        {
            string strQuery = string.Empty;
            //Regex.Replace(strOrderBy, "asc", "", RegexOptions.IgnoreCase);

            if (pageIndex <= 1)
                strQuery = $"SELECT TOP {pageSize} {strFileds} FROM {strTable} {strWhere} {strOrderBy}";
            else
                strQuery = $@"SELECT A.* FROM (SELECT TOP {pageSize * pageIndex} {strFileds} FROM {strTable} {strWhere} {strOrderBy}) A 
                            LEFT JOIN (SELECT TOP {pageSize * (pageIndex - 1)} {strFileds} FROM {strTable} {strWhere} {strOrderBy}) B ON A.ID=B.ID 
                            WHERE B.ID IS NULL ";
            return strQuery;
        }

        /// <summary>
        /// get the MSSQL parameter from a class instance to save changes into DB
        /// </summary>
        /// <typeparam name="T">source class name</typeparam>
        /// <param name="fields">the fields' name that concat with ',' char</param>
        /// <param name="source">the class which contain those source values, it's used for comparison</param>
        /// <returns></returns>
        public static SqlParameter[] GetParametersByFieldNames<T>(string fields, T source) where T : class
        {
            PropertyInfo[] propInfos = source.GetType().GetProperties();

            List<SqlParameter> paramList = new List<SqlParameter>();
            foreach (string fieldName in fields.Split(','))
            {
                foreach (PropertyInfo propInfo in propInfos)
                {
                    if (propInfo.Name.ToUpper() == fieldName.ToUpper())
                    {
                        Type sourceType = PubFuns.GetBasicType(propInfo.PropertyType);
                        SqlParameter parm = new SqlParameter("@" + fieldName, GetMSSQLDbType(sourceType));
                        parm.Value = GetDbValue(sourceType, propInfo.GetValue(source, null));
                        paramList.Add(parm);
                        break;
                    }
                }
            }

            return paramList.ToArray();
        }

        public static OleDbParameter[] GetAccessParametersByFieldNames<T>(string fields, T source) where T : class
        {
            PropertyInfo[] propInfos = source.GetType().GetProperties();

            List<OleDbParameter> paramList = new List<OleDbParameter>();
            foreach (string fieldName in fields.Split(','))
            {
                foreach (PropertyInfo propInfo in propInfos)
                {
                    if (propInfo.Name.ToUpper() == fieldName.ToUpper())
                    {
                        Type sourceType = PubFuns.GetBasicType(propInfo.PropertyType);
                        OleDbParameter parm = new OleDbParameter("@" + fieldName, GetAccessDbType(sourceType));
                        parm.Value = GetDbValue(sourceType, propInfo.GetValue(source, null));
                        paramList.Add(parm);
                        break;
                    }
                }
            }

            return paramList.ToArray();
        }

        /// <summary>
        /// create a command object for inserting data into Access database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">the relevant object which will be transferred to the target Command</param>
        /// <param name="insertNullValue">to identify if the null value should be transferred</param>
        /// <returns></returns>
        public static OleDbCommand GetInsertCommand<T>(T source, bool insertNullValue = false) where T : class
        {
            Type type = source.GetType();
            PropertyInfo[] propInfos = type.GetProperties();
            List<OleDbParameter> paramList = new List<OleDbParameter>();

            string strKeys = string.Empty, strValues = string.Empty;

            strKeys = "INSERT INTO [" + type.Name.ToUpper() + "](";
            strValues = "VALUES(";

            bool bIsFirstProp = true;
            foreach (PropertyInfo propInfo in propInfos)
            {
                if (propInfo.Name.ToUpper() == "ID") continue;
                //if (IsAttributeDefined(source, typeof(UserDefined), true, propInfo)) continue;

                //object[] customAttrs = propInfo.GetCustomAttributes(false);
                //if (customAttrs.Length > 0)
                //{
                //    if ((customAttrs[0] as UserDefined).Value == true)
                //        continue; //忽略user model 的自定义属性
                //}

                if (!insertNullValue)
                {
                    if (propInfo.GetValue(source, null) == null) continue;
                }

                if (!bIsFirstProp)
                {
                    strKeys += ",";
                    strValues += ",";
                }
                strKeys += "[" + propInfo.Name + "]";
                strValues += "@" + propInfo.Name;
                bIsFirstProp = false;

                Type sourceType = GetBasicType(propInfo.PropertyType);
                OleDbParameter parm = new OleDbParameter(propInfo.Name, PubFuns.GetAccessDbType(sourceType));
                parm.Value = GetDbValue(sourceType, propInfo.GetValue(source, null));
                paramList.Add(parm);
            }

            strKeys += ")";
            strValues += ")";

            OleDbCommand cmd = new OleDbCommand()
            {
                CommandText = strKeys + strValues,
                CommandType = CommandType.Text
            };

            cmd.Parameters.AddRange(paramList.ToArray());
            return cmd;
        }

        public static OleDbType GetAccessDbType(Type type)
        {
            switch (type.Name)
            {
                case "Int16":
                    return OleDbType.SmallInt;
                case "Int32":
                    return OleDbType.Integer;
                case "Int64":
                    return OleDbType.BigInt;
                case "Decimal":
                    return OleDbType.Decimal;
                case "String":
                    return OleDbType.VarChar;
                case "DateTime":
                    return OleDbType.DBTimeStamp;
                case "Byte":
                    return OleDbType.Binary;
                case "Double":
                    return OleDbType.Double;
                case "Boolean":
                    return OleDbType.Boolean;
                default:
                    return OleDbType.VarChar;
            }
        }

        public static SqlDbType GetMSSQLDbType(Type type)
        {
            switch (type.Name)
            {
                case "Int16":
                    return SqlDbType.SmallInt;
                case "Int32":
                    return SqlDbType.Int;
                case "Int64":
                    return SqlDbType.BigInt;
                case "Decimal":
                    return SqlDbType.Decimal;
                case "String":
                    return SqlDbType.VarChar;
                case "DateTime":
                    return SqlDbType.DateTime;
                case "Byte":
                    return SqlDbType.Binary;
                case "Double":
                    return SqlDbType.Float;
                case "Boolean":
                    return SqlDbType.Bit;
                default:
                    return SqlDbType.VarChar;
            }
        }

        public static object GetDbValue(Type baseType, object value)
        {
            if (value == null) return DBNull.Value;

            byte byteVal = 0;
            decimal decVal = 0;
            double douVal = 0;
            object val = null;
            switch (baseType.Name)
            {
                case "Int16":
                    val = Convert.ToInt16(value);
                    break;
                case "Int32":
                    val = Convert.ToInt32(value);
                    break;
                case "Int64":
                    val = Convert.ToInt64(value);
                    break;
                case "Byte":
                    val = byte.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out byteVal) ? byteVal : 0;
                    break;
                case "Decimal":
                    val = decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out decVal) ? decVal : 0;
                    break;
                case "Double":
                    val = (double)(decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out decVal) ? decVal : 0);
                    break;
                case "String":
                    if (string.IsNullOrEmpty(value.ToString()))
                        val = DBNull.Value;
                    else
                        val = value.ToString();
                    break;
                case "DateTime":
                    //val = Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    val = Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    break;
                case "Boolean":
                    val = (Convert.ToInt32(value) != 0);
                    break;
                default:
                    val = DBNull.Value;
                    break;
            }
            return val;
        }
        #endregion

        #region 图像及颜色处理
        /// <summary>
        /// 基于原图像获取指定透明度的新图像
        /// </summary>
        /// <param name="image">原始图像</param>
        /// <param name="opacity">不透明度(0~1, 数值越大越不透明(越清晰))</param>
        /// <returns></returns>
        public static Image GetTransparentImage(Image image, float opacity)
        {
            //方法一: 提取点像素
            //if (opacity >= 1 || opacity <= 0) return image; //透明度应在0 - 1之间
            //Bitmap bitmap = new Bitmap(image);
            //for (int i = 0; i < image.Width; i++)
            //{
            //    for (int j = 0; j < image.Height; j++)
            //    {
            //        Color color = bitmap.GetPixel(i, j);
            //        int alpha = (int)(color.A * opacity);
            //        Color newColor = Color.FromArgb(alpha, color);
            //        bitmap.SetPixel(i, j, newColor);
            //    }
            //}

            //方法二: 利用ColorMatrix
            if (opacity >= 1 || opacity <= 0) return image; //透明度应在0 - 1之间
            Bitmap bitmap = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                ColorMatrix matrix = new ColorMatrix { Matrix33 = opacity };
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                g.DrawImage(image, rectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
            return bitmap;
        }

        /// <summary>
        /// 将图像转换灰度图像
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <returns></returns>
        public static Bitmap GetGrayScaleImage(Image sourceImage)
        {
            // create a new bitmap with size the same as image original, 
            // then create a color matrix and convert a color image to grayscale
            Bitmap newBmp = new Bitmap(sourceImage.Width, sourceImage.Height);
            using (Graphics g = Graphics.FromImage(newBmp))
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
                   new float[] {.3f, .3f, .3f, 0, 0},
                   new float[] {.59f, .59f, .59f, 0, 0},
                   new float[] {.11f, .11f, .11f, 0, 0},
                   new float[] {0, 0, 0, 1, 0},
                   new float[] {0, 0, 0, 0, 1}
                   });
                ImageAttributes imgAttr = new ImageAttributes();
                imgAttr.SetColorMatrix(colorMatrix);
                g.DrawImage(sourceImage, new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, imgAttr);
            }
            return newBmp;
        }

        public static Bitmap GetGrayScaleImage0(Bitmap sourceBmp)
        {
            Bitmap bmp = new Bitmap(sourceBmp.Width, sourceBmp.Height, PixelFormat.Format32bppArgb);

            //设定实例BitmapData相关信息   
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            //锁定原位图至内存中
            BitmapData data = sourceBmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            //锁定bmp到系统内存中   
            BitmapData data2 = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            //获取位图中第一个像素数据的地址   
            IntPtr ptr = data.Scan0;
            IntPtr ptr2 = data2.Scan0;
            int numBytes = data.Stride * data.Height;
            int numBytes2 = data2.Stride * data2.Height;
            int n2 = data2.Stride - bmp.Width * 4; //显示宽度与扫描线宽度的间隙
            byte[] rgbValues = new byte[numBytes];
            byte[] rgbValues2 = new byte[numBytes2];
            //将bmp数据Copy到声明的数组中   
            Marshal.Copy(ptr, rgbValues, 0, numBytes);
            Marshal.Copy(ptr2, rgbValues2, 0, numBytes2);
            int n = 0;
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width * 4; x += 4)
                {
                    int i = data.Stride * y + x;
                    double value = rgbValues[i + 2] * 0.299 + rgbValues[i + 1] * 0.587 + rgbValues[i] * 0.114; //计算灰度
                    byte weightedVal = (byte)value;

                    rgbValues2[i + 3] = rgbValues[i + 3]; //保持原透明度
                    rgbValues2[i + 2] = weightedVal;
                    rgbValues2[i + 1] = weightedVal;
                    rgbValues2[i] = weightedVal;

                    //rgbValues2[n] = (byte)value;
                    n++;
                }
                n += n2; //跳过差值
            }
            //将数据Copy到内存指针   
            Marshal.Copy(rgbValues, 0, ptr, numBytes);
            Marshal.Copy(rgbValues2, 0, ptr2, numBytes2);

            ////16位及16位以下图像每像素数据仅仅是色彩索引, 需修改生成位图的索引表, 从伪彩修改为灰度
            ////调色板数组:
            ////序号 red  green blue
            ////0    0    0     0
            ////1    1    1     1
            ////...
            ////255  255  255  255
            //ColorPalette tempPalette;
            //using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            //{
            //    tempPalette = tempBmp.Palette;
            //}
            //for (int i = 0; i < 256; i++)
            //{
            //    tempPalette.Entries[i] = Color.FromArgb(i, i, i);
            //}
            //bmp.Palette = tempPalette;
            //从系统内存解锁bmp   
            sourceBmp.UnlockBits(data);
            bmp.UnlockBits(data2);
            return bmp;
        }

        /// <summary>
        /// 裁剪图片
        /// </summary>
        /// <param name="originImage">原图片</param>
        /// <param name="region">裁剪的方形区域</param>
        /// <returns>裁剪后图片</returns>
        public static Bitmap CropImage(Image originImage, Rectangle region)
        {
            Bitmap result = new Bitmap(region.Width, region.Height);
            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.DrawImage(originImage, new Rectangle(0, 0, region.Width, region.Height), region, GraphicsUnit.Pixel);
            }
            return result;
        }

        /// <summary>
        /// 图片转换为ico文件
        /// </summary>
        /// <param name="origin">原图片路径</param>
        /// <param name="destination">输出ico文件路径</param>
        /// <param name="iconSize">输出ico图标尺寸，不可大于255x255</param>
        /// <returns>是否转换成功</returns>
        public static bool ConvertImageToIcon(string origin, string destination, Size iconSize)
        {
            if (iconSize.Width > 255 || iconSize.Height > 255)
            {
                return false;
            }

            Image image = new Bitmap(new Bitmap(origin), iconSize); //先读取已有的图片为bitmap，并缩放至设定大小
            MemoryStream bitMapStream = new MemoryStream(); //存原图的内存流
            MemoryStream iconStream = new MemoryStream(); //存图标的内存流
            image.Save(bitMapStream, ImageFormat.Png); //将原图读取为png格式并存入原图内存流
            BinaryWriter iconWriter = new BinaryWriter(iconStream); //新建二进制写入器以写入目标图标内存流

            //根据原图信息，进行文件头写入
            iconWriter.Write((short)0);
            iconWriter.Write((short)1);
            iconWriter.Write((short)1);
            iconWriter.Write((byte)image.Width);
            iconWriter.Write((byte)image.Height);
            iconWriter.Write((short)0);
            iconWriter.Write((short)0);
            iconWriter.Write((short)32);
            iconWriter.Write((int)bitMapStream.Length);
            iconWriter.Write(22);
            //写入图像体至目标图标内存流
            iconWriter.Write(bitMapStream.ToArray());
            //保存流，并将流指针定位至头部以Icon对象进行读取输出为文件
            iconWriter.Flush();
            iconWriter.Seek(0, SeekOrigin.Begin);
            Stream iconFileStream = new FileStream(destination, FileMode.Create);
            Icon icon = new Icon(iconStream);
            icon.Save(iconFileStream); //储存图像

            //释放资源
            iconFileStream.Close();
            iconWriter.Close();
            iconStream.Close();
            bitMapStream.Close();
            icon.Dispose();
            image.Dispose();
            return File.Exists(destination);
        }

        /// <summary>
        /// 将图像转换为icon位图图像
        /// </summary>
        /// <param name="originImage">原图像</param>
        /// <param name="iconSize">将转换为的icon图标大小</param>
        /// <returns></returns>
        public static Icon ConvertImageToIcon(Image originImage, Size iconSize)
        {
            if (iconSize.Width > 255 || iconSize.Height > 255)
            {
                return null;
            }

            Icon icon = null;
            Image image = null;
            if (iconSize != Size.Empty)
                image = new Bitmap(originImage, iconSize);
            else
                image = new Bitmap(originImage);

            using (MemoryStream bitMapStream = new MemoryStream(), //原图的内存流
                 iconStream = new MemoryStream()) //图标的内存流
            {
                image.Save(bitMapStream, ImageFormat.Png);      //将原图读取为png格式并存入原图内存流
                using (BinaryWriter iconWriter = new BinaryWriter(iconStream))
                {
                    //根据原图信息，进行文件头写入                                                        
                    iconWriter.Write((short)0);
                    iconWriter.Write((short)1);
                    iconWriter.Write((short)1);
                    iconWriter.Write((byte)image.Width);
                    iconWriter.Write((byte)image.Height);
                    iconWriter.Write((short)0);
                    iconWriter.Write((short)0);
                    iconWriter.Write((short)32);
                    iconWriter.Write((int)bitMapStream.Length);
                    iconWriter.Write(22);
                    //写入图像体至目标图标内存流
                    iconWriter.Write(bitMapStream.ToArray());
                    iconWriter.Flush();
                    iconWriter.Seek(0, SeekOrigin.Begin);
                    icon = new Icon(iconStream);

                    iconWriter.Close();
                }
                iconStream.Close();
                bitMapStream.Close();
                image.Dispose();
            }
            return icon;
        }

        /// <summary>
        /// 对指定颜色增加各分量值
        /// </summary>
        /// <param name="colorBase">基础色</param>
        /// <param name="a">alpha增量</param>
        /// <param name="r">红</param>
        /// <param name="g">绿</param>
        /// <param name="b">蓝</param>
        /// <returns>返回新的颜色</returns>
        public static Color GetColorPlus(Color colorBase, int a, int r, int g, int b)
        {
            int a0 = colorBase.A;
            int r0 = colorBase.R;
            int g0 = colorBase.G;
            int b0 = colorBase.B;
            if (a + a0 > 255) { a = 255; } else { a = Math.Max(a + a0, 0); }
            if (r + r0 > 255) { r = 255; } else { r = Math.Max(r + r0, 0); }
            if (g + g0 > 255) { g = 255; } else { g = Math.Max(g + g0, 0); }
            if (b + b0 > 255) { b = 255; } else { b = Math.Max(b + b0, 0); }

            return Color.FromArgb(a, r, g, b);
        }
        #endregion

        #region 对象类型及属性
        /// <summary>
        /// get the basic type of a source type
        /// </summary>
        /// <param name="srcType">source type</param>
        /// <returns>if srcType is nullable type, return its UnderlyingType, otherwise return srcType</returns>
        public static Type GetBasicType(Type srcType)
        {
            if (srcType.IsGenericType &&
                srcType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                NullableConverter nullableConverter = new NullableConverter(srcType);
                srcType = nullableConverter.UnderlyingType;
            }
            return srcType;
        }

        /// <summary>
        /// Determines whether the given <paramref name="type"/> is a generic list
        /// </summary>
        /// <param name="type">The type to evaluate</param>
        /// <returns><c>True</c> if is generic otherwise <c>False</c></returns>
        public static bool IsGenericList(Type type)
        {
            if (!type.IsGenericType) { return false; }

            var typeDef = type.GetGenericTypeDefinition();
            if (typeDef == typeof(List<>) || typeDef == typeof(IList<>)) { return true; }
            return false;
        }

        /// <summary>
        /// copy the data from one object to another object
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public static void CopyPropertyValues(object src, object dest)
        {
            if (src == null || dest == null) { return; }

            Type objectType = dest.GetType();
            foreach (PropertyInfo prop2 in objectType.GetProperties())
            {
                // 只处理可读的属性（目标属性需要能读，用于递归时取当前值；源属性需要能读）
                if (!prop2.CanRead) continue;

                PropertyInfo prop1 = src.GetType().GetProperty(prop2.Name, prop2.PropertyType);
                if (prop1 == null || !prop1.CanRead) continue;

                // 值类型 / string：直接赋值
                if (prop2.PropertyType.IsValueType || prop2.PropertyType == typeof(string))
                {
                    if (prop2.CanWrite)
                    {
                        prop2.SetValue(dest, prop1.GetValue(src, null), null);
                    }
                    continue;
                }

                // 引用类型（非 string）
                object srcPropValue = prop1.GetValue(src, null);
                object destPropValue = prop2.GetValue(dest, null);

                if (srcPropValue is IEnumerable) { continue; }

                // 源值为 null，无法递归拷贝
                if (srcPropValue == null)
                {
                    if (prop2.CanWrite) prop2.SetValue(dest, null, null);
                    continue;
                }

                // 目标值为 null，无法递归写入
                if (destPropValue == null) { continue; }

                // 递归拷贝
                CopyPropertyValues(srcPropValue, destPropValue);
                if (prop2.CanWrite)
                {
                    prop2.SetValue(dest, destPropValue, null);
                }
            }
        }

        /// <summary>
        /// copy the data from an object and generate a new object with the same class type
        /// </summary>
        /// <typeparam name="T">class type</typeparam>
        /// <param name="fromObject">source object(class)</param>
        /// <param name="ignoreProperties">property names that need to be ignored</param>
        /// <returns></returns>
        public static T CloneObject<T>(T fromObject, params string[] ignoreProperties) where T : new()
        {
            if (fromObject == null) return default(T);

            T ToObject = new T(); //default(T); //对于引用类型默认值是null ..
            Type objectType = fromObject.GetType();
            foreach (PropertyInfo pinfo in objectType.GetProperties())
            {
                if (ignoreProperties.Contains(pinfo.Name, StringComparer.InvariantCultureIgnoreCase)) { continue; }
                if (pinfo.CanWrite)
                {
                    pinfo.SetValue(ToObject, pinfo.GetValue(fromObject, null), null);
                }
            }
            return ToObject;
        }

        /// <summary>
        /// 对象深度拷贝
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">源对象</param>
        /// <returns>新的对象</returns>
        public static T DeepClone<T>(T obj)
        {
            //如果是字符串或值类型则直接返回
            if (obj is string || obj.GetType().IsValueType) return obj;

            // 如果是数组，处理数组类型
            if (obj.GetType().IsArray)
            {
                var elementType = obj.GetType().GetElementType();
                Array arr = obj as Array;
                var copiedArray = Array.CreateInstance(elementType, arr.Length);
                for (int i = 0; i < arr.Length; i++)
                {
                    copiedArray.SetValue(DeepClone(arr.GetValue(i)), i);
                }
                return (T)(object)copiedArray;
            }

            // 处理集合类型，如 List, Dictionary
            if (obj is IEnumerable && !(obj is string))
            {
                var copiedList = Activator.CreateInstance(obj.GetType()) as IList;
                foreach (var item in (IEnumerable)obj)
                {
                    copiedList.Add(DeepClone(item));
                }
                return (T)(object)copiedList;
            }

            object retval = Activator.CreateInstance(obj.GetType());
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                object fieldValue = field.GetValue(obj);
                try { field.SetValue(retval, DeepClone(fieldValue)); }
                catch { }
            }
            return (T)retval;
        }

        /// <summary>
        /// 比较当前对象是否与目标对象具有某些相同属性
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="referenceObj"></param>
        /// <param name="referenceProps"></param>
        /// <returns></returns>
        public static bool HaveSameProperties(object obj, object referenceObj, PropertyInfo[] referenceProps, bool compareValue = true)
        {
            if (obj == null || referenceObj == null) return false;

            //PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo referProp in referenceProps)
            {
                PropertyInfo prop = obj.GetType().GetProperty(referProp.Name, referProp.PropertyType);
                if (prop != null)
                {
                    if (compareValue)
                    {
                        if (prop.PropertyType.IsValueType || prop.PropertyType.FullName == typeof(string).FullName) //可进行简单类型比较
                        {
                            if (!object.Equals(prop.GetValue(obj), referProp.GetValue(referenceObj)))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return HaveSameProperties(prop.GetValue(obj), referProp.GetValue(referenceObj), referProp.PropertyType.GetProperties());
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 是否为目标对象(类或属性)定义了指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源对象(类)</param>
        /// <param name="defType">自定义特性的类型</param>
        /// <param name="defValue">自定义特性的值</param>
        /// <param name="attributeTarget">对象成员属性</param>
        /// <returns></returns>
        public static bool IsAttributeDefined<T>(T source, Type defType, object defValue, MemberInfo attributeTarget = null) where T : class
        {
            bool result = false;
            if (attributeTarget == null) attributeTarget = source.GetType();
            IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(attributeTarget);
            //Attribute.IsDefined(attributeTarget, defType);
            foreach (var attribute in attributes)
            {
                Type t = attribute.Constructor.DeclaringType;
                if (t.Equals(defType))
                {
                    IList<CustomAttributeTypedArgument> posArgs = attribute.ConstructorArguments;
                    foreach (var arg in posArgs)
                    {
                        if (object.Equals(arg.Value, defValue))
                        {
                            result = true;
                            break;
                        }
                    }
                }
                if (result == true) break;
            }
            return result;
        }

        /// <summary>
        /// 获取为目标对象(类或属性)定义的指定类型的特性
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="source">源对象(类)</param>
        /// <param name="defType">自定义特性的类型</param>
        /// <returns></returns>
        public static CustomAttributeData GetAttribute<T>(T source, Type defType) where T : class
        {
            CustomAttributeData attr = null;
            MemberInfo attributeTarget = source.GetType();
            IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(attributeTarget);
            foreach (var attribute in attributes)
            {
                Type t = attribute.Constructor.DeclaringType;
                if (t.Equals(defType))
                {
                    attr = attribute;
                    break;
                }
            }
            return attr;
        }

        /// <summary>
        /// 获取为目标对象(类或属性)指定的特性
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="source">源对象(类)</param>
        /// <param name="defType">自定义特性的类型</param>
        /// <param name="defValue">特性的构造器中指定的参数的值</param>
        /// <returns></returns>
        public static CustomAttributeData GetAttribute<T>(T source, Type defType, object defValue) where T : class
        {
            CustomAttributeData attr = null;
            MemberInfo attributeTarget = source.GetType();
            IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(attributeTarget);
            foreach (var attribute in attributes)
            {
                Type t = attribute.Constructor.DeclaringType;
                if (t.Equals(defType))
                {
                    IList<CustomAttributeTypedArgument> posArgs = attribute.ConstructorArguments;
                    foreach (var arg in posArgs)
                    {
                        if (object.Equals(arg.Value, defValue))
                        {
                            attr = attribute;
                            break;
                        }
                    }
                    if (attr != null) break;
                }
            }
            return attr;
        }

        /// <summary>
        /// 获取自定义特性中指定属性的值
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="attrMemberName"></param>
        /// <returns></returns>
        public static object GetAttributeValue(CustomAttributeData attribute, string attrMemberName)
        {
            object objReturn = null;
            IList<CustomAttributeNamedArgument> posArgs = attribute.NamedArguments;
            foreach (var arg in posArgs)
            {
                if (arg.MemberInfo.Name == attrMemberName)
                {
                    objReturn = arg.TypedValue.Value;
                    break;
                }
            }

            return objReturn;
        }

        #endregion

        /// <summary>
        /// 更新json文件的一个节点值
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="nodeKey"></param>
        /// <param name="nodeValue"></param>
        public static void ModifyJsonFileNode(string filePath, string nodeKey, object nodeValue)
        {
            FileInfo fi = new FileInfo(filePath);
            if (!fi.Directory.Exists) fi.Directory.Create();

            string fileContent = File.ReadAllText(filePath, Encoding.UTF8);
            JObject jo = JObject.Parse(fileContent);
            jo[nodeKey] = JToken.FromObject(nodeValue);

            string jsonText = JsonConvert.SerializeObject(jo, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, jsonText);
        }

        /// <summary>
        /// 异步读取文件的全部内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 40960, useAsync: true))
            {
                byte[] buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer, 0, (int)fs.Length).ConfigureAwait(false);
                return buffer;
            }
        }

        /// <summary>
        /// 将xml文本转换为DataTable对象
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static DataTable ConvertXMLToDataTable(string xmlData)
        {
            StringReader stringReader = null;
            XmlTextReader xmlTextReader = null;
            DataTable result;
            try
            {
                DataSet dataSet = new DataSet();
                stringReader = new StringReader(xmlData);
                xmlTextReader = new XmlTextReader(stringReader);
                dataSet.ReadXml(xmlTextReader);
                if (dataSet != null && dataSet.Tables.Count > 0)
                {
                    result = dataSet.Tables[0];
                }
                else
                {
                    result = null;
                }
            }
            catch
            {
                result = null;
            }
            finally
            {
                if (xmlTextReader != null)
                {
                    xmlTextReader.Close();
                }
                if (stringReader != null)
                {
                    stringReader.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// 将json数组转换为DataTable
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static DataTable ConvertJsonArrayToDataTable(string jsonText)
        {
            DataTable dataTable = new DataTable();
            DataTable result;

            //JObject jo = (JObject)JsonConvert.DeserializeObject(jsonText);
            //JArray ja = (JArray)jo["Data"];
            //jaText = ja.ToString();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
            ArrayList arrayList = javaScriptSerializer.Deserialize<ArrayList>(jsonText);
            if (arrayList.Count > 0)
            {
                foreach (Dictionary<string, object> dictionary in arrayList)
                {
                    if (dictionary.Keys.Count == 0)
                    {
                        result = dataTable;
                        return result;
                    }
                    if (dataTable.Columns.Count == 0)
                    {
                        foreach (string current in dictionary.Keys)
                        {
                            //dataTable.Columns.Add(current, dictionary[current].GetType());
                            dataTable.Columns.Add(current); //deal with every column as string type
                        }
                    }
                    DataRow dataRow = dataTable.NewRow();
                    foreach (string current in dictionary.Keys)
                    {
                        dataRow[current] = dictionary[current];
                    }

                    dataTable.Rows.Add(dataRow);
                }
            }
            result = dataTable;
            return result;
        }

        public static string GetErrors(Exception ex)
        {
            StringBuilder sbErrMsg = new StringBuilder();
            sbErrMsg.AppendLine(ex.Message);
            Exception innerEx = ex.InnerException;
            while (true)
            {
                if (innerEx != null)
                {
                    sbErrMsg.AppendLine("---> " + innerEx.Message);
                    innerEx = innerEx.InnerException;
                }
                else
                    break;
            }
            return sbErrMsg.ToString();
        }

        public static string GetBaseDirectory()
        {
            //string dirPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            string dirPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            return dirPath;
        }

        /// <summary>
        /// 获取文件/文件夹路径
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <param name="createDirectory">若不存在指定的路径, 是否创建其相关目录</param>
        /// <param name="isDirectory">给定的路径是否表示文件夹</param>
        /// <returns></returns>
        public static string GetAbsolutePath(string relativePath, bool createDirectory = false, bool isDirectory = false)
        {
            string absPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            try
            {
                DirectoryInfo dir = null;
                if (isDirectory)
                {
                    dir = new DirectoryInfo(absPath);
                }
                else
                {
                    FileInfo fileInfo = new FileInfo(absPath);
                    dir = fileInfo.Directory;
                    absPath = fileInfo.FullName;
                }

                if (createDirectory)
                {
                    if (!dir.Exists) dir.Create();
                }
            }
            catch
            {
                absPath = "";
            }

            return absPath;
        }

        /// <summary>
        /// 获取当前程序所在硬盘根目录文件夹路径
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="createIfNotExist"></param>
        /// <returns></returns>
        public static string GetBaseDiskFolder(string folderName, bool createIfNotExist = true)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string volume = baseDir.Substring(0, baseDir.IndexOf(Path.VolumeSeparatorChar));
            string folderPath = volume + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar + folderName;
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            if (createIfNotExist)
            {
                if (!dir.Exists) dir.Create();
            }
            return folderPath;
        }

        /// <summary>
        /// 对文件名或目录名过滤非法字符(清理或替换)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="invalidReplacement"></param>
        /// <returns></returns>
        public static string SanitizeFileName(string name, string invalidReplacement = "")
        {
            if (string.IsNullOrWhiteSpace(name)) return "default_folder";

            // 获取系统非法字符
            char[] invalidChars = Path.GetInvalidFileNameChars();

            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (Array.IndexOf(invalidChars, c) < 0)
                    sb.Append(c);
                else
                    sb.Append(invalidReplacement);
            }

            string result = sb.ToString().Trim();

            // 处理 Windows 保留名 (CON, PRN, AUX, NUL, COM1-9, LPT1-9)
            string[] reservedNames = { "CON", "PRN", "AUX", "NUL",
                "COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9",
                "LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9" };

            if (reservedNames.Any(r => string.Equals(r, result, StringComparison.OrdinalIgnoreCase)))
                result = "_" + result;

            // 去掉末尾的点和空格（Windows 不允许）
            result = result.TrimEnd('.', ' ');

            return string.IsNullOrEmpty(result) ? "default_folder" : result;
        }

        /// <summary>
        /// 以多线程形式异步处理指定的任务
        /// </summary>
        /// <param name="action">将在新的线程中执行的方法</param>
        /// <param name="duration">任务执行的时长</param>
        public static async Task AsyncDoWork(ThreadStart action, int duration = -1)
        {
            Task prepareTask = Task.Run(() =>
            {
                action.Invoke();
            });

            await Task.WhenAny(prepareTask, Task.Delay(duration));
        }

        /// <summary>
        /// 异步方式延迟指定的时间(ms)之后执行某个动作
        /// </summary>
        /// <param name="fun">待执行的动作</param>
        /// <param name="dueTime">执行动作前延迟的时长</param>
        /// <param name="tryTimes">重试次数</param>
        /// <param name="callback">异步操作之后的回调方法</param>
        public static void DelayDoWork(Func<bool> fun, int dueTime, int tryTimes, Action callback)
        {
            Thread thd = new Thread(() =>
            {
                for (int i = 0; i < tryTimes; i++)
                {
                    Thread.Sleep(dueTime);
                    if (fun()) break;
                }
                callback?.Invoke();
            });
            thd.Start();
        }

        /// <summary>
        /// 获取本地主机的IPv4地址
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalIP()
        {
            IPAddress localIp = null;
            try
            {
                IPAddress[] ipArray;
                ipArray = Dns.GetHostAddresses(Dns.GetHostName());

                localIp = ipArray.First(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            }
            catch (Exception ex)
            {
            }

            return localIp;
        }

        /// <summary>
        /// 获取自1970年1月1日00:00:00 UTC以来的毫秒数
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentTimeMillis()
        {
            return (DateTime.UtcNow.Ticks - Jan1st1970ns) / 10000;
        }

        /// <summary>
        /// 将指定时间转换为自1970年1月1日00:00:00 UTC以来的毫秒数
        /// </summary>
        /// <param name="dt">UTC时间</param>
        /// <returns></returns>
        public static long GetUTCTimeMillis(DateTime dt)
        {
            return (dt.Ticks - Jan1st1970ns) / 10000;
        }

        /// <summary>
        /// 将一个代表自1970年1月1日0时起的毫秒数, 转换为DateTime
        /// </summary>
        /// <param name="timeMillis">时间戳(毫秒)</param>
        /// <param name="isUTC">传入的时间戳是否为协调世界时</param>
        /// <returns></returns>
        public static DateTime GetDateTime(long timeMillis, bool isUTC = true)
        {
            long offset = 0;
            if (isUTC == false) offset = DateTimeOffset.Now.Offset.Ticks;

            return new DateTime(Jan1st1970ns + timeMillis * 10000L + offset); //.ToLocalTime();
        }

        /// <summary>
        /// 根据当前运行环境的字节存储顺序获取系统可正确解析的字节数组
        /// </summary>
        /// <param name="arrByte"></param>
        /// <param name="isLittleEndian"></param>
        /// <returns></returns>
        public static byte[] GetBySystemEndian(byte[] arrByte, bool isLittleEndian = false)
        {
            //x86平台中运行时, 一般为小端序存储方式(BitConverter.IsLittleEndian==true)
            if (isLittleEndian)
            {
                if (!BitConverter.IsLittleEndian) Array.Reverse(arrByte);
            }
            else
            {
                if (BitConverter.IsLittleEndian) Array.Reverse(arrByte);
            }

            return arrByte;
        }

        /// <summary>
        /// 获取小数位数
        /// </summary>
        /// <param name="decimalV">小数</param>
        /// <returns></returns>
        public static int GetNumberOfDecimalPlaces(double decimalV)
        {
            string[] temp = decimalV.ToString().Split('.');
            if (temp.Length == 2 && temp[1].Length > 0)
            {
                int index = temp[1].Length - 1;
                while (temp[1][index] == '0' && index-- > 0) ;
                return index + 1;
            }
            return 0;
        }

        /// <summary>
        /// 获取小数位数
        /// </summary>
        /// <param name="decimalV">小数</param>
        /// <returns></returns>
        public static int GetNumberOfDecimalPlaces(string valString)
        {
            string[] temp = valString.Split('.');
            if (temp.Length == 2 && temp[1].Length > 0)
            {
                int index = temp[1].Length - 1;
                while (temp[1][index] == '0' && index-- > 0) ;
                return index + 1;
            }
            return 0;
        }

        /// <summary>
        /// 将文本解析为整数值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int? ParseInt(string input)
        {
            // 移除字符串中的非数字字符，只保留数字部分
            string numericPart = string.Empty;
            foreach (char c in input)
            {
                if (char.IsDigit(c) || c == '-' || c == '+') // 允许负号和加号
                {
                    numericPart += c;
                }
                else if (numericPart.Length > 0) // 遇到第一个非数字字符就停止
                {
                    break;
                }
            }

            // 如果找到数字部分，尝试将其转换为整数
            if (int.TryParse(numericPart, out int result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 将文本解析为浮点数
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static float? ParseFloat(string input)
        {
            // 移除字符串中的非数字字符，保留有效的浮点数部分
            string numericPart = string.Empty;
            foreach (char c in input)
            {
                if (char.IsDigit(c) || c == '.' || c == '-' || c == '+') // 允许负号、加号和小数点
                {
                    numericPart += c;
                }
                else if (numericPart.Length > 0) // 遇到第一个非数字字符就停止
                {
                    break;
                }
            }

            // 如果找到数字部分，尝试将其转换为浮点数
            if (float.TryParse(numericPart, out float result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 检查身份证号码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidIdCard(string idCardNo)
        {
            Regex reg = new Regex(@"(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)");

            // 检查值是否为空或不符合正则表达式
            if (!string.IsNullOrEmpty(idCardNo) && !reg.IsMatch(idCardNo))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 检查手机号
        /// </summary>
        /// <param name="phoneNo"></param>
        /// <returns></returns>
        public static bool IsValidPhone(string phoneNo)
        {
            if (!string.IsNullOrEmpty(phoneNo) && !Regex.IsMatch(phoneNo, @"^1[3456789]\d{9}$"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 检查邮箱地址
        /// </summary>
        /// <returns></returns>
        public static bool IsValidEmail(string email)
        {
            var regex = new Regex(@"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$");

            if (!string.IsNullOrEmpty(email) && !regex.IsMatch(email))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 验证字符串是否是有效的IP地址
        /// </summary>
        /// <param name="ipAddress">输入的IP字符串</param>
        /// <returns></returns>
        public static bool IsValidIP(string ipAddress)
        {
            //Regex regIP = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
            //return (ipAddress != "" && regIP.IsMatch(ipAddress.Trim())) ? true : false;

            IPAddress ip;
            bool isValid = !string.IsNullOrEmpty(ipAddress) && IPAddress.TryParse(ipAddress, out ip);
            return isValid;
        }

        /// <summary>
        /// 验证字符串是否符合json格式
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 将16进制文本转换为二进制数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ConvertHexTextToBytes(string hexString)
        {
            hexString = hexString.Replace(" ", "");

            // 将十六进制字符串转换为字节数组
            byte[] byteArray = new byte[hexString.Length / 2];

            for (int i = 0; i < hexString.Length; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return byteArray;
        }

        /// <summary>
        /// 在一个字节序列中查找一个字节数组出现的位置
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="subArray"></param>
        /// <param name="offset">源数组或列表中开始查找的位置或偏移量</param>
        /// <returns></returns>
        public static int FindSubArray(IList<byte> sourceList, byte[] subArray, int offset = 0)
        {
            if (subArray.Length > sourceList.Count - offset)
            {
                return -1;
            }

            for (int i = offset; i <= sourceList.Count - subArray.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < subArray.Length; j++)
                {
                    if (sourceList[i + j] != subArray[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return i; // 找到匹配的子数组  
                }
            }

            return -1; // 没有找到匹配的子数组  
        }

        /// <summary>
        /// 将数组中从开始位置获取指定数量的元素,串联成字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public static string JoinWithEllipsis<T>(T[] array, int maxCount = 10)
        {
            if (array == null || array.Length == 0)
                return string.Empty;

            int count = Math.Min(array.Length, maxCount);
            var segment = new ArraySegment<T>(array, 0, count);
            string result = string.Join(", ", segment);

            if (array.Length > maxCount)
                result += ", ...";

            return result;
        }
    }

    public static class ToolExtensions
    {
        public static string TrimEndNewlines(this string inputVal)
        {
            return inputVal?.TrimEnd((Environment.NewLine + "\r\n").ToCharArray()).Trim();
        }

        public static string Replace(this string str, string[] arr, string newValue)
        {
            if (string.IsNullOrEmpty(str)) return str;

            foreach (string item in arr)
            {
                str = str.Replace(item, newValue);
            }
            return str;
        }

        public static PointF Offset(this PointF p, float x, float y)
        {
            PointF pNew = new PointF(p.X + x, p.Y + y);
            return pNew;
        }
        public static PointF Offset(this PointF p, PointF pos)
        {
            PointF pNew = new PointF(p.X + pos.X, p.Y + pos.Y);
            return pNew;
        }

        public static T GetDRValue<T>(this DataRow dtRow, string strColName, T objNullValue)
        {
            return dtRow.IsNull(strColName) ? objNullValue : (T)dtRow[strColName];
        }

        public static T GetSDRValue<T>(this SqlDataReader drReport, string strColName, T objNullValue)
        {
            return drReport.IsDBNull(drReport.GetOrdinal(strColName)) ? objNullValue : (T)drReport[strColName];
        }

        public static int? GetIntValue(this Dictionary<string, string> dict, string key, int? defaultValue = null, bool convertZero = false)
        {
            bool haveKey = dict.TryGetValue(key, out string value);
            if (haveKey && !string.IsNullOrEmpty(value))
            {
                int val = Convert.ToInt32(value);
                if (val == 0 && convertZero)
                    return defaultValue;
                else
                    return val;
            }
            else
            {
                return defaultValue;
            }
        }

        public static float? GetFloatValue(this Dictionary<string, string> dict, string key, float? defaultValue = null, bool convertZero = false)
        {
            bool haveKey = dict.TryGetValue(key, out string value);
            if (haveKey && !string.IsNullOrEmpty(value))
            {
                float val = Convert.ToSingle(value);
                if (val == 0 && convertZero)
                    return defaultValue;
                else
                    return val;
            }
            else
            {
                return defaultValue;
            }
        }

        public static string GetStringValue(this Dictionary<string, string> dict, string key, string defaultValue = null)
        {
            bool haveKey = dict.TryGetValue(key, out string value);
            if (haveKey)
                return value;
            else
                return defaultValue;
        }

        public static object ToDBValue<T>(this Nullable<T> value) where T : struct
        {
            return value.HasValue ? (object)value.Value : DBNull.Value;
        }

        public static List<T> Clone<T>(this List<T> srcList)
        {
            List<T> items = new List<T>();
            for (int i = 0; i < srcList.Count; i++)
            {
                T srcModel = srcList[i];
                T model = PubFuns.DeepClone(srcModel);
                //T model = PubFuns.DeepClone(srcModel);

                items.Add(model);
            }

            return items;
        }

        public static List<T> ShallowClone<T>(this List<T> srcList) where T : new()
        {
            List<T> items = new List<T>();
            foreach (T srcModel in srcList)
            {
                T model = PubFuns.CloneObject(srcModel);
                //T model = PubFuns.DeepClone(srcModel);

                items.Add(model);
            }

            return items;
        }

        public static void Dispose(this List<Image> imageList)
        {
            if (imageList != null && imageList.Count > 0)
            {
                for (int i = 0; i < imageList.Count; i++)
                {
                    imageList[i]?.Dispose();
                }
            }
        }

        /// <summary>
        /// 按指定小数的精度与其比较大小
        /// </summary>
        /// <param name="inputVal"></param>
        /// <param name="targetVal"></param>
        /// <returns></returns>
        public static int CompareWithFixedLength(this double inputVal, double targetVal)
        {
            double compVal = inputVal;
            if (targetVal != 0)
            {
                int decimals = PubFuns.GetNumberOfDecimalPlaces(targetVal);
                compVal = Math.Round(inputVal, decimals, MidpointRounding.AwayFromZero);
            }

            return compVal.CompareTo(targetVal);
        }

        /// <summary>
        /// 确定此字节数组的开头是否与指定的字节数组匹配
        /// </summary>
        public static bool StartWith(this byte[] bytes, byte[] searchArray)
        {
            bool result = true;
            if (bytes.Length >= searchArray.Length)
            {
                for (int i = 0; i < searchArray.Length; i++)
                {
                    if (bytes[i] != searchArray[i])
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 字节数组转2进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="splitStr"></param>
        /// <returns></returns>
        public static string ToBinaryStr(this byte[] bytes, string splitStr = "")
        {
            StringBuilder sb = new StringBuilder();
            if (bytes != null)
            {
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    sb.Append(Convert.ToString(bytes[i], 2).PadLeft(8, '0')).Append(splitStr);
                }
                sb.Remove(sb.Length - splitStr.Length, splitStr.Length);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 字节数组转10进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="splitStr"></param>
        /// <returns></returns>
        public static string ToDecStr(this byte[] bytes, string splitStr = "")
        {
            StringBuilder sb = new StringBuilder();
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i]).Append(splitStr);
                }
                sb.Remove(sb.Length - splitStr.Length, splitStr.Length);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 字节数组(或列表)转16进制字符串
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static string ToHexStr(this IList<byte> bytes, string splitStr = "")
        {
            StringBuilder sb = new StringBuilder();
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Count; i++)
                {
                    sb.Append(bytes[i].ToString("X2")).Append(splitStr);
                    //if ((i + 1) % 2 == 0) sb.Append(splitStr);
                }
                sb.Remove(sb.Length - splitStr.Length, splitStr.Length);
            }
            return sb.ToString();
        }
    }

}
