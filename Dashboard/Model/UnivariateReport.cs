using Mtb;
using Mtblib.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dashboard.Model
{
    public class UnivariateReport: Report
    {
        /// <summary>
        /// 執行分析項目
        /// </summary>
        /// <param name="proj"></param>
        public override void Execute(Mtb.Project proj)
        {
            if (_rawdata == null || _rawdata.Rows.Count == 0)
            {
                return;
            }
            _rptLst = new List<IRptOutput>(); //重新建立一個分析結果列舉
            Mtb.Worksheet ws = proj.Worksheets.Add(1); //新增工作表
            string varName = _rawdata.Rows[0].Field<string>("ITEM_NAME"); //Get item name 
            string unit = _rawdata.Rows[0].Field<string>("UNIT"); //Get item unit
            string item_index = _rawdata.Rows[0].Field<string>("FURN_ITEM_INDEX"); //Get item_index


            string[] colids;
            colids = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, _rawdata.Columns.Count, Mtblib.Tools.MtbVarType.Column);

            // Copy data to Minitab
            #region 複製資料到 Minitab Worksheet
            Mtblib.Tools.MtbTools.InsertDataTableToMtbWs(_rawdata, ws);

            //因為匯入的資料的 raw data, 趨勢圖要用小時資料繪製，需要另外計算
            var summ = from row in _rawdata.AsEnumerable()
                       group row by row["RPT_TIMEHOUR"] into g
                       select new
                       {
                           TIMESTAMP = g.Key,
                           AVGVALUE = g.Average(r => r["VALUE"] == DBNull.Value ? 1.23456E+30 : Convert.ToDouble(r["Value"].ToString())),
                           _LCL = g.Max(r => r["LCL"] == DBNull.Value ? 1.23456E+30 : Convert.ToDouble(r["LCL"].ToString())),
                           _UCL = g.Max(r => r["UCL"] == DBNull.Value ? 1.23456E+30 : Convert.ToDouble(r["UCL"].ToString())),
                       };
            var icharData = summ.Select(x => new
            {
                x.TIMESTAMP,
                x.AVGVALUE,
                x._LCL,
                x._UCL,
                CL = (x._LCL + x._UCL) / 2,
                OOC = (x._UCL< MISSINGVALUE && x.AVGVALUE > x._UCL) || (x._LCL < MISSINGVALUE && x.AVGVALUE < x._LCL) ? 1 : 0
            });
            colids = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, 6, Mtblib.Tools.MtbVarType.Column);
            ws.Columns.Item(colids[0]).SetData(icharData.Select(x => x.TIMESTAMP).ToArray());
            ws.Columns.Item(colids[1]).SetData(icharData.Select(x => x.AVGVALUE).ToArray());
            ws.Columns.Item(colids[2]).SetData(icharData.Select(x => x._LCL).ToArray());
            ws.Columns.Item(colids[3]).SetData(icharData.Select(x => x._UCL).ToArray());
            ws.Columns.Item(colids[4]).SetData(icharData.Select(x => x.CL).ToArray());
            ws.Columns.Item(colids[5]).SetData(icharData.Select(x => x.OOC).ToArray());
            ws.Columns.Item(colids[0]).Name = "TIMESTAMP";
            ws.Columns.Item(colids[1]).Name = "AVGVALUE";
            ws.Columns.Item(colids[2]).Name = "_LCL";
            ws.Columns.Item(colids[3]).Name = "_UCL";
            ws.Columns.Item(colids[4]).Name = "CL";
            ws.Columns.Item(colids[5]).Name = "OOC";

            #endregion

            

            Mtb.Column a = ws.Columns.Item(1);

            StringBuilder cmnd = new StringBuilder();

            string valueCol = ws.Columns.Item("AVGVALUE").SynthesizedName;
            string timeCol = ws.Columns.Item("TIMESTAMP").SynthesizedName;
            string lclCol = ws.Columns.Item("_LCL").SynthesizedName;
            string uclCol = ws.Columns.Item("_UCL").SynthesizedName;
            string oocCol = ws.Columns.Item("OOC").SynthesizedName;
            string clCol = ws.Columns.Item("CL").SynthesizedName;


            double[] lclData = ws.Columns.Item(lclCol).GetData();
            double[] uclData = ws.Columns.Item(uclCol).GetData();
            string gpath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("ichart_{0}.jpg", item_index));

            cmnd.AppendLine("notitle");
            cmnd.AppendLine("brief 0");
            cmnd.AppendFormat("date {0} {0};\r\n", timeCol);
            cmnd.AppendLine("format(dtyyyy-M-d H:mm).");

            #region 建立管制圖語法
            if (!lclData.Any(x => x < MISSINGVALUE) && !uclData.Any(x => x < MISSINGVALUE))
            {
                cmnd.AppendFormat("ichart {0};\r\n", valueCol);
                cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath);
                cmnd.AppendLine("repl;");
                cmnd.AppendLine("jpeg;");
                cmnd.AppendLine("cline;");
                cmnd.AppendLine("lshow 0;");
                cmnd.AppendLine("climit;");
                cmnd.AppendLine("lshow 0;");
                cmnd.AppendLine("datlab;");
                cmnd.AppendLine("tcolor 0;");
                cmnd.AppendFormat("cenlen {0};\r\n", clCol);
                cmnd.AppendFormat("conlim {0}-{1};\r\n", lclCol, uclCol);
                cmnd.AppendFormat("trest {0};\r\n", oocCol);
            }
            else
            {
                cmnd.AppendFormat("tsplot {0} {1} {2} {3};\r\n", lclCol, uclCol, clCol, valueCol);
                cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath);
                cmnd.AppendLine("repl;");
                cmnd.AppendLine("jpeg;");
                cmnd.AppendLine("over;");
                cmnd.AppendFormat("symb {0};\r\n", oocCol);
                cmnd.AppendLine("type &");
                double[] oocData = ws.Columns.Item(oocCol).GetData();

                if (oocData.Any(x => x == 0) && oocData.Any(x => x == 1)) cmnd.AppendLine("0 0 0 0 0 0 &");
                else cmnd.AppendLine("0 0 0 &");
                if (oocData.Any(x => x == 0)) cmnd.AppendLine("6 &");
                if (oocData.Any(x => x == 1)) cmnd.AppendLine("12 &");

                cmnd.AppendLine(";");
                cmnd.AppendLine("size 1;");
                cmnd.AppendLine("color &");
                if (oocData.Any(x => x == 0) && oocData.Any(x => x == 1)) cmnd.AppendLine("0 0 0 0 0 0 &");
                else cmnd.AppendLine("0 0 0 &");
                if (oocData.Any(x => x == 0)) cmnd.AppendLine("1 &"); //r17 color 64
                if (oocData.Any(x => x == 1)) cmnd.AppendLine("2 &");
                cmnd.AppendLine(";");
                cmnd.AppendLine("conn;");
                cmnd.AppendLine("type 1 1 1 1;");
                cmnd.AppendLine("color 2 2 120 1;"); //r17 conn:64 cl: 9, climit:8
                //cmnd.AppendLine("graph;");
                //cmnd.AppendLine("color 22;");
                cmnd.AppendLine("nole;");
            }
            cmnd.AppendFormat("stamp {0};\r\n", timeCol);
            cmnd.AppendLine("scale 1;");
            cmnd.AppendFormat("tick 1:{0}/{1};\r\n", icharData.Count(),
                icharData.Count() > 35 ? Math.Ceiling((double)icharData.Count() / 35) : 1);
            cmnd.AppendLine("axla 1;");
            cmnd.AppendLine("adis 0;");
            cmnd.AppendLine("axla 2;");
            cmnd.AppendLine("adis 0;");
            cmnd.AppendLine("graph 8 4;");
            cmnd.AppendFormat("title \"個別值管制圖 {0}({1})\";\r\n", varName, unit);
            cmnd.AppendFormat("footn \"更新時間: {0}\";\r\n", DateTime.Now);
            cmnd.AppendFormat("ZTag \"{0}\";\r\n", "_ICHART");
            cmnd.AppendLine(".");
            //刪除所有圖形
            //cmnd.AppendLine("gmana;");
            //cmnd.AppendLine("all;");
            //cmnd.AppendLine("close;");
            //cmnd.AppendLine("nopr.");

            #endregion

            string filepath;
            try
            {
                filepath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mychart.mtb", cmnd.ToString());
                proj.ExecuteCommand(string.Format("exec \"{0}\" 1", filepath), ws);
                this.Contents.Add(new RptOutput()
                {
                    OType = MtbOType.GRAPH,
                    OutputInByteArr = File.ReadAllBytes(gpath), //將檔案轉為二進位陣列
                });

                
            }
            catch
            {
                
            }



            //取得管制界限資訊
            double clvalue = ws.Columns.Item(clCol).GetData(1, 1);
            double lclvalue = ws.Columns.Item(lclCol).GetData(1, 1);
            double uclvalue = ws.Columns.Item(uclCol).GetData(1, 1);
            double[] oocId = ws.Columns.Item(oocCol).GetData();
            var tmp = oocId.Zip(((DateTime[])ws.Columns.Item(timeCol).GetData()), (x, y) => new { Date = y, OOC = x });
            DateTime[] oocDate = oocId.Zip(((DateTime[])ws.Columns.Item(timeCol).GetData()), (x, y) => new { Date = y, OOC = x }).
                Where(x => x.OOC == 1).Select(x => x.Date).ToArray();

            // 計算製程能力指標 --> 另外開 Worksheet 處理
            // 將標準差 = 0 或 NULL 且沒有對應到規格資訊的資料的移除掉
            DataTable subRawData = new DataTable();
            try
            {
                subRawData = _rawdata.Select("STDDEV >0 AND STDDEV is not null AND (LSL is not null OR USL is not null) ").OrderBy(r => r["RPT_DATETIME"]).CopyToDataTable();
            }
            catch
            {
                subRawData = null; //沒有符合條件的時候設為 NULL
            }

            List<double> PpkList = new List<double>();
            List<double> USLInfo = new List<double>();
            List<double> LSLInfo = new List<double>();
            if (subRawData == null)
            {
                PpkList.Add(MISSINGVALUE);
            }
            else
            {
                cmnd.Clear();
                Mtb.Worksheet subWs = proj.Worksheets.Add(1);
                proj.SetComActiveWorksheet(subWs);
                Mtblib.Tools.MtbTools.InsertDataTableToMtbWs(subRawData, subWs);
                Mtb.Column subRawDataCol = subWs.Columns.Item("VALUE");
                Mtb.Column lslCol = subWs.Columns.Item("LSL");
                Mtb.Column uslCol = subWs.Columns.Item("USL");
                Mtb.Column byValCol = subWs.Columns.Item("BYVAL");

                colids = Mtblib.Tools.MtbTools.CreateVariableStrArray(subWs, 1, Mtblib.Tools.MtbVarType.Column);
                string PpkCol = colids[0];
                if (byValCol.GetNumDistinctRows() == 1) //只有一組規格
                {
                    double usl = uslCol.GetData(1, 1);
                    double lsl = lslCol.GetData(1, 1);
                    cmnd.AppendFormat("capa {0} 1;\r\n", subRawDataCol.SynthesizedName);
                    if (usl < MISSINGVALUE) cmnd.AppendFormat("uspec {0};\r\n", usl);
                    if (lsl < MISSINGVALUE) cmnd.AppendFormat("lspec {0};\r\n", lsl);
                    cmnd.AppendLine("nochart;");
                    cmnd.AppendLine("ztag \"_Capa\";");
                    cmnd.AppendFormat("ppk {0}.", PpkCol);
                    USLInfo.Add(usl);
                    LSLInfo.Add(lsl);
                }
                else
                {
                    string[] tmpColId = Mtblib.Tools.MtbTools.CreateVariableStrArray(subWs, 2, Mtblib.Tools.MtbVarType.Column);
                    cmnd.AppendFormat("stat {0} {1};\r\n", lslCol.SynthesizedName, uslCol.SynthesizedName);
                    cmnd.AppendFormat("by {0};\r\n", byValCol.SynthesizedName);
                    cmnd.AppendFormat("maxi {0} {1}.\r\n", tmpColId[0], tmpColId[1]);

                    // 要留意某一組內資料完全相同的狀況
                    cmnd.AppendFormat("mcapa {0};\r\n", subRawDataCol.SynthesizedName);
                    cmnd.AppendFormat("by {0};\r\n", byValCol.SynthesizedName);
                    cmnd.AppendLine("size 1;");
                    cmnd.AppendLine("brief 0;");
                    cmnd.AppendLine("ztag \"_Capa\";");
                    cmnd.AppendFormat("lspec {0};\r\n", tmpColId[0]);
                    cmnd.AppendFormat("uspec {0};\r\n", tmpColId[1]);
                    cmnd.AppendFormat("ppk {0}.\r\n", PpkCol);

                    DataTable _dt = Mtblib.Tools.MtbTools.Apply("LSL", Mtblib.Tools.Arithmetic.Max, new string[] { "BYVAL" }, subRawData);
                    LSLInfo.AddRange(_dt.AsEnumerable().Select(r => r.Field<double>("VALUE")));
                    _dt = Mtblib.Tools.MtbTools.Apply("USL", Mtblib.Tools.Arithmetic.Max, new string[] { "BYVAL" }, subRawData);
                    USLInfo.AddRange(_dt.AsEnumerable().Select(r => r.Field<double>("VALUE")));
                }

                try
                {
                    filepath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mycapa.mtb", cmnd.ToString());
                    proj.ExecuteCommand(string.Format("exec \"{0}\" 1", filepath), subWs);
                    PpkList.AddRange(subWs.Columns.Item(PpkCol).GetData());
                }
                catch
                {
                    PpkList.Add(MISSINGVALUE);
                }

            }



            //計算基本統計量
            Mtb.Column rawDataCol = ws.Columns.Item("VALUE");
            double mean = MISSINGVALUE;
            double stdev = MISSINGVALUE;
            double maximum = MISSINGVALUE;
            double minimum = MISSINGVALUE;
            try
            {
                mean = ((double[])rawDataCol.GetData()).Where(x => x < MISSINGVALUE).Average();
                stdev = ((double[])rawDataCol.GetData()).Where(x => x < MISSINGVALUE).StdDev();
                maximum = ((double[])rawDataCol.GetData()).Where(x => x < MISSINGVALUE).Max();
                minimum = ((double[])rawDataCol.GetData()).Where(x => x < MISSINGVALUE).Min();
            }
            catch
            {
            }

            //組合報表物件
            // Mean, Max, Min, LCL, UCL, LSL, USL, Ppk, Level
            string summPath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("summ_{0}.txt", item_index));
            using (StreamWriter sw = new StreamWriter(summPath, false))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("平均數\t{0}\r\n", mean < MISSINGVALUE ? mean.ToString("N") : "*");
                sb.AppendFormat("標準差\t{0}\r\n", stdev < MISSINGVALUE ? stdev.ToString("N") : "*");
                sb.AppendFormat("最大值\t{0}\r\n", maximum < MISSINGVALUE ? maximum.ToString("N") : "*");
                sb.AppendFormat("最小值\t{0}\r\n", minimum < MISSINGVALUE ? minimum.ToString("N") : "*");
                sb.AppendFormat("Cpk\t{0}\r\n", string.Join(";", PpkList.Select(x => x < MISSINGVALUE ? x.ToString("N") : "*")));
                sb.AppendFormat("Level\t{0}\r\n", string.Join(";",
                    PpkList.Select(x => x == MISSINGVALUE ? "*" :
                        x < 0.67 ? "E" :
                        x >= 0.67 && x < 1 ? "D" :
                        x >= 1 && x < 1.33 ? "C" :
                        x >= 1.33 && x < 1.67 ? "B" : "A"
                        )));
                sw.WriteLine(sb.ToString());
               
            }
            this.Contents.Add(
                new RptOutput()
                {
                    OType = MtbOType.PARAGRAPH,
                    OutputInByteArr = File.ReadAllBytes(summPath),
                }
                );

            //刪除工作表和指令
            proj.Worksheets.Delete();
            proj.Commands.Delete();

        }
    }
}
