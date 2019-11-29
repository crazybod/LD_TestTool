using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Collections.Specialized;

namespace MyLR.Utils
{
    /// <summary>
    /// MySql连接工具类
    /// </summary>
    internal class DBHelper
    {
        /// <summary>
        /// 用于存储不同IP对应的MySql数据库连接
        /// 键：对应appSettings 里的key
        /// 值：对应的MySql数据库连接
        /// </summary>
        private static Dictionary<string, MySqlConnection> SqlCollections = new Dictionary<string, MySqlConnection>();
        static DBHelper()
        {
            NameValueCollection appSettings = ConfigurationSettings.AppSettings;
            foreach (var item in appSettings.Keys)
            {
                //读取配置文件中的数据库连接串
                string connStr = ConfigurationSettings.AppSettings[item.ToString()];
                MySqlConnection mySqlConnection = new MySqlConnection(connStr);
                if (mySqlConnection != null)
                {
                    SqlCollections.Add(item.ToString(), mySqlConnection);
                }
            }
        }
        /// <summary>
        /// 操作数据库并返回DataTable
        /// </summary>
        /// <param name="connStrKey">IP对应的appSetting里的key值</param>
        /// <param name="sqlCommand">SQL查询命令</param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string connStrKey,string sqlCommand)
        {
            DataSet ds = new DataSet();//声明数据集  
            MySqlConnection mySqlConnection = new MySqlConnection();
            try
            {
                if (SqlCollections.TryGetValue(connStrKey,out mySqlConnection))
                {
                    mySqlConnection.Open();
                    using (MySqlDataAdapter MyAdp = new MySqlDataAdapter(sqlCommand, mySqlConnection))
                    {
                        MyAdp.Fill(ds);
                    }
                }
            }
            finally
            {
                mySqlConnection.Close();
            }
            return ds.Tables[0];//返回数据集，用于绑定控件作为数据源  
        }

        /// <summary>
        /// 操作数据库
        /// </summary>
        /// <param name="connStrKey">IP对应的appSetting里的key值</param>
        /// <param name="sqlCommand">SQL命令</param>
        /// <returns></returns>
        public static int Execute(string connStrKey,string sqlCommand)
        {
            int result = -1;
            MySqlConnection mySqlConnection = new MySqlConnection();
            try
            {
                if (SqlCollections.TryGetValue(connStrKey,out mySqlConnection))
                {
                    mySqlConnection.Open();
                    MySqlCommand Command = new MySqlCommand(sqlCommand, mySqlConnection);
                    result = Command.ExecuteNonQuery();
                }
            }
            finally
            {
                mySqlConnection.Close();
            }
            return result;
        }
    }
}
