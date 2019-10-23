using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LDsdkDefineEx;
using LDBizTagDefine;
namespace MyLR
{
    public delegate void OnPublishHandler(Connection connection, string strTopicName, LDFastMessageAdapter fastMsg);
    public delegate void OnReceivedExHandler(Connection connection, int hSend, LDFastMessageAdapter lpFastMsg, int result);

    public class Connection
    {
        internal event Action<int,string, LDFastMessageAdapter> OnPublishCallBack;
        internal event Action<int ,int, LDFastMessageAdapter, int> OnReceivedExCallBack;
        public int ConnectionID { get; set; }
        public bool ConnectionStatus { get; set; }
        public String ConnnectName { get; set; }
        public CConnectionIAdapter Connect { get; set; }
        public Connection(string connectionName, uint connimeOut)
        {
            ConnnectName = connectionName;
            ConnectionStatus = false;
            Connect = new CConnectionIAdapter(connectionName);
            OnConnect(connimeOut);
        }
        private void OnConnect(uint connimeOut)
        {
            Connect.OnReceivedEx += Connect_OnReceivedEx;
            Connect.OnPublish += Connect_OnPublish;
            Connect.Create(App.Current.GetType().ToString()+ ConnectionID.ToString());
            if (Connect.Connect((int)connimeOut) == 0)
            {
                ConnectionStatus = true;
            }
           
        }

        private void Connect_OnPublish(string strTopicName, LDFastMessageAdapter lpFastMsg)
        {
            if (OnPublishCallBack != null)
            {
                OnPublishCallBack(ConnectionID,strTopicName,lpFastMsg);
            }
        }

        private void Connect_OnReceivedEx(int hSend, LDFastMessageAdapter lpFastMsg, int result)
        {
            if (OnReceivedExCallBack != null)
            {
                OnReceivedExCallBack(ConnectionID, hSend, lpFastMsg, result);
            }
        }
    }
   
    public class ConnectionManager
    {
        private CConfigIAdapter _config=null;
        private System.Collections.Generic.List<Connection> _connectioniInfos = new System.Collections.Generic.List<Connection>();//操作员信息
        private static ConnectionManager instance = new ConnectionManager();
        /// <summary>
        /// 连接序号
        /// </summary>
        private static int maxConnectionID = 1;
        public static ConnectionManager Instance
        {
            get { return ConnectionManager.instance; }
        }
        public event OnPublishHandler ConnectionManagerOnPublish;
        private  void OnPublish(Connection connection, string strTopicName, LDFastMessageAdapter fastMsg)
        {
            if (ConnectionManagerOnPublish != null)
            {
                ConnectionManagerOnPublish(connection, strTopicName,fastMsg);
            }
        }
        public event OnReceivedExHandler ConnectionManagerOnReceivedEx;
        private  void OnReceivedEx(Connection connection, int hSend, LDFastMessageAdapter lpFastMsg, int result)
        {
            if (ConnectionManagerOnReceivedEx != null)
            {
                ConnectionManagerOnReceivedEx(connection, hSend, lpFastMsg, result);
            }
        }
       

        private ConnectionManager()
        {
            _config = new CConfigIAdapter();
            _config.Load("config.ini");
            _config.Init();
        }
        /// <summary>
        /// 操作员数量
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            return _connectioniInfos.Count;
        }
        public List<Connection> GetConnections()
        {
            return _connectioniInfos.ToList();
        }
        public Connection NewConnection(string connectionName,uint connimeOut)
        {
            var connectionInfo = new Connection(connectionName, connimeOut);
            connectionInfo.ConnectionID = maxConnectionID;
            maxConnectionID++;
            _connectioniInfos.Add(connectionInfo);
            connectionInfo.OnPublishCallBack += ConnectionInfo_OnPublishCallBack;
            connectionInfo.OnReceivedExCallBack += ConnectionInfo_OnReceivedExCallBack;
            return connectionInfo;

        }

        private void ConnectionInfo_OnReceivedExCallBack(int connectionID, int hSend, LDFastMessageAdapter lpFastMsg, int result)
        {

            OnReceivedEx(GetConnectionInfo(connectionID),hSend,lpFastMsg,result);
        }

        private void ConnectionInfo_OnPublishCallBack(int connectionID, string strTopicName, LDFastMessageAdapter fastMsg)
        {
            OnPublish(GetConnectionInfo(connectionID),strTopicName,fastMsg);
        }

        public Connection GetConnectionInfo(int connectionID)
        {
            if (_connectioniInfos.Count == 1)
            {
                return _connectioniInfos.ElementAt(0);
            }
            else if (_connectioniInfos == null || _connectioniInfos.Count == 0)
            {
                return null;
            }
            else
            {
                return _connectioniInfos.FirstOrDefault(p => p.ConnectionID == connectionID);
            }
        }
        private Connection _CurConnection = null;
        /// <summary>
        /// 获取当前连接
        /// 没有连接抛出异常 throw  new Exception("have no Connect");
        /// </summary>
        public Connection CurConnection
        {

            get
            {
                if (_connectioniInfos.Count>0 && _CurConnection == null)
                {
                    _CurConnection = _connectioniInfos[0];
                }
                if (_CurConnection == null)
                {
                    throw new Exception("have no Connect");
                }
                return _CurConnection;
            }
            set
            {
                _CurConnection = value;
            }
        }
    }
}
