using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using LDsdkDefineEx;
using LDBizTagDefine;
using ICSharpCode.AvalonEdit;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Threading;
using System.Runtime.InteropServices;
using MyLR.HqReplay;
using MyLR.Utils;
using System.Diagnostics;

namespace MyLR
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        #region 变量申明
        /// <summary>
        /// 计次
        /// </summary>
        int i = 0;
        /// <summary>
        /// 运动轨迹线段绘制
        /// </summary>
        PathFigure pf = new PathFigure();
        /// <summary>
        /// 组合绘制的线段
        /// </summary>
        PathGeometry pg = new PathGeometry();
        /// <summary>
        /// 绘制轨迹曲线的容器，用于显示
        /// </summary>
        Path pa;
        /// <summary>
        /// 发送数据包的连接适配器
        /// </summary>
        private CConnectionIAdapter sendConnectionAdapter;

        /// <summary>
        /// 是否暂停状态
        /// </summary>
        bool isPause = false;
        /// <summary>
        /// 阻塞线程句柄
        /// </summary>
        EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        /// <summary>
        /// 延迟时间(毫秒)
        /// </summary>
        double delay = 0;
        /// <summary>
        /// 券码条数限制
        /// </summary>
        int stockCountLimit = 0;
        /// <summary>
        /// 停止发送的券码集合
        /// </summary>
        List<string> forbidStocks = new List<string>();
        /// <summary>
        /// 是否发送异常行情包
        /// </summary>
        bool? isSendWrongData = false;
        #endregion

        MainViewModel mv;
        public MainWindow()
        {
            InitializeComponent();

            //加载证券模板数据
            if (!LoadStockInfo.LoadSrcData())
            {
                MessageBox.Show("证券模板数据加载失败！");
                Environment.Exit(0);
            }
            //建立中间件连接
            sendConnectionAdapter = ConnectionManager.Instance.NewConnection("config", 10000).Connect;

            mv = new MainViewModel(this.Dispatcher);
            this.DataContext = mv;

            mv.SetRichTextBox(this.PrintData);
        }

        /// <summary>
        /// 开启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            i++;
            forbidStocks = txtForbid.Text.Split('/').ToList(); //获取停止发送的券码集合
            stockCountLimit = int.Parse(txtStockCountLimit.Text);//获取最大发送的券码条数
            delay = Convert.ToDouble(txtDelay.Text);//获取延时毫秒数
            isSendWrongData = ckbIsSendWrongData.IsChecked;//是否发送异常行情包
            List<SolidColorBrush> ls = new List<SolidColorBrush>() { Brushes.Red, Brushes.Green, Brushes.Gold };

            pa = new Path();
            pa.Stroke = ls[i % 3];//绘制颜色，亮绿色
            pa.StrokeThickness = 2;//绘制的线宽
            pf.Segments.Clear();
            Task.Run(() => { DrawSin(); });
            btnStart.IsEnabled = false;
        }

        /// <summary>
        /// 发送数据并绘制正弦曲线
        /// </summary>
        private async void DrawSin()
        {
            try
            {
                int Zoom = 15;  //放大倍数
                Point center = new Point(0, 174); //原点

                float x1 = (float)(0 * Math.PI * Zoom / 180 + center.X);
                float y1 = (float)(Math.Sin(0 * Math.PI / 180) * 11 * Zoom + center.Y);
                
                this.Dispatcher.Invoke(() => {
                    //定义绘制的第一个点
                    pf.StartPoint = center;
                    //加入到PathFigure对象中，true表示描绘线段
                    pf.Segments.Add(new LineSegment(center, true));
                    //组合绘制的线段，只要操作一次
                    pg.Figures.Add(pf);
                    //作为Path对象的数据
                    pa.Data = pg;
                    //加入Canvas在屏幕显示
                    this.cvsDraw.Children.Add(pa);
                });
                for (double i = 1; i < 360 * 4; i++)   //角
                {
                    
                    float x2 = (float)(i * Math.PI * Zoom / 180 + center.X);
                    float y2 = (float)(Math.Sin(i * Math.PI / 180) * (-1) * 11 * Zoom + center.Y);
                    
                    this.Dispatcher.Invoke(() => {
                        pf.Segments.Add(new LineSegment(new Point(x2, y2), true));
                    });

                    if (isPause)
                    {
                        waitHandle.WaitOne();
                    }

                    #region 针对每个券码发送一个生成并发送一个数据包
                    int rows = LoadStockInfo.excelData.Rows.Count;//券码总条数

                    for (int j = 1; j < (rows < stockCountLimit ? rows : stockCountLimit); j++)
                    {
                        LDFastMessageAdapter fastMsg = new LDFastMessageAdapter("pubL.16.5", 0);

                        //券码
                        string stockNo = LoadStockInfo.excelData.Rows[j]["stock_code"].ToString();
                        if (forbidStocks.Contains(stockNo))
                        {
                            continue;
                        }
                        char[] stockNos = stockNo.ToCharArray();
                        //市场
                        int exchNo = Convert.ToInt32(LoadStockInfo.excelData.Rows[j]["exch_no"]);

                        //对5位数的券码进行数据处理
                        if (stockNos.Length < 5 || stockNos.Length > 6)
                        {
                            continue;
                        }
                        if (stockNos.Length < 6)
                        {
                            var tempStocks = stockNos.ToList();
                            tempStocks.Add('\0');
                            stockNos = tempStocks.ToArray();
                        }
                        //涨停价
                        double upPrice = Convert.ToDouble(LoadStockInfo.excelData.Rows[j]["down_limit_price"]);
                        //跌停价
                        double downPrice = Convert.ToDouble(LoadStockInfo.excelData.Rows[j]["up_limit_price"]);
                        //初始价格
                        double price0 = (upPrice + downPrice) / 2.00;
                        //递增/递减价格
                        double price1 = (upPrice - price0) * Math.Sin(i * Math.PI / 180) * (-1) + price0;
                        double price2 = (upPrice - price0) * Math.Sin((i + 1) * Math.PI / 180) * (-1) + price0;
                        double price3 = (upPrice - price0) * Math.Sin((i + 2) * Math.PI / 180) * (-1) + price0;
                        double price4 = (upPrice - price0) * Math.Sin((i + 3) * Math.PI / 180) * (-1) + price0;
                        double price5 = (upPrice - price0) * Math.Sin((i + 4) * Math.PI / 180) * (-1) + price0;

                        InitFastMsg.InitMsg("pubL.16.5", ref fastMsg);

                        fastMsg.SetInt32(LDBizTag.LDBIZ_EXCH_NO_INT, exchNo);
                        fastMsg.SetString(LDBizTag.LDBIZ_STOCK_CODE_STR, stockNo);

                        //开始组装数据包
                        /*
                         价格信息以正弦曲线的方式进行变动  price = (upPrice - price0) * Math.Sin((i + 4) * Math.PI / 180) + price0;
                         数量信息呈线性增长  y = kx
                         */
                        //数据基数
                        int tmpValue = 1000;
                        int tmpValue0 = 100;

                        
                        IntPtr ptr;
                        if ((bool)isSendWrongData)
                        {
                            #region 创建异常的行情包
                            WrongStockLevelRealTimeData wrongStockFiled = new WrongStockLevelRealTimeData();

                            wrongStockFiled.WrongData = tmpValue * 111;
                            wrongStockFiled.PriceUnit = tmpValue;
                            wrongStockFiled.UpPrice = (int)(downPrice * tmpValue);
                            wrongStockFiled.DownPrice = (int)(upPrice * tmpValue);
                            wrongStockFiled.FiveDayVol = 0;
                            wrongStockFiled.OpenPrice = (int)(price0 * tmpValue);
                            wrongStockFiled.PrevClose = (int)(price0 * tmpValue); ;

                            wrongStockFiled.BuyPrice1 = (int)(price1 * tmpValue);    //买一价
                            wrongStockFiled.BuyPrice2 = (int)(price2 * tmpValue);    //买二价
                            wrongStockFiled.BuyPrice3 = (int)(price3 * tmpValue);    //买三价
                            wrongStockFiled.BuyPrice4 = (int)(price4 * tmpValue);    //买四价
                            wrongStockFiled.BuyPrice5 = (int)(price5 * tmpValue);    //买五价
                            wrongStockFiled.BuyCount1 = (uint)(tmpValue0 * i);
                            wrongStockFiled.BuyCount2 = (uint)(tmpValue0 * i * 2);
                            wrongStockFiled.BuyCount3 = (uint)(tmpValue0 * i * 3);
                            wrongStockFiled.BuyCount4 = (uint)(tmpValue0 * i * 4);
                            wrongStockFiled.BuyCount5 = (uint)(tmpValue0 * i * 5);

                            wrongStockFiled.SellPrice1 = (int)(price1 * tmpValue);   //卖一价
                            wrongStockFiled.SellPrice2 = (int)(price2 * tmpValue);   //卖二价
                            wrongStockFiled.SellPrice3 = (int)(price3 * tmpValue);   //卖三价
                            wrongStockFiled.SellPrice4 = (int)(price4 * tmpValue);   //卖四价
                            wrongStockFiled.SellPrice5 = (int)(price5 * tmpValue);   //卖五价
                            wrongStockFiled.SellCount1 = (uint)(tmpValue0 * i);
                            wrongStockFiled.SellCount2 = (uint)(tmpValue0 * i * 2);
                            wrongStockFiled.SellCount3 = (uint)(tmpValue0 * i * 3);
                            wrongStockFiled.SellCount4 = (uint)(tmpValue0 * i * 4);
                            wrongStockFiled.SellCount5 = (uint)(tmpValue0 * i * 5);
                            //成交额
                            wrongStockFiled.AvgPrice = (int)(price1 * tmpValue) * (uint)(tmpValue0 * i) + (int)(price2 * tmpValue) * (uint)(tmpValue0 * i * 2)
                                + (int)(price3 * tmpValue) * (uint)(tmpValue0 * i * 3) + (int)(price4 * tmpValue) * (uint)(tmpValue0 * i * 4)
                                + (int)(price5 * tmpValue) * (uint)(tmpValue0 * i * 5);

                            wrongStockFiled.MaxPrice = (int)(price5 * tmpValue);
                            wrongStockFiled.MinPrice = (int)(price1 * tmpValue);
                            wrongStockFiled.NewPrice = (int)(price3 * tmpValue);
                            wrongStockFiled.HandNum = 100;
                            wrongStockFiled.CodeType = 4361;
                            wrongStockFiled.Decimal = 3;
                            wrongStockFiled.StockCode = stockNos;
                            //成交量
                            wrongStockFiled.Total = (uint)(tmpValue0 * i * 15);
                            #endregion
                            
                            //将数据包转化成非托管类型
                            ptr = Marshal.AllocHGlobal(Marshal.SizeOf(wrongStockFiled));
                            Marshal.StructureToPtr(wrongStockFiled, ptr, false);
                            fastMsg.SetRawData(LDBizTag.LDBIZ_QUOT_PRICE_INFO_STR, ptr, Marshal.SizeOf(wrongStockFiled));
                        }
                        else
                        {
                            #region 创建正常的数据包
                            StockLevelRealTimeData stockFiled = new StockLevelRealTimeData();

                            stockFiled.PriceUnit = tmpValue;
                            stockFiled.UpPrice = (int)(downPrice * tmpValue);
                            stockFiled.DownPrice = (int)(upPrice * tmpValue);
                            stockFiled.FiveDayVol = 0;
                            stockFiled.OpenPrice = (int)(price0 * tmpValue);
                            stockFiled.PrevClose = (int)(price0 * tmpValue); ;

                            stockFiled.BuyPrice1 = (int)(price1 * tmpValue);    //买一价
                            stockFiled.BuyPrice2 = (int)(price2 * tmpValue);    //买二价
                            stockFiled.BuyPrice3 = (int)(price3 * tmpValue);    //买三价
                            stockFiled.BuyPrice4 = (int)(price4 * tmpValue);    //买四价
                            stockFiled.BuyPrice5 = (int)(price5 * tmpValue);    //买五价
                            stockFiled.BuyCount1 = (uint)(tmpValue0 * i);
                            stockFiled.BuyCount2 = (uint)(tmpValue0 * i * 2);
                            stockFiled.BuyCount3 = (uint)(tmpValue0 * i * 3);
                            stockFiled.BuyCount4 = (uint)(tmpValue0 * i * 4);
                            stockFiled.BuyCount5 = (uint)(tmpValue0 * i * 5);

                            stockFiled.SellPrice1 = (int)(price1 * tmpValue);   //卖一价
                            stockFiled.SellPrice2 = (int)(price2 * tmpValue);   //卖二价
                            stockFiled.SellPrice3 = (int)(price3 * tmpValue);   //卖三价
                            stockFiled.SellPrice4 = (int)(price4 * tmpValue);   //卖四价
                            stockFiled.SellPrice5 = (int)(price5 * tmpValue);   //卖五价
                            stockFiled.SellCount1 = (uint)(tmpValue0 * i);
                            stockFiled.SellCount2 = (uint)(tmpValue0 * i * 2);
                            stockFiled.SellCount3 = (uint)(tmpValue0 * i * 3);
                            stockFiled.SellCount4 = (uint)(tmpValue0 * i * 4);
                            stockFiled.SellCount5 = (uint)(tmpValue0 * i * 5);
                            //成交额
                            stockFiled.AvgPrice = (int)(price1 * tmpValue) * (uint)(tmpValue0 * i) + (int)(price2 * tmpValue) * (uint)(tmpValue0 * i * 2)
                                + (int)(price3 * tmpValue) * (uint)(tmpValue0 * i * 3) + (int)(price4 * tmpValue) * (uint)(tmpValue0 * i * 4)
                                + (int)(price5 * tmpValue) * (uint)(tmpValue0 * i * 5);

                            stockFiled.MaxPrice = (int)(price5 * tmpValue);
                            stockFiled.MinPrice = (int)(price1 * tmpValue);
                            stockFiled.NewPrice = (int)(price3 * tmpValue);
                            stockFiled.HandNum = 100;
                            stockFiled.CodeType = 4361;
                            stockFiled.Decimal = 3;
                            stockFiled.StockCode = stockNos;
                            //成交量
                            stockFiled.Total = (uint)(tmpValue0 * i * 15);
                            #endregion
                            
                            //将数据包转化成非托管类型
                            ptr = Marshal.AllocHGlobal(Marshal.SizeOf(stockFiled));
                            Marshal.StructureToPtr(stockFiled, ptr, false);
                            fastMsg.SetRawData(LDBizTag.LDBIZ_QUOT_PRICE_INFO_STR, ptr, Marshal.SizeOf(stockFiled));
                        }

                        //制造阻塞，确保数据包发送之后才进行内存回收 int result = 
                        await SendPackage(fastMsg, "quote.realtime");

                        Marshal.FreeHGlobal(ptr);
                        Thread.Sleep(TimeSpan.FromMilliseconds(delay));
                    }
                    #endregion
                }
                this.Dispatcher.Invoke(() => { btnStart.IsEnabled = true; });
            }
            catch (Exception error)
            {
                this.Dispatcher.Invoke(() => { btnStart.IsEnabled = true; });
                MessageBox.Show(error.Message + "\r\n" + error.StackTrace);
            }
        }
        
        /// <summary>
        /// 发送广播数据包的方法
        /// </summary>
        /// <param name="fastMsg"></param>
        /// <param name="funcId"></param>
        /// <returns></returns>
        private Task<int> SendPackage(LDFastMessageAdapter fastMsg,string funcId)
        {
            int result = -1;
            Task<int> tsk = Task<int>.Run(() => {
                result = sendConnectionAdapter.PubTopics(funcId, fastMsg);
                fastMsg.FreeMsg();
                return result;
            });
            return tsk;
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            forbidStocks = txtForbid.Text.Split('/').ToList();//获取停止发送的券码集合
            stockCountLimit = int.Parse(txtStockCountLimit.Text);//获取最大发送的券码条数
            delay = Convert.ToDouble(txtDelay.Text);//获取延时毫秒数
            isSendWrongData = ckbIsSendWrongData.IsChecked;//是否发送异常行情包
            isPause = false;
            waitHandle.Set();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            isPause = true;
        }
    }
}
