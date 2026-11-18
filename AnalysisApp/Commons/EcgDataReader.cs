using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace AnalysisApp.Commons
{
    public class EcgDataReader : IDisposable
    {
        private const int HeaderSize = 8;
        private const int ShortsPerSample = 8;
        private const int CharsPerSample = 2;
        private const int SampleSize = ShortsPerSample * 2 + 2;

        public const int LEAD_I_INDEX = 0;
        public const int LEAD_II_INDEX = 1;
        public const int LEAD_V1_INDEX = 2;
        public const int LEAD_V2_INDEX = 3;
        public const int LEAD_V3_INDEX = 4;
        public const int LEAD_V4_INDEX = 5;
        public const int LEAD_V5_INDEX = 6;
        public const int LEAD_V6_INDEX = 7;

        public const int LEAD_I_INDEX_FINAL = 0;
        public const int LEAD_II_INDEX_FINAL = 1;
        public const int LEAD_III_INDEX_FINAL = 2;
        public const int LEAD_AVR_INDEX_FINAL = 3;
        public const int LEAD_AVL_INDEX_FINAL = 4;
        public const int LEAD_AVF_INDEX_FINAL = 5;
        public const int LEAD_V1_INDEX_FINAL = 6;
        public const int LEAD_V2_INDEX_FINAL = 7;
        public const int LEAD_V3_INDEX_FINAL = 8;
        public const int LEAD_V4_INDEX_FINAL = 9;
        public const int LEAD_V5_INDEX_FINAL = 10;
        public const int LEAD_V6_INDEX_FINAL = 11;

        public const int TotalLeads = 12;

        private readonly string filePath;
        private MemoryMappedFile memoryMappedFile;
        private long fileSize;
        private bool isDisposed;

        private List<byte[]> sampleStatusBits;

        public EcgDataReader(string filePath)
        {
            this.filePath = filePath;
            InitializeMemoryMappedFile();
        }

        private void InitializeMemoryMappedFile()
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"ECG数据文件未找到: {filePath}");
            }

            FileInfo fileInfo = new FileInfo(filePath);
            fileSize = fileInfo.Length;

            try
            {
                memoryMappedFile = MemoryMappedFile.CreateFromFile(
                    filePath,
                    FileMode.Open,
                    null,
                    fileSize,
                    MemoryMappedFileAccess.Read);
            }
            catch
            {
                memoryMappedFile?.Dispose();
                memoryMappedFile = null;
            }
        }

        public List<List<float>> ReadEcgDataBySample(int startSample, int sampleCount)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(EcgDataReader));
            }

            try
            {
                long headerOffset = HeaderSize;
                long sampleOffset = startSample * SampleSize;
                long dataOffset = headerOffset + sampleOffset;

                int totalSamples = sampleCount;
                long bytesToRead = totalSamples * SampleSize;

                if (dataOffset + bytesToRead > fileSize)
                {
                    long availableBytes = fileSize - dataOffset;
                    if (availableBytes <= 0)
                    {
                        totalSamples = 0;
                        bytesToRead = 0;
                    }
                    else
                    {
                        totalSamples = (int)(availableBytes / SampleSize);
                        bytesToRead = totalSamples * SampleSize;
                    }
                }

                if (totalSamples <= 0)
                {
                    return CreateEmptyData();
                }

                List<List<float>> ecgData = ReadRawDataMemoryMapped(dataOffset, totalSamples);
                ecgData = CalculateDerivedLeads(ecgData);

                return ecgData;
            }
            catch (Exception ex)
            {
                throw new IOException($"读取ECG数据时发生错误: {ex.Message}", ex);
            }
        }

        private List<List<float>> ReadRawDataMemoryMapped(long dataOffset, int totalSamples)
        {
            List<List<float>> ecgData = new List<List<float>>(ShortsPerSample);
            for (int i = 0; i < ShortsPerSample; i++)
            {
                ecgData.Add(new List<float>(totalSamples));
            }

            sampleStatusBits = new List<byte[]>(totalSamples);

            long bytesToRead = totalSamples * SampleSize;

            if (memoryMappedFile == null)
            {
                return ReadWithFileStream(dataOffset, totalSamples, ecgData);
            }

            try
            {
                using (MemoryMappedViewStream viewStream = memoryMappedFile.CreateViewStream(dataOffset, bytesToRead, MemoryMappedFileAccess.Read))
                {
                    byte[] buffer = new byte[SampleSize];
                    for (int sampleIndex = 0; sampleIndex < totalSamples; sampleIndex++)
                    {
                        int bytesRead = viewStream.Read(buffer, 0, SampleSize);
                        if (bytesRead < SampleSize)
                        {
                            break;
                        }

                        int offset = 0;
                        for (int i = 0; i < ShortsPerSample; i++)
                        {
                            short rawValue = BitConverter.ToInt16(buffer, offset);
                            ecgData[i].Add(rawValue / 1000.0f);
                            offset += 2;
                        }

                        byte[] statusBits = new byte[CharsPerSample];
                        statusBits[0] = buffer[offset++];
                        statusBits[1] = buffer[offset];
                        sampleStatusBits.Add(statusBits);
                    }
                }
            }
            catch
            {
                ecgData.Clear();
                return ReadWithFileStream(dataOffset, totalSamples, ecgData);
            }

            return ecgData;
        }

        private List<List<float>> ReadWithFileStream(long dataOffset, int totalSamples, List<List<float>> ecgData)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
            using (BinaryReader binaryReader = new BinaryReader(fileStream))
            {
                fileStream.Seek(dataOffset, SeekOrigin.Begin);

                for (int sampleIndex = 0; sampleIndex < totalSamples; sampleIndex++)
                {
                    for (int i = 0; i < ShortsPerSample; i++)
                    {
                        short rawValue = binaryReader.ReadInt16();
                        ecgData[i].Add(rawValue / 1000.0f);
                    }

                    byte[] statusBits = new byte[CharsPerSample];
                    statusBits[0] = binaryReader.ReadByte();
                    statusBits[1] = binaryReader.ReadByte();
                    sampleStatusBits.Add(statusBits);
                }
            }

            return ecgData;
        }

        private List<List<float>> CreateEmptyData()
        {
            List<List<float>> emptyData = new List<List<float>>();
            for (int i = 0; i < TotalLeads; i++)
            {
                emptyData.Add(new List<float>());
            }
            return emptyData;
        }

        public List<byte[]> GetSampleStatusBits()
        {
            return sampleStatusBits;
        }
        private List<List<float>> CalculateDerivedLeads(List<List<float>> ecgData)
        {
            if (ecgData.Count < 8)
            {
                throw new ArgumentException("原始ECG数据必须包含至少8个导联（I, II, V1-V6）");
            }

            int dataLength = ecgData[0].Count;
            if (dataLength == 0)
            {
                return CreateEmptyData();
            }

            List<List<float>> extendedEcgData = new List<List<float>>(TotalLeads)
            {
                ecgData[LEAD_I_INDEX],
                ecgData[LEAD_II_INDEX]
            };

            List<float> leadIII = new List<float>(dataLength);
            List<float> leadAVR = new List<float>(dataLength);
            List<float> leadAVL = new List<float>(dataLength);
            List<float> leadAVF = new List<float>(dataLength);

            for (int i = 0; i < dataLength; i++)
            {
                float leadI = ecgData[LEAD_I_INDEX][i];
                float leadII = ecgData[LEAD_II_INDEX][i];

                leadIII.Add(leadII - leadI);
                leadAVR.Add(-(leadI + leadII) / 2.0f);
                leadAVL.Add(leadI - leadII / 2.0f);
                leadAVF.Add(leadII - leadI / 2.0f);
            }

            extendedEcgData.Add(leadIII);
            extendedEcgData.Add(leadAVR);
            extendedEcgData.Add(leadAVL);
            extendedEcgData.Add(leadAVF);

            extendedEcgData.Add(ecgData[LEAD_V1_INDEX]);
            extendedEcgData.Add(ecgData[LEAD_V2_INDEX]);
            extendedEcgData.Add(ecgData[LEAD_V3_INDEX]);
            extendedEcgData.Add(ecgData[LEAD_V4_INDEX]);
            extendedEcgData.Add(ecgData[LEAD_V5_INDEX]);
            extendedEcgData.Add(ecgData[LEAD_V6_INDEX]);

            return extendedEcgData;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                memoryMappedFile?.Dispose();
                memoryMappedFile = null;
                isDisposed = true;
            }
        }
    }
}
