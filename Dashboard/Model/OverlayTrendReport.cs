using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.Model
{
    public class OverlayTrendReport : Report
    {
        /// <summary>
        /// 設定或取得原始資料
        /// </summary>
        public override DataTable RawData
        {
            get
            {
                return base.RawData;
            }
            set
            {
                base.RawData = value;
                /*
                 * 修正欄位名稱...
                 * Avoid the invalid character in the column name of Minitab
                 * 
                 */
                string name = "";
                for (int i = 0; i < _rawdata.Columns.Count; i++)
                {
                    DataColumn col = _rawdata.Columns[i];
                    switch (col.ColumnName)
                    {
                        case "TIMESTAMP":
                            break;
                        default:
                            name = col.ColumnName;
                            name = name.Replace("#", "_").Replace("*", "_").Replace("'", "_");
                            col.ColumnName = name;
                            break;
                    }
                }
            }
        }
        public override void Execute(Mtb.Project proj)
        {
            if (_rawdata == null || _rawdata.Rows.Count == 0) return;
            _rptLst = new List<IRptOutput>(); //重新建立一個分析結果列舉
            Mtb.Worksheet ws = proj.Worksheets.Add(1); //新增工作表            
            Mtblib.Tools.MtbTools.InsertDataTableToMtbWs(_rawdata, ws); //匯入資料至 Minitab
            List<string> varnames = new List<string>();
            List<Mtb.Column> varCols = new List<Mtb.Column>(); //變數的欄位集合
            for (int i = 0; i < _rawdata.Columns.Count; i++)
            {
                DataColumn col = _rawdata.Columns[i];
                switch (col.ColumnName)
                {
                    case "TIMESTAMP":
                    case "CHART_PARA_INDEX":
                        break;
                    default:
                        varnames.Add(col.ColumnName);
                        varCols.Add(ws.Columns.Item(col.ColumnName));
                        break;
                }
            }

            Mtb.Column timeCol = ws.Columns.Item("TIMESTAMP");

            foreach (var col in varCols)
            {
                if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
                {
                    throw new ArgumentNullException(string.Format("[{0}]查無資料-多變量管制圖", col.Name));
                }
            }

            StringBuilder cmnd = new StringBuilder();
            cmnd.AppendLine("notitle");
            cmnd.AppendLine("brief 0");

            cmnd.AppendFormat("fdate {0};\r\n", timeCol.SynthesizedName);
            cmnd.AppendLine("format(dtyyyy-MM-dd hh:mm).");

            cmnd.AppendFormat("tsplot {0};\r\n", string.Join(" ", varCols.Select(x => x.SynthesizedName)));

            double gHeight = 4;
            if (varCols.Count > 4) gHeight = Math.Min(30, 4 + ((double)varCols.Count - 4) * 0.8);

            cmnd.AppendFormat("graph 8 {0};\r\n", gHeight);
            string gpath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("overlytrnd_{0}.jpg", _rawdata.TableName));
            cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath);
            cmnd.AppendLine("repl;");
            cmnd.AppendLine("jpeg;");
            cmnd.AppendFormat("stamp {0};\r\n", timeCol.SynthesizedName);
            cmnd.AppendLine("axlab 1;");
            cmnd.AppendLine("adis 0;");
            cmnd.AppendLine("scale 1;");
            cmnd.AppendLine(" psize 4;");
            cmnd.AppendLine(" angle 90;");
            cmnd.AppendLine("scale 2;");
            cmnd.AppendLine(" psize 5;");
            cmnd.AppendLine("symbol;");
            cmnd.AppendLine("conn;");
            cmnd.AppendLine("pane;");
            cmnd.AppendFormat("rc {0} 1;\r\n", varCols.Count);
            cmnd.AppendLine("label;");
            cmnd.AppendLine(" psize 5;");
            cmnd.AppendLine("noal;");
            cmnd.AppendLine("nodt.");
            //刪除所有圖形
            cmnd.AppendLine("gmana;");
            cmnd.AppendLine("all;");
            cmnd.AppendLine("close;");
            cmnd.AppendLine("nopr.");

            string macroPath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
            proj.ExecuteCommand(string.Format("exec \"{0}\" 1", macroPath), ws);

            //將檔案轉為二進位陣列
            this.Contents.Add(new RptOutput()
            {
                OType = MtbOType.GRAPH,
                OutputInByteArr = File.ReadAllBytes(gpath)
            });
        }
    }
}
