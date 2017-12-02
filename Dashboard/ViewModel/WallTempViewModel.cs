using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using System.Data;
using OxyPlot;
using Dashboard.Model;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Media;

//test using 
using System.Drawing;

namespace Dashboard.ViewModel
{
    public class WallTempViewModel : BasicPage
    {
        public WallTempViewModel()
        {
            try
            {
                Load();
            }
            catch (Exception ex)
            {
                throw new Exception("Error in WallTemp Load \r\n" + ex.Message);
            }
        }

        #region properties
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
                    //OnSiteChanged();
                }
            }
        }

        public ObservableCollection<WallReportContent> WallContent
        {
            get { return _wallContent; }
            set
            {
                _wallContent = value;
                _wallContent.CollectionChanged += _wallContent_CollectionChanged;
                RaisePropertyChanged("WallContent");
            }
        }

        private ObservableCollection<WallMtbReportContent> _wallMtbContent = null;

        public ObservableCollection<WallMtbReportContent> WallMtbContent
        {
            get { return _wallMtbContent; }
            set
            {
                _wallMtbContent = value;
                _wallMtbContent.CollectionChanged += _wallMtbContent_CollectionChanged;
                RaisePropertyChanged("WallMtbContent");
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

            //基本變數初始值
            _siteId = null;

            Sites = Database.DBQueryTool.GetWallSiteInfo(); // revise SQL
            if(Sites !=null) Sites.DefaultView.Sort = "Plant";
            

            //Initialize variate
            _bgWorker = new BackgroundWorker();
            _bgWorker.DoWork += _bgWorker_DoWork;
            _bgWorker.RunWorkerCompleted += _bgWorker_RunWorkerCompleted;
            _bgWorker.WorkerSupportsCancellation = true;
            _bgWorker.WorkerReportsProgress = true;

            _wallRptItems = new ObservableCollection<Model.IReport>();
            _wallRptItems.CollectionChanged += _wallRptItems_CollectionChanged;
            WallContent = new ObservableCollection<WallReportContent>();
            WallMtbContent = new ObservableCollection<WallMtbReportContent>();

            #region test chart(close)
            #region pieChart(close)

            // OnPropertyChanged

            #region here is test one
            //dtPie.Columns.Add("Result", typeof(string));
            //dtPie.Columns.Add("Counts", typeof(int));
            //if (dtPie != null)
            //{
            //    DataRow r1 = dtPie.NewRow();
            //    DataRow r2 = dtPie.NewRow();
            //    r1[0] = "over10"; r1[1] = 10;
            //    dtPie.Rows.Add(r1);
            //    r2[0] = "in10"; r2[1] = 30;
            //    dtPie.Rows.Add(r2);
            //    pieChart.RawData = dtPie;
            //}

            //pieChart.Execute();
            //plotPieModel = pieChart.Model;
            #endregion end of test pie

            #endregion

            #region barChart(close)
            //bardt.Columns.Add("AreaNumber", typeof(int));
            //bardt.Columns.Add("MaxRange", typeof(double));
            //if (bardt != null)
            //{
            //    DataRow r1 = bardt.NewRow();
            //    DataRow r2 = bardt.NewRow();
            //    r1[0] = 1; r1[1] = 13;
            //    bardt.Rows.Add(r1);
            //    r2[0] = 2; r2[1] = 17;
            //    bardt.Rows.Add(r2);
            //    barChart.RawData = bardt;
            //}
            //barChart.Execute();
            //plotBarModel = barChart.Model;
            #endregion

            #region lineSeries(close)

            #region here is test 2(close)
            //barChart.IndexOfNearestBarChart = 1;
            //if (barChart.IndexOfNearestBarChart != -1)
            //{
            //    string indexRow =
            //        (barChart.IndexOfNearestBarChart < 9) ?
            //        string.Format("0{0}", barChart.IndexOfNearestBarChart + 1) : string.Format("{0}", barChart.IndexOfNearestBarChart + 1);
            //    DataRow[] rows = dtRaw.Select(string.Format("Area = {0}", indexRow));
            //    linedt = new DataTable();
            //    //linedt.Columns.Add("Plant"); 
            //    //linedt.Columns.Add("Area"); 
            //    linedt.Columns.Add("DateTime", typeof(DateTime));
            //    linedt.Columns.Add("Value", typeof(double));
            //    foreach (DataRow row in rows)
            //    {
            //        DataRow tmpdr = linedt.NewRow();
            //        tmpdr[0] = row[2]; tmpdr[1] = row[3];
            //        linedt.Rows.Add(tmpdr);
            //    }
            //    lineChart.RawData = linedt;
            //    lineChart.Execute();
            //    PlotLineModel = lineChart.Model;
            //}
            #endregion

            #region here is test 1 successful (close)
            //linedt.Columns.Add("Date", typeof(DateTime));
            //linedt.Columns.Add("Value", typeof(double));
            //if (linedt != null)
            //{
            //    DataRow r1 = linedt.NewRow();
            //    DataRow r2 = linedt.NewRow();
            //    r1[0] = new DateTime(2016, 6, 18, 12, 0, 0); r1[1] = 5;
            //    linedt.Rows.Add(r1);
            //    r2[0] = new DateTime(2016, 6, 19, 12, 0, 0); r2[1] = 8;
            //    linedt.Rows.Add(r2);
            //    lineChart.RawData = linedt;
            //}
            //lineChart.Execute();
            //plotLineModel = lineChart.Model;
            #endregion

            #endregion

            #endregion

            #region total time series plot(close)

            #endregion
        }

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
        //public void OnSiteChanged() { }
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

            //write rptoutput preparation and test
            #region preparing data and check
            dtRaw = new DataTable();
            string start = string.Format("{0:yyyy-MM-dd} {1}", StartDate.Date, StartTimeValue);
            string end = string.Format("{0:yyyy-MM-dd} {1}", EndDate.Date, EndTimeValue);
            dtRaw = Database.DBQueryTool.GetWallData(SITE_ID, start, end);

            Areas = Database.DBQueryTool.GetWallAreaData(SITE_ID, start, end);
            if (Areas != null) Areas.DefaultView.Sort = "Area";
            if (dtRaw.Rows.Count == 0) return;
            #endregion

            #region clear the old graph
            PlotBarModel = null;
            PlotPieModel = null;
            PlotLineMRModel = null;
            PlotLineModel = null;
            #endregion

            #region new region, this is for output data and graph
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                _wallRptItems.Clear(); //_corrRptItems.Clear();
                _wallContent.Clear(); //CorrContent.Clear();
                _wallMtbContent.Clear();
            });
            #endregion

            #region PieChart renew
            //Model.OxyPieChart rpt = new Model.OxyPieChart();
            dtPie = new DataTable();
            dtPie.Columns.Add("class", typeof(string));
            dtPie.Columns.Add("count", typeof(int));
            DataRow drdtPieOver10 = dtPie.NewRow();
            DataRow drdtPieUnder10 = dtPie.NewRow();
            drdtPieOver10[0] = "高於 10"; drdtPieOver10[1] = 0;
            drdtPieUnder10[0] = "低於 10"; drdtPieUnder10[1] = 0;
            
            pieChart.RawData = dtRaw;

            //if (pieChart.AreaCount == 0) return; // if there is no data, return, use rawdata.rows.count to replacement

            DataTable dtAreaMR = new DataTable();
            dtAreaMR.Columns.Add("Area", typeof(string));
            dtAreaMR.Columns.Add("MaxMR", typeof(double));

            dtMR = new DataTable();
            dtMR.Columns.Add("Plant");
            dtMR.Columns.Add("Area");
            dtMR.Columns.Add("DateTime");
            dtMR.Columns.Add("Value");
            int tmpAreaCount = pieChart.AreaCount;
            for (int i = 0; i < pieChart.AreaCount; i++)
            {
                string indexRowtmp =
                    (i < 9) ? string.Format("0{0}", i + 1) : string.Format("{0}", i + 1);
                DataRow[] rows = dtRaw.Select(string.Format("Area = {0}", indexRowtmp));
                double tmpMaxMR = 0;
                for (int j = 0; j < (rows.Count() - 1); j++)
                {
                    DataRow drMR = dtMR.NewRow();
                    if (j == 0)
                    {
                        tmpMaxMR = Math.Abs(Convert.ToDouble(rows[j + 1][3]) - Convert.ToDouble(rows[j][3]));
                    } 
                    else
                    {
                        tmpMaxMR = Math.Max(tmpMaxMR, Convert.ToDouble(rows[j + 1][3]) - Convert.ToDouble(rows[j][3]));
                    }
                    drMR[0] = rows[j + 1][0];
                    drMR[1] = rows[j + 1][1];
                    drMR[2] = rows[j + 1][2];
                    drMR[3] = Math.Abs(Convert.ToDouble(rows[j + 1][3]) - Convert.ToDouble(rows[j][3]));
                    dtMR.Rows.Add(drMR);
                }
                DataRow dr = dtAreaMR.NewRow();
                dr[0] = indexRowtmp; dr[1] = tmpMaxMR;
                dtAreaMR.Rows.Add(dr);

                if (tmpMaxMR >= 10) drdtPieOver10[1] = Convert.ToInt32(drdtPieOver10[1]) + 1;
                else drdtPieUnder10[1] = Convert.ToInt32(drdtPieUnder10[1]) + 1;
            }
            dtPie.Rows.Add(drdtPieOver10);
            dtPie.Rows.Add(drdtPieUnder10);

            pieChart.RawData = dtPie;

            pieChart.Title = "爐外壁溫度概況";
            pieChart.Execute();
            
            _wallRptItems.Add(pieChart);
            PlotPieModel = pieChart.Model;
            #endregion end of PieChart renew

            #region Bar Chart renew

            // rearrange the order of rawdata
            //dtAreaMR.DefaultView.Sort = "MaxMR";
            DataView dv = new DataView(dtAreaMR);
            dv.Sort = "MaxMR DESC";
            //DataTable dtOrderedAreaMR = dv.ToTable();
            dtOrderedAreaMR = dv.ToTable();
            //Sites.DefaultView.Sort = "Plant"; //datatable sort
            if (AllAreaVisible) barChart.RawData = dtOrderedAreaMR;
            else 
            { 
                //dt.Rows.Cast<System.Data.DataRow>().Take(n)
                //dtOrderedAreaMR10th = new DataTable();
                dtOrderedAreaMR10th = dtOrderedAreaMR.AsEnumerable().Take(10).CopyToDataTable();
                //dtOrderedAreaMR10th.Columns.Add("Area", typeof(string));
                //dtOrderedAreaMR10th.Columns.Add("MaxMR", typeof(double));
                //dtOrderedAreaMR10th.Rows.Add(dtOrderedAreaMR.Rows.Cast<System.Data.DataRow>().Take(10));
                barChart.RawData = dtOrderedAreaMR10th;
                // only plot the 10th max MR
            }

            barChart.Title = "爐外壁移動全距>10爐區";
            barChart.XTitle = "爐區";
            barChart.YTitle = "最大移動全距";

            //bardt.Columns.Add("AreaNumber", typeof(int));
            //bardt.Columns.Add("MaxRange", typeof(double));
            //if (bardt != null)
            //{
            //    DataRow r1 = bardt.NewRow();
            //    DataRow r2 = bardt.NewRow();
            //    r1[0] = 1; r1[1] = 13;
            //    bardt.Rows.Add(r1);
            //    r2[0] = 2; r2[1] = 17;
            //    bardt.Rows.Add(r2);
            //    barChart.RawData = bardt;
            //}
            barChart.Execute();
            _wallRptItems.Add(barChart);
            PlotBarModel = barChart.Model;

            #endregion

            #region Tsplot renew
            string tmpDir = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab");
            Array.ForEach(System.IO.Directory.GetFiles(tmpDir), System.IO.File.Delete); //刪除暫存區所有檔案
            int sizeOfTSPlot = 16;

            int forCount = Convert.ToInt16(Math.Ceiling(Convert.ToDouble(tmpAreaCount) / sizeOfTSPlot));
            int forCountMod = tmpAreaCount - sizeOfTSPlot * (forCount - 1);
            List<string> _areaArray;
            //forCount = 2; // for testing
            for (int i = 0; i < forCount; i++)
            {
                Model.TSPlot rpt = new Model.TSPlot();
                rpt.Title = "時間趨勢圖";
                DataTable dttsplot = new DataTable();
                var criteria = new List<string>();
                _areaArray = new List<string>();

                #region logic to include the area for graphs
                if (i < forCount - 1) // here handle sizeOfTSPlot area graph
                {

                    rpt.CountPlotArea = sizeOfTSPlot;
                    if (i == 0)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            criteria.Add(string.Format("0{0}", j + 1));
                            _areaArray.Add(string.Format("爐區-0{0}", j + 1));
                        }
                        for (int j = 9; j < sizeOfTSPlot; j++)
                        {
                            criteria.Add((j + 1).ToString());
                            _areaArray.Add(string.Format("爐區-{0}", j + 1));
                        }
                    }
                    else
                    {
                        for (int j = 0; j < sizeOfTSPlot; j++)
                        {
                            criteria.Add((sizeOfTSPlot * i + j + 1).ToString());
                            _areaArray.Add(string.Format("爐區-{0}", sizeOfTSPlot * i + j + 1));
                        }
                    }

                }
                else // handle the last graph
                {
                    rpt.CountPlotArea = forCountMod;
                    for (int j = 0; j < forCountMod; j++)
                    {
                        criteria.Add((sizeOfTSPlot * i + j + 1).ToString());
                        _areaArray.Add(string.Format("爐區-{0}", sizeOfTSPlot * i + j + 1));
                    }
                }
                rpt.AreaArray = _areaArray;
                #endregion

                //write the code to prepare datatable for tsplot
                //criteria.Add("01"); criteria.Add("02");
                var filterRows = dtRaw.AsEnumerable()
                    .Where(row => criteria.Contains(row["Area"].ToString()));
                dttsplot = filterRows.CopyToDataTable();

                rpt.RawData = dttsplot;
                if (rpt.RawData != null && rpt.RawData.Rows.Count > 0)
                {
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
                        _wallRptItems.Add(rpt);
                    });
                }
                //Console.WriteLine(); // only for checking
            }
            #endregion

            #region MR Chart renew

                //linedt.Columns.Add("Date", typeof(DateTime));
                //linedt.Columns.Add("Value", typeof(double));
                //if (linedt != null)
                //{
                //    DataRow r1 = linedt.NewRow();
                //    DataRow r2 = linedt.NewRow();
                //    r1[0] = new DateTime(2016, 6, 18, 12, 0, 0); r1[1] = 5;
                //    linedt.Rows.Add(r1);
                //    r2[0] = new DateTime(2016, 6, 19, 12, 0, 0); r2[1] = 8;
                //    linedt.Rows.Add(r2);
                //    lineChart.RawData = linedt;
                //}
                //lineChart.Execute();
                //PlotLineModel = lineChart.Model;

                barChart.PropertyChanged += barChart_PropertyChanged;
            #endregion
        }

        private void _wallRptItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            List<Model.IReport> newItems;

            #region test output for graph

            #region preparing job, unknown
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

            WallReportContent tmpContent;
            WallMtbReportContent tmpMtbContent;
            #endregion

            #region reflection to action
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < newItems.Count; i++)
                    {
                        //WallMtbContent.Clear();
                        tmpContent = new WallReportContent();
                        tmpMtbContent = new WallMtbReportContent();
                        #region oxyone
                        if (newItems[i] is Model.IOxyReport)
                        { //handle the oxy type output
                            Model.IOxyReport rpt = newItems[i] as Model.IOxyReport;
                            for (int j = 0; j < rpt.OxyContents.Count; j++)
                            {
                                Model.IOxyRptOutput output = rpt.OxyContents[j];
                                switch (output.OxyOType)
                                {
                                    case Dashboard.Model.OxyOType.GRAPH:
                                        switch (output.Tag.ToString())
                                        {
                                            case "Bar":
                                                tmpContent.BarChart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                                break;
                                            case "Pie":
                                                tmpContent.PieChart = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                                break;
                                            case "Individual":
                                                tmpContent.TSPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                                break;
                                            case "MR":
                                                tmpContent.TSPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            tmpContent.Title = rpt.Title;
                            tmpContent.RawData = rpt.RawData;
                            WallContent.Add(tmpContent);
                        }
                        #endregion

                        #region mtb one
                        else // handle the MTB type rpt
                        {
                            Model.IReport rpt = newItems[i];
                            for (int j = 0; j < rpt.Contents.Count; j++)
                            {
                                Model.IRptOutput output = rpt.Contents[j];
                                switch (output.OType)
                                {
                                    case Dashboard.Model.MtbOType.GRAPH:
                                        switch (output.Tag.ToString())
                                        {
                                            case "TSPlot":
                                                tmpContent.TSPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                                tmpMtbContent.TSPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            tmpContent.Title = rpt.Title;
                            tmpContent.RawData = rpt.RawData;
                            WallContent.Add(tmpContent);

                            tmpMtbContent.Title = rpt.Title;
                            tmpMtbContent.RawData = rpt.RawData;
                            WallMtbContent.Add(tmpMtbContent);
                        }
                        #endregion
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    WallContent.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    //need to add some Code for handling oxy and mtb, need testing => no need?
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                default:
                    break;
            }
            #endregion

            #endregion

            #region successful one(close)
            //List<Model.IOxyReport> newItems;

            //if (e.NewItems != null)
            //{
            //    newItems = e.NewItems.Cast<Model.IOxyReport>().ToList();
            //}
            //else
            //{
            //    newItems = new List<Model.IOxyReport>();
            //}
            //List<Model.IOxyReport> oldItems;
            //if (e.OldItems != null)
            //{
            //    oldItems = e.OldItems.Cast<Model.IOxyReport>().ToList();
            //}
            //else
            //{
            //    oldItems = new List<Model.IOxyReport>();
            //}
            //WallReportContent tmpContent;
            //switch (e.Action)
            //{
            //    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
            //        for (int i = 0; i < newItems.Count; i++)
            //        {
            //            Model.IOxyReport rpt = newItems[i];
            //            tmpContent = new WallReportContent();
            //            for (int j = 0; j < rpt.OxyContents.Count; j++)
            //            {
            //                Model.IOxyRptOutput output = rpt.OxyContents[j];
            //                switch (output.OxyOType)
            //                {
            //                    case Dashboard.Model.OxyOType.GRAPH:
            //                        switch (output.Tag.ToString())
            //                        {
            //                            case "Bar":
            //                                tmpContent.BarChart = Tool.BinaryToWPFImage(output.OutputInByteArr);
            //                                break;
            //                            case "Pie":
            //                                tmpContent.PieChart = Tool.BinaryToWPFImage(output.OutputInByteArr);
            //                                break;
            //                            case "Individual":
            //                                tmpContent.TSPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
            //                                break;
            //                            case "MR":
            //                                tmpContent.TSPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
            //                                break;
            //                            default:
            //                                break;
            //                        }
            //                        break;
            //                    default:
            //                        break;
            //                }
            //            }
            //            tmpContent.Title = rpt.Title;
            //            tmpContent.RawData = rpt.RawData; //?
            //            WallContent.Add(tmpContent);
            //        }
            //        break;
            //    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
            //        WallContent.Clear();
            //        break;
            //    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
            //        #region after testing add, could be open
            //        //for (int i = 0; i < newItems.Count; i++)
            //        //{
            //        //    Model.IOxyReport rpt = newItems[i];
            //        //    tmpContent = new WallReportContent();
            //        //    for (int j = 0; j < rpt.Contents.Count; j++)
            //        //    {
            //        //        Model.IOxyRptOutput output = rpt.OxyContents[j];
            //        //        switch (output.OxyOType)
            //        //        {
            //        //            case Dashboard.Model.OxyOType.GRAPH:
            //        //                switch (output.Tag.ToString())
            //        //                {
            //        //                    case "Bar":
            //        //                        tmpContent.BarChart = Tool.BinaryToWPFImage(output.OutputInByteArr);
            //        //                        break;
            //        //                    case "Pie":
            //        //                        tmpContent.PieChart = Tool.BinaryToWPFImage(output.OutputInByteArr);
            //        //                        break;
            //        //                    case "Individual":
            //        //                        tmpContent.TSPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
            //        //                        break;
            //        //                    case "MR":
            //        //                        tmpContent.TSPlot = Tool.BinaryToWPFImage(output.OutputInByteArr);
            //        //                        break;
            //        //                    default:
            //        //                        break;
            //        //                }
            //        //                break;
            //        //            default:
            //        //                break;
            //        //        }
            //        //    }
            //        //    tmpContent.Title = rpt.Title;
            //        //    tmpContent.RawData = rpt.RawData; //?
            //        //    WallContent[e.OldStartingIndex + i] = tmpContent;
            //        //}
            //        #endregion
            //        break;
            //    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
            //    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
            //    default:
            //        break;
            //}
            #endregion

            RaisePropertyChanged("_wallRptItems");
        }

        private void _wallContent_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("WallContent");
        }

        void barChart_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (barChart.IndexOfNearestBarChart != -1)
            {
                DataTable dtOrderedAreaMRtmp = dtOrderedAreaMR;
                string indexRow = (dtOrderedAreaMRtmp.Rows[barChart.IndexOfNearestBarChart])[0].ToString();
                IndexRowClone = indexRow;
            }
        }

        void _wallMtbContent_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("WallMtbContent");
        }
        #endregion

        void _selectIndexChanged(string index)
        {

            #region here is individual chart

            string indexRow = index;
            DataRow[] rows = dtRaw.Select(string.Format("Area = {0}", indexRow));
            linedt = new DataTable();
            linedt.Columns.Add("DateTime", typeof(DateTime));
            linedt.Columns.Add("Value", typeof(double));
            foreach (DataRow row in rows)
            {
                DataRow tmpdr = linedt.NewRow();
                tmpdr[0] = row[2]; tmpdr[1] = row[3];
                linedt.Rows.Add(tmpdr);
            }
            lineChart.RawData = linedt;
            lineChart.Title = string.Format("{0}號爐區-溫度趨勢圖", indexRow);
            lineChart.XTitle = "時間";
            lineChart.YTitle = "溫度";

            #region new statTable handler
            lineChart.Tag = "MR"; // tag MR will compute stat table
            StatTableRaw = new DataTable();
            StatTableRaw.Columns.Add("爐區", typeof(string));
            StatTableRaw.Columns.Add("樣本數", typeof(int));
            StatTableRaw.Columns.Add("平均", typeof(double));
            StatTableRaw.Columns.Add("標準差", typeof(double));
            StatTableRaw.Columns.Add("最大值", typeof(double));
            StatTableRaw.Columns.Add("最小值", typeof(double));
            DataRow drStatTableRaw = StatTableRaw.NewRow();
            drStatTableRaw[0] = indexRow;
            drStatTableRaw[1] = lineChart.N;
            drStatTableRaw[2] = lineChart.Mean;
            drStatTableRaw[3] = lineChart.stDev;
            drStatTableRaw[4] = lineChart.Max;
            drStatTableRaw[5] = lineChart.Min;
            StatTableRaw.Rows.Add(drStatTableRaw);
            lineChart.statTable = StatTableRaw;
            #endregion

            lineChart.Execute();
            _wallRptItems.Add(lineChart);
            PlotLineModel = lineChart.Model;
            
            #endregion

            #region here is MR chart
            DataRow[] rowsMR = dtMR.Select(string.Format("Area = {0}", indexRow));
            linedtMR = new DataTable();
            linedtMR.Columns.Add("DateTime", typeof(DateTime));
            linedtMR.Columns.Add("Value", typeof(double));
            foreach (DataRow row in rowsMR)
            {
                DataRow tmpdr = linedtMR.NewRow();
                tmpdr[0] = row[2]; tmpdr[1] = row[3];
                linedtMR.Rows.Add(tmpdr);
            }
            lineChartMR.RawData = linedtMR;
            lineChartMR.Title = "移動全距趨勢圖";
            lineChartMR.XTitle = "時間";
            lineChartMR.YTitle = "溫度移動全距";

            #region new statTable handler
            lineChartMR.Tag = "MR"; // tag MR will compute stat table
            StatTable = new DataTable();
            StatTable.Columns.Add("爐區R", typeof(string));
            StatTable.Columns.Add("樣本數", typeof(int));
            StatTable.Columns.Add("平均", typeof(double));
            StatTable.Columns.Add("標準差", typeof(double));
            StatTable.Columns.Add("最大值", typeof(double));
            StatTable.Columns.Add("最小值", typeof(double));
            DataRow drStatTable = StatTable.NewRow();
            drStatTable[0] = indexRow;
            drStatTable[1] = lineChartMR.N;
            drStatTable[2] = lineChartMR.Mean;
            drStatTable[3] = lineChartMR.stDev;
            drStatTable[4] = lineChartMR.Max;
            drStatTable[5] = lineChartMR.Min;
            StatTable.Rows.Add(drStatTable);
            lineChartMR.statTable = StatTable;
            #endregion

            lineChartMR.Execute();
            _wallRptItems.Add(lineChartMR);
            PlotLineMRModel = lineChartMR.Model;
            //PlotLineMRModel2 = lineChartMR.Model;
            Console.Write("");
            #endregion
        }


        #region 變數
        //public PlotModel PlotLineMRModel2
        //{
        //    get
        //    {
        //        return plotLineMRModel2;
        //    }
        //    set
        //    {
        //        plotLineMRModel2 = value;
        //        RaisePropertyChanged("PlotLineMRModel2");
        //    }
        //}
        //private PlotModel plotLineMRModel2;

        public DataTable dtMR;

        public DataTable dtOrderedAreaMR;

        public DataTable StatTable 
        {
            get
            {
                return _statTable;
            }
            set
            {
                _statTable = value;
                RaisePropertyChanged("StatTable");
            }
        }

        private DataTable _statTable;

        public DataTable StatTableRaw
        {
            get
            {
                return _statTableRaw;
            }
            set
            {
                _statTableRaw = value;
                RaisePropertyChanged("StatTableRaw");
            }
        }

        private DataTable _statTableRaw;

        private ObservableCollection<Model.IReport> _wallRptItems = null;

        #region PieChart
        private PlotModel plotPieModel; // this is for testing pieChart, prefer to use list<plotmodel>
        public PlotModel PlotPieModel
        {
            get
            {
                return plotPieModel;
            }
            set
            {
                plotPieModel = value;
                RaisePropertyChanged("PlotPieModel");
            }
        }
        public DataTable dtRaw;
        public DataTable dtPie;
        public Model.OxyPieChart pieChart = new OxyPieChart();
        //dat.Columns.Add("Result", typeof(string));
        #endregion

        #region barChart

        private PlotModel plotBarModel;

        public PlotModel PlotBarModel
        {
            get
            {
                return plotBarModel;
            }
            set
            {
                plotBarModel = value;
                RaisePropertyChanged("PlotBarModel");
            }
        }

        public DataTable bardt = new DataTable();

        public OxyBarChart barChart = new OxyBarChart();

        DataTable dtOrderedAreaMR10th;

        public bool AllAreaVisible 
        {
            get { return _allAreaVisible; }
            set 
            {
                _allAreaVisible = value;
                RaisePropertyChanged("AllAreaVisible");
            }
        }

        private bool _allAreaVisible = false;

        public string IndexRowClone
        {
            get { return _indexRowClone; }
            set
            {
                _indexRowClone = value;
                DataTable dtOrderedAreaMRtmp = dtOrderedAreaMR;
                _selectIndexChanged(_indexRowClone);
                RaisePropertyChanged("IndexRowClone");
            }
        }

        private string _indexRowClone;

        #endregion

        #region lineSeries


        /// <summary>
        /// 取得爐區資訊
        /// </summary>
        public DataTable Areas
        {
            get { return _dtAreaInfo; }
            protected set
            {
                if (!Database.DBQueryTool.CompareDataTableRow(_dtAreaInfo, value))
                {
                    _dtAreaInfo = value;
                    //RaisePropertyChanged("SiteInfo"); // this one is no use? 20171002
                    RaisePropertyChanged("Areas"); // this one is added 20171002
                }
            }
        }

        private DataTable _dtAreaInfo = null;

        #region this one is for Individual chart
        private PlotModel plotLineModel;
        public PlotModel PlotLineModel
        {
            get
            {
                return plotLineModel;
            }
            set
            {
                plotLineModel = value;
                RaisePropertyChanged("PlotLineModel");
            }
        }
        public DataTable linedt = new DataTable();
        public OxyLineSeries lineChart = new OxyLineSeries();
        #endregion

        #region this one is for MR chart
        private PlotModel plotLineMRModel;
        public PlotModel PlotLineMRModel
        {
            get
            {
                return plotLineMRModel;
            }
            set
            {
                plotLineMRModel = value;
                RaisePropertyChanged("PlotLineMRModel");
            }
        }
        public DataTable linedtMR = new DataTable();
        public OxyLineSeries lineChartMR = new OxyLineSeries();
        #endregion

        public int AreaIndexToPlot
        {
            get
            {
                return _areaIndexToPlot;
            }
            set
            {
                _areaIndexToPlot = value;
                RaisePropertyChanged("AreaIndexToPlot");
            }
        }

        private int _areaIndexToPlot = -1;
        #endregion

        //沒用到? => 輸出 data
        public override List<Model.IReport> ReportItems
        {
            get
            {
                List<Model.IReport> rptItems = new List<Model.IReport>();
                if (_wallRptItems != null)
                {
                    foreach (var item in _wallRptItems)
                    {
                        rptItems.Add(item);
                    }
                }
                return rptItems;
            }
        }

        private ObservableCollection<WallReportContent> _wallContent = null;

        #endregion

        #region command
        public override ICommand UpdatePageCommand
        {
            get { return new Command.RelayCommand(UpdatePage); }
        }
        #endregion

        // wallreportcontent, considering to move it to basic page
        public class WallReportContent
        {
            public ImageSource PieChart { set; get; }
            public ImageSource BarChart { set; get; }
            public ImageSource TSPlot { set; get; }
            public string Title { set; get; }
            public DataTable RawData { set; get; }
        }

        public class WallMtbReportContent
        {
            public ImageSource TSPlot { set; get; }
            public string Title { set; get; }
            public DataTable RawData { set; get; }
        }
    }
}
