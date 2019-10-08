using LDBizTagDefine;
using LDsdkDefineEx;
using MyLR.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyLR
{
    public class Compile
    {
        /// <summary>
        /// 用来存储SQL语句返回的查询结果的集合
        /// </summary>
        private Dictionary<string, DataTable> _dataTableCollection = new Dictionary<string, DataTable>();

        private Dictionary<string, BizFunction> _sendFuncList = new Dictionary<string, BizFunction>();
        private bool _exit;
        public string LogFileName { get; set; }
        public bool IsPreComile { get; set; }

        public Compile()
        {
          
            _exit = false;
            IsPreComile = true;
            LogFileName = @"log\ScriptLog.log";
        }

        /// <summary>
        /// 对脚本的语法合法性进行校验
        /// </summary>
        /// <param name="strBui">脚本文件</param>
        /// <returns></returns>
        public bool PreCompile(string[] strBui)
        {
            try
            {
                IsPreComile = true;
                _exit = false;
                bool bSuccess = true;

                for (int i = 0; i < strBui.Length; i++)
                {
                    Analys(strBui[i].Trim('\r'));
                }
                return bSuccess;
            }
            catch (Exception error)
            {
                throw new Exception($"{error.Message}\r\n{error.StackTrace}");
            }
        }
        /// <summary>
        /// 执行线性回归脚本案例
        /// </summary>
        /// <param name="strBui">案例</param>
        /// <param name="result">输出执行结果</param>
        public void Run(string[] strBui,ref string result)
        {
            try
            {
                _sendFuncList.Clear();
                _dataTableCollection.Clear();
                if (PreCompile(strBui))//先对脚本的合法性进行校验
                {
                    IsPreComile = false;
                    _exit = false;
                    ExcelHelper excel = new ExcelHelper(@"defaultvalue.xlsx");
                    DataTable defaultTable = excel.ExcelToDataTable("Sheet1", true);
                    for (int i = 0; i < strBui.Length; i++)
                    {
                        RunMsg(strBui[i].Trim('\r'), defaultTable);
                        if (_exit && !IsPreComile)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                result = $"{error.Message}\r\n{error.StackTrace}";
            }
        }
        /// <summary>
        /// 运行数据包？
        /// </summary>
        /// <param name="strBui"></param>
        /// <param name="defaultTable"></param>
        private void RunMsg(string strBui, DataTable defaultTable)
        {
            bool note = strBui.Trim().StartsWith("#");
            if (note)
            {
                return;
            }

            //if开头
            bool isIf = strBui.Trim().StartsWith("if ");
            if (isIf)
            {
                string[] strIf = strBui.Trim().Replace("if", "").Trim().Split(':');
                if (strIf.Count() != 2)
                {
                    throw new Exception("if 语句不符合规则");
                }
                string leftValue = "";
                string rightValue = "";
                //拿出条件判断式的左右两边数据，并且得到条件类型
                ConditionEnum conType = UtilTool.GetConditionRL(strIf[0], out leftValue, out rightValue);
                if (conType  == ConditionEnum.ConditionEnum_Unkown)
                {
                    throw new Exception("if 语句不符合规则");
                }

                if (ConditionIsTrue(leftValue, rightValue, conType))
                {
                    strBui = strIf[1];
                }
                else
                {
                    return;
                }
            }

            //set开头
            bool isSetValue = strBui.Trim().StartsWith("set");
            if (isSetValue)
            {
                string fieldValue = strBui.Split('=')[1].Trim();//等号右边值   这个值在哪里赋给变量的？？？？
                string fieldName = strBui.Split('=')[0].Split('.')[1].Trim();//入参名称
                //代表功能号的变量，类似funOrder.Answer().error_code中的funOrder
                string funVar = strBui.Split('=')[0].Split('.')[0].Replace("set", "").Trim();
                BizFunction fun = null;
                if (_sendFuncList.TryGetValue(funVar, out fun))
                {
                    BizRequestField bizReq= fun.ReqFields.FirstOrDefault(o=>o.FieldName.Equals(fieldName));
                    if (bizReq != null && bizReq.IsVarValue)
                    {
                        //请求参数的值是功能号返回的结果集的，要替换
                        BizFunction outFun = null;
                        if (_sendFuncList.TryGetValue(bizReq.VarFunValue, out outFun))
                        {
                            BizAnswerField bizAnaswer=outFun.Answer.RspFields.FirstOrDefault(o=>o.FieldName.Equals(bizReq.VarFieldName));
                            if (bizAnaswer !=null)
                            {
                                bizReq.FieldValue = bizAnaswer.FieldValue;
                            }
                        }
                    }

                    //郑建翔新增部分逻辑，之前的请求参数是变量只有可能是功能号返回的结果，现在
                    //有可能是SQL查询回来的结果        2019-09-30
                    if (bizReq != null && bizReq.IsSqlValue)
                    {
                        DataTable dt = null;
                        if (_dataTableCollection.TryGetValue(bizReq.DataTableName,out dt))
                        {
                            if (dt != null)
                            {
                                string columnName = fieldValue.Split('.')[1].Trim();
                                bizReq.FieldValue = dt.Rows[0][columnName].ToString();
                            }
                        }
                    }
                }
                return;
            }


            bool isSend = strBui.Trim().EndsWith("Send()");
            if (isSend)
            {
                string funVar = strBui.Split('.')[0].Trim();
                BizFunction fun = null;
                if (_sendFuncList.TryGetValue(funVar, out fun))
                {
                    //发送数据包
                    SendFastMsgFun(fun, defaultTable);
                }
                return;
            }
            bool isSleep = strBui.Trim().StartsWith("sleep");
            if (isSleep)
            {
                string funSleep = strBui.Split('=')[1].Trim();
                Thread.Sleep(int.Parse(funSleep));
                return;
            }

            //print开头
            bool isPrint = strBui.Trim().StartsWith("Print ");
            if (isPrint)
            {
                //打印到控制台
                string printMsg = strBui.Trim().Replace("Print", "").Trim();
                string[] printAry = printMsg.Split('&');
                StringBuilder printBu = new StringBuilder();
                for (int i=0;i<printAry.Count();i++)
                {
                    BizFunction fun;
                    DataTable dt = new DataTable();
                    string answerFieldName = GetAnswerFieldName(printAry[i],out fun);
                    if (fun == null)
                    {
                        if (_dataTableCollection.TryGetValue(printAry[i].Split('.')[0].Trim(),out dt))
                        {
                            printBu.Append(dt.Rows[0][printAry[i].Split('.')[1].Trim()].ToString());
                        }
                        else
                        {
                            printBu.Append(printAry[i].Trim('\''));
                        }
                    }
                    else
                    {
                       BizAnswerField bizAnswer =fun.Answer.RspFields.FirstOrDefault(o=>o.FieldName.Equals(answerFieldName));
                        if (bizAnswer == null)
                        {
                            printBu.Append(printAry[i].Trim('\''));
                        }
                        else
                        {
                            printBu.Append(bizAnswer.FieldValue);
                        }
                    }
                }
               // PrintHelper.Print.WriteLine(printBu.ToString());
                PrintHelper.Print.AppendLine(printBu.ToString());
                return;
            }
            bool isWriteLog = strBui.Trim().StartsWith("WriteLog");
            if (isWriteLog)
            {
                //打印到控制台
                string writeLogMsg = strBui.Trim().Replace("WriteLog", "").Trim();
                string[] printAry = writeLogMsg.Split('&');
               
                return;
            }
            if ("Exit".Equals(strBui.Trim()))
            {
                _exit = true;
                return;
            }

        }

        /// <summary>
        /// 发送请求数据包的方法
        /// </summary>
        /// <param name="fun">已经构建好的请求应答包</param>
        /// <param name="defaultTable">对应defaultvalue.xlsx文件，取默认值用的</param>
        private unsafe void SendFastMsgFun(BizFunction fun, DataTable defaultTable)
        {
            LDsdkDefineEx.LDFastMessageAdapter fastmsg = new LDFastMessageAdapter(fun.FunctonId, 0);
            LDRecordAdapter lpRecord = fastmsg.GetBizBodyRecord();
            //获取字段总数量
            int iFieldCount = lpRecord.GetFieldCount();
            int iIndex = 0;

            for (iIndex = 0; iIndex < iFieldCount; iIndex++)
            {
                //取得字段类型
                int fieldtype = lpRecord.GetFieldType(iIndex);
                //取得字段名称
                string fieldName = lpRecord.GetFieldNamebyIndex(iIndex);

                BizRequestField bizField=fun.ReqFields.FirstOrDefault(o=>o.FieldName.Equals(fieldName));
                String fieldValue = "";
                if (bizField != null)
                {
                    fieldValue = bizField.FieldValue; //取得入参的值      
                }
                if (string.IsNullOrEmpty(fieldValue)) //如果没有手动赋值，则去默认值
                {
                    fieldValue = UtilTool.GetDefaultValue(defaultTable, fieldName);
                }
                if ("func_code".Equals(fieldName))  //如果字段名称是func_code，则把功能号作为值赋给字段
                {
                    fieldValue = fun.FunctonId;
                }
                UtilTool.SetFastMsgValue(fastmsg, fieldtype, fieldValue, iIndex);
            }
            //应答包
            LDFastMessageAdapter outFast = null;
            int nRet=(int)ConnectionManager.Instance.CurConnection.Connect.SendAndReceive(fun.FunctonId, fastmsg, ref outFast, 5000);
            fastmsg.FreeMsg();
            /**********************************开始对应答包进行处理**************************************/
            if (nRet<0)
            {
                LogHelper.Logger.Error(fun.FunctonId+"发送失败，错误号=" + nRet);
                PrintHelper.Print.AppendLine(fun.FunctonId + "发送失败，错误号=" + nRet);
            }
            if (nRet>=0 && outFast.Record != null )
            {
                int iErrorNo = outFast.GetInt32(LDSdkTag.LDTAG_ERRORNO);
                if (iErrorNo != 0)
                {
                    LogHelper.Logger.Error("SystemErrorInfo="+outFast.GetString(LDSdkTag.LDTAG_ERRORINFO));
                }
                else
                {
                    //将回归脚本的应答包信息写入文件
                    //string savePath = Environment.CurrentDirectory + @"\RegressionTestResult.txt";//文件保存路径
                    //LogAnswerPackageInfo.OutPutResult(outFast, savePath);

                    if (fun.AnswerFlag)
                    {
                        //获取应答包包体数据
                        LDRecordAdapter lpOutRecord = outFast.GetBizBodyRecord();
                        //获取应答包字段总数
                        int iAnswerFieldCount = lpOutRecord.GetFieldCount();
                        //遍历获取应答包字段的 出参=值 信息，并塞入到对应的请求应答包里面去 BizFunction fun
                        int iAnswerIndex = 0;
                        for (iAnswerIndex = 0; iAnswerIndex < iAnswerFieldCount; iAnswerIndex++)
                        {
                            //获取出参字段名
                            string fieldName = lpOutRecord.GetFieldNamebyIndex(iAnswerIndex);
                            //获取出参字段类型
                            int fieldType = lpOutRecord.GetFieldType(iAnswerIndex);
                            //只把会用到的字段塞入
                            BizAnswerField bizField = fun.Answer.RspFields.FirstOrDefault(o => o.FieldName.Equals(fieldName));
                            if (bizField != null)
                            {
                                bizField.FieldValue = UtilTool.GetFastMsgValue(outFast, fieldType, iAnswerIndex);
                                bizField.FieldType = fieldType;
                            }
                        }
                    }
                }
                outFast.FreeMsg();
            }
        }

        /// <summary>
        /// 往BizFunction包里塞入应答包字段模板信息
        /// </summary>
        /// <param name="srcValue"></param>
        private void SetAnswerField(string srcValue)
        {
            if (!string.IsNullOrWhiteSpace(srcValue))
            {
                if (srcValue.Contains(".Answer()."))
                {
                    string funVarName = srcValue.Substring(0, srcValue.IndexOf(".Answer()."));//功能号变量
                    BizFunction fun = null;
                    if (_sendFuncList.TryGetValue(funVarName, out fun))
                    {
                        //需要的输出字段
                        string answerFiledName = srcValue.Substring(srcValue.IndexOf(".Answer().") + 10);
                        if (null ==fun.Answer.RspFields.FirstOrDefault(o=>o.FieldName.Equals(answerFiledName.Trim())))
                        {
                            fun.Answer.RspFields.Add(new BizAnswerField(answerFiledName.Trim(), "" ));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查找对应的应答结果集并获取值
        /// </summary>
        /// <param name="srcValue">格式类似 funOrder.Answer().error_code </param>
        /// <param name="fun">查找到的对应的功能号唯一标识的请求应答包</param>
        /// <returns>输出字段名称，类似error_code</returns>
        private string GetAnswerFieldName(string srcValue,out BizFunction fun)
        {
            fun = null;
            string answerFiledName = "";
            if (!string.IsNullOrWhiteSpace(srcValue))
            {
                if (srcValue.Contains(".Answer()."))
                {
                    //查找到funOrder这玩意儿
                    string funVarName = srcValue.Substring(0, srcValue.IndexOf(".Answer()."));//功能号变量
                    //根据funOrder到集合里找出对应的请求应答包
                    if (_sendFuncList.TryGetValue(funVarName, out fun))
                    {
                        //需要的输出字段名称，类似error_code
                        //靠参数的字段长度来定位并获取参数值，这他妈是什么神级操作？？？？？
                         answerFiledName = srcValue.Substring(srcValue.IndexOf(".Answer().") + 10).Trim();
                    }
                }
            }
            return answerFiledName;
        }

 
        //0和引用要它何用？
        private string GetRequestFieldName(string srcValue, out BizFunction fun)
        {
            fun = null;
            string requestFiledName = "";
            if (!string.IsNullOrWhiteSpace(srcValue))
            {
                if (srcValue.Contains(".Request()."))
                {
                    string funVarName = srcValue.Substring(0, srcValue.IndexOf(".Request()."));//功能号变量
                    if (_sendFuncList.TryGetValue(funVarName, out fun))
                    {
                        //需要的输出字段名称
                        requestFiledName = srcValue.Substring(srcValue.IndexOf(".Request().") + 11).Trim();
                    }
                }
            }
            return requestFiledName;
        }

        /// <summary>
        /// 逐行解析回归测试脚本
        /// </summary>
        /// <param name="strBui">回归测试脚本单条语句</param>
        private void Analys(string strBui)
        {
            //if开头
            bool isIf = strBui.Trim().StartsWith("if ");
            if (isIf)
            {
                //是否是注释信息
                bool note = strBui.Trim().StartsWith("#");
                if (note)
                {
                    return;
                }

                string[] strIf = strBui.Trim().Replace("if", "").Trim().Split(':');
                if (strIf.Count() != 2)
                {
                    throw new Exception("if 语句不符合规则");
                }
                string leftValue = "";
                string rightValue = "";
                if (UtilTool.GetConditionRL(strIf[0], out leftValue, out rightValue) == ConditionEnum.ConditionEnum_Unkown)
                {
                    throw new Exception("if 语句不符合规则");
                }
                //判断条件两变是否有输出字段
                SetAnswerField(leftValue);
                SetAnswerField(rightValue);
                strBui = strIf[1]; //if语句只取右边？
            }

            //Exit开头
            if ("Exit".Equals(strBui.Trim()))
            {
                _exit = true;
                return;
            }

            //Function开头
            bool isfun = strBui.Trim().StartsWith("Function");
            if (isfun)
            {
                //功能号
                string funID = strBui.Split('=')[1].Trim();
                //请求包的集合
                string funVar = strBui.Split('=')[0].Trim().Replace("Function", "").Trim();
                //构建了一个功能号唯一标识的请求应答包并塞进集合
                _sendFuncList.Add(funVar, new BizFunction(funID, funVar));
                return;
            }

            //set开头
            bool isSetValue = strBui.Trim().StartsWith("set");
            if (isSetValue)
            {
                //等号右边的值，如果该值是上一个功能号的返回结果集，如：funOrderBatchNo.Answer().curr_no
                //则需要进行特殊处理以获得真正的值
                string fieldValue = strBui.Split('=')[1].Trim();//参数对应的值
                string fieldName = strBui.Split('=')[0].Split('.')[1].Trim();//参数名
                string funVar = strBui.Split('=')[0].Split('.')[0].Replace("set", "").Trim();//代表功能号的变量

                BizFunction fun = null;
                if (_sendFuncList.TryGetValue(funVar, out fun))
                {
                    //构建一个请求包的 参数=值 的对象
                    BizRequestField bizField = new BizRequestField(fieldName, fieldValue);

                    BizFunction outFun = null;

                    //如上文提到的，set的=号右边是功能号的返回结果作为值，查找对应的应答结果集并获取值
                    //fieldValue 的格式类似 funOrder.Answer().error_code
                    string outFieldName = GetAnswerFieldName(fieldValue, out outFun);//outFieldName的格式：error_code

                    if (outFun != null && outFun.AnswerFlag)
                    {
                        //如果应答包里没有这个字段，则添加一个并赋空值
                        if (outFun.Answer.RspFields.FirstOrDefault(o=>o.FieldName.Equals(outFieldName))==null)
                        {
                            outFun.Answer.RspFields.Add(new BizAnswerField(outFieldName, " "));
                        }

                        //如果变量的值是前一个功能号调用返回的结果，则采用这种方法赋值
                        bizField.IsVarValue = true;
                        bizField.VarFieldName = outFieldName;//变量字段名 类似funOrder.Answer().error_code中的error_code
                        bizField.VarFunValue = outFun.VarName;//代表功能号的变量 类似funOrder.Answer().error_code中的funOrder
                    }

                    //如果set的 = 号右边是SQL的返回结果作为值，查找对应的结果集并取值
                    //此时的fieldValue 的格式类似 dt.ParamName
                    string dataTableName = fieldValue.Split('.')[0].Trim();
                    DataTable dt = null;
                    if (_dataTableCollection.TryGetValue(dataTableName,out dt))
                    {
                        if (dt != null)
                        {
                            bizField.IsSqlValue = true;
                            bizField.DataTableName = dataTableName;
                        }
                    }

                    fun.ReqFields.Add(bizField);
                }

                return;
            }

            //Send()结尾
            bool isSend = strBui.Trim().EndsWith("Send()");
            if (isSend)
            {
                string funVar = strBui.Split('.')[0].Trim();
                BizFunction fun = null;
                if (_sendFuncList.TryGetValue(funVar, out fun))
                {
                    fun.SendFlag = true;
                }
                return;

            }

            //GetAnswer()结尾
            bool isAnswer = strBui.Trim().EndsWith("GetAnswer()");
            if (isAnswer)
            {
                string funVar = strBui.Split('.')[0].Trim();
                BizFunction fun = null;
                if (_sendFuncList.TryGetValue(funVar, out fun))
                {
                    fun.AnswerFlag = true;
                }
                return;

            }

            //sleep开头
            bool isSleep = strBui.Trim().StartsWith("sleep");
            if (isSleep)
            {
                return;
            }

            //Print开头
            bool isPrint = strBui.Trim().StartsWith("Print ");
            if (isPrint)
            {
                //打印到控制台
                string printMsg = strBui.Trim().Replace("Print", "").Trim();
                string [] printAry=printMsg.Split('&');
                for (int i=0;i<printAry.Count();i++)
                {
                    SetAnswerField(printAry[i]);
                }
                return;
            }

            //WriteLog开头
            bool isWriteLog = strBui.Trim().StartsWith("WriteLog");
            if (isWriteLog)
            {
                //打印到控制台
                string writeLogMsg = strBui.Trim().Replace("WriteLog", "").Trim();
                string[] printAry = writeLogMsg.Split('&');
                for (int i = 0; i < printAry.Count(); i++)
                {
                    SetAnswerField(printAry[i]);
                }
                return;
            }

            //DataTable开头
            //处理逻辑：直接执行SQL语句返回结果集，并将返回的结果集放入_dataTableCollect中存储
            bool isDataTable = strBui.Trim().StartsWith("DataTable");
            if (isDataTable)
            {
                string dataTableName = strBui.Split(':')[0].Replace("DataTable", "").Trim();//DataTable表明
                string connStrKey = strBui.Split(':')[1].Split('|')[0].Trim();//IP对应的appSetting里的key值
                string sqlCommand = strBui.Split(':')[1].Split('|')[1].Trim();//sql命令
                DataTable dt = DBHelper.ExecuteDataTable(connStrKey,sqlCommand);
                if (dt != null)
                {
                    int count = dt.Rows.Count;
                    _dataTableCollection.Add(dataTableName,dt);
                }
                return;
            }

        }

        /// <summary>
        /// 验证if条件是否成立
        /// 左值可能是 funOrder.Answer().error_code 格式，也可能是DataTable.value格式
        /// </summary>
        /// <param name="leftData">左值</param>
        /// <param name="rightData">右值</param>
        /// <param name="conType">成立条件</param>
        /// <returns></returns>
        private bool ConditionIsTrue(string leftData,string rightData, ConditionEnum conType)
        {
            bool isTrue = false ;
            BizFunction leftFun = null;
            DataTable dt = new DataTable();
            int leftFieldType = LDSdkTag.TypeUnKnown;

            //如果数据格式是应答包格式，即funOrder.Answer().error_code，采用如下处理方式
            string leftAnswerFieldName = GetAnswerFieldName(leftData, out leftFun);
            if (leftFun != null)
            {
                BizAnswerField bizField= leftFun.Answer.RspFields.FirstOrDefault(o=>o.FieldName.Equals(leftAnswerFieldName));
                if (bizField != null)
                {
                    leftData = bizField.FieldValue;
                    leftFieldType = bizField.FieldType;
                }
            }

            //如果数据格式是SQL返回值格式，即DataTable.value，采用如下处理方式
            string[] leftDatas = leftData.Split('.');
            if (_dataTableCollection.TryGetValue(leftDatas[0].Trim(), out dt))
            {
                if (dt != null)
                {
                    leftData = dt.Rows[0][leftDatas[1]].ToString();
                    leftFieldType = LDSdkTag.TypeString;
                }
            }

            BizFunction rightFun = null;
            string rightAnswerFieldName = GetAnswerFieldName(leftData, out rightFun);
            int rightFieldType = LDSdkTag.TypeUnKnown;
            if (rightFun != null)
            {
                BizAnswerField bizField = rightFun.Answer.RspFields.FirstOrDefault(o => o.FieldName.Equals(rightAnswerFieldName));
                if (bizField != null)
                {
                    rightData = bizField.FieldValue;
                    rightFieldType = bizField.FieldType;
                }
            }

            if (string.IsNullOrWhiteSpace(leftData) || string.IsNullOrWhiteSpace(rightData))
            {
                return false;
            }
            switch (leftFieldType)
            {

                case LDSdkTag.TypeuInt16:
                case LDSdkTag.TypeuInt8:
                case LDSdkTag.TypeuInt32:
                case LDSdkTag.TypeuInt64:
                case LDSdkTag.TypeInt16:
                case LDSdkTag.TypeInt8:
                case LDSdkTag.TypeInt32:
                case LDSdkTag.TypeInt64:
                    //整数类型统一转为 int64比较
                    Int64 leftIntData = Int64.Parse(leftData);
                    #region
                    switch (rightFieldType)
                    {
                        case LDSdkTag.TypeuInt16:
                        case LDSdkTag.TypeuInt8:
                        case LDSdkTag.TypeuInt32:
                        case LDSdkTag.TypeuInt64:
                        case LDSdkTag.TypeInt16:
                        case LDSdkTag.TypeInt8:
                        case LDSdkTag.TypeInt32:
                        case LDSdkTag.TypeInt64:
                        case LDSdkTag.TypeString:
                        case LDSdkTag.TypeVector:
                        case LDSdkTag.TypeUnKnown:;
                            //整数类型统一转为 int64比较
                            Int64 rightIntData = 0;
                            Int64.TryParse(rightData,out rightIntData);
                            switch (conType)
                            {
                                case ConditionEnum.ConditionEnum_Eq:
                                    isTrue = leftIntData == rightIntData;
                                    break;
                                case ConditionEnum.ConditionEnum_Neq:
                                    isTrue = leftIntData != rightIntData;
                                    break;
                                case ConditionEnum.ConditionEnum_Great:
                                    isTrue = leftIntData > rightIntData;
                                    break;
                                case ConditionEnum.ConditionEnum_GreatOrEq:
                                    isTrue = leftIntData >= rightIntData;
                                    break;
                                case ConditionEnum.ConditionEnum_Less:
                                    isTrue = leftIntData < rightIntData;
                                    break;
                                case ConditionEnum.ConditionEnum_LessOrEq:
                                    isTrue = leftIntData <= rightIntData;
                                    break;

                            }
                            break;
                        case LDSdkTag.TypeDouble:
                            double rightDoubleData = double.Parse(rightData);
                            switch (conType)
                            {
                                case ConditionEnum.ConditionEnum_Eq:
                                    isTrue =Math.Abs( (leftIntData - rightDoubleData)) < 0.00001;
                                    break;
                                case ConditionEnum.ConditionEnum_Neq:
                                    isTrue = Math.Abs((leftIntData - rightDoubleData)) > 0.00001;
                                    break;
                                case ConditionEnum.ConditionEnum_Great:
                                    isTrue = leftIntData > rightDoubleData;
                                    break;
                                case ConditionEnum.ConditionEnum_GreatOrEq:
                                    isTrue = leftIntData >= rightDoubleData;
                                    break;
                                case ConditionEnum.ConditionEnum_Less:
                                    isTrue = leftIntData < rightDoubleData;
                                    break;
                                case ConditionEnum.ConditionEnum_LessOrEq:
                                    isTrue = leftIntData <= rightDoubleData;
                                    break;

                            }
                            break;                                                                      
                    }
                    #endregion
                    break;
                case LDSdkTag.TypeDouble:
                    double leftDoubleData = double.Parse(leftData);
                    #region
                    switch (rightFieldType)
                    {

                        case LDSdkTag.TypeuInt16:
                        case LDSdkTag.TypeuInt8:
                        case LDSdkTag.TypeuInt32:
                        case LDSdkTag.TypeuInt64:
                        case LDSdkTag.TypeInt16:
                        case LDSdkTag.TypeInt8:
                        case LDSdkTag.TypeInt32:
                        case LDSdkTag.TypeInt64:
                        case LDSdkTag.TypeString:
                        case LDSdkTag.TypeVector:
                        case LDSdkTag.TypeUnKnown:
                        case LDSdkTag.TypeDouble:
                            //整数类型统一转为 int64比较
                            double rightDoubleData = 0;
                            double.TryParse(rightData, out rightDoubleData);
                            switch (conType)
                            {
                                case ConditionEnum.ConditionEnum_Eq:
                                    isTrue =Math.Abs( leftDoubleData - rightDoubleData)<0.000001;
                                    break;
                                case ConditionEnum.ConditionEnum_Neq:
                                    isTrue = Math.Abs(leftDoubleData - rightDoubleData) > 0.000001;
                                    break;
                                case ConditionEnum.ConditionEnum_Great:
                                    isTrue = leftDoubleData > rightDoubleData;
                                    break;
                                case ConditionEnum.ConditionEnum_GreatOrEq:
                                    isTrue = leftDoubleData >= rightDoubleData;
                                    break;
                                case ConditionEnum.ConditionEnum_Less:
                                    isTrue = leftDoubleData < rightDoubleData;
                                    break;
                                case ConditionEnum.ConditionEnum_LessOrEq:
                                    isTrue = leftDoubleData <= rightDoubleData;
                                    break;

                            }
                            break;
                    }
                    #endregion
                    break;
                case LDSdkTag.TypeString:
                case LDSdkTag.TypeVector:
                case LDSdkTag.TypeUnKnown:
                    #region
                    switch (rightFieldType)
                    {
                        case LDSdkTag.TypeuInt16:
                        case LDSdkTag.TypeuInt8:
                        case LDSdkTag.TypeuInt32:
                        case LDSdkTag.TypeuInt64:
                        case LDSdkTag.TypeInt16:
                        case LDSdkTag.TypeInt8:
                        case LDSdkTag.TypeInt32:
                        case LDSdkTag.TypeInt64:
                        case LDSdkTag.TypeDouble:
                            leftDoubleData = 0;
                            double.TryParse(leftData, out leftDoubleData);
                            double rightDoubleData = 0;
                            double.TryParse(rightData, out rightDoubleData);
                            switch (conType)
                            {
                                case ConditionEnum.ConditionEnum_Eq:
                                    isTrue = Math.Abs(leftDoubleData - rightDoubleData) < 0.000001;
                                    break;
                                case ConditionEnum.ConditionEnum_Neq:
                                    isTrue = Math.Abs(leftDoubleData - rightDoubleData) > 0.000001;
                                    break;
                                case ConditionEnum.ConditionEnum_Great:
                                    isTrue = leftDoubleData > rightDoubleData;
                                    break;
                                case ConditionEnum.ConditionEnum_GreatOrEq:
                                    isTrue = leftDoubleData >= rightDoubleData;
                                    break;
                                case ConditionEnum.ConditionEnum_Less:
                                    isTrue = leftDoubleData < rightDoubleData;
                                    break;
                                case ConditionEnum.ConditionEnum_LessOrEq:
                                    isTrue = leftDoubleData <= rightDoubleData;
                                    break;

                            }
                            break;
                        default:
                            switch (conType)
                            {
                                case ConditionEnum.ConditionEnum_Eq:
                                    isTrue = leftData.Equals(rightData);
                                    break;
                                case ConditionEnum.ConditionEnum_Neq:
                                    isTrue = !leftData.Equals(rightData);
                                    break;
                                case ConditionEnum.ConditionEnum_Great:
                                    isTrue = leftData.CompareTo(rightData) > 0;
                                    break;
                                case ConditionEnum.ConditionEnum_GreatOrEq:
                                    isTrue = leftData.CompareTo(rightData) >= 0;
                                    break;
                                case ConditionEnum.ConditionEnum_Less:
                                    isTrue = leftData.CompareTo(rightData) < 0;
                                    break;
                                case ConditionEnum.ConditionEnum_LessOrEq:
                                    isTrue = leftData.CompareTo(rightData) <= 0;
                                    break;

                            }
                            break;

                    }
                    #endregion
                    break;
            }

            return isTrue;
        }
    }
}
