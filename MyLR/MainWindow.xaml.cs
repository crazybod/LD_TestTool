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
using System.IO;

namespace MyLR
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        #region 变量声明
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        /// <summary>
        /// 用于控制是否取消线程
        /// </summary>
        CancellationToken token = new CancellationToken();
        /// <summary>
        /// 涨跌幅比例
        /// </summary>
        double limitPercent = 10.00;
        /// <summary>
        /// 当前批量选中的案例(全局变量)
        /// </summary>
        public static List<CaseScript> selectedItems = new List<CaseScript>();

        /// <summary>
        /// 发送行情固定间隔时间3000ms
        /// </summary>
        int interval = 3000;

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
        System.Windows.Shapes.Path pa;

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
        /// 延迟时间(毫秒);
        /// 延迟时间的设置由 倍数、券码条数、固定间隔时间 三者共同决定
        /// 计算公式为：延迟时间 = (固定间隔时间/券码条数)/倍数
        /// 目前固定间隔时间为3000ms，默认倍数为1，券码条数由模板数据决定
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
        /// 发送异常数据包的券码
        /// </summary>
        List<string> errorStocks = new List<string>();
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

        #region 行情回放界面按钮点击事件区域
        /// <summary>
        /// 开启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            i++;
            forbidStocks = txtForbid.Text.Split('/').ToList(); //获取停止发送的券码集合
            errorStocks = txtError.Text.Split('/').ToList();   //获取发送异常数据包的券码集合 
            stockCountLimit = int.Parse(txtStockCountLimit.Text);//获取最大发送的券码条数
            limitPercent = double.Parse(txtUpDownLimit.Text);//获取涨跌幅比例

            int times = Convert.ToInt32(txtTimes.Text);//获取加速倍数
            if (times < 1)
            {
                MessageBox.Show("加速倍数不能小于1", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            int stockCount = LoadStockInfo.excelData.Rows.Count;//券码总条数
            int sends = stockCount < stockCountLimit ? stockCount : stockCountLimit; //实际应该发送多少条券码
            //计算出实际发送每条行情数据之间的时间间隔
            delay = (interval / sends) / times;

            List<SolidColorBrush> ls = new List<SolidColorBrush>() { Brushes.Red, Brushes.Green, Brushes.Gold };

            pa = new System.Windows.Shapes.Path();
            pa.Stroke = ls[i % 3];//绘制颜色，三选一
            pa.StrokeThickness = 2;//绘制的线宽
            pf.Segments.Clear();

            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
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
                for (double i = 1; i < 360 * 99; i++)   //角
                {
                    if (isPause)//是否暂停
                    {
                        waitHandle.WaitOne();
                    }
                    if (token.IsCancellationRequested)//是否重置(取消当前线程)
                    {
                        return;
                    }

                    float x2 = (float)(i * Math.PI * Zoom / 180 + center.X);
                    float y2 = (float)(Math.Sin(i * Math.PI / 180) * (-1) * 11 * Zoom + center.Y);
                    
                    this.Dispatcher.Invoke(() => {
                        pf.Segments.Add(new LineSegment(new Point(x2, y2), true));
                    });

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
                        //证券类型
                        int stockType = Convert.ToInt32(LoadStockInfo.excelData.Rows[j]["stock_type"]);

                        if (stockNos.Length < 5 || stockNos.Length > 6)
                        {
                            continue;
                        }
                        //对5位数的券码进行数据处理
                        if (stockNos.Length < 6)
                        {
                            var tempStocks = stockNos.ToList();
                            tempStocks.Add('\0');
                            stockNos = tempStocks.ToArray();
                        }

                        //昨收价格作为初始价格
                        double preClosePrice = Convert.ToDouble(LoadStockInfo.excelData.Rows[j]["pre_close_price"]);
                        //涨停价
                        double upPrice = preClosePrice * (1 + limitPercent/100.00);
                        //跌停价
                        double downPrice = preClosePrice * (1 - limitPercent / 100.00);

                        //递增/递减价格
                        double price1 = CaculatePrecision((upPrice - preClosePrice) * Math.Sin(i * Math.PI / 180) * (-1) + preClosePrice, exchNo, stockType);
                        double price2 = CaculatePrecision((upPrice - preClosePrice) * Math.Sin((i + 1) * Math.PI / 180) * (-1) + preClosePrice,exchNo,stockType);
                        double price3 = CaculatePrecision((upPrice - preClosePrice) * Math.Sin((i + 2) * Math.PI / 180) * (-1) + preClosePrice, exchNo, stockType);
                        double price4 = CaculatePrecision((upPrice - preClosePrice) * Math.Sin((i + 3) * Math.PI / 180) * (-1) + preClosePrice, exchNo, stockType);
                        double price5 = CaculatePrecision((upPrice - preClosePrice) * Math.Sin((i + 4) * Math.PI / 180) * (-1) + preClosePrice, exchNo, stockType);

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
                        if (errorStocks.Contains(stockNo))
                        {
                            #region 创建异常的行情包
                            WrongStockLevelRealTimeData wrongStockFiled = new WrongStockLevelRealTimeData();

                            wrongStockFiled.WrongData = tmpValue * 111;
                            wrongStockFiled.PriceUnit = tmpValue;
                            wrongStockFiled.UpPrice = (int)(downPrice * tmpValue);
                            wrongStockFiled.DownPrice = (int)(upPrice * tmpValue);
                            wrongStockFiled.FiveDayVol = 0;
                            wrongStockFiled.OpenPrice = (int)(preClosePrice * tmpValue);
                            wrongStockFiled.PrevClose = (int)(preClosePrice * tmpValue); ;

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
                            stockFiled.OpenPrice = (int)(preClosePrice * tmpValue);
                            stockFiled.PrevClose = (int)(preClosePrice * tmpValue); ;

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
        /// 该函数用于计算价格精度
        /// </summary>
        /// <param name="price">未确认精度的价格</param>
        /// <param name="exchNo">市场编号</param>
        /// <param name="stockType">证券类型</param>
        /// <returns></returns>
        private double CaculatePrecision(double price,int exchNo,int stockType)
        {
            if ((exchNo == 1 || exchNo == 2) && (stockType >= 1 && stockType <= 8))
            {
                price = Convert.ToInt32(price / 0.01) * 0.01;
            }
            else if ((exchNo == 1) && (stockType >= 21 && stockType <= 45))
            {
                price = Convert.ToInt32(price / 0.005) * 0.005;
            }
            else if ((exchNo == 2) && (stockType >= 21 && stockType <= 45))
            {
                price = Convert.ToInt32(price / 0.001) * 0.001;
            }
            else if (stockType >= 51 && stockType <= 64)
            {
                price = Convert.ToInt32(price / 0.001) * 0.001;
            }
            else if ((exchNo == 3 || exchNo == 4) && (stockType == 1))
            {
                //港股根据价格所在区间确定最小差价
                if (price >= 0.01 && price < 0.25)
                {
                    price = Convert.ToInt32(price / 0.001) * 0.001;
                }
                else if (price >= 0.25 && price < 0.5)
                {
                    price = Convert.ToInt32(price / 0.005) * 0.005;
                }
                else if (price >= 0.5 && price < 10)
                {
                    price = Convert.ToInt32(price / 0.01) * 0.01;
                }
                else if (price >= 10 && price < 20)
                {
                    price = Convert.ToInt32(price / 0.02) * 0.02;
                }
                else if (price >= 20 && price < 100)
                {
                    price = Convert.ToInt32(price / 0.05) * 0.05;
                }
                else if (price >= 100 && price < 200)
                {
                    price = Convert.ToInt32(price / 0.1) * 0.1;
                }
                else if (price >= 200 && price < 500)
                {
                    price = Convert.ToInt32(price / 0.2) * 0.2;
                }
                else if (price >= 500 && price < 1000)
                {
                    price = Convert.ToInt32(price / 0.5) * 0.5;
                }
                else if (price >= 1000 && price < 2000)
                {
                    price = Convert.ToInt32(price / 1) * 1;
                }
                else if (price >= 2000 && price < 5000)
                {
                    price = Convert.ToInt32(price / 2) * 2;
                }
                else if (price >= 5000 && price <= 9995)
                {
                    price = Convert.ToInt32(price / 5) * 5;
                }
            }
            return price;
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
            errorStocks = txtError.Text.Split('/').ToList();   //获取发送异常数据包的券码集合 
            stockCountLimit = int.Parse(txtStockCountLimit.Text);//获取最大发送的券码条数
            limitPercent = double.Parse(txtUpDownLimit.Text);//获取涨跌幅比例

            int times = Convert.ToInt32(txtTimes.Text);//获取加速倍数
            if (times < 1)
            {
                MessageBox.Show("加速倍数不能小于1","警告",MessageBoxButton.OK,MessageBoxImage.Warning);
            }
            int stockCount = LoadStockInfo.excelData.Rows.Count;//券码总条数
            int sends = stockCount < stockCountLimit ? stockCount : stockCountLimit; //实际应该发送多少条券码
            //计算出实际发送每条行情数据之间的时间间隔
            delay = (interval / sends) / times;
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

        /// <summary>
        /// 重置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            tokenSource.Cancel();
            pf.Segments.Clear();
            btnStart.IsEnabled = true;
        }
        #endregion

        #region 回归测试界面按钮点击事件区域
        /// <summary>
        /// 批量删除案例
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            PrintHelper.Print.PrintData = this.PrintData;
            try
            {
                var selectedItems = this.dgCase.SelectedItems;
                if (MessageBox.Show("确定删除所选案例?", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    if (MessageBox.Show("案例一经删除无法恢复!", "警告", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                    {

                        foreach (var item in selectedItems)
                        {
                            CaseScript convertItem = item as CaseScript;
                            if (File.Exists(convertItem.FullPath))
                            {
                                File.Delete(convertItem.FullPath);
                            }
                        }
                    }
                }
                var flush = mv.SelectProject;
                PrintHelper.Print.AppendLine("批量删除案例成功");
            }
            catch (Exception error)
            {
                PrintHelper.Print.AppendLine(error.Message + "\r\n" + error.StackTrace);
            }
        }

        /// <summary>
        /// 一键运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRunAll_Click(object sender, RoutedEventArgs e)
        {
            Compile cp = new Compile();
            Task.Run(
                () =>
                {
                    int i = -1;
                    foreach (CaseScript Script in mv.CaseScripts)
                    {
                        i++;
                        string result = "";//案例执行结果 成功/失败
                        string info = CSVFileHelper.OpenScript(Script.FullPath).ToString();
                        string[] textArry = info.Split('\n');
                        PrintHelper.Print.PrintData = this.PrintData;
                        PrintHelper.Print.AppendLine($"\r\n**********开始执行案例：{Script.CaseName}**********");
                        cp.Run(textArry, ref result);

                        DataGridRow row = (DataGridRow)this.dgCase.ItemContainerGenerator.ContainerFromIndex(i);
                        if (result != "")
                        {
                            this.Dispatcher.Invoke(() => { row.Foreground = new SolidColorBrush(Colors.Red); });
                            MessageBox.Show($"案例{Script.CaseName}执行失败,error:{result}", "异常");
                            continue;
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() => { row.Foreground = new SolidColorBrush(Colors.Green); });
                        }
                        
                    }
                    PrintHelper.Print.AppendLine($"\r\n所有案例脚本执行完毕！！！！！！！！！！！！");
                }
                );
        }

        /// <summary>
        /// 新建案例
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateScript_Click(object sender, RoutedEventArgs e)
        {
            PrintHelper.Print.PrintData = this.PrintData;
            try
            {
                if (mv.SelectProject == null)
                {
                    PrintHelper.Print.AppendLine("请先选中新建案例所在项目！");
                    return;
                }
                //新建案例默认临时命名
                string tempFileName = "temp" + DateTime.Now.ToString("HHmmss") + ".script";
                //新建案例默认属于当前选择项目
                string tempFullName = mv.SelectProject.FullPath + $@"\{tempFileName}";
                //创建一个空文件
                File.Create(tempFullName).Close();
                //重新加载项目信息
                mv.LoadScriptInfo(mv.SelectProject.FullPath);
                //将当前案例的选中项设置为刚刚新建的空案例
                mv.SelectScript = mv.CaseScripts.FirstOrDefault(o => o.FullPath == tempFullName);
            }
            catch (Exception error)
            {
                PrintHelper.Print.AppendLine(error.Message + "\r\n" + error.StackTrace);
            }
        }

        /// <summary>
        /// 保存案例脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveScript_Click(object sender, RoutedEventArgs e)
        {
            PrintHelper.Print.PrintData = this.PrintData;
            string result = "";
            try
            {
                if (mv.CaseScripts.Count == 0 || mv.SelectScript.FullPath == null)
                {
                    PrintHelper.Print.AppendLine("请先选中要保存的脚本！");
                    return;
                }
                result = CSVFileHelper.SaveScript(mv.SelectScript.FullPath, mv.SrciptDoc.Text);
                string caseName = mv.SelectScript.CaseName;
                string fullPath = mv.SelectScript.FullPath;
                //重新加载项目
                mv.LoadScriptInfo(mv.SelectProject.FullPath);
                //将焦点重置回去
                mv.SelectScript = mv.CaseScripts.FirstOrDefault(o => o.FullPath == fullPath);
                PrintHelper.Print.AppendLine(result);
            }
            catch (Exception error)
            {
                PrintHelper.Print.AppendLine(error.Message + "\r\n" + error.StackTrace);
            }
        }

        /// <summary>
        /// 删除案例脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteScript_Click(object sender, RoutedEventArgs e)
        {
            PrintHelper.Print.PrintData = this.PrintData;
            try
            {
                if (mv.CaseScripts.Count == 0 || mv.SelectScript.FullPath == null)
                {
                    PrintHelper.Print.AppendLine("请先选中要删除的脚本！");
                    return;
                }

                this.Dispatcher.Invoke(
                    () =>
                    {
                        if (MessageBox.Show("确定删除该案例？", "警告", MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
                        {
                            if (File.Exists(mv.SelectScript.FullPath))
                            {
                                File.Delete(mv.SelectScript.FullPath);
                            }
                        }
                    }
                    );

                //重新加载项目
                mv.LoadScriptInfo(mv.SelectProject.FullPath);
                PrintHelper.Print.AppendLine("案例删除成功");
            }
            catch (Exception error)
            {
                PrintHelper.Print.AppendLine(error.Message + "\r\n" + error.StackTrace);
            }
        }

        /// <summary>
        /// 选择项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            PrintHelper.Print.PrintData = this.PrintData;
            try
            {
                System.Windows.Forms.FolderBrowserDialog openProject = new System.Windows.Forms.FolderBrowserDialog();//定义文件夹实体
                openProject.Description = "选择项目";//对话框标题
                openProject.SelectedPath = Environment.CurrentDirectory;
                openProject.ShowNewFolderButton = true;//是否显示新建文件夹
                if (openProject.ShowDialog() == System.Windows.Forms.DialogResult.OK && !(openProject.SelectedPath == string.Empty))
                {
                    //获取项目文件夹信息
                    string projectPath = openProject.SelectedPath;
                    //获取文件夹名称
                    string projectName = projectPath.Split('\\').Last();
                    //限制不能重复加载同一项目
                    if (mv.ProjectInfos.Count(i => i.FullPath == projectPath) > 0)
                    {
                        MessageBox.Show("项目已加载,请勿重复加载", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    mv.ProjectInfos.Add(new ProjectInfo(projectName, projectPath));
                }
            }
            catch (Exception error)
            {
                PrintHelper.Print.AppendLine(error.Message + "\r\n" + error.Source);
            }
        }

        /// <summary>
        /// 脚本另存为
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveScriptTo_Click(object sender, RoutedEventArgs e)
        {
            PrintHelper.Print.PrintData = this.PrintData;
            string result = "";
            //获取的文件保存的路径
            string savePath = "";
            //文件名
            string fileName = "";
           
            try
            {
                if (mv.CaseScripts.Count == 0 || mv.SelectScript.FullPath == null)
                {
                    PrintHelper.Print.AppendLine("请先选中要保存的脚本！");
                    return;
                }
                string selectPath = mv.SelectScript.FullPath;

                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();//定义保存文本框实体
                saveDialog.Title = "保存脚本文件";//对话框标题
                saveDialog.Filter = "文件(.script)|*.script|所有文件|*.*";//文件扩展名
                saveDialog.InitialDirectory = selectPath;//默认显示当前选中的脚本名称
                saveDialog.FileName = mv.SelectScript.CaseName;
                saveDialog.OverwritePrompt = false;

                if (saveDialog.ShowDialog().GetValueOrDefault())
                {

                    savePath = saveDialog.FileName;
                    fileName = saveDialog.SafeFileName;

                    //如果当前选择的文件路径名称与原有的不同则将源文件删除并保存当前文件（即实现修改名称）
                    if (savePath != selectPath)
                    {
                        //先将文件保存
                        result = CSVFileHelper.SaveScript(selectPath, mv.SrciptDoc.Text);
                        //再将文件改名
                        File.Move(selectPath, savePath);
                    }
                    else
                    {
                        result = CSVFileHelper.SaveScript(savePath, mv.SrciptDoc.Text);
                    }

                    //重新加载项目
                    mv.LoadScriptInfo(mv.SelectProject.FullPath);
                    //将当前案例的选中项设置为刚刚保存的空案例
                    mv.SelectScript = mv.CaseScripts.FirstOrDefault(o => o.FullPath == savePath);
                }
                else
                {
                    //重新加载项目
                    mv.LoadScriptInfo(mv.SelectProject.FullPath);
                    //将当前案例的选中项设置为刚刚保存的空案例
                    mv.SelectScript = mv.CaseScripts.FirstOrDefault(o => o.FullPath == selectPath);
                }
                PrintHelper.Print.AppendLine(result);
            }
            catch (Exception error)
            {
                PrintHelper.Print.AppendLine(error.Message + "\r\n" + error.StackTrace);
            }
        }
        #endregion

        
    }
}
