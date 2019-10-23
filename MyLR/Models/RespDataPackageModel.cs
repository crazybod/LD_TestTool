using LDsdkDefineEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR.Models
{
    /// <summary>
    /// 应答包数据模板
    /// </summary>
    public class RespDataPackageModel
    {
        public RespDataPackageModel(int send, LDFastMessageAdapter fastMsg)
        {
            hSend = send;
            lpFastMsg = fastMsg;
        }
        public RespDataPackageModel() { }
        /// <summary>
        /// 应答包唯一句柄
        /// </summary>
        internal int hSend;
        /// <summary>
        /// 应答数据包
        /// </summary>
        internal LDFastMessageAdapter lpFastMsg;
        /// <summary>
        /// 该队列用于暂存应答包的数据进行处理
        /// </summary>
        internal static ConcurrentQueue<RespDataPackageModel> DataQueue = new ConcurrentQueue<RespDataPackageModel>();
    }
}
