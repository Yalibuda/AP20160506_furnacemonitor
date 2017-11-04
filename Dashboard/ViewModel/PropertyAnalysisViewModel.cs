using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dashboard.ViewModel
{
    public class PropertyAnalysisViewModel : BasicPage
    {
        public PropertyAnalysisViewModel()
        {
            Load();
        }

        #region Property
        /// <summary>
        /// 取得分析儀器/方法資訊
        /// </summary>
        public DataTable Categories
        {
            get { return _ds.Tables["CATE_INFO"]; }
        }

        /// <summary>
        /// 設定或取得使用的分析儀器資訊
        /// </summary>
        public string CATE_NAME
        {
            get
            {
                return _cateName;
            }
            set
            {
                if (_cateName != value)
                {
                    _cateName = value;
                    RaisePropertyChanged("CATE_NAME");
                    OnCategoryChanged();
                }
            }
        }
        private string _cateName;

        /// <summary>
        /// 設定或取得選擇的分析項目
        /// </summary>
        public string SelectedPropInfoString
        {
            get { return _selectPropInfoString; }
            private set
            {
                _selectPropInfoString = value;
                RaisePropertyChanged("SelectedPropInfoString");
            }
        }

        /// <summary>
        /// 設定或取得選擇的成分項目
        /// </summary>
        public string SelectedCompInfoString
        {
            get { return _selectCompInfoString; }
            private set
            {
                _selectCompInfoString = value;
                RaisePropertyChanged("SelectedCompInfoString");
            }
        }

        /// <summary>
        /// 分析報表內容的集合(用於顯示在 Page)
        /// </summary>
        public ObservableCollection<PropReportContent> PropContent
        {
            get { return _propContent; }
            set
            {
                _propContent = value;                
                RaisePropertyChanged("PropContent");
            }
        }

        /// <summary>
        /// 分析報表集合
        /// </summary>
        public override List<Model.IReport> ReportItems
        {
            get
            {
                List<Model.IReport> rptItems = new List<Model.IReport>();
                if (_propRptItems != null)
                {
                    foreach (var item in _propRptItems)
                    {
                        rptItems.Add(item);
                    }
                }
                return rptItems;
            }
        }

        #endregion

        #region 方法

        protected override void Load()
        {
            DateTime datetime = DateTime.Now;
            EndDate = datetime.Date;
            StartDate = datetime.AddDays(-1).Date;
            EndTimeValue = string.Format("{0:HH:mm}", datetime);
            StartTimeValue = EndTimeValue;

            //Initialize variate
            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += _bgWorker_DoWork;
            _bgWorker.RunWorkerCompleted += _bgWorker_RunWorkerCompleted;
            _bgWorker.WorkerSupportsCancellation = true;
            _bgWorker.WorkerReportsProgress = true;

            _propRptItems = new ObservableCollection<Model.IReport>();
            _propRptItems.CollectionChanged += _propRptItems_CollectionChanged;
            PropContent = new ObservableCollection<PropReportContent>();
            PropContent.CollectionChanged += PropContent_CollectionChanged;

            //Create a dataset include the needed datatable
            #region 基本資料準備
            _ds = new DataSet("Lims");
            StringBuilder sqlCmnd = new StringBuilder();
            sqlCmnd.AppendLine("select distinct CATE_NAME from PROPERTY_INFO; select * from PROPERTY_INFO;");
            sqlCmnd.AppendLine("select a.PROP_INDEX, a.TEST_ITEM_INDEX, b.CATE_NAME, b.PROD_NAME, c.ITEM_NAME, c.ITEM_UNIT from TEST_DETAIL a");
            sqlCmnd.AppendLine("left join PROPERTY_INFO b on a.PROP_INDEX = b.PROP_INDEX");
            sqlCmnd.AppendLine("left join TEST_ITEM_INFO c on a.TEST_ITEM_INDEX = c.TEST_ITEM_INDEX");
            sqlCmnd.AppendLine("group by a.PROP_INDEX, a.TEST_ITEM_INDEX,b.CATE_NAME, b.PROD_NAME,c.ITEM_NAME, c.ITEM_UNIT");

            using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString("LIMS")))
            {
                using (SqlCommand sql = new SqlCommand(sqlCmnd.ToString(), conn))
                {
                    try
                    {
                        conn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(sql);
                        da.Fill(_ds);
                        conn.Close();
                        da.Dispose();
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("資料查詢錯誤");
                        return;
                    }
                }
            }
            _ds.Tables[0].TableName = "CATE_INFO"; //分析儀器方法清單
            _ds.Tables[1].TableName = "PROPERTY_INFO"; //分析項目清單
            _ds.Tables[2].TableName = "PROP_COMP_INFO"; //成分清單 
            #endregion
            sqlCmnd.Clear();

            //基本變數初始值
            _siteId = null;

            //把資訊放到各控制項




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
        /// <summary>
        /// 顯示項目設定的對話框
        /// </summary>
        /// <param name="obj"></param>
        private void ShowPropSelectionDialog(object obj)
        {
            if (CATE_NAME == null || string.IsNullOrWhiteSpace(CATE_NAME))
            {
                System.Windows.MessageBox.Show("請先指定分析儀器/方法", "物性分析", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
                return;
            }

            using (View.SelectItemView2 f = new View.SelectItemView2(_selectedPropViewModel))
            {
                f.ShowDialog();
                Dictionary<int, string> tmpSelectedProps = ((SelectItemViewModel)f.DataContext).SelectedItemList;
                //_selectedProps = ((SelectItemViewModel)f.DataContext).SelectedItemList;
                if (tmpSelectedProps != null && tmpSelectedProps.Count > 0)
                {
                    if (_selectedProps == null || !tmpSelectedProps.SequenceEqual(_selectedProps))
                    {
                        _selectedProps = tmpSelectedProps;
                        string lab = string.Join(",", tmpSelectedProps.Select(x => x.Value));
                        if (lab.Length > 60)
                        {
                            lab = lab.Substring(0, 57) + "...";
                        }
                        SelectedPropInfoString = lab;
                        OnPropChanged();
                    }
                }
            }
        }

        /// <summary>
        /// 顯示分析成分設定的對話框
        /// </summary>
        /// <param name="obj"></param>
        private void ShowCompSelectionDialog(object obj)
        {
            if (_selectedProps == null || _selectedProps.Count == 0)
            {
                System.Windows.MessageBox.Show("請至少選擇一個分析項目", "物性分析", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
                return;
            }

            using (View.SelectItemView2 f = new View.SelectItemView2(_selectedCompViewModel))
            {
                f.ShowDialog();
                Dictionary<int, string> tmpSelectedComps = ((SelectItemViewModel)f.DataContext).SelectedItemList;
                if (tmpSelectedComps != null && tmpSelectedComps.Count > 0)
                {
                    if (_selectedComps == null || !tmpSelectedComps.SequenceEqual(_selectedComps))
                    {
                        _selectedComps = tmpSelectedComps;
                        string lab = string.Join(",", tmpSelectedComps.Select(x => x.Value));
                        if (lab.Length > 60)
                        {
                            lab = lab.Substring(0, 57) + "...";
                        }
                        SelectedCompInfoString = lab;
                    }
                }
            }
        }

        /// <summary>
        /// 當選擇的分析儀器/方法改變時，觸發可選用的分析項目清單改變
        /// </summary>
        private void OnCategoryChanged()
        {

            _selectedCompViewModel = new SelectItemViewModel(); // 如果更改分析儀器/方法，整個成分選取項要取消
            string sqlstring = string.Format("CATE_NAME='{0}'", this.CATE_NAME);
            Dictionary<int, string> availableItems = _ds.Tables["PROPERTY_INFO"].Select(sqlstring, "PROD_NAME")
                .Select(x => new { Key = x.Field<int>("PROP_INDEX"), Value = x.Field<string>("PROD_NAME") })
                .ToDictionary(x => x.Key, x => x.Value);
            _selectedPropViewModel.AvailableItemList = availableItems;
            SelectedPropInfoString = "";
            _selectedProps = null;

            //_selectedCompViewModel = new SelectItemViewModel(); // 如果更改分析儀器/方法，整個成分選取項要取消            
            //availableItems = _ds.Tables["TEST_ITEM_INFO"].Rows.Cast<DataRow>()
            //    .Select(x => new { Key = x.Field<int>("TEST_ITEM_INDEX"), Value = x.Field<string>("ITEM_NAME") })
            //    .ToDictionary(x => x.Key, x => x.Value);
            //_selectedCompViewModel.AvailableItemList = availableItems;
            //SelectedCompInfoString = "";
            //_selectedComps = null;
        }

        /// <summary>
        /// 當選擇的分析項目改變時，觸發可選用的成分清單改變
        /// </summary>
        private void OnPropChanged()
        {
            StringBuilder sqlString = new StringBuilder();
            sqlString.AppendFormat("PROP_INDEX in ({0})",
                string.Join(",", _selectedProps.Select(x => "'" + x.Key + "'")));
            _selectedCompViewModel = new SelectItemViewModel();

            Dictionary<int, string> availableItems = _ds.Tables["PROP_COMP_INFO"].Select(sqlString.ToString())
                .Select(x => new { Key = x.Field<int>("TEST_ITEM_INDEX"), Value = x.Field<string>("ITEM_NAME") })
                .Distinct().ToDictionary(x => x.Key, x => x.Value);
            _selectedCompViewModel.AvailableItemList = availableItems;
            SelectedCompInfoString = "";
            _selectedComps = null;
        }

        /// <summary>
        /// 報表內容異動，觸發顯示於頁面的內容也要更動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _propRptItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            PropReportContent tmpContent;
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new PropReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    tmpContent.VisibilityOfChart = true;
                                    break;
                                case Dashboard.Model.MtbOType.TABLE:
                                    tmpContent.Table = Tool.BinaryToDataTable(output.OutputInByteArr);
                                    tmpContent.ShowTable = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        PropContent.Add(tmpContent);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    PropContent.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        Model.IReport rpt = newItems[i];
                        tmpContent = new PropReportContent();
                        for (int j = 0; j < rpt.Contents.Count; j++)
                        {
                            Model.IRptOutput output = rpt.Contents[j];
                            switch (output.OType)
                            {
                                case Dashboard.Model.MtbOType.GRAPH:
                                    tmpContent.Chart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                    tmpContent.VisibilityOfChart = true;
                                    break;
                                case Dashboard.Model.MtbOType.TABLE:
                                    tmpContent.Table = Tool.BinaryToDataTable(output.OutputInByteArr);
                                    tmpContent.ShowTable = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                        tmpContent.Title = rpt.Title;
                        tmpContent.RawData = rpt.RawData;
                        PropContent[e.OldStartingIndex + i] = tmpContent;

                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                default:
                    break;
            }
        }

        private void PropContent_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("PropContent");
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
            //_selectedProps = new Dictionary<int, string>();
            //_selectedProps.Add(12,"AA");
            //_selectedComps = new Dictionary<int, string>();
            //_selectedComps.Add(28, "AAA");
            if (_selectedComps == null || _selectedComps.Count == 0) return;

            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                _propRptItems.Clear();
                PropContent.Clear();
            });

            lock (_selectedComps)
            {
                //決定最後要分析的項目
                StringBuilder sqlString = new StringBuilder();
                sqlString.AppendFormat("PROP_INDEX in ({0}) and TEST_ITEM_INDEX in ({1})",
                    string.Join(",", _selectedProps.Select(x => "'" + x.Key + "'")),
                    string.Join(",", _selectedComps.Select(x => "'" + x.Key + "'")));
                DataRow[] prop_comp_list;
                prop_comp_list = _ds.Tables["PROP_COMP_INFO"].Select(sqlString.ToString(), "PROD_NAME");
                string[] itemList = prop_comp_list.Select(x => x.Field<int>("PROP_INDEX") + "/" + x.Field<int>("TEST_ITEM_INDEX")).Distinct().ToArray();

                string start = string.Format("{0:yyyy-MM-dd} {1}", StartDate.Date, StartTimeValue);                
                string end = string.Format("{0:yyyy-MM-dd} {1}", EndDate.Date, EndTimeValue);
                DataTable rawData;
                Model.PropertyReport rpt;

                //itemList = new string[]{"12/28","12/29","12/31","14/28","14/31","14/32"};
                //start = string.Format("2016-10-01 {1}", StartDate.Date, StartTimeValue);

                rawData = Database.DBQueryTool.GetPropertyTestData(itemList, start, end);
                if (rawData == null || rawData.Rows.Count == 0)
                {
                    throw new ArgumentNullException("查無資料");
                }

                //把資料依分析項和成分分群
                List<DataTable> subData = rawData.AsEnumerable().GroupBy(x => new { PROP_ID = x.Field<int>("PROP_INDEX"), ITEM_ID = x.Field<int>("TEST_ITEM_INDEX") })
                    .Select(g => g.CopyToDataTable()).ToList();

                string tmpDir = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab");
                Array.ForEach(System.IO.Directory.GetFiles(tmpDir), System.IO.File.Delete); //刪除暫存區所有檔案                


                for (int i = 0; i < subData.Count; i++)
                {
                    rpt = new Model.PropertyReport();
                    rpt.RawData = subData[i];
                    rpt.Title = subData[i].Rows[0]["PROD_NAME"].ToString() + ": " + subData[i].Rows[0]["ITEM_NAME"].ToString();
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
                        _propRptItems.Add(rpt);
                    });
                }

            }
        }
        #endregion

        #region 變數
        private ObservableCollection<Model.IReport> _propRptItems = null;
        private ObservableCollection<PropReportContent> _propContent = null;
        private SelectItemViewModel _selectedCompViewModel = new SelectItemViewModel();
        private SelectItemViewModel _selectedPropViewModel = new SelectItemViewModel();
        private DataSet _ds = null;
        private Dictionary<int, string> _selectedProps;
        private Dictionary<int, string> _selectedComps;
        private string _selectPropInfoString = ""; //顯示分析項目文字
        private string _selectCompInfoString = ""; //顯示成分項目文字

        #endregion

        #region Command

        public override ICommand UpdatePageCommand
        {
            get { return new Command.RelayCommand(UpdatePage); }
        }
        public ICommand ShowCompSelectionDialogCommand
        {
            get { return new Command.RelayCommand(ShowCompSelectionDialog); }
        }

        public ICommand ShowPropSelectionDialogCommand
        {
            get { return new Command.RelayCommand(ShowPropSelectionDialog); }
        }

        #endregion
    }
}
