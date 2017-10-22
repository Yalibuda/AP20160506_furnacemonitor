using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using OxyPlot;

namespace Dashboard.Model
{
    /// <summary>
    /// 分析報表的抽象類別
    /// </summary>
    public abstract class OxyReport : IOxyReport
    {
        protected DataTable _rawdata = new DataTable();
        protected List<IOxyRptOutput> _oxyrptLst = null;
        protected List<IRptOutput> _rptLst = null;
        protected const double MISSINGVALUE = 1.23456E+30;

        /// <summary>
        /// 報表資料
        /// </summary>
        public virtual DataTable RawData
        {
            get
            {
                return _rawdata;
            }
            set
            {
                _rawdata = value;
            }
        }
        /// <summary>
        /// 分析項標題
        /// </summary>
        public virtual string Title 
        {
            get { return this.title; }
            set { this.title = value; }
        }

        private string title = "";

        public string Flag { get; set; }
        /// <summary>
        /// 執行分析項目
        /// </summary>
        public abstract void Execute();
        /// <summary>
        /// 執行分析項目
        /// </summary>
        public abstract void Execute(Mtb.Project project);
        /// <summary>
        /// 取得分析結果的列舉
        /// </summary>
        public List<IOxyRptOutput> OxyContents
        {
            get { return _oxyrptLst; }
        }
        /// <summary>
        /// 取得分析結果的列舉
        /// </summary>
        public List<IRptOutput> Contents
        {
            get { return _rptLst; }
        }
        /// <summary>
        /// PlotModel
        /// </summary>
        public PlotModel Model { get; set; }
        /// <summary>
        /// x-axis title
        /// </summary>
        public string XTitle 
        {
            get { return _xTytle; }
            set { _xTytle = value; }
        }
        private string _xTytle = "";
        /// <summary>
        /// y-axis title
        /// </summary>
        public string YTitle 
        {
            get { return _yTitle; }
            set { _yTitle = value; }
        }
        private string _yTitle = "";
    }
}
