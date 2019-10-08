using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR
{
    public class BizBaseField
    {
        public BizBaseField(string fieldName, string fieldValue)
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }
        /// <summary>
        /// 变量名
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 变量值
        /// </summary>
        public string FieldValue { get; set; }
    }
}
