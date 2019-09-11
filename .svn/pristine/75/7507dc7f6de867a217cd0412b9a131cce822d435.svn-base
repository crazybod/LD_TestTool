using LDBizTagDefine;
using LDsdkDefineEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLR.Utils
{
    public class InitFastMsg
    {
        /// <summary>
        /// 初始化数据包
        /// </summary>
        /// <param name="funcId">功能号</param>
        /// <param name="fastMsg">需要进行初始化的数据包</param>
        public static void InitMsg(string funcId,ref LDFastMessageAdapter fastMsg)
        {
            fastMsg.SetInt32(LDBizTag.LDBIZ_OPOR_CO_NO_INT, 9999);
            fastMsg.SetInt32(LDBizTag.LDBIZ_OPOR_NO_INT, 99990001);
            fastMsg.SetString(LDBizTag.LDBIZ_OPOR_PWD_STR, "ZTI0MTA1ZDRiNmVmNjIxNWViMTA2ZmQ1YzkxMjYyNDM=");
            fastMsg.SetString(LDBizTag.LDBIZ_OPER_MAC_STR, " ");
            fastMsg.SetString(LDBizTag.LDBIZ_OPER_IP_ADDR_STR, " ");
            fastMsg.SetString(LDBizTag.LDBIZ_OPER_INFO_STR, " ");
            fastMsg.SetString(LDBizTag.LDBIZ_OPER_WAY_STR, "1");
            fastMsg.SetInt32(LDBizTag.LDBIZ_MENU_NO_INT, 0);
            fastMsg.SetString(LDBizTag.LDBIZ_FUNC_CODE_STR, funcId);
        }
    }
}
