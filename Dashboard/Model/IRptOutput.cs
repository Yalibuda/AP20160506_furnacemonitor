using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.Model
{
    /// <summary>
    /// 每一個報表產出的介面 (例如某一熔爐項的管制圖)
    /// </summary>
    public interface IRptOutput
    {
        /// <summary>
        /// 產出類型
        /// </summary>
        MtbOType OType { get; set; }
        /// <summary>
        /// Report output object(as byte[]), you should convert it to string, bitmap etc., if you 
        /// want to use it in UI. (Search byte[] to image or string vice versa.)
        /// </summary>
        byte[] OutputInByteArr { get; set; }
        /// <summary>
        /// 用於標註出或掛上一些資訊於於該產出
        /// </summary>
        object Tag { get; set; }

    }


    /// <summary>
    /// 報表產出類型的列舉
    /// </summary>
    public enum MtbOType
    {
        ///<summary>圖形</summary>
        GRAPH,
        ///<summary>文字描述</summary>
        PARAGRAPH,
        ///<summary>表格</summary>
        TABLE,
        ///<summary>標題</summary>
        TITLE
    }

}
