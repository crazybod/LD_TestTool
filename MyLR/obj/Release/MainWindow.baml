   M S B A M L   `   `   `  ���� ?  ;MyLR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:MyLRMyLR      MyLR.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     ] YICSharpCode.AvalonEdit, Version=5.0.3.0, Culture=neutral, PublicKeyToken=9cc39be672370310?
avalonEdit.http://icsharpcode.net/sharpdevelop/avalonedit  Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  localclr-namespace:MyLR    � Title$  	左右手��.���� .@��� $��压力测试��.��� ���	#FFF1E9D7 J� RenderTransformOrigin$ 0.512,0.575&�$��0,0,-4,-0.4q� � RowDefinitions �� $
��Auto ��� $	��280 ��� $��90 ��� $��* � � ColumnDefinitions �� $
��81.8 ��� $	��520 ��� $��5 ��� $��* �.+��� $��0��$��0��$��
10,0,0.4,0q� �� Orientation$ 
Horizontal=�$��2��.+��� $��70��$��4 5 0 0q�$
��Left=�$��LoadCase������ *
LoadCase+�� $��70��$��4 5 0 0q�$
��Left=�$��Start������ *
StartRun+{��� *StartEnable+�� $��70��$��4 5 0 0q�$
��Left=�$
��Stop������ *	StopRun+�� $&�� 案例在case.xml文件中维护��$��	100 5 0 0q�&   System.Windows.Controls.DataGrid  $��1��$��0��   CanUserAddRows .     CanUserDeleteRows .     AutoGenerateColumns .  ��.    GridLinesVisibility$ Vertical=�$��3��$��0,0.2,4.4,56q����� *
CaseGroups+	 �� SelectedItem	 �� *
SelectCase+
   RowStyle
 ��  �� 
TargetType)  #System.Windows.Controls.DataGridRow"  .���  �� Property � �� �� Value$ 12(� �� Triggers P�  P� Property �@��AlternationIndex P� Value$ 0��. ���  � �� �#cbedfaP�  �@��AlternationIndex$ 1��. ���  � �� �	#FFFFFFFF   Columns 4  .System.Windows.Controls.DataGridCheckBoxColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ 选择��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Check+0  *System.Windows.Controls.DataGridTextColumn  $ 案例名称�� �� *	GroupName+  $ 发送成功��   
IsReadOnly .  �� *SendOkCount+  $ 发送失败�� .  �� *SendFailCount+  $ 	未处理�� .  �� *UnDealCount+  $ 应答成功�� .  �� *
RecOkCount+  $ 应答失败�� .  �� *RecFailCount+  $ 发送次数(0无限)�� �� *	LoopCount+  $ 差值�� .  �� *Diff+  $ 积压上限�� �� *	UpStage+  $ 积压下限�� �� *	DownStage+  $ 脚本路径�� �� *
FileName+  $��1��$��0�� .   .   .  ��. $ Vertical=�$��2��$��0,226,0,-0.4q�$��3������ *ReplyRateInfos+
 �� "  .���  � ��$ 12(� P�  �@��AlternationIndex$ 0��. ���  � �� �#cbedfaP�  �@��AlternationIndex$ 1��. ���  � �� �	#FFFFFFFF   $ 	功能号�� .  �� *
functionId+  $! 当前应答频率(条/秒)�� .  �� *currentRate+  $! 最大应答频率(条/秒)�� .  �� *	maxRate+  $! 最小应答频率(条/秒)�� .  �� *	minRate+  $ 应答数量�� .  �� *
replyCount+�� $��1��$��2��$��2��$��Stretch=����	#FF98E415$��1.6,0,2,-0.4q�$��2���� $��3��$��1��$��
0,0,0,-0.4q�$��4��.+�   .   .   .  $	��451��$
��Left=�$
��Auto��$��0,0,-11.8,0q����� *CaseFunctions+   $ 选择�� .   �� *Check+  $ 	功能号�� .  �� *FunID+  $ 功能名称�� .  �� *	FunName+�� $��脚本回归测试��.��� ���	#FFF1E9D7 �� $
��Auto ��� $��* ��� $	��240 � �� $	��160 ��� $��* ��� $��1 �.+��� $(��"案例在script.xml文件中维护��$��0 5 0 0q�  $��1��$��0��$��1��   
IsReadOnly .  .   .   .  $ Vertical=����� *ProjectInfos+	 �� *SelectProject+   $ 项目�� �� *ProjectName+  $ 路径�� �� *
FullPath+  -   $��dgCase��$��2��$��0��$��1�� .  .   .   .  $ Vertical=����� *CaseScripts+	 �� *SelectScript+   $ 案例�� �� *
CaseName+  $ 路径�� �� *
FullPath+�� $��0��$��1��$��10 0 0 0q�$ 
Horizontal=�.+��� $��70��$��25��$��4 0 0 0q�$
��Left=�$	��Run������ *	RunScript+�� $��70��$��25��$��4 0 0 0q�$
��Left=�$��Complise������ *ComplieScript+�� -   $��	btnRunAll��$��60��$��25��$��40,0,0,-0.2q�$
��Left=�$��一键运行���� -   $��
SaveScript��$��45��$��25��$��
4,0,0,-0.2q�$
��Left=�$��保存���� -   $��CreateScript��$��57��$��25��$��
4,0,0,-0.2q�$
��Left=�$��新建案例���� -   $��DeleteScript��$��56��$��25��$��
4,0,0,-0.2q�$
��Left=�$��删除案例���� -   $��btnDeleteAll��$��81��$��25��$��
4,0,0,-0.2q�$
��Left=�$��批量删除案例���� -   $��OpenProject��$��58��$��25��$��
4,0,0,-0.2q�$
��Left=�$��选择项目���� -   $��SaveScriptTo��$��49��$��25��$��
4,0,0,-0.2q�$
��Left=�$��	另存为���� $��脚本��$��1��$��1��.��'  !ICSharpCode.AvalonEdit.TextEditor  -	   $��
SrciptData��   SyntaxHighlightingM  GICSharpCode.AvalonEdit.Highlighting.HighlightingDefinitionTypeConverter$ C# $��Consolas)�$
��10pt(�"   VerticalScrollBarVisibility$ Visible=�   ShowLineNumbers .    Document �� *	SrciptDoc+   Text. �� $��输出��$��1��$��2��$��0,29.6,0.2,10.4q�.���� .+�� $	��176��$��	0,0,0.4,0q� �� $��30 ��� $��* �.+��� $��0��$��40��$��20��$��	3,8,0,2.4q�$
��Left=�$��清空������ *
ClearPrint+�� $��0��$��69��$��20��$��503,8,0,2.4q�$
��Left=�$��结果另存为������ *SaveOutCome+�� -
    ��Name$ 	PrintData�� �� 
IsReadOnly . " �� VerticalScrollBarVisibility$
 Auto=�$��10(�$��0,30,0,10.4q�$��2��. �/� ��.  /� 
LineHeight$ 1���� $��行情回放��.��� ���	#FFF1E9D7$��
0,2,0,-2.4q� �� $
��Auto ��� $��* ��� $
��Auto � �� $
��16.8 ��� $��80 ��� $
��30.4 ��� $
��33.2 ��� $	��27* ��� $
��568* ��� $��1 �.+�  $��5��$��0,0,27,10.4q�$��2�� .   .   .  ���� *
StockInfos+   $ 市场��   Width5	  /System.Windows.Controls.DataGridLengthConverter$ 80	  .  �� *	StockType+  $ 券码��$ 90	  .  �� *	StockNo+�� -     ��Name$  btnStart��$��运行��$��5��$
��Left=�$��
317,10,0,0q�$��1��$	��Top=�$��42��$��25���� -   $  btnRun��$��取消暂停��$��5��$
��Left=�$��
410,10,0,0q�$��1��$	��Top=�$��57��$��25���� -   $  btnPause��$��暂停��$��5��$
��Left=�$��
366,10,0,0q�$��2��$	��Top=�$��38��$��25���� -   $  btnReset��$��重置��$��5��$
��Left=�$��
473,10,0,0q�$��2��$	��Top=�$��38��$��25���� -   ! ��Name$! cvsDraw��$��5��$
��Left=�$	��348��$��
10,120,0,0q�$��2��$	��Top=�$	��548���� -   " ��Name$" txtTimes��$��5��$
��Left=�$��23��$��1��$��	59,10,0,0q�$��2��# �� TextWrapping$
# Wrap=�$	��Top=�$��70���� -   $ ��Name$$ label��$��加速倍数��$��5��$
��Left=�$��2,11,0,0q�$��2��$	��Top=��� -   $$ label1��$��券码条数��$��5��$
��Left=�$��	148,9,0,0q�$��2��$	��Top=��� -   $" txtStockCountLimit��$��5��$
��Left=�$��23��$��
211,10,0,0q�$	��100��$
# Wrap=�$	��Top=�$��69��$��2���� -   $" 	txtForbid��$��5��$
��Left=�$��23��$��	10,64,0,0q�$
# Wrap=�$	��Top=�$	��219��$��2���� -   $$ label2��$>��8停止发送以下券码行情(券码之间以'/'隔开)��$��5��$
��Left=�$��5,40,0,0q�$��1��$	��Top=�$	��239���� -   $" txtError��$��5��$
��Left=�$��23��$��
258,64,0,0q�$
# Wrap=�$	��Top=�$	��175��$��2��$ 0.616,-0.017&��� -   $$ label2_Copy��$2��,异常行情券码(券码之间以'/'隔开)��$��5��$
��Left=�$��
254,40,0,0q�$��1��$	��Top=�$	��189���� -   $" txtUpDownLimit��$��10��$��5��$
��Left=�$��23��$��
459,64,0,0q�$
# Wrap=�$	��Top=�$��71��$��2��$ 0.616,-0.017&��� -   $$ lable4��$��涨跌幅比例��$��5��$
��Left=�$��
459,40,0,0q�$��1��$	��Top=�$��71��$ 2.482,-0.797&�