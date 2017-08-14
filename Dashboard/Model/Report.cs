using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.Model
{    /// <summary>
    /// 分析報表的抽象類別
    /// </summary>
    public abstract class Report : IReport
    {
        protected DataTable _rawdata = new DataTable();
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
        public virtual string Title { get; set; }

        /// <summary>
        /// 執行分析項目
        /// </summary>
        /// <param name="proj"></param>
        public abstract void Execute(Mtb.Project proj);

        /// <summary>
        /// 取得分析結果的列舉
        /// </summary>
        public List<IRptOutput> Contents
        {
            get { return _rptLst; }
        }

        public string Flag { get; set; }
    }
}
