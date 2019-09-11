using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR
{

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
        /// 变量名称
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

        public List<BizRequestField> ReqFields { get; set; }

        public BizAnswer Answer {get;set;}



    }
}
