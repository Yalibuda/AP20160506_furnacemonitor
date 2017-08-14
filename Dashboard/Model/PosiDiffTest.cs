using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.Model
{
    public class PosiDiffTest : Report
    {
        public PosiDiffTest()
        {

        }
        /// <summary>
        /// 指定或取得紡位斷絲率差異分析的資料表，資料欄位應包含:
        /// SITE_ID, TIMESTAMP, GROUP_ID, MIN_POSITION, MAX_POSITION, GROUP_DESC, BK
        /// 指定資料時，應確認資料不包含 GROUP=0 (全紡位群斷絲率)
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
            }
        }

        public override void Execute(Mtb.Project proj)
        {
            // 將資料匯入 Minitab
            if (RawData == null || RawData.Rows.Count == 0)
            {
                return;
            }
            _rptLst = new List<IRptOutput>(); //重新建立一個分析結果列舉
            proj.Commands.Delete();
            proj.Worksheets.Delete();
            Mtb.Worksheet ws = proj.Worksheets.Add(1); //新增工作表            
            Mtblib.Tools.MtbTools.InsertDataTableToMtbWs(_rawdata, ws); //匯入資料至 Minitab

            Mtb.Column bkCol = ws.Columns.Item("BK"); bkCol.Name = "斷絲率";
            Mtb.Column grouCol = ws.Columns.Item("GROUP_DESC"); grouCol.Name = "紡位群";
            grouCol.SetValueOrder(Mtb.MtbValueOrderTypes.WorksheetOrder);

            //如果沒有資料或欄位長度不一致就離開
            if ((int)bkCol.DataType == 3 || (int)grouCol.DataType == 3 ||
                (bkCol.RowCount != grouCol.RowCount))
            {
                return;
            }

            StringBuilder cmnd = new StringBuilder();
            cmnd.AppendLine("notitle");
            cmnd.AppendLine("brief 0");

            //繪製Boxplot
            cmnd.AppendFormat("boxplot ({0})*{1};\r\n", bkCol.SynthesizedName, grouCol.SynthesizedName);
            string gpath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("posidiff_{0}.jpg", _rawdata.TableName));
            cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath);
            cmnd.AppendLine("repl;");
            cmnd.AppendLine("jpeg;");
            cmnd.AppendLine("iqrbox;");
            cmnd.AppendLine("outl.");

            //計算統計量
            cmnd.AppendFormat("stat {0};\r\n", bkCol.SynthesizedName);
            cmnd.AppendFormat("by {0};\r\n", grouCol.SynthesizedName);
            string[] colstr = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, 5, Mtblib.Tools.MtbVarType.Column);
            Mtb.Column
                gvalCol = ws.Columns.Item(colstr[0]),
                meanCol = ws.Columns.Item(colstr[1]),
                stdevCol = ws.Columns.Item(colstr[2]),
                minCol = ws.Columns.Item(colstr[3]),
                maxCol = ws.Columns.Item(colstr[4]);
            cmnd.AppendFormat("mean {0};\r\n", meanCol.SynthesizedName);
            cmnd.AppendFormat("stdev {0};\r\n", stdevCol.SynthesizedName);
            cmnd.AppendFormat("mini {0};\r\n", minCol.SynthesizedName);
            cmnd.AppendFormat("maxi {0};\r\n", maxCol.SynthesizedName);
            cmnd.AppendFormat("gval {0}.\r\n", gvalCol.SynthesizedName);

            string mPath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());

            proj.ExecuteCommand(string.Format("Exec \"{0}\" 1", mPath), ws);

            if (File.Exists(gpath)) //如果檔案存在就加入報表內容
            {
                _rptLst.Add(
                    new RptOutput
                    {
                        OType = MtbOType.GRAPH,
                        OutputInByteArr = File.ReadAllBytes(gpath)
                    }
                    );
            }

            if (gvalCol.RowCount > 0 && (int)gvalCol.DataType != 3) // 有產出報表就加入報表
            {
                DataTable descStatTable = Mtblib.Tools.MtbTools.GetDataTableFromMtbCols(
                    new Mtb.Column[] { gvalCol, meanCol, stdevCol, minCol, maxCol });
                if (descStatTable != null)
                {
                    descStatTable.Columns[0].ColumnName = "紡位群組";
                    descStatTable.Columns[1].ColumnName = "平均";
                    descStatTable.Columns[2].ColumnName = "標準差";
                    descStatTable.Columns[3].ColumnName = "最小值";
                    descStatTable.Columns[4].ColumnName = "最大值";
                    _rptLst.Add(
                        new RptOutput
                        {
                            OType = MtbOType.TABLE,
                            OutputInByteArr = Tool.ConvertDataSetToByteArray(descStatTable)
                        }
                        );
                }
            }

        }
    }

    
}
