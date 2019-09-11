using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR
{
    public class BizAnswerField : BizBaseField
    {
        public BizAnswerField(string fieldName, string fieldValue) : base(fieldName, fieldValue)
        {
        }
        public int FieldType { get; set; }
    }
}
