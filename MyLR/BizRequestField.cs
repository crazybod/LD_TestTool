using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR
{
    public class BizRequestField : BizBaseField
    {
        public BizRequestField(string fieldName, string fieldValue) : base(fieldName, fieldValue)
        {
            IsVarValue = false;
        }
        public bool IsVarValue { get; set; }
        public string VarFunValue { get; set; }
        public string VarFieldName { get; set; }
    }
}
