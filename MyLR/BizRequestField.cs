using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR
{
    /// <summary>
    /// 用来表示入参的模板 参数=值
    /// </summary>
    public class BizRequestField : BizBaseField
    {
        public BizRequestField(string fieldName, string fieldValue) : base(fieldName, fieldValue)
        {
            IsVarValue = false;
            IsSqlValue = false;
        }
        /// <summary>
        /// 是否是功能号返回的结果作为参数值
        /// </summary>
        public bool IsVarValue { get; set; }
        /// <summary>
        /// 是否是SQL语句返回的结果作为参数值
        /// </summary>
        public bool IsSqlValue { get; set; }
        /// <summary>
        /// 代表功能号的变量 类似funOrder.Answer().error_code中的funOrder
        /// </summary>
        public string VarFunValue { get; set; }
        /// <summary>
        /// 变量字段名 类似funOrder.Answer().error_code中的error_code
        /// </summary>
        public string VarFieldName { get; set; }
        /// <summary>
        /// 如果入参的值是SQL的返回结果，此属性表示与其绑定的DataTable表名
        /// </summary>
        public string DataTableName { get; set; }
    }
}
