using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR.Models
{
    /// <summary>
    /// 应答频率模板
    /// </summary>
    public class ReplyRateModel : BindableBase
    {
        public ReplyRateModel(string funcId, DateTime timeStamp)
        {
            functionId = funcId;
            startTime = timeStamp;
            _replyCount = 1;
            _maxRate = 0;
            _minRate = 0;
            _currentRate = 0;
        }

        private string _functionId;
        /// <summary>
        /// 功能号ID
        /// </summary>
        public string functionId
        {
            get
            {
                return _functionId;
            }
            set
            {
                this.SetProperty(ref _functionId, value);
            }
        }

        private long _currentRate;
        /// <summary>
        /// 当前应答频率
        /// </summary>
        public long currentRate
        {
            get
            {
                return _currentRate;
            }
            set
            {
                this.SetProperty(ref _currentRate, value);
            }
        }

        private long _maxRate;
        /// <summary>
        /// 最大应答频率
        /// </summary>
        public long maxRate
        {
            get
            {
                return _maxRate;
            }
            set
            {
                this.SetProperty(ref _maxRate, value);
            }
        }

        private long _minRate;
        /// <summary>
        /// 最小应答频率
        /// </summary>
        public long minRate
        {
            get
            {
                return _minRate;
            }
            set
            {
                this.SetProperty(ref _minRate, value);
            }
        }

        /// <summary>
        /// 第一次接收到应答的时间
        /// </summary>
        public DateTime startTime { get; set; }
        
        /// <summary>
        /// 最新一次的时间戳
        /// </summary>
        public DateTime timeStamp { get; set; }

        private long _replyCount;
        /// <summary>
        /// 总共接收到应答的数量
        /// </summary>
        public long replyCount
        {
            get
            {
                return _replyCount;
            }
            set
            {
                this.SetProperty(ref _replyCount, value);
                //当数据更新时，同步更新当前频率、最大频率、最小频率
                //在此处进行更新，就可以做到只要保证replyCount的递增是原子性时，频率的变化也必然是原子性的
                if (replyCount < 80)
                {
                    return;
                }
                //计算出毫秒级的时间差
                int timeSpan = Convert.ToInt32((timeStamp - startTime).TotalMilliseconds);
                if (timeSpan > 0)
                {
                    //因为是毫秒级的时间差，故要乘以1000转化成秒级，即：条/秒
                    currentRate = 1000 * replyCount / timeSpan;
                    if (replyCount == 80)
                    {
                        minRate = 1000 * replyCount / timeSpan;
                        maxRate = 1000 * replyCount / timeSpan;
                    }
                    else
                    {
                        if (currentRate > maxRate)
                        {
                            maxRate = currentRate;
                        }
                        if (currentRate < minRate)
                        {
                            minRate = currentRate;
                        }
                    }
                }
            }
        }
    }
}
