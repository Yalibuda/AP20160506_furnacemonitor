using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.Model
{
    /// <summary>
    /// 每一個OXY產出的介面 (例如一張Bar Chart)
    /// </summary>
    public interface IOxyRptOutput : IRptOutput
    {
        /// <summary>
        /// 產出類型
        /// </summary>
        OxyOType OxyOType { get; set; }
    }

    /// <summary>
    /// 報表產出類型的列舉
    /// </summary>
    public enum OxyOType
    {
        /// <summary>圖形 </summary>
        GRAPH,
        /// <summary>表格 </summary>
        TABLE,

        /// <summary>
        /// 互動式圖形
        /// </summary>
        OBJECT //?
    }
}
