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
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }
}
