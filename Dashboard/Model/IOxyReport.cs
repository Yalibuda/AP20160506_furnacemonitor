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
    /// OxyPlot分析的報表物件，包含分析類型、標題、資料與內容等
    /// </summary>
    public interface IOxyReport : IReport
    {
        /// <summary>
        /// 分析項目使用的資料
        /// </summary>
        //DataTable RawData { get; set; }
        /// <summary>
        /// 分析項目的標題
        /// </summary>
        //string Title { get; set; }
        /// <summary>
        /// 分析的類型，可用於判斷該分析是什麼分析
        /// </summary>
        //string Flag { get; set; }
        /// <summary>
        /// 執行分析項目，每次執行會清除掉原本的 Report 內容
        /// <param name="proj">Minitab Project</param>
        /// </summary>
        void Execute();
        /// <summary>
        /// 分析的內容
        /// </summary>
        //List<IOxyRptOutput> Contents { get; }
        /// <summary>
        /// PlotModel
        /// </summary>
        PlotModel Model { get; set; }
        /// <summary>
        /// x-axis title
        /// </summary>
        string XTitle { get; set; }
        /// <summary>
        /// y-axis title
        /// </summary>
        string YTitle { get; set; }
        /// <summary>
        /// OxyPlot分析的內容
        /// </summary>
        List<IOxyRptOutput> OxyContents { get; }


    }
}
