using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Dashboard.ViewModel
{
    public abstract class BasicPage : INotifyPropertyChanged
    {
        #region 屬性
        /// <summary>
        /// 設定或取得使用中的 Minitab Project
        /// </summary>
        public Mtb.Project Project
        {
            set { _proj = value; }
            get { return _proj; }
        }
        protected Mtb.Project _proj = null;

        /// <summary>
        /// 取得廠別資訊
        /// </summary>
        public DataTable Sites
        {
            get { return _dtSiteInfo; }
            protected set
            {
                if (!Database.DBQueryTool.CompareDataTableRow(_dtSiteInfo, value))
                {
                    _dtSiteInfo = value;
                    RaisePropertyChanged("SiteInfo");
                }
            }
        }
        protected DataTable _dtSiteInfo = null;

        /// <summary>
        /// 設定或取得使用的廠別資訊
        /// </summary>
        public virtual string SITE_ID
        {
            get { return _siteId; }
            set
            {
                if (_siteId != value)
                {
                    _siteId = value;
                    RaisePropertyChanged("SITE_ID");
                }

            }
        }
        protected string _siteId;
        /// <summary>
        /// 取得查詢的起始日期
        /// </summary>
        public virtual DateTime StartDate
        {
            get { return _startDate; }
            protected set
            {
                _startDate = value;
                RaisePropertyChanged("StartDateTime");
            }
        }
        protected DateTime _startDate = DateTime.Now.AddDays(-1);
        /// <summary>
        /// 取得查詢的起始時間
        /// </summary>
        public virtual string StartTimeValue
        {
            get { return _startTimeValue; }
            protected set
            {
                _startTimeValue = value;
                RaisePropertyChanged("StartTimeValue");
            }
        }
        protected string _startTimeValue = null;
        /// <summary>
        /// 取得查詢的結束日期
        /// </summary>
        public virtual DateTime EndDate
        {
            get { return _endDate; }
            protected set
            {
                _endDate = value;
                RaisePropertyChanged("EndDateTime");
            }
        }
        protected DateTime _endDate = DateTime.Now;
        /// <summary>
        /// 取得查詢的結束時間
        /// </summary>
        public virtual string EndTimeValue
        {
            get { return _endTimeValue; }
            protected set
            {
                _endTimeValue = value;
                RaisePropertyChanged("EndTimeValue");
            }
        }
        protected string _endTimeValue = null;
        /// <summary>
        /// 取得報表結果的List
        /// </summary>
        public abstract List<Model.IReport> ReportItems { get; }
        /// <summary>
        /// 取得該分頁的工作是否正在執行
        /// </summary>
        public virtual bool IsBusy
        {
            get
            {
                bool isBusy = false;
                if (_bgWorker != null)
                {
                    isBusy = _bgWorker.IsBusy;
                }
                return isBusy;
            }
            protected set
            {
                _isBusy = value;
                RaisePropertyChanged("IsBusy");
            }
        }

        #endregion

        #region 方法
        protected abstract void Load();
        public virtual void StopCurrentWork()
        {
            if (Project != null)
            {
                try
                {
                    if (_bgWorker != null && _bgWorker.IsBusy)
                    {
                        _bgWorker.CancelAsync();
                    }
                    Project.CancelCommand();
                }
                catch (Exception ex)
                {
                    // 0x800706BA -2147023174                    
                    // 這裡是一個不好的作法，因為直接使用 Minitab.Project 產生相依性，但是為了必成的即時更新報表的穩定性，出此下策
                    if (ex.HResult == -2147023174)
                    {
                        Project = Minitab.Project;
                        Project.CancelCommand();
                    }
                    else
                    {

                    }
                }

            }
        }


        #endregion

        #region 事件方法

        protected abstract void _bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e);
        protected abstract void _bgWorker_DoWork(object sender, DoWorkEventArgs e);


        #endregion

        #region 變數
        protected BackgroundWorker _bgWorker;
        protected bool _isBusy;
        #endregion

        #region Command
        public abstract ICommand UpdatePageCommand { get; }
        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class MultiReportContent
    {
        public ImageSource Chart { set; get; }
        public DataTable DecomTable { set; get; }
        public string Title { set; get; }
        public DataTable RawData { set; get; }
        public bool ShowTable { get; set; }
    }

    public class UniReportContent
    {
        public ImageSource Chart { set; get; }
        public string Summary { set; get; }
        public string Title { set; get; }
        public DataTable RawData { set; get; }
    }

    public class CorrReportContent
    {
        public ImageSource TrendChart { set; get; }
        public bool VisibilityOfTrendChart { set; get; }
        public ImageSource ScatterPlot { set; get; }
        public bool VisibilityOfScatterPlot { set; get; }
        public DataTable CorrTable { set; get; }
        public bool ShowTable { set; get; }
        public string Title { set; get; }
        public DataTable RawData { set; get; }
    }

    public class PropReportContent
    {
        public ImageSource Chart { set; get; }
        public bool VisibilityOfChart { set; get; }
        public DataTable Table { set; get; }
        public bool ShowTable { set; get; }
        public string Title { set; get; }
        public DataTable RawData { set; get; }
    }

}
