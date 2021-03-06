﻿using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MyLR
{
    public class CaseGroup : CaseGroupBase
    {
        private List<CaseFunction> _FunList = new List<CaseFunction>();
        public List<CaseFunction> FunList
        {
            get
            {
                return this._FunList;
            }
            set
            {
                this.SetProperty(ref this._FunList, value);
            }
        }
    }
    /// <summary>
    /// 案例脚本信息类
    /// </summary>
    public class CaseScript : BindableBase
    {
        public CaseScript()
        {

        }

        public CaseScript(string caseName,string fullPath)
        {
            this.CaseName = caseName;
            this.FullPath = fullPath;
        }
        /// <summary>
        /// 案例名称
        /// </summary>
        private String _CaseName;
        [XmlAttribute("CaseName")]
        public String CaseName
        {
            get
            {
                return _CaseName;
            }
            set
            {
                this.SetProperty(ref this._CaseName, value);
            }
        }
        /// <summary>
        /// 相对路径
        /// </summary>
        private String _FilePath;
        [XmlAttribute("FilePath")]
        public String FilePath
        {
            get
            {
                return _FilePath;
            }
            set
            {
                this.SetProperty(ref this._FilePath, value);
            }
        }

        /// <summary>
        /// 绝对路径
        /// </summary>
        private String _FullPath;
        [XmlAttribute("FullPath")]
        public String FullPath
        {
            get
            {
                return _FullPath;
            }
            set
            {
                this.SetProperty(ref this._FullPath, value);
            }
        }

        /// <summary>
        /// 是否被选中
        /// </summary>
        private bool _IsFocused;
        [XmlAttribute("IsFocused")]
        public bool IsFocused
        {
            get
            {
                return _IsFocused;
            }
            set
            {
                this.SetProperty(ref this._IsFocused, value);
            }
        }
    }

    public class CaseGroupBase : BindableBase
    {
        private int _ID;
        [XmlAttribute("ID")]
        public int ID
        {
            get
            {
                return _ID;
            }
            set
            {
                this.SetProperty(ref this._ID, value);
            }
        }
        private int _Check;
        [XmlAttribute("Check")]
        public int Check
        {
            get
            {
                return _Check;
            }
            set
            {
                this.SetProperty(ref this._Check, value);
            }
        }
        private String _GroupName;
        [XmlAttribute("GroupName")]
        public String GroupName
        {
            get
            {
                return _GroupName;
            }
            set
            {
                SetProperty(ref this._GroupName, value);
            }
        }
        private String _FileName;
        [XmlAttribute("FileName")]
        public String FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                this.SetProperty(ref this._FileName, value);
            }
        }
        private int _DownStage;
        [XmlAttribute("DownStage")]
        public int DownStage
        {
            get
            {
                return _DownStage;
            }
            set
            {
                this.SetProperty(ref this._DownStage, value);
            }
        }
        private int _UpStage;
        [XmlAttribute("UpStage")]
        public int UpStage
        {
            get
            {
                return _UpStage;
            }
            set
            {
                this.SetProperty(ref this._UpStage, value);
            }
        }
        private int _LoopCount;
        [XmlAttribute("LoopCount")]
        public int LoopCount
        {
            get
            {
                return _LoopCount;
            }
            set
            {
                this.SetProperty(ref this._LoopCount, value);
            }
        }
       
        private int _SendOkCount;
        [XmlAttribute("SendOkCount")]
        public int SendOkCount
        {
            get
            {
                return _SendOkCount;
            }
            set
            {
                this.SetProperty(ref this._SendOkCount, value);
            }
        }

        private int _SendFailCount;
        [XmlAttribute("SendFailCount")]
        public int SendFailCount
        {
            get
            {
                return _SendFailCount;
            }
            set
            {
                this.SetProperty(ref this._SendFailCount, value);
            }
        }
        private int _RecOkCount;
        [XmlAttribute("RecOkCount")]
        public int RecOkCount
        {
            get
            {
                return _RecOkCount;
            }
            set
            {
                this.SetProperty(ref this._RecOkCount, value);
            }
        }
        private int _RecFailCount;
        [XmlAttribute("RecFailCount")]
        public int RecFailCount
        {
            get
            {
                return _RecFailCount;
            }
            set
            {
                this.SetProperty(ref this._RecFailCount, value);
            }
        }
        private int _UnDealCount;
        public int UnDealCount
        {
            get
            {
                return _UnDealCount;
            }
            set
            {
                this.SetProperty(ref this._UnDealCount, value);
            }
        }
    }
    /// <summary>
    /// 项目信息类
    /// </summary>
    public class ProjectInfo : BindableBase
    {
        public ProjectInfo(string projectName,string fullPath)
        {
            this.ProjectName = projectName;
            this.FullPath = fullPath;
        }

        public ProjectInfo()
        {

        }
        /// <summary>
        /// 项目名称
        /// </summary>
        private String _ProjectName;
        [XmlAttribute("ProjectName")]
        public String ProjectName
        {
            get
            {
                return _ProjectName;
            }
            set
            {
                this.SetProperty(ref this._ProjectName, value);
            }
        }

        
        private String _FullPath;
        /// <summary>
        /// 绝对路径
        /// </summary>
        [XmlAttribute("FullPath")]
        public String FullPath
        {
            get
            {
                return _FullPath;
            }
            set
            {
                this.SetProperty(ref this._FullPath, value);
            }
        }
    }
}
