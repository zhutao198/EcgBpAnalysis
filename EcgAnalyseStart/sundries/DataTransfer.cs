using System.Net;

namespace EcgAnalyseStart
{
    public class DataTransfer
    {
        protected const int REQUEST_TIMEOUT = 36000000;   //设置获得响应的超时时间（10小时）

        /// <summary>
        /// 已上传的字节块数
        /// </summary>
        public int TransferredBlockCount { get; set; }

        /// <summary>
        /// 已上传的字节数
        /// </summary>
        public long TransferredLength { get; set; }

        /// <summary>
        /// 需上传的总字节数
        /// </summary>
        public long TotalContentLength { get; set; }

        /// <summary>
        /// 上传的文件总个数
        /// </summary>
        public int FileNum { get; set; }

        /// <summary>
        /// 是否已开始上传
        /// </summary>
        public bool IsTransferStarted { get; set; }

        ///// <summary>
        ///// 当前会话待上传的总字节数
        ///// </summary>
        //public long SessionContentLength { get; set; }

        /// <summary>
        /// 上传的目标地址(远端)
        /// </summary>
        public string DestinationAddress { get; set; }

        public HttpWebRequest TransferRequest { get; set; } //HTTP文件传输类

        /// <summary>
        /// 上传前数据处理占用的基础(上传)百分比
        /// </summary>
        public int BaseOffset { get; set; }

        /// <summary>
        /// 本程序内数据处理及上传的百分比总权重(相对于100%)
        /// </summary>
        public int TotalPercent { get; set; } = 100;

        public bool TransferCompleted { get; set; }

        private int transferredPercent = 0;
        public int TransferredPercent
        {
            get
            {
                if (IsTransferStarted)
                {
                    if (TransferCompleted)
                    {
                        transferredPercent = TotalPercent;
                    }
                    else if (TotalContentLength == 0)
                    {
                        transferredPercent = 0;
                    }
                    else
                    {
                        double absolutePercent = BaseOffset + (100d - BaseOffset) * (TransferredLength / TotalContentLength);
                        transferredPercent = (int)(absolutePercent / 100 * TotalPercent);
                    }
                }
                return transferredPercent;
            }
            set { transferredPercent = value; }
        }

        private bool isTranferCancelled = false;
        public bool IsCancelled
        {
            get => isTranferCancelled;
            set
            {
                isTranferCancelled = value;
                if (TransferRequest != null)
                {
                    if (TransferRequest.HaveResponse == false)
                    {
                        TransferRequest.Abort();
                    }
                }
            }
        }

        /// <summary>
        /// 开始上传
        /// </summary>
        /// <param name="totalLength"></param>
        public void StartTransfer(long totalLength)
        {
            this.TotalContentLength = totalLength;
            IsTransferStarted = true;
        }

        /// <summary>
        /// 重置上传状态, 准备开始上传
        /// </summary>
        public void Reset()
        {
            TransferredLength = 0;
            TransferredBlockCount = 0;
            TotalContentLength = 0;
            isTranferCancelled = false;
        }
    }
}
