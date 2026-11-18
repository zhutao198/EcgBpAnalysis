using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Zoncare.Utilities
{
    /// <summary>
    /// INI文件读写类。
    /// Copyright (C) Maticsoft
    /// </summary>
    public class INIFile
    {
        public string path;

        public INIFile(string INIPath)
        {
            path = INIPath;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);


        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();
        }

        public byte[] IniReadValues(string section, string key)
        {
            byte[] temp = new byte[255];
            int i = GetPrivateProfileString(section, key, "", temp, 255, this.path);
            return temp;
        }

        /// <summary>
        /// 删除ini文件下所有段落
        /// </summary>
        public void ClearAllSection()
        {
            IniWriteValue(null, null, null);
        }

        /// <summary>
        /// 删除ini文件下personal段落下的所有键
        /// </summary>
        /// <param name="Section"></param>
        public void ClearSection(string Section)
        {
            IniWriteValue(Section, null, null);
        }

        /// <summary>
        /// 按行读取ini文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ReadIni(string filePath)
        {
            var iniDictionary = new Dictionary<string, string>();
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
                foreach (string line in lines)
                {
                    string[] keyValue = line.Split("=".ToCharArray(), 2);
                    if (keyValue.Length > 0)
                    {
                        string key = keyValue[0];
                        string value = "";
                        if (keyValue.Length == 2) value = keyValue[1];
                        iniDictionary[key] = value;
                    }

                }
            }
            return iniDictionary;
        }

        public static Dictionary<string, string> ReadIniContent(string fileContent)
        {
            var iniDictionary = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(fileContent))
            {
                using (StringReader reader = new StringReader(fileContent))
                {
                    string line;
                    while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
                    {
                        if (line.Contains("="))
                        {
                            string[] keyValue = line.Split("=".ToCharArray(), 2);
                            if (keyValue.Length == 2)
                            {
                                string key = keyValue[0];
                                string value = keyValue[1];
                                iniDictionary[key] = value;
                            }
                        }
                    }
                }
            }
            return iniDictionary;
        }

        public static void WriteIni(string filePath, Dictionary<string, string> iniDictionary)
        {
            var sb = new StringBuilder();
            foreach (var kvp in iniDictionary)
            {
                sb.AppendLine($"{kvp.Key}={kvp.Value}");
            }
            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(false));
        }
    }
}