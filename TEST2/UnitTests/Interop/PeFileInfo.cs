using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EcgBpAnalysis.T2.Tests.Interop
{
    /// <summary>
    /// PE 文件解析器：提取导出表符号与位数信息。
    /// 纯二进制解析，不依赖任何第三方库。
    /// </summary>
    public class PeFileInfo
    {
        public ushort Machine;          // 0x014C=x86, 0x8664=AMD64
        public bool Is64Bit { get { return Machine == 0x8664; } }
        public string Architecture
        {
            get
            {
                switch (Machine)
                {
                    case 0x014C: return "x86";
                    case 0x8664: return "x64";
                    case 0x01C0: return "ARM";
                    case 0xAA64: return "ARM64";
                    default: return "0x" + Machine.ToString("X4");
                }
            }
        }
        public HashSet<string> Exports = new HashSet<string>(StringComparer.Ordinal);
        public bool HasExportTable;

        /// <summary>
        /// 解析 PE 文件；失败时返回 null（不抛异常，由调用方决定断言）。
        /// </summary>
        public static PeFileInfo Parse(string path)
        {
            try
            {
                byte[] b = File.ReadAllBytes(path);
                return Parse(b);
            }
            catch { return null; }
        }

        public static PeFileInfo Parse(byte[] b)
        {
            try
            {
                if (b.Length < 0x40) return null;
                if (b[0] != 'M' || b[1] != 'Z') return null;           // MZ

                int peOff = BitConverter.ToInt32(b, 0x3C);
                if (peOff <= 0 || peOff + 24 > b.Length) return null;
                if (BitConverter.ToUInt32(b, peOff) != 0x00004550) return null;  // PE\0\0

                var info = new PeFileInfo();
                int coff = peOff + 4;
                info.Machine = BitConverter.ToUInt16(b, coff);
                int numberOfSections = BitConverter.ToUInt16(b, coff + 2);
                int sizeOfOptionalHeader = BitConverter.ToUInt16(b, coff + 16);

                int opt = coff + 20;
                bool pe32Plus = false;
                if (sizeOfOptionalHeader >= 2)
                    pe32Plus = BitConverter.ToUInt16(b, opt) == 0x020B;

                // 数据目录：Export Table 是第 0 项
                int dataDirOffset = opt + (pe32Plus ? 112 : 96);
                if (dataDirOffset + 8 > b.Length) return info;   // 无导出目录

                uint exportRva = BitConverter.ToUInt32(b, dataDirOffset);
                uint exportSize = BitConverter.ToUInt32(b, dataDirOffset + 4);
                info.HasExportTable = exportRva != 0 && exportSize != 0;
                if (!info.HasExportTable) return info;

                // 节表
                int sectionTable = opt + sizeOfOptionalHeader;
                var sections = new List<int[]>();   // {virtualAddress, virtualSize, rawAddress, rawSize}
                for (int i = 0; i < numberOfSections; i++)
                {
                    int s = sectionTable + i * 40;
                    if (s + 40 > b.Length) break;
                    sections.Add(new[]
                    {
                        BitConverter.ToInt32(b, s + 12),   // VirtualAddress
                        BitConverter.ToInt32(b, s + 8),    // VirtualSize
                        BitConverter.ToInt32(b, s + 20),   // PointerToRawData
                        BitConverter.ToInt32(b, s + 16)    // SizeOfRawData
                    });
                }

                Func<uint, int> rvaToOffset = rva =>
                {
                    foreach (var sec in sections)
                    {
                        int va = sec[0], vs = sec[1], raw = sec[2], rawSize = sec[3];
                        int size = Math.Max(vs, rawSize);
                        if (rva >= va && rva < va + size) return raw + (int)(rva - va);
                    }
                    return -1;
                };

                int expOff = rvaToOffset(exportRva);
                if (expOff < 0 || expOff + 40 > b.Length) return info;

                uint numberOfNames = BitConverter.ToUInt32(b, expOff + 24);
                uint addressOfNames = BitConverter.ToUInt32(b, expOff + 32);
                uint baseOrdinal = BitConverter.ToUInt32(b, expOff + 16);

                int namesOff = rvaToOffset(addressOfNames);
                if (namesOff < 0) return info;

                for (uint i = 0; i < numberOfNames; i++)
                {
                    int p = namesOff + (int)(i * 4);
                    if (p + 4 > b.Length) break;
                    uint nameRva = BitConverter.ToUInt32(b, p);
                    int nameOff = rvaToOffset(nameRva);
                    if (nameOff < 0) continue;

                    int end = nameOff;
                    while (end < b.Length && b[end] != 0) end++;
                    string name = Encoding.ASCII.GetString(b, nameOff, end - nameOff);
                    if (name.Length > 0) info.Exports.Add(name);
                }
                return info;
            }
            catch { return null; }
        }
    }
}
