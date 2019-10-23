using LDBizTagDefine;
using LDsdkDefineEx;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR
{
    public enum ConditionEnum
    {
        ConditionEnum_Eq,
        ConditionEnum_Neq,
        ConditionEnum_Great,
        ConditionEnum_GreatOrEq,
        ConditionEnum_Less,
        ConditionEnum_LessOrEq,
        ConditionEnum_Unkown

    }
    public class UtilTool
    {
        public static String GetDefaultValue1(DataTable defaultTable,string filedName)
        {
            String filedValue = "";
            int columnIndex = defaultTable.Columns["字段"].Ordinal;
            int targer = defaultTable.Columns["参数值"].Ordinal;
            for (int i = 0;i < defaultTable.Rows.Count;i++)
            {
                if(defaultTable.Rows[i][columnIndex].ToString() == filedName)
                {
                    filedValue = defaultTable.Rows[i][targer].ToString();
                }
            }
            return filedValue;
        }
        /// <summary>
        /// 对字段进行赋默认值操作
        /// </summary>
        /// <param name="defaultTable">默认值集合，本程序对应defaultvalue.xlsx文件</param>
        /// <param name="filedName">字段名</param>
        /// <returns></returns>
        public static String GetDefaultValue(DataTable defaultTable, string filedName)
        {
            String filedValue = "";
            DataRow dr =  defaultTable.Select("字段='"+ filedName+"'")[0];
            int targer = defaultTable.Columns["参数值"].Ordinal;
            filedValue = dr[targer].ToString();
            return filedValue;
        }

        /// <summary>
        /// 获取应答包内的字段和值的信息
        /// </summary>
        /// <param name="fastmsg">应答数据包</param>
        /// <param name="filedType">字段类型</param>
        /// <param name="filedIndex">字段索引</param>
        /// <returns></returns>
        public static string GetFastMsgValue(LDFastMessageAdapter fastmsg, int filedType, int filedIndex)
        {
            string filedValue = "";
            switch (filedType)
            {
                case LDSdkTag.TypeInt16:
                case LDSdkTag.TypeuInt16:
                case LDSdkTag.TypeInt32:
                case LDSdkTag.TypeuInt32:
                    Int32 intValue = 0;
                 
                    intValue =(int) fastmsg.GetInt32byIndex(filedIndex);
                    filedValue = intValue.ToString();
                    break;
                case LDSdkTag.TypeInt64:
                case LDSdkTag.TypeuInt64:
                    Int64 longValue = 0;
                    longValue = (Int64)fastmsg.GetInt64byIndex(filedIndex);
                    filedValue = longValue.ToString();
                    break;
                case LDSdkTag.TypeDouble:
                    double doubleValue = 0;
                    doubleValue =fastmsg.GetDoublebyIndex(filedIndex);
                    filedValue = doubleValue.ToString();
                    break;
                case LDSdkTag.TypeString:
                case LDSdkTag.TypeVector:

                    filedValue = fastmsg.GetStringbyIndex(filedIndex);
                    break;
                default:
                    break;

            }
            return filedValue;
        }
        /// <summary>
        /// 根据字段在模板中的索引进行赋值构建请求数据包
        /// </summary>
        /// <param name="fastmsg">请求包</param>
        /// <param name="filedType">字段类型</param>
        /// <param name="filedValue">字段值</param>
        /// <param name="filedIndex">字段在模板中的索引</param>
        public static void SetFastMsgValue(LDFastMessageAdapter fastmsg, int filedType, string filedValue, int filedIndex)
        {
            switch (filedType)
            {
                case LDSdkTag.TypeInt16:
                case LDSdkTag.TypeuInt16:
                case LDSdkTag.TypeInt32:
                case LDSdkTag.TypeuInt32:
                    Int32 intValue = 0;
                    Int32.TryParse(filedValue, out intValue);
                    fastmsg.SetInt32byIndex(filedIndex, intValue);
                    //fastmsg.SetInt32(filedIndex, intValue);
                    break;
                case LDSdkTag.TypeInt64:
                case LDSdkTag.TypeuInt64:
                    Int64 longValue = 0;
                    Int64.TryParse(filedValue, out longValue);
                    fastmsg.SetInt64byIndex(filedIndex, (ulong)longValue);
                    break;
                case LDSdkTag.TypeDouble:
                    double doubleValue = 0;
                    double.TryParse(filedValue, out doubleValue);
                    fastmsg.SetDoublebyIndex(filedIndex, doubleValue);
                    break;
                case LDSdkTag.TypeString:
                case LDSdkTag.TypeVector:
                    if (string.IsNullOrEmpty(filedValue))
                    {
                        filedValue = " ";
                    }
                    fastmsg.SetStringbyIndex(filedIndex, filedValue);
                    break;
                default:
                    break;

            }
        }
        /// <summary>
        /// 根据字段的FieldId进行构建数据包
        /// </summary>
        /// <param name="fastmsg"></param>
        /// <param name="filedType"></param>
        /// <param name="filedValue"></param>
        /// <param name="filedIndex"></param>
        public static void SetFastMsgValueById(LDFastMessageAdapter fastmsg, int filedType, string filedValue, int fieldId)
        {
            switch (filedType)
            {
                case LDSdkTag.TypeInt16:
                case LDSdkTag.TypeuInt16:
                case LDSdkTag.TypeInt32:
                case LDSdkTag.TypeuInt32:
                    Int32 intValue = 0;
                    Int32.TryParse(filedValue, out intValue);
                    //fastmsg.SetInt32byIndex(filedIndex, intValue);
                    fastmsg.SetInt32(fieldId, intValue);
                    break;
                case LDSdkTag.TypeInt64:
                case LDSdkTag.TypeuInt64:
                    Int64 longValue = 0;
                    Int64.TryParse(filedValue, out longValue);
                    fastmsg.SetInt64(fieldId, (ulong)longValue);
                    break;
                case LDSdkTag.TypeDouble:
                    double doubleValue = 0;
                    double.TryParse(filedValue, out doubleValue);
                    fastmsg.SetDouble(fieldId, doubleValue);
                    break;
                case LDSdkTag.TypeString:
                case LDSdkTag.TypeVector:
                    if (string.IsNullOrEmpty(filedValue))
                    {
                        filedValue = " ";
                    }
                    fastmsg.SetString(fieldId, filedValue);
                    break;
                default:
                    break;

            }
        }
        /// <summary>
        /// 取条件语句的左右表达式
        /// </summary>
        public static ConditionEnum GetConditionRL(string strSrc, out string leftValue, out string rightValue)
        {
            ConditionEnum conditionType = ConditionEnum.ConditionEnum_Unkown;
            leftValue = "";
            rightValue = "";
            if (strSrc.Contains("=="))
            {

                leftValue = strSrc.Substring(0, strSrc.IndexOf("==")).Trim();
                rightValue = strSrc.Substring(strSrc.IndexOf("==")+2).Trim();
                conditionType = ConditionEnum.ConditionEnum_Eq;
            }
            else if (strSrc.Contains("!="))
            {
                leftValue = strSrc.Substring(0, strSrc.IndexOf("!=")).Trim();
                rightValue = strSrc.Substring(strSrc.IndexOf("!=")+2).Trim();
                conditionType = ConditionEnum.ConditionEnum_Neq;
            }
            else if (strSrc.Contains(">="))
            {
                leftValue = strSrc.Substring(0, strSrc.IndexOf(">=")).Trim();
                rightValue = strSrc.Substring(strSrc.IndexOf(">=")+2).Trim();
                conditionType = ConditionEnum.ConditionEnum_GreatOrEq;
            }
            else if (strSrc.Contains(">"))
            {
                leftValue = strSrc.Substring(0, strSrc.IndexOf(">")).Trim();
                rightValue = strSrc.Substring( strSrc.IndexOf(">")+1).Trim();
                conditionType = ConditionEnum.ConditionEnum_Great;
            }
            else if (strSrc.Contains("<="))
            {
                leftValue = strSrc.Substring(0, strSrc.IndexOf("<=")).Trim();
                rightValue = strSrc.Substring(strSrc.IndexOf("<=") + 2).Trim();
                conditionType = ConditionEnum.ConditionEnum_LessOrEq;
            }
            else if (strSrc.Contains("<"))
            {
                leftValue = strSrc.Substring(0, strSrc.IndexOf("<")).Trim();
                rightValue = strSrc.Substring(strSrc.IndexOf("<") + 1).Trim();
                conditionType = ConditionEnum.ConditionEnum_Less;
            }
          
            return conditionType;
        }
    }
}
