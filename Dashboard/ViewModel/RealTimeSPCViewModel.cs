using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TimersTimer = System.Timers.Timer;

namespace Dashboard.ViewModel
{
    public class RealTimeSPCViewModel : BasicPage
    {
        public RealTimeSPCViewModel()
        {
            Load();
        }

        #region 屬性
        /// <summary>
        /// 設定或取得使用的廠別資訊
        /// </summary>
        public override string SITE_ID
        {
            get
            {
                return base.SITE_ID;
            }
            set
            {
                base.SITE_ID = value;
                OnSiteChanged(_siteId);
            }
        }
        /// <summary>
        /// 取得報表結果的List
        /// </summary>
        public override List<Model.IReport> ReportItems
        {
            get
            {
                List<Model.IReport> rptItems = new List<Model.IReport>();
                if (_multiRptItems != null)
                {
                    foreach (var item in _multiRptItems)
                    {
                        rptItems.Add(item);
                    }
                }
                if (_overviewItems != null)
                {
                    foreach (var item in _overviewItems)
                    {
                        rptItems.Add(item);
                    }
                }
                if (_uniRptItems != null)
                {
                    foreach (var item in _uniRptItems)
                    {
                        rptItems.Add(item);
                    }
                }
                return rptItems;
            }
        }
        /// <summary>
        /// 取得是否可切換廠別
        /// </summary>
        public bool EnableSiteChange
        {
            get { return _enableChangeSite; }
            set
            {
                _enableChangeSite = value;
                RaisePropertyChanged("EnableSiteChange");
            }
        }
        private bool _enableChangeSite = true;

        /// <summary>
        /// 取得是否可停止工作
        /// </summary>
        public bool EnableCancelDoWork
        {
            get { return _enableCancelDoWork; }
            set
            {
                _enableCancelDoWork = value;
                RaisePropertyChanged("EnableCancelDoWork");
            }
        }
        private bool _enableCancelDoWork = false;

        /// <summary>
        /// 取得是否要顯示多變量管制圖和單變量管制圖之間的分隔線
        /// </summary>
        public bool ShowSeparator
        {
            get { return _separator; }
            set
            {
                _separator = value;
                RaisePropertyChanged("ShowSeparator");
            }
        }
        private bool _separator = false;


        /// <summary>
        /// 取得多變量報表的圖、表、標題、資料
        /// </summary>
        public ObservableCollection<MultiReportContent> MultiVariateContent
        {
            get { return _multivariateContent; }
            set
            {
                _multivariateContent = value;
                _multivariateContent.CollectionChanged += _multivariateContent_CollectionChanged;
                RaisePropertyChanged("MultiVariateContent");
            }
        }
        private ObservableCollection<Model.IReport> _multiRptItems = null;
        private ObservableCollection<MultiReportContent> _multivariateContent = null;


        /// <summary>
        /// 取得單變量報表
        /// </summary>
        public ObservableCollection<UniReportContent> UnivariateContent
        {
            get
            {
                return _univariateContent;
            }
            private set
            {
                _univariateContent = value;
                _univariateContent.CollectionChanged += _univariateContent_CollectionChanged;
                RaisePropertyChanged("UnivariateChart");
            }
        }
        private ObservableCollection<Model.IReport> _uniRptItems = null;
        private ObservableCollection<UniReportContent> _univariateContent = null;

        public ObservableCollection<UniReportContent> OverviewTrend
        {
            get { return _overviewContent; }
            private set
            {
                _overviewContent = value;
                _overviewContent.CollectionChanged += _overviewContent_CollectionChanged;
                RaisePropertyChanged("OverviewTrend");
            }
        }
        private ObservableCollection<Model.IReport> _overviewItems = null;
        private ObservableCollection<UniReportContent> _overviewContent = null;

        #endregion

        #region 方法
        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Load()
        {
            // 計時器相關
            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += _bgWorker_DoWork;
            _bgWorker.WorkerReportsProgress = true;
            _bgWorker.WorkerSupportsCancellation = true;
            _bgWorker.RunWorkerCompleted += _bgWorker_RunWorkerCompleted;

            _doWorkTimer = new TimersTimer();
            _doWorkTimer.Interval = 720000; //12mins
            _doWorkTimer.Elapsed += _doWorkTimer_Elapsed;

            //_updateTimer = new DispatcherTimer();
            //_updateTimer.Interval = new TimeSpan(0, 0, 1); //1mins
            //_updateTimer.Tick += _updateTimer_Tick;


            //基本變數初始值
            _siteId = null;
            Sites = Database.DBQueryTool.GetSiteInfo();
        }

        /// <summary>
        /// 開始即時報表功能
        /// </summary>
        private void Start()
        {
            if (_proj == null) throw new ArgumentNullException("Start: _proj is null.");
            _doWorkTimer.Start();
            _doWorkTimer_Elapsed(0, null);

        }

        public override void StopCurrentWork()
        {
            base.StopCurrentWork();
            _doWorkTimer.Stop();
        }

        #endregion

        #region 事件方法
        private void _doWorkTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("doworker timer...");
            if (!_bgWorker.IsBusy)
            {
                EnableSiteChange = false;
                EnableCancelDoWork = true;
                Minitab.ReStart();
                Project = Minitab.Project; //不好的做法，產生相依性
                _bgWorker.RunWorkerAsync();
            }
            else
            {
                Console.WriteLine("bgworker is busy...");
            }
        }

        protected override void _bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableSiteChange = true;
            EnableCancelDoWork = false;
            IsBusy = false;
            _initialRun = false;
            if (e.Error != null)
            {
                System.Windows.MessageBox.Show(string.Format("**Error**\t{0}", e.Error.Message), "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        protected override void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_initialRun) IsBusy = true;
            if (_spcItemInfo == null || _spcItemInfo.Rows.Count == 0) return;

            
            lock (_spcItemInfo)
            {
                lock (_uniRptItems)
                {
                    lock (_multiRptItems)
                    {
                        string connString = Database.DBQueryTool.GetConnString();
                        DateTime end = DateTime.Now;                        
                        DateTime start = end.AddHours(-24);
                        StringBuilder query = new StringBuilder();

                        DataRow[] spcItemRow = _spcItemInfo.Rows.Cast<DataRow>().ToArray();
                        List<Model.IReport> rptItems = new List<Model.IReport>();

                        DataRow[] spcItemRow_uni = new DataRow[] { };
                        DataRow[] spcItemRow_mul = new DataRow[] { };
                        if (spcItemRow.Any(x => x.Field<string>("FLAG") == "T2")) spcItemRow_mul = spcItemRow.Where(x => x.Field<string>("FLAG") == "T2").ToArray();
                        if (spcItemRow.Any(x => x.Field<string>("FLAG") == "I")) spcItemRow_uni = spcItemRow.Where(x => x.Field<string>("FLAG") == "I").ToArray();

                        //Check whether the length of _uniRptItems is equal to spcItemRow_uni and 
                        //the length of _multiRptItem is equal to spcItemRow_mul. 
                        if (_multiRptItems.Count != spcItemRow_mul.Length || _uniRptItems.Count != spcItemRow_uni.Length) return; //something have been modified by other thread.

                        string tmpDir = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab");
                        try
                        {
                            Array.ForEach(System.IO.Directory.GetFiles(tmpDir), System.IO.File.Delete); //刪除暫存區所有檔案
                        }
                        catch
                        {
                            //do nothing...反正這次刪不掉下次再刪
                        }


                        for (int i = 0; i < spcItemRow_mul.Length; i++)
                        {
                            if (_bgWorker.CancellationPending)
                            {
                                Console.WriteLine("**We need to cancel the work...call _bgWorker_RunWorkerCompleted**");
                                return;

                            }
                            query.Clear();
                            DataRow dr = spcItemRow_mul[i];
                            Model.MultivariateReport rpt = new Model.MultivariateReport();
                            string[] item = dr["ITEM_LIST"].ToString().Split(',');
                            string sTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", start);
                            string eTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", end);
                            rpt.RawData = Database.DBQueryTool.GetPivotDataForMultivariateChart(_siteId, item, sTime, eTime);
                            if (_multiRptItems[i] != null && Database.DBQueryTool.CompareDataTableRow(_multiRptItems[i].RawData, rpt.RawData))
                            {
                                continue; //資料相等就不執行這次結果
                            }

                            //搜尋是否有參數
                            DataTable paraDetTable = null;
                            query.AppendLine("SELECT a.CHART_PARA_INDEX, b.FLAG, b.ROWNO, b.COLNO, b.VALUE, a.ITEM_LIST FROM CHART_PARAMETER a LEFT JOIN PARAMETER_DETAIL b");
                            query.AppendLine("ON a.CHART_PARA_INDEX = b.CHART_PARA_INDEX");
                            query.AppendFormat("WHERE a.ITEM_LIST='{0}'", dr["ITEM_LIST"].ToString());
                            paraDetTable = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString());
                            rpt.Parameters = paraDetTable;

                            if (rpt.RawData != null && rpt.RawData.Rows.Count > 0)
                            {
                                rpt.Title = dr["TITLE"].ToString();
                                rpt.Flag = dr["FLAG"].ToString();
                                try
                                {
                                    rpt.Execute(Project);
                                }                                   
                                catch (Exception ex)
                                {                                    
                                    //進到下一個迴圈
                                    continue;
                                    //throw new Exception("Minitab run time error\r\n" + ex.Message);
                                }
                                rptItems.Add(rpt);
                                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                                {
                                    _multiRptItems[i] = rpt;
                                });
                            }
                            if (_bgWorker.CancellationPending)
                            {
                                Console.WriteLine("**We need to cancel the work...call _bgWorker_RunWorkerCompleted**");
                                return;
                            }
                        }

                        /* 
                         * 針對單變量取得時間內所有資料 (不用處理SPC的工作)
                         * 
                         */
                        for (int i = 0; i < 1; i++)
                        {
                            string[] item = spcItemRow_uni.Select(x => x.Field<string>("ITEM_LIST")).ToArray();
                            Model.OverlayTrendReport rpt = new Model.OverlayTrendReport();
                            string sTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", start);
                            string eTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", end);
                            rpt.RawData = Database.DBQueryTool.GetPivotDataForMultivariateChart(SITE_ID, item, sTime, eTime);
                            if (rpt.RawData != null && rpt.RawData.Rows.Count > 0)
                            {
                                rpt.Title = "整體趨勢圖";
                                try
                                {
                                    rpt.Execute(Project);
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                                rptItems.Add(rpt);
                                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                                {
                                    _overviewItems[i] = rpt;
                                });
                            }
                        }

                        for (int i = 0; i < spcItemRow_uni.Length; i++)
                        {
                            if (_bgWorker.CancellationPending)
                            {
                                Console.WriteLine("**We need to cancel the work...call _bgWorker_RunWorkerCompleted**");
                                return;
                            }
                            query.Clear();
                            DataRow dr = spcItemRow_uni[i];
                            Model.UnivariateReport rpt = new Model.UnivariateReport();
                            string[] item = dr["ITEM_LIST"].ToString().Split(',');
                            item = item.Select(x => "'" + x + "'").ToArray();
                            query.Clear();                            
                            //query.AppendLine("SELECT * FROM VW_FURNACEDATAINSPC");
                            //query.AppendFormat("WHERE FURN_ITEM_INDEX IN ({0}) ", string.Join(",", item));
                            //string sTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", start);
                            //string eTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", end);
                            //query.AppendFormat("AND RPT_DATETIME BETWEEN '{0}' AND '{1}'\r\n", sTime, eTime);
                            //query.AppendLine("ORDER BY RPT_DATETIME");
                            string sTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", start);
                            string eTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", end);
                            query.AppendLine(Database.DBQueryTool.GetSQLString_FurnacedataInSPC(SITE_ID, item, sTime, eTime));
                            query.AppendLine("SELECT * FROM @tmpfurnacedatainspc order by rpt_datetime");

                            rpt.RawData = Database.DBQueryTool.GetData(query.ToString(), connString);
                            if (_uniRptItems[i] != null && Database.DBQueryTool.CompareDataTableRow(_uniRptItems[i].RawData, rpt.RawData))
                            {
                                continue; //資料相等就不執行這次結果
                            }

                            if (rpt.RawData != null && rpt.RawData.Rows.Count > 0)
                            {
                                rpt.Title = dr["TITLE"].ToString();
                                rpt.Flag = dr["FLAG"].ToString();
                                try
                                {
                                    rpt.Execute(Project);
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                    //throw new Exception("Minitab run time error\r\n" + ex.Message);
                                }
                                rptItems.Add(rpt);
                                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                                {
                                    _uniRptItems[i] = rpt;
                                });
                            }
                            if (_bgWorker.CancellationPending)
                            {
                                Console.WriteLine("**We need to cancel the work...call _bgWorker_RunWorkerCompleted**");
                                return;
                            }

                        }
                    }
                }
            }



        }

        /// <summary>
        /// 廠別改變之後需要重刷內容
        /// <param name="obj"></param>
        /// </summary>
        public void OnSiteChanged(object obj)
        {
            Console.WriteLine(obj.ToString());
            //Stop current work...
            StopCurrentWork();

            if (_multiRptItems != null) _multiRptItems.Clear();
            if (_uniRptItems != null) _uniRptItems.Clear();
            ShowSeparator = false;
            _initialRun = true;

            // 抓 spc_item
            if (_siteId != null && _siteId != string.Empty)
            {
                _spcItemInfo = Database.DBQueryTool.GetRealTimeSPCItems(_siteId);
            }



            if (_spcItemInfo != null && _spcItemInfo.Rows.Count > 0)
            {
                int iCnt = 0, t2Cnt = 0;
                t2Cnt = _spcItemInfo.Select("FLAG='T2'").Count();
                iCnt = _spcItemInfo.Select("FLAG='I'").Count();

                if (t2Cnt * iCnt != 0) ShowSeparator = true;

                _multiRptItems = new ObservableCollection<Model.IReport>(new Model.IReport[t2Cnt]);
                _multiRptItems.CollectionChanged += _multiRptItems_CollectionChanged;
                _multivariateContent = new ObservableCollection<MultiReportContent>(new MultiReportContent[t2Cnt]);
                _multivariateContent.CollectionChanged += _multivariateContent_CollectionChanged;

                _uniRptItems = new ObservableCollection<Model.IReport>(new Model.IReport[iCnt]);
                _uniRptItems.CollectionChanged += _uniRptItems_CollectionChanged;
                _univariateContent = new ObservableCollection<UniReportContent>(new UniReportContent[iCnt]);
                _univariateContent.CollectionChanged += _univariateContent_CollectionChanged;

                if (iCnt > 0)
                {
                    _overviewItems = new ObservableCollection<Model.IReport>(new Model.IReport[1]);
                    _overviewItems.CollectionChanged += _overviewItems_CollectionChanged;
                    _overviewContent = new ObservableCollection<UniReportContent>(new UniReportContent[1]);
                    _overviewContent.CollectionChanged += _overviewContent_CollectionChanged;

                }



                Start();
            }
            // 更新 rpt_items
        }

        /// <summary>
        /// 多變量報表變更時的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _multiRptItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            List<Model.IReport> newItems;
            if (e.NewItems != null)
            {
                newItems = e.NewItems.Cast<Model.IReport>().ToList();
            }
            else
            {
                newItems = new List<Model.IReport>();
            }

            List<Model.IReport> oldItems;
            if (e.OldItems != null)
            {
                oldItems = e.OldItems.Cast<Model.IReport>().ToList();
            }
            else
            {
                oldItems = new List<Model.IReport>();
            }
            MultiReportContent tmpContent;

            #region 針對動作修改對應的 _multivariateContent 內容
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new MultiReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                case Dashboard.Model.MtbOType.TABLE:
                                    tmpContent.DecomTable = Tool.BinaryToDataTable(output.OutputInByteArr);
                                    if (tmpContent.DecomTable != null && tmpContent.DecomTable.Rows.Count > 0)
                                    {
                                        tmpContent.ShowTable = true;
                                    }
                                    else
                                    {
                                        tmpContent.ShowTable = false;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        MultiVariateContent.Add(tmpContent);
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    MultiVariateContent.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new MultiReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                case Dashboard.Model.MtbOType.TABLE:
                                    tmpContent.DecomTable = Tool.BinaryToDataTable(output.OutputInByteArr);
                                    if (tmpContent.DecomTable != null && tmpContent.DecomTable.Rows.Count > 0)
                                    {
                                        tmpContent.ShowTable = true;
                                    }
                                    else
                                    {
                                        tmpContent.ShowTable = false;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        MultiVariateContent[e.OldStartingIndex + i] = tmpContent;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    List<MultiReportContent> tmpLst = new List<MultiReportContent>();
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new MultiReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                case Dashboard.Model.MtbOType.TABLE:
                                    tmpContent.DecomTable = Tool.BinaryToDataTable(output.OutputInByteArr);
                                    if (tmpContent.DecomTable != null && tmpContent.DecomTable.Rows.Count > 0)
                                    {
                                        tmpContent.ShowTable = true;
                                    }
                                    else
                                    {
                                        tmpContent.ShowTable = false;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        tmpLst.Add(tmpContent);
                    }
                    MultiVariateContent.Clear();
                    MultiVariateContent = new ObservableCollection<MultiReportContent>(tmpLst);
                    break;
                default:
                    break;
            }
            #endregion
        }
        private void _multivariateContent_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("MultiVariateContent");
        }

        /// <summary>
        /// 單變量報表變更時的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _uniRptItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            List<Model.IReport> newItems;
            if (e.NewItems != null)
            {
                newItems = e.NewItems.Cast<Model.IReport>().ToList();
            }
            else
            {
                newItems = new List<Model.IReport>();
            }
            List<Model.IReport> oldItems;
            if (e.OldItems != null)
            {
                oldItems = e.OldItems.Cast<Model.IReport>().ToList();
            }
            else
            {
                oldItems = new List<Model.IReport>();
            }
            UniReportContent tmpContent;

            #region 針對動作修改對應的 _univariateContent 內容
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new UniReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                case Dashboard.Model.MtbOType.PARAGRAPH:
                                    tmpContent.Summary = System.Text.Encoding.UTF8.GetString(output.OutputInByteArr);
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        UnivariateContent.Add(tmpContent);
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    UnivariateContent.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new UniReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                case Dashboard.Model.MtbOType.PARAGRAPH:
                                    tmpContent.Summary = System.Text.Encoding.UTF8.GetString(output.OutputInByteArr);
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        UnivariateContent[e.OldStartingIndex + i] = tmpContent;

                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    List<UniReportContent> tmpLst = new List<UniReportContent>();
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new UniReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                case Dashboard.Model.MtbOType.PARAGRAPH:
                                    tmpContent.Summary = System.Text.Encoding.UTF8.GetString(output.OutputInByteArr);
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        tmpLst.Add(tmpContent);
                    }
                    UnivariateContent.Clear();
                    UnivariateContent = new ObservableCollection<UniReportContent>(tmpLst);
                    break;
                default:
                    break;
            }
            #endregion
        }
        private void _univariateContent_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() => RaisePropertyChanged("UnivariateContent")));
            RaisePropertyChanged("UnivariateContent");
            RaisePropertyChanged("Chart");

        }

        /// <summary>
        /// 彙整趨勢圖變更時的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _overviewItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            List<Model.IReport> newItems;
            if (e.NewItems != null)
            {
                newItems = e.NewItems.Cast<Model.IReport>().ToList();
            }
            else
            {
                newItems = new List<Model.IReport>();
            }
            List<Model.IReport> oldItems;
            if (e.OldItems != null)
            {
                oldItems = e.OldItems.Cast<Model.IReport>().ToList();
            }
            else
            {
                oldItems = new List<Model.IReport>();
            }
            UniReportContent tmpContent;

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new UniReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);                                    
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        OverviewTrend.Add(tmpContent);
                    }

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    UnivariateContent.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new UniReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        OverviewTrend[e.OldStartingIndex + i] = tmpContent;

                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    List<UniReportContent> tmpLst = new List<UniReportContent>();
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new UniReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        tmpLst.Add(tmpContent);
                    }
                    OverviewTrend.Clear();
                    OverviewTrend = new ObservableCollection<UniReportContent>(tmpLst);
                    break;
                default:
                    break;
            }

        }
        private void _overviewContent_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("OverviewTrend");
        }
        #endregion

        #region 變數

        private TimersTimer _doWorkTimer = null;
        private DataTable _spcItemInfo = null;
        private bool _initialRun = true;
        //private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;
        #endregion

        #region Command

        /// <summary>
        /// 頁面更新
        /// </summary>
        public override ICommand UpdatePageCommand
        {
            get { return new Command.RelayCommand(OnSiteChanged); }
        }
        #endregion





    }


}
