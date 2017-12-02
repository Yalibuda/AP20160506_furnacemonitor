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
        /// 執行分析項目，每次執行會清除掉原本的 Report 內容
        /// <param name="proj">Minitab Project</param>
        /// </summary>
        void Execute();
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
