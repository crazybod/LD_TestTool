using System;
using System.IO;
using System.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using Microsoft.Win32;
using System.Windows.Documents;
using System.Collections.Generic;
using NPOI.SS.Util;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace MyLR
{
    public class ExcelHelper : IDisposable
    {
        private string fileName = null; //文件名
        private IWorkbook workbook = null;
        private FileStream fs = null;
        private bool disposed;
        public static string FailedResult = "failed";
        public static string CancelResult = "cancel";
        public ExcelHelper(string fileName)//构造函数，读入文件名
        {
            this.fileName = fileName;
            disposed = false;
        }
        /// 将excel中的数据导入到DataTable中
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(string sheetName, bool isFirstRowColumn)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            int startRow = 0;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                workbook = WorkbookFactory.Create(fs);
                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    if (sheet == null)
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号，即总的列数
                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            cell.SetCellType(CellType.String);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;//得到项标题后
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }
                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                                dataRow[j] = row.GetCell(j).ToString();
                        }
                        data.Rows.Add(dataRow);
                    }
                }
                return data;
            }
            catch (Exception ex)//打印错误信息
            {
                throw ex;
            }
        }

        //将DataTable数据导入到excel中
        //<param name="data">要导入的数据</param>
        //<param name="sheetName">要导入的excel的sheet的名称</param>
        //<param name="isColumnWritten">DataTable的列名是否要导入</param>
        //<returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcel(DataTable data, string sheetName, bool isColumnWritten)
        {
            int i = 0;
            int j = 0;
            int count = 0;
            ISheet sheet = null;

            fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                workbook = new XSSFWorkbook();
            else if (fileName.IndexOf(".xls") > 0) // 2003版本
                workbook = new HSSFWorkbook();
            #region 样式
            ICellStyle style1 = workbook.CreateCellStyle();//样式
            style1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;//文字水平对齐方式
            style1.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//文字垂直对齐方式
            //设置边框
            style1.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.WrapText = true;//自动换行
            IDataFormat dataFormat1 = workbook.CreateDataFormat();
            style1.DataFormat = dataFormat1.GetFormat("@");
            #endregion
            ICell cell;
            try
            {
                if (workbook != null)
                {
                    sheet = workbook.CreateSheet(sheetName);
                }
                else
                {
                    return -1;
                }
                int[] columnWidth = { 15, 15, 15, 10 };
                for (int Length = 0; Length < columnWidth.Length; Length++)
                {
                    //设置列宽度，256*字符数，因为单位是1/256个字符
                    sheet.SetColumnWidth(Length, 256 * columnWidth[Length]);
                }

                if (isColumnWritten == true) //写入DataTable的列名
                {
                    IRow row = sheet.CreateRow(0);
                    row.Height = 25 * 20;
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        cell = row.CreateCell(j);
                        cell.SetCellValue(data.Columns[j].ColumnName);
                        cell.CellStyle = style1;
                    }
                    count = 1;
                }
                else
                {
                    count = 0;
                }

                for (i = 0; i < data.Rows.Count; ++i)
                {
                    IRow row = sheet.CreateRow(count);
                    row.Height = 25 * 20;
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        cell = row.CreateCell(j);
                        cell.SetCellValue(data.Rows[i][j].ToString());
                        cell.CellStyle = style1;
                    }
                    ++count;
                }
                workbook.Write(fs); //写入到excel
                fs.Close();
                fs.Dispose();
                return count;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }
        public int DataTableToExcel(DataTable data, string sheetName, bool isColumnWritten, string[] mergedColumNames)
        {
            int i = 0;
            int j = 0;
            int count = 0;
            ISheet sheet = null;

            fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                workbook = new XSSFWorkbook();
            else if (fileName.IndexOf(".xls") > 0) // 2003版本
                workbook = new HSSFWorkbook();
            #region 样式
            ICellStyle style1 = workbook.CreateCellStyle();//样式
            style1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;//文字水平对齐方式
            style1.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//文字垂直对齐方式
            //设置边框
            style1.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            style1.WrapText = true;//自动换行
            IDataFormat dataFormat1 = workbook.CreateDataFormat();
            style1.DataFormat = dataFormat1.GetFormat("@");
            #endregion
            ICell cell;
            try
            {
                if (workbook != null)
                {
                    sheet = workbook.CreateSheet(sheetName);
                }
                else
                {
                    return -1;
                }
                int[] columnWidth = { 15, 15, 15, 10 };
                for (int Length = 0; Length < columnWidth.Length; Length++)
                {
                    //设置列宽度，256*字符数，因为单位是1/256个字符
                    sheet.SetColumnWidth(Length, 256 * columnWidth[Length]);
                }

                if (isColumnWritten == true) //写入DataTable的列名
                {
                    IRow row = sheet.CreateRow(0);
                    row.Height = 25 * 20;
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        cell = row.CreateCell(j);
                        cell.SetCellValue(data.Columns[j].ColumnName);
                        cell.CellStyle = style1;
                    }
                    count = 1;
                }
                else
                {
                    count = 0;
                }

                for (i = 0; i < data.Rows.Count; ++i)
                {
                    IRow row = sheet.CreateRow(count);
                    row.Height = 25 * 20;
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        string cellText = data.Rows[i][j].ToString();
                        cell = row.CreateCell(j);
                        cell.SetCellValue(cellText);
                        cell.CellStyle = style1;
                        if (i > 0)
                        {

                            bool needMerged = false;
                            if (data.Columns[j] != null)
                            {
                                foreach (var mergedColumName in mergedColumNames)
                                {
                                    if (string.Equals(mergedColumName, data.Columns[j].ColumnName))
                                    {
                                        if (i < data.Rows.Count - 1)
                                        {
                                            string nextCellText = data.Rows[i + 1][j].ToString();
                                            if (nextCellText != cellText)
                                            {
                                                needMerged = true;
                                            }
                                        }
                                        else
                                        {
                                            needMerged = true;
                                        }
                                    }
                                }
                            }
                            if (needMerged)
                            {
                                int firstrow = MergedRegionGetFirstRowNo(row.RowNum, j, sheet, cellText);
                                CellRangeAddress region = new CellRangeAddress(firstrow, row.RowNum, j, j);
                                sheet.AddMergedRegion(region);
                            }
                        }
                        //cell = row.CreateCell(j);
                        //cell.SetCellValue(data.Rows[i][j].ToString());

                    }
                    ++count;
                }
                workbook.Write(fs); //写入到excel
                fs.Close();
                fs.Dispose();
                return count;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }

        private int MergedRegionGetFirstRowNo(int nowRow, int colIndex, ISheet sheet, string value)
        {
            if (sheet == null)
                return -1;
            int result = nowRow;
            for (int row = nowRow; row >= 0; row--)
            {
                var cell = sheet.GetRow(row).GetCell(colIndex);
                if (string.Equals(cell.ToString(), value))
                {
                    result = row;
                    continue;
                }
                break;
            }
            return result;
        }
     
        public void Dispose()//IDisposable为垃圾回收相关的东西，用来显式释放非托管资源,这部分目前还不是非常了解
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }
                fs = null;
                disposed = true;
            }
        }
    }
}
