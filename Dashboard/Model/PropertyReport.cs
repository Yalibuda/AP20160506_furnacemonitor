using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;

namespace Dashboard.Model
{
    public class PropertyReport : Report
    {
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
            // 將資料匯入 Minitab
            if (_rawdata == null || _rawdata.Rows.Count == 0)
            {
                throw new ArgumentNullException("查無對應資料");
            }

            _rptLst = new List<IRptOutput>(); //重新建立一個分析結果列舉，因為 Cotent 唯讀
            proj.Commands.Delete();
            proj.Worksheets.Delete();
            Mtb.Worksheet ws = proj.Worksheets.Add(1); //新增工作表            
            Mtblib.Tools.MtbTools.InsertDataTableToMtbWs(_rawdata, ws); //匯入資料至 Minitab

            List<Mtb.Column> varCols = new List<Mtb.Column>(); //變數的欄位集合
            varCols.Add(ws.Columns.Item("VALUE"));
            Mtb.Column timeCol = ws.Columns.Item("TEST_DATE");

            #region Create Minitab command
            StringBuilder cmnd = new StringBuilder();
            cmnd.AppendLine("notitle");
            cmnd.AppendLine("brief 0");
            cmnd.AppendFormat("fdate {0};\r\n", timeCol.SynthesizedName);
            cmnd.AppendLine("format(dtyyyy-MM-dd).");

            cmnd.AppendFormat("tsplot {0};\r\n", string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));
            cmnd.AppendFormat(" stamp {0};\r\n", timeCol.SynthesizedName);

            string propName = ws.Columns.Item("PROD_NAME").GetData(1, 1);
            string compName = ws.Columns.Item("ITEM_NAME").GetData(1, 1);
            string unitName = ws.Columns.Item("ITEM_UNIT").GetData(1, 1);
            string gpath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("trend_{0}_{1}.jpg", propName, compName));

            cmnd.AppendFormat(" gsave \"{0}\";\r\n", gpath);
            cmnd.AppendLine("  repl;");
            cmnd.AppendLine("  jpeg;");
            cmnd.AppendFormat(" axlab 2 \"{0}({1})\";\r\n", compName, unitName);
            cmnd.AppendLine(" scale 1;");
            cmnd.AppendLine("  angle 90;");
            cmnd.AppendLine(" axlab 1;");
            cmnd.AppendLine("  adis 0;");
            cmnd.AppendLine(" conn;");
            cmnd.AppendLine(" symb;");
            cmnd.AppendLine("  size 2;");
            cmnd.AppendLine(" nodt.");


            cmnd.AppendLine("brief 2");
            cmnd.AppendFormat("Desc {0};\r\n", string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));
            //cmnd.AppendFormat(" by {0};\r\n");
            cmnd.AppendLine(" n;");
            cmnd.AppendLine(" mean;");
            cmnd.AppendLine(" stdev;");
            cmnd.AppendLine(" mini;");
            cmnd.AppendLine(" maxi;");
            cmnd.AppendLine(" ztag \"DESC\".");
            cmnd.AppendLine("brief 0");
            //刪除所有圖形
            cmnd.AppendLine("gmana;");
            cmnd.AppendLine("all;");
            cmnd.AppendLine("close;");
            cmnd.AppendLine("nopr."); 
            #endregion

            string fpath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
            proj.ExecuteCommand(string.Format("exec \"{0}\" 1", fpath), ws);

            //取得圖和敘述統計量
            try
            {
                if (File.Exists(gpath))
                {
                    this.Contents.Add(new RptOutput()
                    {
                        OType = MtbOType.GRAPH,
                        OutputInByteArr = File.ReadAllBytes(gpath),
                        Tag = "Trend"
                    });
                }


            }
            catch (Exception ex)
            {
                this.Contents.Add(new RptOutput()
                {
                    OType = MtbOType.GRAPH,
                    OutputInByteArr = null,
                    Tag = "Trend"
                });
            }

            #region 取得敘述統計量表格
            try
            {
                //將報表結果放到 DataTable
                DataTable dt = new DataTable();

                //取得結果的 html string
                string htmlCmnd = proj.Commands.Cast<Mtb.Command>().Where(x => x.Tag == "DESC").Select(x => x.Outputs).FirstOrDefault().Item(2).HTMLText;

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlCmnd);

                //取得並設定表頭
                htmlDoc.LoadHtml(htmlDoc.DocumentNode.SelectSingleNode("//tr").OuterHtml);
                var htmlHeader = htmlDoc.DocumentNode.SelectSingleNode("//tr");
                foreach (var cell in htmlHeader.SelectNodes("//td"))
                {
                    if (cell.InnerText != "\n") dt.Columns.Add(cell.InnerText);
                }

                //取得表格內容
                htmlDoc.LoadHtml(htmlCmnd);
                var htmlRows = htmlDoc.DocumentNode.SelectNodes("//table/tr");
                DataRow dr;
                HtmlDocument tmpHtmlDoc = new HtmlDocument();
                for (int i = 1; i < htmlRows.Count; i++)
                {
                    dr = dt.NewRow();
                    var row = htmlRows[i];
                    tmpHtmlDoc.LoadHtml(row.OuterHtml);
                    var xx = tmpHtmlDoc.DocumentNode.SelectNodes("//td").Select(x => x.InnerText).ToArray();
                    dr.ItemArray = xx;
                    dt.Rows.Add(dr);
                }

                //轉成 DataTable
                this.Contents.Add(new RptOutput()
                {
                    OType = MtbOType.TABLE,
                    OutputInByteArr = Tool.ConvertDataSetToByteArray(dt),
                    Tag = "Table"
                });
            }
            catch (Exception ex)
            {
                this.Contents.Add(new RptOutput()
                {
                    OType = MtbOType.TABLE,
                    OutputInByteArr = null,
                    Tag = "Table"
                });
            }
            #endregion


        }
    }
}
