using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyLR.HqReplay
{
    public class StockInfo : BindableBase
    {
        public StockInfo(string stockType,string stockNo)
        {
            StockType = stockType;
            StockNo = stockNo;
        }
        /// <summary>
        /// 证券类型
        /// </summary>
        private string _StockType;
        public string StockType
        {
            get
            {
                return this._StockType;
            }
            set
            {
                SetProperty(ref this._StockType, value);
            }
        }
        /// <summary>
        /// 券码
        /// </summary>
        private string _StockNo;
        public string StockNo
        {
            get
            {
                return this._StockNo;
            }
            set
            {
                SetProperty(ref this._StockNo, value);
            }
        }
    }
}
