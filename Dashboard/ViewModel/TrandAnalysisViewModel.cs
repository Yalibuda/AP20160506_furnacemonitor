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

namespace Dashboard.ViewModel
{
    public class TrandAnalysisViewModel : BasicPage
    {
        public TrandAnalysisViewModel()
        {
            Load();
        }

        #region 屬性
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

        /// <summary>
        /// 設定或取得欲繪製的熔爐項目
        /// </summary>
        public string SelectedItemInfoString
        {
            get
            {
                return _selectItemInfoString;
            }
            set
            {
                _selectItemInfoString = value;
                RaisePropertyChanged("SelectedItemInfoString");
            }
        }
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
                if (_siteId != value)
                {
                    _siteId = value;
                    RaisePropertyChanged("SITE_ID");
                    OnSiteChanged();
                }
            }
        }

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

        #endregion

        #region 方法
        /// <summary>
        /// 初始化頁面
        /// </summary>
        protected override void Load()
        {
            DateTime datetime = DateTime.Now;
            EndDate = datetime.Date;
            StartDate = datetime.AddDays(-1).Date;
            EndTimeValue = string.Format("{0:HH:mm}", datetime);
            StartTimeValue = EndTimeValue;

            //基本變數初始值
            _siteId = null;
            Sites = Database.DBQueryTool.GetSiteInfo();

            //Initialize variate
            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += _bgWorker_DoWork;
            _bgWorker.RunWorkerCompleted += _bgWorker_RunWorkerCompleted;
            _bgWorker.WorkerSupportsCancellation = true;
            _bgWorker.WorkerReportsProgress = true;

            _uniRptItems = new ObservableCollection<Model.IReport>();
            _uniRptItems.CollectionChanged += _uniRptItems_CollectionChanged;
            UnivariateContent = new ObservableCollection<UniReportContent>();

            _multiRptItems = new ObservableCollection<Model.IReport>();
            _multiRptItems.CollectionChanged += _multiRptItems_CollectionChanged;
            MultiVariateContent = new ObservableCollection<MultiReportContent>();

            _overviewItems = new ObservableCollection<Model.IReport>();
            _overviewItems.CollectionChanged += _overviewItems_CollectionChanged;
            OverviewTrend = new ObservableCollection<UniReportContent>();

        }
        /// <summary>
        /// 依據設定項更新報表頁面
        /// </summary>
        /// <param name="obj"></param>
        private void UpdatePage(object obj)
        {
            if (!_bgWorker.IsBusy)
            {
                _bgWorker.RunWorkerAsync();

            }
            else
            {
                System.Windows.MessageBox.Show("工作正在執行中，請稍後再執行", "",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
        }
        /// <summary>
        /// 顯示熔爐項目設定的對話框
        /// </summary>
        /// <param name="obj"></param>
        private void ShowItemSelectionDialog(object obj)
        {
            using (View.SelectTrendItemDialogView f = new View.SelectTrendItemDialogView(_selectedItemViewModel))
            {
                f.ShowDialog();
                _spcItemInfoTable = ((SelectTrendItemViewModel)f.DataContext).SPCItemInfoTable;
                if (_spcItemInfoTable != null && _spcItemInfoTable.Rows.Count > 0)
                {
                    string lab = string.Join(",", _spcItemInfoTable.Rows.Cast<DataRow>().Select(r => r.Field<string>("TITLE")));
                    if (lab.Length > 60)
                    {
                        lab = lab.Substring(0, 57) + "...";
                    }
                    SelectedItemInfoString = lab;
                }


            }
        }

        #endregion

        #region 事件方法
        public void OnSiteChanged()
        {
            _selectedItemViewModel = new SelectTrendItemViewModel(); // 如果更改廠別，整個選取項要取消
            _selectedItemViewModel.SITE_ID = this.SITE_ID;
            SelectedItemInfoString = "";
            _spcItemInfoTable = null;
            _uniRptItems.Clear();
            _multiRptItems.Clear();
            _overviewItems.Clear();
            ShowSeparator = false;


        }
        protected override void _bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("Task completed.");
            IsBusy = false;
            if (e.Error != null)
            {
                System.Windows.MessageBox.Show(string.Format("**Error**\t{0}", e.Error.Message), "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        protected override void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;
            if (_spcItemInfoTable == null || _spcItemInfoTable.Rows.Count == 0) return;

            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                _uniRptItems.Clear();
                _multiRptItems.Clear();
                _overviewItems.Clear();
                UnivariateContent.Clear();
                MultiVariateContent.Clear();
                OverviewTrend.Clear();
                ShowSeparator = false;
            });


            lock (_spcItemInfoTable)
            {
                string connString = Database.DBQueryTool.GetConnString();
                StringBuilder query = new StringBuilder();

                DataRow[] spcItemRow = _spcItemInfoTable.Rows.Cast<DataRow>().ToArray();
                List<Model.IReport> rptItems = new List<Model.IReport>();

                DataRow[] spcItemRow_uni = new DataRow[] { };
                DataRow[] spcItemRow_mul = new DataRow[] { };
                if (spcItemRow.Any(x => x.Field<string>("FLAG") == "T2")) spcItemRow_mul = spcItemRow.Where(x => x.Field<string>("FLAG") == "T2").ToArray();
                if (spcItemRow.Any(x => x.Field<string>("FLAG") == "I")) spcItemRow_uni = spcItemRow.Where(x => x.Field<string>("FLAG") == "I").ToArray();

                if (spcItemRow_mul.Length > 0 && spcItemRow_uni.Length > 0) ShowSeparator = true;

                string start = string.Format("{0:yyyy-MM-dd} {1}", StartDate.Date, StartTimeValue);
                string end = string.Format("{0:yyyy-MM-dd} {1}", EndDate.Date, EndTimeValue);

                string tmpDir = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab");
                try
                {
                    Array.ForEach(System.IO.Directory.GetFiles(tmpDir), System.IO.File.Delete); //刪除暫存區所有檔案
                }
                catch
                {
                }

                for (int i = 0; i < spcItemRow_mul.Length; i++)
                {
                    query.Clear();
                    Project.Commands.Delete();
                    Project.Worksheets.Delete();

                    DataRow dr = spcItemRow_mul[i];
                    Model.MultivariateReport rpt = new Model.MultivariateReport();
                    string[] item = dr["ITEM_LIST"].ToString().Split(',');
                    rpt.RawData = Database.DBQueryTool.GetPivotDataForMultivariateChart(_siteId, item, start, end);

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
                            rpt.Execute(_proj);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Minitab run time error\r\n" + ex.Message);
                        }

                        rptItems.Add(rpt);
                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            _multiRptItems.Add(rpt);
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
                int iCnt = 0;
                iCnt = _spcItemInfoTable.Select("FLAG='I'").Count();
                if (iCnt >1)//超過1組資料才要畫整體趨勢圖
                {
                    string[] item = spcItemRow_uni.Select(x => x.Field<string>("ITEM_LIST")).ToArray();
                    Model.OverlayTrendReport rpt = new Model.OverlayTrendReport();
                    string sTime = string.Format("{0:yyyy-MM-dd hh:mm:ss}", start);
                    string eTime = string.Format("{0:yyyy-MM-dd hh:mm:ss}", end);
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
                            throw new Exception("Minitab run time error\r\n" + ex.Message);                      
                        }
                        rptItems.Add(rpt);
                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            _overviewItems.Add(rpt);
                        });
                    }
                    

                }


                for (int i = 0; i < spcItemRow_uni.Length; i++)
                {
                    query.Clear();
                    Project.Commands.Delete();
                    Project.Worksheets.Delete();
                    DataRow dr = spcItemRow_uni[i];
                    Model.UnivariateReport rpt = new Model.UnivariateReport();
                    string[] item = dr["ITEM_LIST"].ToString().Split(',');
                    item = item.Select(x => "'" + x + "'").ToArray();
                    query.Clear();
                    //query.AppendLine("SELECT * FROM VW_FURNACEDATAINSPC");
                    //query.AppendFormat("WHERE FURN_ITEM_INDEX IN ({0}) ", string.Join(",", item));
                    //query.AppendFormat("AND RPT_DATETIME BETWEEN '{0}' AND '{1}'\r\n", start, end);
                    //query.AppendLine("ORDER BY RPT_DATETIME");

                    query.AppendLine(Database.DBQueryTool.GetSQLString_FurnacedataInSPC(SITE_ID, item, start, end));
                    query.AppendLine("SELECT * FROM @tmpfurnacedatainspc order by rpt_datetime");
                    rpt.RawData = Database.DBQueryTool.GetData(query.ToString(), connString);

                    if (rpt.RawData != null && rpt.RawData.Rows.Count > 0)
                    {
                        rpt.Title = dr["TITLE"].ToString();
                        rpt.Flag = dr["FLAG"].ToString();
                        rpt.Execute(_proj);
                        rptItems.Add(rpt);
                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            _uniRptItems.Add(rpt);
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
                                case Dashboard.Model.MtbOType.TABLE:
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
                                case Dashboard.Model.MtbOType.TABLE:
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
            RaisePropertyChanged("UnivariateContent");
        }
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
        private ObservableCollection<Model.IReport> _multiRptItems = null;
        private ObservableCollection<Model.IReport> _uniRptItems = null;
        private ObservableCollection<MultiReportContent> _multivariateContent = null;
        private ObservableCollection<UniReportContent> _univariateContent = null;
        private SelectTrendItemViewModel _selectedItemViewModel = new SelectTrendItemViewModel();
        private DataTable _spcItemInfoTable = null;
        private string _selectItemInfoString = "";
        private bool _separator = false;
        #endregion

        #region Command
        public override ICommand UpdatePageCommand
        {
            get { return new Command.RelayCommand(UpdatePage); }
        }

        public ICommand ShowItemSelectionDialogCommand
        {
            get { return new Command.RelayCommand(ShowItemSelectionDialog); }
        }

        #endregion




    }
}
