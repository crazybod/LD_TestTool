using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR
{
    /// <summary>
    /// 同时保存了请求包的模压和应答包的模板
    /// 以及数据信息
    /// </summary>
    public class BizFunction
    {
        public BizFunction(string funId, string varName)
        {
            Sleep = 0;
            FunctonId = funId;
            VarName = varName;
            SendFlag = false;
            AnswerFlag = false;
            ReqFields = new List<BizRequestField>();
            Answer = new BizAnswer();
        }
        public void Clear()
        {
            ReqFields.Clear();
        }
        /// <summary>
        /// 变量名称(指的是该次请求的唯一标识，即功能号左边的变量)
        /// </summary>
        public string VarName { get; set; }
        public int Sleep { get; set; }
        /// <summary>
        /// 功能号
        /// </summary>
        public string FunctonId { get; set; }
        /// <summary>
        /// 发送标志 
        /// </summary>
        public bool SendFlag { get; set; }
        /// <summary>
        /// 应答标志
        /// </summary>
        public bool AnswerFlag { get; set; }

        /// <summary>
        /// 入参集合
        /// </summary>
        public List<BizRequestField> ReqFields { get; set; }

        /// <summary>
        /// 应答包
        /// </summary>
        public BizAnswer Answer {get;set;}



    }
}
