using Amib.Threading;
using LDBizTagDefine;
using LDsdkDefineEx;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using System.Timers;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using MyLR.HqReplay;
using System.Windows.Forms;
using MyLR.Models;
using System.Windows.Markup;
using MyLR.Utils;

namespace MyLR
{
   
    public class MainViewModel : BindableBase
    {
        private static readonly object ReqLock = new object();
        /// <summary>
        /// 更新ReplyRateInfos集合时使用的锁
        /// </summary>
        private static readonly object lockFlag = new object();
        /// <summary>
        /// 对ReplyRateInfos内的元素进行递增更新时使用的锁
        /// </summary>
        private static readonly object addFlag = new object();
        List<ReqDTO> ReqAllList { get; set; }
        private System.Timers.Timer timer = new System.Timers.Timer();
        private static bool IsWork;
        private static int inTimer = 0;
        private List<CaseFileData> caseList = new List<CaseFileData>();
        /// <summary>
        /// 压力测试案例总目录路径
        /// </summary>
        private static string caseFilePath = "case.xml";

        private ObservableCollection<CaseFunction> _CaseFunctions;
        public ObservableCollection<CaseFunction> CaseFunctions
        {
            get { return _CaseFunctions; }
            set { this.SetProperty(ref this._CaseFunctions, value); }
        }

        /// <summary>
        /// 证券信息集合
        /// </summary>
        private ObservableCollection<StockInfo> _StockInfos;
        public ObservableCollection<StockInfo> StockInfos {
            get
            {
                return _StockInfos;
            }
            set
            {
                SetProperty(ref _StockInfos, value);
            }
        }

        
        private ObservableCollection<ReplyRateModel> _ReplyRateInfos;
        /// <summary>
        /// 应答频率集合
        /// </summary>
        public ObservableCollection<ReplyRateModel> ReplyRateInfos
        {
            get
            {
                if (_ReplyRateInfos == null)
                {
                    _ReplyRateInfos = new ObservableCollection<ReplyRateModel>();
                }
                return _ReplyRateInfos;
            }
            set
            {
                SetProperty(ref _ReplyRateInfos, value);
            }
        }

        #region 显示项目
        private ObservableCollection<ProjectInfo> _ProjectInfos;
        /// <summary>
        /// 项目信息集合
        /// </summary>
        public ObservableCollection<ProjectInfo> ProjectInfos
        {
            get
            {
                if (_ProjectInfos == null)
                {
                    _ProjectInfos = new ObservableCollection<ProjectInfo>();
                }
                return _ProjectInfos;
            }
            set
            {
                SetProperty(ref _ProjectInfos, value);
            }
        }

        /// <summary>
        /// 选中项目
        /// </summary>
        private ProjectInfo _SelectProject;
        public ProjectInfo SelectProject
        {
            get
            {
                if (_SelectProject != null)
                {
                    LoadScriptInfo(_SelectProject.FullPath);
                }

                return _SelectProject;
            }
            set
            {
                SetProperty(ref this._SelectProject, value);
            }

        }

        /// <summary>
        /// 加载项目下的案例信息
        /// </summary>
        /// <param name="path">项目文件夹路径</param>
        public void LoadScriptInfo(string path)
        {
            PrintHelper.Print.PrintData = printData;
            try
            {
                //先清空原来的信息
                CaseScripts.Clear();
                //加载案例脚本信息
                DirectoryInfo scriptInfos = new DirectoryInfo(path);
                foreach (var scriptInfo in scriptInfos.GetFiles())
                {
                    if (scriptInfo.Extension.ToLower() != ".script")
                    {
                        continue;
                    }
                    string caseName = scriptInfo.Name;
                    string caseFullPath = scriptInfo.FullName;
                    CaseScripts.Add(new CaseScript(caseName, caseFullPath));
                }
            }
            catch (Exception error)
            {
                PrintHelper.Print.AppendLine(error.Message + "\r\n" + error.StackTrace);
            }
        }
        #endregion

        private CaseGroup _SelectCase;
        public CaseGroup SelectCase
        {
            get { return _SelectCase; }
            set
            {
                SetProperty(ref this._SelectCase, value);
                if (value != null)
                {
                    CaseFunctions = new ObservableCollection<CaseFunction>(value.FunList);
                }
            }

        }

        #region 显示案例脚本
        /// <summary>
        /// 选中脚本
        /// </summary>
        private CaseScript _SelectScript;
        public CaseScript SelectScript
        {
            get
            {
                if (_SelectScript == null)
                {
                    _SelectScript = new CaseScript();
                }
                return _SelectScript;
            }
            set
            {
                SetProperty(ref this._SelectScript, value);
                LoadScript(_SelectScript.FullPath);
            }

        }

        private ObservableCollection<CaseScript> _CaseScripts = new ObservableCollection<CaseScript>();
        /// <summary>
        /// 与DataGrid绑定的案例集合
        /// </summary>
        public ObservableCollection<CaseScript> CaseScripts
        {
            get
            {
                if (_CaseScripts == null)
                {
                    _CaseScripts = new ObservableCollection<CaseScript>();
                }
                return _CaseScripts;
            }
            set { this.SetProperty(ref this._CaseScripts, value); }
        }
        #endregion


        private ObservableCollection<CaseGroup> _CaseGroups = new ObservableCollection<CaseGroup>();
        public ObservableCollection<CaseGroup> CaseGroups
        {
            get { return _CaseGroups; }
            set { this.SetProperty(ref this._CaseGroups, value); }
        }
        private Dispatcher Dispatcher{get;set;}
        public DataTable defaultTable { get; set; }

        public MainViewModel(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            ConnectionManager.Instance.NewConnection("config", 10000);
            ConnectionManager.Instance.ConnectionManagerOnReceivedEx += Instance_ConnectionManagerOnReceivedEx;
            InitCase();
            InitStockInfo();
            #region 定时器
            timer.AutoReset = true;
            timer.Interval = 400;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            #endregion

        }

        public MainViewModel()
        {

        }

        /// <summary>
        /// 初始化证券信息
        /// </summary>
        public void InitStockInfo()
        {
            StockInfos = new ObservableCollection<StockInfo>();
            if (LoadStockInfo.excelData.Rows.Count > 0)
            {
                for (int i = 0; i < LoadStockInfo.excelData.Rows.Count; i++)
                {
                    string stockType = "";
                    string stockNo = LoadStockInfo.excelData.Rows[i]["stock_code"].ToString();
                    string exchangeNo = LoadStockInfo.excelData.Rows[i]["exch_no"].ToString();
                    switch (exchangeNo)
                    {
                        case "1":
                            stockType = "上海";
                            break;
                        case "2":
                            stockType = "深圳";
                            break;
                        case "3":
                            stockType = "沪港通";
                            break;
                        case "4":
                            stockType = "深港通";
                            break;
                    }
                    StockInfos.Add(new StockInfo(stockType, stockNo));
                }
            }
        }

        public void InitCase()
        {
            DoCaseLoad();
            IsWork = false;
            ReqAllList = new List<ReqDTO>();
            //LoadScript();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Interlocked.Exchange(ref inTimer, 1) == 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (var data in caseList)
                    {
                        CaseGroup caseGroup=CaseGroups.FirstOrDefault(o=>o.ID == data.ID);
                        if (caseGroup != null)
                        {
                            caseGroup.SendOkCount = data.SendOkCount;
                            caseGroup.SendFailCount = data.SendFailCount;
                            caseGroup.RecFailCount = data.RecFailCount;
                            caseGroup.RecOkCount = data.RecOkCount;
                        }
                    }
                }));
                Interlocked.Exchange(ref inTimer, 0);
            }
        }

        System.Windows.Controls.RichTextBox printData;

        public void SetRichTextBox(System.Windows.Controls.RichTextBox printData)
        {
            this.printData = printData;
        }
       
        /// <summary>
        /// 接收并处理应答的函数
        /// </summary>
        /// <param name="connection">连接</param>
        /// <param name="hSend">句柄</param>
        /// <param name="lpFastMsg">应答包</param>
        /// <param name="result">结果</param>
        private void Instance_ConnectionManagerOnReceivedEx(Connection connection, int hSend, LDFastMessageAdapter lpFastMsg, int result)
        {
            DateTime timeStamp = DateTime.Now;
            ReqDTO req = null;
            lock (ReqLock)
            {
                req = ReqAllList.FirstOrDefault(o => o.SendID == hSend);
                ReqAllList.Remove(req);
            }
            if (req != null && lpFastMsg !=null)
            {
                //功能号
                string strFunCode = lpFastMsg.GetString(LDSdkTag.LDTAG_FUNCID);

                /***********************************郑建翔新加部分，用于计算应答频率***************************************/
                lock (lockFlag)
                {
                    //如果集合中不存这个功能号则添加
                    if (ReplyRateInfos.Count(i => i.functionId == strFunCode) == 0)
                    {
                        DateTime startTime = DateTime.Now;
                        this.Dispatcher.Invoke(() => ReplyRateInfos.Add(new ReplyRateModel(strFunCode, startTime)));
                        /*****************************接收到每个功能号的第一条应答时，保存应答信息**************************/
                        //文件保存路径
                        string savePath = Environment.CurrentDirectory + @"\ScriptOutPut.txt";
                        LogAnswerPackageInfo.OutPutResult(lpFastMsg, savePath);
                    }
                    else
                    {
                        //如果集合中已经存在这个功能号，开始更新并计算数据
                        ReplyRateModel replyInfo = ReplyRateInfos.FirstOrDefault(i => i.functionId == strFunCode);
                        replyInfo.timeStamp = timeStamp;
                        replyInfo.replyCount++;
                    }
                }
                /*******************************************修改结束*********************************************************/

                //Console.WriteLine("Connect_OnReceivedEx  " + strFunCode);
                Interlocked.Increment(ref req.CaseData.RecCount);
                int iErrorNo=lpFastMsg.GetInt32(LDSdkTag.LDTAG_ERRORNO);
                if (iErrorNo != 0)
                {
                    Interlocked.Increment(ref req.CaseData.RecFailCount);
                }
                else
                {
                    string strErrorCode = lpFastMsg.GetString(LDBizTag.LDBIZ_ERROR_CODE_STR);
                   
                    if (!"0".Equals(strErrorCode))
                    {
                        Interlocked.Increment(ref req.CaseData.RecFailCount);
                        string strErrorInfo = lpFastMsg.GetString(LDBizTag.LDBIZ_ERROR_INFO_STR);
                        string strErrorPrompt = lpFastMsg.GetString(LDBizTag.LDBIZ_ERROR_PROMPT_STR);
                        string logInfo = string.Format("功能号[{0}]，errorCode[{1}]，errorinfo[{2}],errorprompt[{3}]", strFunCode, strErrorCode, strErrorInfo, strErrorPrompt);
                        LogHelper.Logger.Error(logInfo);
                    }
                    else
                    {
                        Interlocked.Increment(ref req.CaseData.RecOkCount);
                    }
                    if (req.CaseData.SendCount - req.CaseData.RecCount >= req.CaseData.DownStage)
                    {
                        req.CaseData.ResetEvent.Set();
                    }
                }
              
            }
            if (lpFastMsg !=null)
            {
                lpFastMsg.FreeMsg();
            }
        }

        
        /// <summary>
        /// 加载案例详细脚本信息
        /// </summary>
        /// <param name="filePath">案例脚本保存路径</param>
        /// <returns></returns>
        private List<CaseFunction> LoadCaseFunction(String filePath)
        {
            List<CaseFunction> list = new List<CaseFunction>();

            try
            {
                if (!File.Exists(filePath))
                {
                    return list;
                }
                XElement rootXML = XDocument.Load(filePath).Element("bizmsgs");
                var funsXml = rootXML.Elements("function");
                foreach (var item in funsXml)
                {
                    string functionid = item.Attribute("functionid").Value;
                    string packettype = item.Attribute("packettype").Value;
                    if (packettype.Equals("0"))
                    {
                        string functionname = item.Attribute("functionname").Value;
                        CaseFunction fun = new CaseFunction()
                        {
                            FunID = functionid,
                            FunType = 0,
                            Check = 1,
                            FunName = functionname
                        };

                        list.Add(fun);
                    }
                }
                return list;
            }
            catch (Exception error)
            {
                this.Dispatcher.Invoke(() => { MessageBox.Show(error.Message + "\r\n" + error.StackTrace, "络町"); });
                return list;
            }

        }

        /// <summary>
        /// 加载案例数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="funList"></param>
        /// <returns></returns>
        public  CaseFileData LoadCaseData(String filePath, List<CaseFunction> funList)
        {
            CaseFileData datas = new CaseFileData();
            try
            {
                if (!File.Exists(filePath))
                {
                    return datas;
                }
                datas.FilePath = @filePath;
                XElement rootXML = XDocument.Load(filePath).Element("bizmsgs");
                var funsXml = rootXML.Elements("function");
                foreach (var item in funsXml)
                {

                    string functionid = item.Attribute("functionid").Value;
                    string packettype = item.Attribute("packettype").Value;
                    var CheckFun = funList.FirstOrDefault(o => o.Check == 1 && o.FunID == functionid);
                    if (packettype.Equals("0") && CheckFun != null)
                    {
                        string functionname = item.Attribute("functionname").Value;
                        CaseFunction fun = new CaseFunction()
                        {
                            FunID = functionid,
                            FunType = 0,
                            Check = 1,
                            FunName = functionname
                        };
                        //发送次数
                        if (item.Attribute("value") != null)
                        {
                            string sendTimes = item.Attribute("times").Value;
                            int nSendTimes = 1;
                            int.TryParse(sendTimes, out nSendTimes);
                            fun.SendTimes = nSendTimes;
                        }
                        datas.Functions.Add(fun);
                        var filedsXml = item.Elements("field");

                        LDsdkDefineEx.LDFastMessageAdapter fastmsg = new LDFastMessageAdapter(functionid, 0);
                        LDRecordAdapter lpRecord = fastmsg.GetBizBodyRecord();

                        int iIndex = 0;
                        foreach (var filedItem in filedsXml)
                        {
                            string fieldid = filedItem.Attribute("fieldid").Value;
                            string fieldname = filedItem.Attribute("fieldname").Value;
                            int filedtype = lpRecord.GetFieldType(iIndex);

                            PARAMTYPE paramtype = PARAMTYPE.emFile;
                            string value = "";
                            if (filedItem.Attribute("value") != null)
                            {
                                value = filedItem.Attribute("value").Value;
                            }
                            if (filedItem.Attribute("paramtype") != null)
                            {
                                string strparamtype = filedItem.Attribute("paramtype").Value;
                                if ("file".Equals(strparamtype))
                                {
                                    paramtype = PARAMTYPE.emFile;
                                }
                            }
                            CaseFiled filed = new CaseFiled()
                            {
                                FiledTag = int.Parse(fieldid),
                                FiledName = fieldname,
                                FiledValue = value,
                                FiledType = filedtype,
                                FiledIndex = iIndex,
                                ParamType = paramtype
                            };
                            var fileElemt = filedItem.Element("file");
                            if (fileElemt != null && paramtype == PARAMTYPE.emFile)
                            {

                                filed.ColumnName = fieldname;
                                if (fileElemt.Attribute("columnname") != null)
                                {
                                    filed.ColumnName = fileElemt.Attribute("columnname").Value;
                                }

                                if (fileElemt.Attribute("filename") != null)
                                {
                                    filed.FileName = fileElemt.Attribute("filename").Value;
                                    CSVFile csvFile = null;
                                    if (File.Exists(filed.FileName) && !fun.AllCsvFile.TryGetValue(filed.FileName, out csvFile))
                                    {
                                        var data = CSVFileHelper.OpenCSV(filed.FileName);
                                        if (data != null)
                                        {

                                            csvFile = new CSVFile();
                                            csvFile.FileName = filed.FiledName;
                                            csvFile.Table = data;
                                            fun.AllCsvFile.Add(filed.FileName, csvFile);
                                        }

                                    }
                                    if (csvFile != null && string.IsNullOrWhiteSpace(csvFile.DefaultFieldName))
                                    {
                                        csvFile.DefaultFieldName = fieldname;

                                    }

                                }



                                filed.SelectRow = VALUETYPE.emSameLine;
                                if (fileElemt.Attribute("selectrow") != null)
                                {
                                    try
                                    {
                                        string SelectRow = fileElemt.Attribute("selectrow").Value;
                                        if ("sequence".Equals(SelectRow))
                                        {
                                            filed.SelectRow = VALUETYPE.emSequence;
                                        }
                                        else if (new Regex("same_line").Match(SelectRow).Success)
                                        {
                                            filed.SameLineColumn = SelectRow.Split('[')[1].Split(']')[0];
                                            filed.SelectRow = VALUETYPE.emSameLine;
                                        }
                                        else if ("random".Equals(SelectRow))
                                        {
                                            //默认("random".Equals(SelectRow))
                                            filed.SelectRow = VALUETYPE.emRandom;
                                        }

                                    }
                                    catch (Exception)
                                    {

                                        //throw;
                                    }

                                }


                            }
                            fun.Fileds.Add(filed);
                            iIndex++;
                        }
                        fastmsg.FreeMsg();
                    }
                }

                return datas;
            }
            catch (Exception error)
            {
                this.Dispatcher.Invoke(() => { MessageBox.Show($"{error.Message} \r\n {error.StackTrace}"); });
                return datas;
            }
        }

      
        private ICommand _CaseLoad;
        public ICommand CaseLoad
        {
            get
            {
                if (_CaseLoad == null)
                {
                    _CaseLoad = new DelegateCommand(DoCaseLoad);
                }
                return _CaseLoad;
            }
        }

        private ICommand _StartRun;
        public ICommand StartRun
        {
            get
            {
                if (_StartRun == null)
                {
                    _StartRun = new DelegateCommand(DoStart);
                }
                return _StartRun;
            }
        }

        /// <summary>
        /// 启动压力测试
        /// </summary>
        public void DoStart()
        {
            LogHelper.Logger.Info("开始启动");
            ExcelHelper excel = new ExcelHelper(@"defaultvalue.xlsx");
            defaultTable = excel.ExcelToDataTable("Sheet1", true);
            IsWork = false;
            ReplyRateInfos.Clear();
            caseList.Clear();
            IsWork = true;
            foreach (var item in CaseGroups)
            {
                if (item.Check == 1)
                {
                    Task task = Task.Factory.StartNew(() =>
                    {
                        Work(item);
                    });
                }
            }
        }

        private ICommand _StopRun;
        public ICommand StopRun
        {
            get
            {
                if (_StopRun == null)
                {
                    _StopRun = new DelegateCommand(DoStop);
                }
                return _StopRun;
            }
        }

        /// <summary>
        /// 1、加载案例最上级目录
        /// 2、根据最上级目录加载具体的脚本信息
        /// </summary>
        public void DoCaseLoad()
        {
            //CaseScripts = new ObservableCollection<CaseScript>(XmlDataLoader.LoadData<List<CaseScript>>(@"script.xml"));
            CaseGroups.Clear();
            List<CaseGroupBase>  baseGroups=XmlDataLoader.LoadData<List<CaseGroupBase>>(caseFilePath);
            if (baseGroups != null)
            {

                foreach (var item in baseGroups)
                {
                    var caseFunList = LoadCaseFunction(item.FileName);

                    CaseGroups.Add(new CaseGroup() {
                         Check = item.Check,
                         DownStage = item.DownStage,
                         FileName = item.FileName,
                         FunList = caseFunList,
                         GroupName = item.GroupName,
                         ID = item.ID,
                         LoopCount = item.LoopCount,
                         RecFailCount = item.RecFailCount,
                         RecOkCount = item.RecOkCount,
                         SendFailCount = item.SendFailCount,
                         SendOkCount = item.SendOkCount,
                         UpStage = item.UpStage

                    });
                }
            }

        }

        

        private void DoStop()
        {
            LogHelper.Logger.Info("结束启动");
            IsWork = false;
        }

        private void Work(CaseGroup caseGroup)
        {
            string result = "";
            try
            {
                CaseFileData data = null;
                try
                {
                    data = LoadCaseData(caseGroup.FileName, caseGroup.FunList);
                }
                catch (Exception e)
                {
                    LogHelper.Logger.Error("发现文件格式错误", e);
                    return;
                }

                data.ID = caseGroup.ID;
                data.UpStage = caseGroup.UpStage;
                data.DownStage = caseGroup.DownStage;
                caseList.Add(data);

                if (caseGroup.LoopCount == 0)   //发送次数为0时，无限循环
                {
                    while (IsWork)
                    {
                        bool bNeedWait = (data.SendCount - data.RecCount) >= data.UpStage;
                        if (bNeedWait)
                        {
                            data.ResetEvent.Reset();
                            if (!data.ResetEvent.WaitOne(10000))
                            {//等超时了，说明丢包了，重新置数
                                //data.RecCount = data.SendCount;
                            }
                        }

                        SendFastMsgReq(data, out result);
                        if (result == "error")
                        {
                            return;
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < caseGroup.LoopCount; i++)
                    {
                        if (!IsWork)
                        {
                            return;
                        }
                        bool bNeedWait = (data.SendCount - data.RecCount) >= data.UpStage;
                        if (bNeedWait)
                        {
                            data.ResetEvent.Reset();
                            if (!data.ResetEvent.WaitOne(10000))
                            {//等超时了，说明丢包了，重新置数
                                data.RecCount = data.SendCount;
                            }
                        }

                        SendFastMsgReq(data, out result);
                        if (result == "error")
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                this.Dispatcher.Invoke(() => { MessageBox.Show($"{error.Message} \r\n {error.StackTrace}"); });
            }
        }

        /// <summary>
        /// 发送压力测试的请求包
        /// </summary>
        /// <param name="file"></param>
        /// <param name="result"></param>
        private void SendFastMsgReq(CaseFileData file,out string result)
        {
            try
            {
                foreach (CaseFunction funitem in file.Functions)
                {

                    int nSendTimes = 0;
                    do
                    {
                        nSendTimes++;
                        #region 发送包
                        LDsdkDefineEx.LDFastMessageAdapter fastmsg = new LDFastMessageAdapter(funitem.FunID, funitem.FunType);
                        //读取储存默认值的Excel文档，给未复制的参数赋予默认值
                        foreach (var filedItem in funitem.Fileds)
                        {
                            string FiledValue = filedItem.FiledValue;
                            int filedType = filedItem.FiledType;
                            string filedName = filedItem.FiledName;

                            FiledValue = GetParamValue(funitem, filedItem);
                            //如果获取参数值失败，直接退出
                            if (FiledValue == "error")
                            {
                                result = "error";
                                return;
                            }
                            if (string.IsNullOrEmpty(FiledValue))
                            {
                                FiledValue = GetDefaultValue(filedItem.FiledName, FiledValue);
                            }
                            UtilTool.SetFastMsgValueById(fastmsg, filedType, FiledValue, filedItem.FiledTag);
                        }
                        //赋值完毕，异步发送数据包
                        uint nRet = ConnectionManager.Instance.CurConnection.Connect.AsyncSend(funitem.FunID, fastmsg, 5000);

                        lock (ReqLock)
                        {
                            ReqAllList.Add(new ReqDTO() { SendID = nRet, CaseData = file });
                        }

                        Interlocked.Increment(ref file.SendCount);
                        if (nRet > 0)
                        {

                            Interlocked.Increment(ref file.SendOkCount);
                        }
                        else
                        {
                            Interlocked.Increment(ref file.SendFailCount);
                        }
                        fastmsg.FreeMsg();
                        #endregion
                    }
                    while (funitem.SendTimes > nSendTimes);
                }
                result = "OK";
            }
            catch (Exception error)
            {
                this.Dispatcher.Invoke(() => { MessageBox.Show($"{error.Message} \r\n {error.StackTrace}"); });
                result = "error";
            }
        }

        private String GetParamValue(CaseFunction funitem,CaseFiled filed)
        {
            String Value = filed.FiledValue;
            try
            {
                if (filed.ParamType == PARAMTYPE.emFile)
                {
                    if (File.Exists(filed.FileName))
                    {
                        CSVFile csfFile = null;
                        if (funitem.AllCsvFile.TryGetValue(filed.FileName, out csfFile))
                        {
                            DataTable data = csfFile.Table;
                            if (filed.SelectRow == VALUETYPE.emRandom && data != null && data.Rows.Count > 0)
                            {
                                long tick = DateTime.Now.Ticks;
                                Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
                                csfFile.CurrFileRowIndex = ran.Next(data.Rows.Count);
                                Value = data.Rows[csfFile.CurrFileRowIndex][filed.ColumnName].ToString().Replace("\"", "");
                            }
                            else if (filed.SelectRow == VALUETYPE.emSequence && data.Rows.Count > 0)
                            {
                                if (data.Rows.Count > csfFile.CurrFileRowIndex + 1)
                                {
                                    csfFile.CurrFileRowIndex++;
                                    Value = data.Rows[csfFile.CurrFileRowIndex][filed.ColumnName].ToString().Replace("\"", "");
                                }
                                else
                                {
                                    csfFile.CurrFileRowIndex = 0; ;
                                    Value = data.Rows[csfFile.CurrFileRowIndex][filed.ColumnName].ToString().Replace("\"", "");
                                }
                            }
                            else if (filed.SelectRow == VALUETYPE.emSameLine && csfFile.CurrFileRowIndex >= 0 && data.Rows.Count > 0)
                            {

                                Value = data.Rows[csfFile.CurrFileRowIndex][filed.ColumnName].ToString().Replace("\"", "");
                            }
                        }

                    }
                }
                if (filed.FiledTag == 1674)
                {
                    Value = funitem.FunID;
                }
                return Value;
            }
            catch (Exception error)
            {
                this.Dispatcher.Invoke(() => { MessageBox.Show($"{error.Message} \r\n {error.StackTrace}"); });
                return "error";
            }

        }

        /// <summary>
        /// 根据行列索引查找值
        /// </summary>
        /// <param name="filedTag"></param>
        /// <param name="FiledValue"></param>
        /// <returns></returns>
        private String GetDefaultValue(int filedTag, string FiledValue)
        {

            int columnIndex = defaultTable.Columns["参数值"].Ordinal;
            int rowIndex = filedTag - 1100 - 1;

            FiledValue = defaultTable.Rows[rowIndex][columnIndex].ToString().Trim(' ');
            return FiledValue;
        }

        /// <summary>
        /// 根据所需的字段名和列名查找值
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="FiledValue">字段值(多此一举的东西,与zjx无关)</param>
        /// <returns></returns>
        private string GetDefaultValue(string fieldName, string FiledValue)
        {
            DataRow[] dr = defaultTable.Select($"字段 = '{fieldName}'");
            if (dr.Length > 0)
            {
                FiledValue = dr[0]["参数值"].ToString().Trim(' ');
            }
            return FiledValue;
        }


        /// <summary>
        /// 脚本信息显示
        /// </summary>
        private ICSharpCode.AvalonEdit.Document.TextDocument _SrciptDoc = new ICSharpCode.AvalonEdit.Document.TextDocument();
        public ICSharpCode.AvalonEdit.Document.TextDocument SrciptDoc
        {
            get
            {
                return this._SrciptDoc;
            }
            set
            {
                this.SetProperty(ref _SrciptDoc, value);
            }
        }

        private ICommand _ClearPrint;
        public ICommand ClearPrint
        {
            get
            {
                if (_ClearPrint == null)
                {
                    _ClearPrint = new DelegateCommand(DoClearPrint);
                }
                return _ClearPrint;
            }
        }

        private void DoClearPrint()
        {
            printData.Document.Blocks.Clear();
        }

        private ICommand _RunScript;
        public ICommand RunScript
        {
            get
            {
                if (_RunScript == null)
                {
                    _RunScript = new DelegateCommand(DoRunScript);
                }
                return _RunScript;
            }
        }

        /// <summary>
        /// 运行脚本
        /// </summary>
        public void DoRunScript()
        {
            string[] textArry = _SrciptDoc.Text.Split('\n');
            PrintHelper.Print.PrintData = printData;
            Task.Run(() =>
            {
                string result = "";
                Compile cp = new Compile();
                cp.Run(textArry,ref result);
                if (result != "")
                {
                    this.Dispatcher.Invoke(() => { MessageBox.Show(result,"错误提示"); });
                }
            });
          
        }

        private ICommand _ComplieScript;
        public ICommand ComplieScript
        {
            get
            {
                if (_ComplieScript == null)
                {
                    _ComplieScript = new DelegateCommand(DoComplieScript);
                }
                return _ComplieScript;
            }
        }

        private void DoComplieScript()
        {
            string[] textArry = _SrciptDoc.Text.Split('\n');
            Task.Run(() =>
            {
                Compile cp = new Compile();
                cp.PreCompile(textArry);

            });
         
        }

        /// <summary>
        /// 加载并显示脚本信息
        /// </summary>
        private string LoadScript(string path)
        {
            string info = "";
            if (File.Exists(path))
            {
                SrciptDoc.FileName = path;
                info = CSVFileHelper.OpenScript(path).ToString();
                SrciptDoc.Text = info;
            }
            return info;
        }

        #region 重新加载压力测试案例
        private ICommand _LoadCase;
        public ICommand LoadCase
        {
            get
            {
                if (_LoadCase == null)
                {
                    _LoadCase = new DelegateCommand(GetAndOpenCase);
                }
                return _LoadCase;
            }
        }

        private void GetAndOpenCase()
        {
            try
            {
                if (_LoadCase != null)
                {
                    System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                    ofd.Filter = "(*.xml)|*.xml";
                    ofd.InitialDirectory = Environment.CurrentDirectory;
                    ofd.Multiselect = false;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        caseFilePath = ofd.FileName;
                        ReplyRateInfos.Clear();
                    }
                }
                DoCaseLoad();
            }
            catch (Exception error)
            {
                this.Dispatcher.Invoke(() => { MessageBox.Show($"{error.Message} \r\n {error.StackTrace}"); });
            }
        }
        #endregion

    }
}
