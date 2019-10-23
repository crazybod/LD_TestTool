using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyLR;
using System.Data;
using System.IO;

namespace MyLR.HqReplay
{
    public class LoadStockInfo
    {
        public static DataTable excelData;

        /// <summary>
        /// 初始化加载交易市场和证券信息
        /// </summary>
        /// <returns></returns>
        public static bool LoadSrcData()
        {
            string path = Environment.CurrentDirectory + @"\Data.xlsx";
            bool result = false;
            try
            {
                if (File.Exists(path))
                {
                    using (ExcelHelper excelHelper = new ExcelHelper(path))
                    {
                        excelData = excelHelper.ExcelToDataTable("证券模板数据", true);
                    }
                    result = true;
                }
                else
                {
                    result = false;
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }
            
        }
    }
}
