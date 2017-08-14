using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.Model
{
    public class RptOutput : IRptOutput
    {
        byte[] _buffer = null;
        object _tag = null;

        /// <summary>
        /// 設定或取得報表的類型列舉
        /// </summary>
        public MtbOType OType { get; set; }

        /// <summary>
        /// 報表內容的二進位格式
        /// </summary>
        public byte[] OutputInByteArr
        {
            get
            {
                return _buffer;
            }
            set
            {
                _buffer = value;
            }
        }

        /// <summary>
        /// 報表內容的附加資訊
        /// </summary>
        public object Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
            }
        }
    }
}
