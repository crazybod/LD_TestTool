using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyLR
{
    public enum VALUETYPE
    {
        emRandom,
        emSequence,
        emSameLine

    }
    public enum PARAMTYPE
    {
        emFixed,
        emFile

    }
    public class CaseFiled
    {
        public CaseFiled()
        {
            ParamType = PARAMTYPE.emFixed;
            SelectRow = VALUETYPE.emRandom;
        }
        public string FiledName { get; set; }
        public int FiledTag { get; set; }
        public int FiledIndex { get; set; }
        public int FiledType { get; set; }
        public PARAMTYPE ParamType { get; set; }
        public string FiledValue { get; set; }
        public string FileName { get; set; }
        public string ColumnName { get; set; }
        public VALUETYPE SelectRow { get; set; }
        public string SameLineColumn { get; set; }
    }

    public class CSVFile
    {
        public CSVFile()
        {
            CurrFileRowIndex = 0;
            DefaultFieldName = "";
        }
        public string FileName { get; set; }

        public int CurrFileRowIndex { get; set; }

        public string DefaultFieldName { get; set; }
        public DataTable Table { get;set; }
    }

    public class CaseFunction: BindableBase
    {
        public Dictionary<string, CSVFile> AllCsvFile { get; set; }
        private int _Check;
        public int Check
        {
            get
            {
                return this._Check;
            }
            set
            {
                SetProperty(ref this._Check, value);
            }

        }
        private string _FunID;
        public string FunID
        {
            get
            {
                return this._FunID;
            }
            set
            {
                SetProperty(ref this._FunID,value);
            }
        }

        private string _FunName;
        public string FunName
        {
            get
            {
                return this._FunName;
            }
            set
            {
                SetProperty(ref this._FunName, value);
            }
        }

        public int FunType { get; set; }
        public List<CaseFiled> Fileds { get; set; }
      
        /// <summary>
        /// 执行一次发送次数
        /// </summary>
        public int SendTimes { get; set; }
        public CaseFunction()
        {
            SendTimes = 1;
          
            Fileds = new List<CaseFiled>();
            AllCsvFile = new Dictionary<string, CSVFile>();
        }
    }

    public class CaseFileData
    {
        public int SendOkCount;
        public int SendFailCount;
        public int SendCount;
        public int RecCount;
        public int RecOkCount;
        public int RecFailCount;
        public int UpStage;
        public int DownStage;
        public int ID;
        public AutoResetEvent ResetEvent { get; set; }
        public string FilePath { get; set; }
        public List<CaseFunction> Functions { get; set; }
        public CaseFileData()
        {
            SendOkCount = 0;
            SendFailCount = 0;
            SendCount = 0;
            RecCount = 0;
            RecOkCount = 0;
            RecFailCount = 0;
            ID = 0;
            UpStage = 0;
            DownStage=0;
            ResetEvent = new AutoResetEvent(true);
            Functions = new List<CaseFunction>();
        }
    }
}
