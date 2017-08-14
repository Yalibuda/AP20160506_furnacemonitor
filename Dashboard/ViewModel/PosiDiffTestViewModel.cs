using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.ViewModel
{
    public class PosiDiffTestViewModel : BasicPage
    {
        public PosiDiffTestViewModel()
        {
            Load();
        }

        #region 屬性
        /// <summary>
        /// 取得差異分析的結果
        /// </summary>
        public ObservableCollection<DiffTestContent> Content
        {
            get { return _diffContent; }
            set
            {
                _diffContent = value;
                _diffContent.CollectionChanged += _diffContent_CollectionChanged;
                RaisePropertyChanged("Content");
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

            _diffRptItem = new ObservableCollection<Model.IReport>();
            _diffRptItem.CollectionChanged += _diffRptItem_CollectionChanged;
            _diffContent = new ObservableCollection<DiffTestContent>();


        }
        /// <summary>
        /// 取得報表結果的List
        /// </summary>
        public override List<Model.IReport> ReportItems
        {
            get
            {
                List<Model.IReport> rptItems = new List<Model.IReport>();
                if (_diffRptItem != null)
                {
                    foreach (var item in _diffRptItem)
                    {
                        rptItems.Add(item);
                    }
                }
                return rptItems;
            }
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


        #endregion

        #region 事件方法

        public void OnSiteChanged()
        {
            _diffRptItem.Clear();
        }
        protected override void _bgWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("Task completed.");
            IsBusy = false;
            if (e.Error != null)
            {
                System.Windows.MessageBox.Show(string.Format("**Error**\t{0}", e.Error.Message), "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        protected override void _bgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            IsBusy = true;
            if (SITE_ID == null || SITE_ID == string.Empty) return;

            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                _diffRptItem.Clear();
            });

            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT * FROM VW_BKHOUR");
            string start = string.Format("{0:yyyy-MM-dd} {1}", StartDate.Date, StartTimeValue);
            string end = string.Format("{0:yyyy-MM-dd} {1}", EndDate.Date, EndTimeValue);
            query.AppendFormat("WHERE SITE_ID='{0}' AND [GROUP_ID]!=0 AND [TIMESTAMP] BETWEEN '{1}' AND '{2}'\r\n",
                SITE_ID, start, end);
            query.AppendLine("ORDER BY [TIMESTAMP],[GROUP_ID]");
            DataTable rawdata = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString());

            string tmpDir = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab");
            Array.ForEach(System.IO.Directory.GetFiles(tmpDir), System.IO.File.Delete); //刪除暫存區所有檔案

            if (rawdata != null && rawdata.Rows.Count > 0)
            {
                Model.PosiDiffTest rpt = new Model.PosiDiffTest();
                rpt.RawData = rawdata;
                rpt.Flag = "PosiDiff";
                rpt.Title = "各紡位的斷絲率比較";
                try
                {
                    rpt.Execute(Project);
                }
                catch (Exception ex)
                {
                    throw new Exception("Minitab run time error\r\n" + ex.Message);
                }
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    _diffRptItem.Add(rpt);
                });
            }
        }
        private void _diffContent_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Content");
        }
        private void _diffRptItem_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            List<Model.IReport> newItems;
            if (e.NewItems != null) newItems = e.NewItems.Cast<Model.IReport>().ToList();
            else newItems = new List<Model.IReport>();

            List<Model.IReport> oldItems;
            if (e.OldItems != null) oldItems = e.OldItems.Cast<Model.IReport>().ToList();
            else oldItems = new List<Model.IReport>();

            DiffTestContent tmpContent;
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new DiffTestContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.BoxPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                case Dashboard.Model.MtbOType.TABLE:
                                    tmpContent.SummTable = Tool.BinaryToDataTable(output.OutputInByteArr);
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        Content.Add(tmpContent);

                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    Content.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new DiffTestContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.BoxPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                case Dashboard.Model.MtbOType.TABLE:
                                    tmpContent.SummTable = Tool.BinaryToDataTable(output.OutputInByteArr);
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        Content[e.OldStartingIndex + i] = tmpContent;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    List<DiffTestContent> tmpLst = new List<DiffTestContent>();
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new DiffTestContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.BoxPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    break;
                                case Dashboard.Model.MtbOType.TABLE:
                                    tmpContent.SummTable = Tool.BinaryToDataTable(output.OutputInByteArr);
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        tmpLst.Add(tmpContent);
                    }
                    Content.Clear();
                    Content = new ObservableCollection<DiffTestContent>(tmpLst);
                    break;
                default:
                    break;
            }

        }

        #endregion

        #region 變數
        private ObservableCollection<Model.IReport> _diffRptItem;
        private ObservableCollection<DiffTestContent> _diffContent;

        #endregion

        #region Command
        public override System.Windows.Input.ICommand UpdatePageCommand
        {
            get { return new Command.RelayCommand(UpdatePage); }
        }
        #endregion

        
    }
    /// <summary>
    /// 用於View上顯示差異分析的結果
    /// </summary>
    public class DiffTestContent
    {
        public System.Windows.Media.ImageSource BoxPlot { set; get; }
        public System.Data.DataTable SummTable { get; set; }
        public string Title { set; get; }
        public DataTable RawData { set; get; }
    }
}
