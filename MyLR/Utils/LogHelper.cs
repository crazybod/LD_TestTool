using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR
{
    public enum LDLogLevel
    {
        ERROR,
        WARN,
        INFO,
        DEBUG,
        FATAL
    }
    public class LogInfo
    {
        public DateTime dtLogTime { get; set; }
        public LDLogLevel eLogLevel { get; set; }
        public Exception ex { get; set; }
        public string strMessage { get; set; }
    }
    public class LogHelper
    {
        /// <summary>
        /// 调试专用，日志输出到单个文件
        /// </summary>
        public static readonly LogHelper Logger = new LogHelper("errorlog");
        private QueueThreadSafe<LogInfo> _LogCaches;
        private LogHelper(string logName)
        {
            log4net.Config.XmlConfigurator.Configure();
            logger = LogManager.GetLogger(logName);
            _LogCaches = new QueueThreadSafe<LogInfo>();
            Task.Run(() => { WriteLog(); });
        }

        private ILog logger;

        public void Info(string msg)
        {
            LogInfo logContent = new LogInfo
            {
                strMessage = msg,
                eLogLevel = LDLogLevel.INFO,
                dtLogTime = DateTime.Now,
                ex = null,
            };
            _LogCaches.Enqueue(logContent);
            //logger.Info(msg);
        }

        public void Info(string msg, Exception e)
        {
            LogInfo logContent = new LogInfo
            {
                strMessage = msg,
                eLogLevel = LDLogLevel.INFO,
                dtLogTime = DateTime.Now,
                ex = e,
            };
            _LogCaches.Enqueue(logContent);
            //logger.Info(msg, e);
        }

        public void Debug(string msg)
        {
            LogInfo logContent = new LogInfo
            {
                strMessage = msg,
                eLogLevel = LDLogLevel.DEBUG,
                dtLogTime = DateTime.Now,
                ex = null,
            };
            _LogCaches.Enqueue(logContent);
            // logger.Debug(msg);
        }

        public void Debug(string msg, Exception e)
        {
            LogInfo logContent = new LogInfo
            {
                strMessage = msg,
                eLogLevel = LDLogLevel.DEBUG,
                dtLogTime = DateTime.Now,
                ex = e,
            };
            _LogCaches.Enqueue(logContent);
            //logger.Debug(msg, e);
        }

        public void Warn(string msg)
        {
            LogInfo logContent = new LogInfo
            {
                strMessage = msg,
                eLogLevel = LDLogLevel.WARN,
                dtLogTime = DateTime.Now,
                ex = null,
            };
            _LogCaches.Enqueue(logContent);
            //  logger.Warn(msg);
        }

        public void Warn(string msg, Exception e)
        {
            LogInfo logContent = new LogInfo
            {
                strMessage = msg,
                eLogLevel = LDLogLevel.WARN,
                dtLogTime = DateTime.Now,
                ex = e,
            };
            _LogCaches.Enqueue(logContent);
            //logger.Warn(msg, e);
        }

        public void Error(string msg)
        {
            LogInfo logContent = new LogInfo
            {
                strMessage = msg,
                eLogLevel = LDLogLevel.ERROR,
                dtLogTime = DateTime.Now,
                ex = null,
            };
            _LogCaches.Enqueue(logContent);
            //logger.Error(msg);
        }

        public void Error(string msg, Exception e)
        {
            LogInfo logContent = new LogInfo
            {
                strMessage = msg,
                eLogLevel = LDLogLevel.ERROR,
                dtLogTime = DateTime.Now,
                ex = e,
            };
            _LogCaches.Enqueue(logContent);
            //logger.Error(msg, e);
        }

        public void Fatal(string msg)
        {
            LogInfo logContent = new LogInfo
            {
                strMessage = msg,
                eLogLevel = LDLogLevel.FATAL,
                dtLogTime = DateTime.Now,
                ex = null,
            };
            _LogCaches.Enqueue(logContent);
            //logger.Fatal(msg);
        }

        public void Fatal(string msg, Exception e)
        {
            LogInfo logContent = new LogInfo
            {
                strMessage = msg,
                eLogLevel = LDLogLevel.FATAL,
                dtLogTime = DateTime.Now,
                ex = e,
            };
            _LogCaches.Enqueue(logContent);
            // logger.Fatal(msg, e);
        }
        private void WriteLog()
        {
            while (true)
            {

                if (_LogCaches.Count > 0)
                {
                    var info = _LogCaches.Dequeue();
                    try
                    {
                        WriteLogInfo(info);
                    }
                    catch (Exception)
                    {

                    }
                    System.Threading.Thread.Sleep(1);
                }
                else
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
        }
        private void WriteLogInfo(LogInfo info)
        {
            string strMessage = "";
            strMessage = strMessage + info.dtLogTime.ToString("HH:mm:ss") + "," + info.dtLogTime.ToString("fff") + " " + info.strMessage;
            switch (info.eLogLevel)
            {
                case LDLogLevel.INFO:
                    if (info.ex == null)
                    {
                        logger.Info(strMessage);
                    }
                    else
                    {
                        logger.Info(strMessage, info.ex);
                    }
                    break;
                case LDLogLevel.DEBUG:
                    if (info.ex == null)
                    {
                        logger.Debug(strMessage);
                    }
                    else
                    {
                        logger.Debug(strMessage, info.ex);
                    }
                    break;
                case LDLogLevel.WARN:
                    if (info.ex == null)
                    {
                        logger.Warn(strMessage);
                    }
                    else
                    {
                        logger.Warn(strMessage, info.ex);
                    }
                    break;
                case LDLogLevel.ERROR:
                    if (info.ex == null)
                    {
                        logger.Error(strMessage);
                    }
                    else
                    {
                        logger.Error(strMessage, info.ex);
                    }
                    break;
                case LDLogLevel.FATAL:
                    if (info.ex == null)
                    {
                        logger.Fatal(strMessage);
                    }
                    else
                    {
                        logger.Fatal(strMessage, info.ex);
                    }
                    break;

            }

        }
    }
}
