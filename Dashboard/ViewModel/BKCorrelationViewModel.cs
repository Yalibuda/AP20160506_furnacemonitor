﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;

namespace Dashboard.ViewModel
{
    public class BKCorrelationViewModel : BasicPage
    {
        public BKCorrelationViewModel()
        {
            Load();
        }

        #region Properties

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

        public string SelectedItemInfoString
        {
            get { return _selectItemInfoString; }
            private set
            {
                _selectItemInfoString = value;
                RaisePropertyChanged("SelectedItemInfoString");
            }
        }

        public ObservableCollection<CorrReportContent> CorrContent
        {
            get { return _corrContent; }
            set
            {
                _corrContent = value;
                _corrContent.CollectionChanged += _corrContent_CollectionChanged;
                RaisePropertyChanged("CorrContent");
            }
        }

        public override List<Model.IReport> ReportItems
        {
            get
            {
                List<Model.IReport> rptItems = new List<Model.IReport>();
                if (_corrRptItems != null)
                {
                    foreach (var item in _corrRptItems)
                    {
                        rptItems.Add(item);
                    }
                }
                return rptItems;
            }
        }

        public bool IsByPosition
        {
            get { return _isByPosition; }
            set
            {
                _isByPosition = value;
                RaisePropertyChanged("IsByPosition");

            }
        }
        private bool _isByPosition = false;
        #endregion

        #region 方法
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

            _corrRptItems = new ObservableCollection<Model.IReport>();
            _corrRptItems.CollectionChanged += _corrRptItems_CollectionChanged;
            CorrContent = new ObservableCollection<CorrReportContent>();
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
            using (View.SelectItemView f = new View.SelectItemView(_selectedItemViewModel))
            {
                f.ShowDialog();
                _itemInfoTable = ((SelectTrendItemViewModel)f.DataContext).SPCItemInfoTable;
                if (_itemInfoTable != null && _itemInfoTable.Rows.Count > 0)
                {
                    string lab = string.Join(",", _itemInfoTable.Rows.Cast<DataRow>().Select(r => r.Field<string>("TITLE")));
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
            _itemInfoTable = null;

        }
        public void OnByPositionChanged()
        {

        }
        protected override void _bgWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            if (e.Error != null)
            {
                if (e.Error is ArgumentNullException)
                {
                    System.Windows.MessageBox.Show(e.Error.Message, "",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
                else
                {
                    System.Windows.MessageBox.Show(string.Format("**Error**\t{0}", e.Error.Message), "",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
        protected override void _bgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            IsBusy = true;
            if (_itemInfoTable == null || _itemInfoTable.Rows.Count == 0) return;

            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                _corrRptItems.Clear();
                CorrContent.Clear();
            });

            lock (_itemInfoTable)
            {
                string[] itemList = _itemInfoTable.Rows.Cast<DataRow>().Select(x => x.Field<string>("ITEM_LIST")).ToArray();
                string start = string.Format("{0:yyyy-MM-dd} {1}", StartDate.Date, StartTimeValue);
                string end = string.Format("{0:yyyy-MM-dd} {1}", EndDate.Date, EndTimeValue);
                DataTable rawdata;
                Model.BkCorrelation rpt;
                if (IsByPosition)
                {
                    rawdata = Database.DBQueryTool.GetPivotDataForMulBKCorrelation(SITE_ID, itemList, start, end);
                    rpt = new Model.BKCorrelation2();
                }
                else
                {
                    //rawdata = OpenCSV(@"C:\Users\MinitabTeam\Downloads\20190104.csv");
                    rawdata = Database.DBQueryTool.GetPivotDataForBKCorrelation(SITE_ID, itemList, start, end);
                    rpt = new Model.BkCorrelation();
                }

                string tmpDir = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab");
                Array.ForEach(System.IO.Directory.GetFiles(tmpDir), System.IO.File.Delete); //刪除暫存區所有檔案


                rpt.RawData = rawdata;
                if (rpt.RawData != null && rpt.RawData.Rows.Count > 0)
                {
                    string[] itemNames = Database.DBQueryTool.GetFurnNameByItemList(string.Join(",", itemList));
                    rpt.Title = string.Join(", ", itemNames);
                    try
                    {
                        rpt.Execute(Project);
                    }
                    catch (ArgumentNullException argnullex)
                    {
                        throw new ArgumentNullException(argnullex.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Minitab run time error\r\n" + ex.Message);
                    }
                    App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                    {
                        //List<Model.IReport> _rptLst = new List<Model.IReport>();

                        if (rpt is Model.BKCorrelation2)
                        {
                            List<string> colNames = rawdata.Columns.Cast<DataColumn>()
                                .Select(x => x.ColumnName).Where(x => x != "SITE_ID" & x != "TIMESTAMP" & x != "RPT_TIMEHOUR" & x != "GROUP_ID").ToList();

                            /*
                             * 為了讓 View 的 ItemControl 顯示所有的內容且保有彈性，最後決定將原來的 rpt 依內容分拆
                             * 出多個 rpt 物件，每一個 rpt 只包含一個 IReport 物件，並且有獨立的 Title 名稱。
                             * Raw data 就指定放到第一個分拆的 rpt，讓匯出資料表功能可正常運作。
                             * 
                             */ 
                            Model.IRptOutput item;
                            for (int i = 0; i < rpt.Contents.Count; i++)
                            {
                                item = rpt.Contents[i];
                                Model.BKCorrelation2 _tmpRpt = new Model.BKCorrelation2();
                                
                                if (i == 0) _tmpRpt.RawData = rpt.RawData; 

                                _tmpRpt.Flag = rpt.Flag;
                                _tmpRpt.AddContent(item);
                                //重新定義 Title
                                if (item.Tag.ToString() == "Trend")
                                {
                                    _tmpRpt.Title = colNames[i] + "時間趨勢圖";
                                }
                                else if (item.Tag.ToString() == "Scatter")
                                {
                                    _tmpRpt.Title = "各紡位 vs" + rpt.Title + "散佈圖";
                                }
                                else
                                {
                                    _tmpRpt.Title = "各紡位 vs" + rpt.Title + "相關係數表";
                                }

                                _corrRptItems.Add(_tmpRpt);


                            }

                        }
                        else
                        {
                            _corrRptItems.Add(rpt);
                        }


                        //
                    });
                }
            }

        }
        private void _corrRptItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            CorrReportContent tmpContent;
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new CorrReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    if (output.Tag.ToString() == "Trend")
                                    {
                                        tmpContent.TrendChart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                        tmpContent.VisibilityOfTrendChart = true;
                                    }
                                    else
                                    {
                                        tmpContent.ScatterPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                        tmpContent.VisibilityOfScatterPlot = true;
                                    }
                                    break;
                                case Dashboard.Model.MtbOType.TABLE:
                                    tmpContent.CorrTable = Tool.BinaryToDataTable(output.OutputInByteArr);
                                    tmpContent.ShowTable = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        CorrContent.Add(tmpContent);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    CorrContent.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new CorrReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    if (output.Tag.ToString() == "Trend")
                                    {
                                        tmpContent.TrendChart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    }
                                    else
                                    {
                                        tmpContent.ScatterPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    }
                                    break;
                                case Dashboard.Model.MtbOType.TABLE:
                                    tmpContent.CorrTable = Tool.BinaryToDataTable(output.OutputInByteArr);
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        CorrContent[e.OldStartingIndex + i] = tmpContent;

                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                default:
                    break;
            }
        }
        private void _corrContent_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("CorrContent");
        }
        #endregion

        #region 變數
        private ObservableCollection<Model.IReport> _corrRptItems = null;
        private ObservableCollection<CorrReportContent> _corrContent = null;
        private SelectTrendItemViewModel _selectedItemViewModel = new SelectTrendItemViewModel();
        private DataTable _itemInfoTable = null;
        private string _selectItemInfoString = "";
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

        #region read csv closing
        //public static DataTable OpenCSV(string filePath)
        //{
        //    DataTable dt = new DataTable();
        //    FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        //    StreamReader sr = new StreamReader(fs, Encoding.UTF8);
        //    //StreamReader sr = new StreamReader(fs, encoding);
        //    //string fileContent = sr.ReadToEnd();
        //    //記錄每次讀取的一行記錄
        //    string strLine = "";
        //    //記錄每行記錄中的各字段內容
        //    string[] aryLine = null;
        //    string[] tableHead = null;
        //    //標示列數
        //    int columnCount = 0;
        //    //標示是否是讀取的第一行
        //    bool IsFirst = true;
        //    //逐行讀取CSV中的數據
        //    while ((strLine = sr.ReadLine()) != null)
        //    {
        //        if (IsFirst == true)
        //        {
        //            tableHead = strLine.Split(',');
        //            IsFirst = false;
        //            columnCount = tableHead.Length;
        //            //創建列
        //            for (int i = 0; i < columnCount; i++)
        //            {
        //                tableHead[i] = tableHead[i].Replace("\"", "");
        //                DataColumn dc = new DataColumn(tableHead[i]);
        //                dt.Columns.Add(dc);
        //            }
        //        }
        //        else
        //        {
        //            aryLine = strLine.Split(',');
        //            DataRow dr = dt.NewRow();
        //            for (int j = 0; j < columnCount; j++)
        //            {
        //                dr[j] = aryLine[j].Replace("\"", "");
        //            }
        //            dt.Rows.Add(dr);
        //        }
        //    }
        //    if (aryLine != null && aryLine.Length > 0)
        //    {
        //        dt.DefaultView.Sort = tableHead[2] + " " + "DESC";
        //    }
        //    sr.Close();
        //    fs.Close();
        //    return dt;
        //}
        #endregion
    }

}
