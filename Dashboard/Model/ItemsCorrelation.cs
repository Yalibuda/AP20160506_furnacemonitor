using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.Model
{
    public class ItemsCorrelation : Report
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
        #region Revised Code 0
        //public override void Execute(Mtb.Project proj)
        //{
        //    # region read data
        //    // 將資料匯入 Minitab
        //    if (_rawdata == null || _rawdata.Rows.Count == 0)
        //    {
        //        throw new ArgumentNullException("查無對應資料");
        //    }

        //    _rptLst = new List<IRptOutput>(); //重新建立一個分析結果列舉
        //    proj.Commands.Delete();
        //    proj.Worksheets.Delete();
        //    Mtb.Worksheet ws = proj.Worksheets.Add(1); //新增工作表            
        //    Mtblib.Tools.MtbTools.InsertDataTableToMtbWs(_rawdata, ws); //匯入資料至 Minitab

        //    List<Mtb.Column> varCols = new List<Mtb.Column>(); //變數的欄位集合
        //    List<Mtb.Column> bkCol = new List<Mtb.Column>(); //斷絲率的集合
        //    Mtb.Column timeCol = null;

        //    for (int i = 0; i < _rawdata.Columns.Count; i++)
        //    {
        //        DataColumn col = _rawdata.Columns[i];
        //        switch (col.ColumnName)
        //        {
        //            case "TIMESTAMP":
        //                timeCol = ws.Columns.Item(col.ColumnName);
        //                break;
        //            case "RPT_TIMEHOUR":
        //            case "GROUP_ID":
        //                break;
        //            case "BK":
        //                bkCol.Add(ws.Columns.Item(col.ColumnName));
        //                break;
        //            default:
        //                varCols.Add(ws.Columns.Item(col.ColumnName));
        //                break;
        //        }
        //    }

        //    foreach (var col in bkCol)
        //    {
        //        if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
        //        {
        //            throw new ArgumentNullException(string.Format("[{0}]查無資料", col.Name));
        //        }
        //    }
        //    foreach (var col in varCols)
        //    {
        //        if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
        //        {
        //            throw new ArgumentNullException(string.Format("[{0}]查無資料", col.Name));
        //        }
        //    }
        //    # endregion

        //    # region Create command
        //    StringBuilder cmnd = new StringBuilder();
        //    /*
        //     * Create TSPlot
        //     * 該圖會將數據於多個平面中顯示(k*1的方式)，所以要先設定 Panel 的安排方式
        //     * 
        //     */
        //    cmnd.AppendLine("notitle");
        //    cmnd.AppendLine("brief 0");
        //    cmnd.AppendFormat("fdate {0};\r\n", timeCol.SynthesizedName);
        //    cmnd.AppendLine("format(dtyyyy-MM-dd hh:mm).");
        //    cmnd.AppendFormat("tsplot {0} {1};\r\n", string.Join(" &\r\n", bkCol.Select(x => x.SynthesizedName)),
        //        string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));//

        //    double gHeight = 4;
        //    if (varCols.Count > 3) gHeight = Math.Min(30, 4 + ((double)varCols.Count - 3) * 0.8);
        //    cmnd.AppendFormat("graph 8 {0};\r\n", gHeight);
        //    string gpath_trend = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("trend_{0}.jpg", _rawdata.TableName));
        //    cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath_trend);
        //    cmnd.AppendLine("repl;");
        //    cmnd.AppendLine("jpeg;");
        //    cmnd.AppendLine("panel;");
        //    cmnd.AppendFormat("rc {0} 1;\r\n", varCols.Count + bkCol.Count);
        //    cmnd.AppendLine("noal;");
        //    cmnd.AppendLine("label;");
        //    cmnd.AppendLine(" psize 5;");
        //    cmnd.AppendLine("scale 1;");
        //    cmnd.AppendLine(" psize 5;");
        //    cmnd.AppendLine("axlab 1;");
        //    cmnd.AppendLine("lshow;");
        //    cmnd.AppendLine(" angle 90;");
        //    cmnd.AppendLine("scale 2;");
        //    cmnd.AppendLine(" psize 5;");
        //    if (timeCol != null && (int)timeCol.DataType != 3) cmnd.AppendFormat("stamp {0};\r\n", timeCol.SynthesizedName);
        //    cmnd.AppendLine("symb;");
        //    cmnd.AppendLine("conn;");
        //    //cmnd.AppendFormat("foot \"建立時間: {0}\";\r\n", DateTime.Now);
        //    cmnd.AppendLine("nodt.");

        //    //Create Matrix Plot
        //    cmnd.AppendFormat("matrixplot {0} {1};\r\n", string.Join(" &\r\n", bkCol.Select(x => x.SynthesizedName)),
        //       string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));//

        //    double gWidth = 8;
        //    if (varCols.Count > 5) gWidth = Math.Min(30, 8 + ((double)varCols.Count - 5) * 0.8);
        //    cmnd.AppendFormat("graph {0} 5;\r\n", gWidth);
        //    string gpath_corr = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("corr_{0}.jpg", _rawdata.TableName));
        //    cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath_corr);
        //    cmnd.AppendLine("repl;");
        //    cmnd.AppendLine("jpeg;");
        //    cmnd.AppendLine("scale 1;");
        //    cmnd.AppendLine(" psize 5;");
        //    cmnd.AppendLine("nojitter;");
        //    cmnd.AppendLine("full;");
        //    cmnd.AppendLine("bound;");
        //    cmnd.AppendLine("noal;");
        //    cmnd.AppendLine("regr;");
        //    cmnd.AppendLine("symb;");
        //    cmnd.AppendLine("nodt.");

        //    //Calculate correlation coefficient 
        //    string[] matIds = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, 1, Mtblib.Tools.MtbVarType.Matrix);
        //    cmnd.AppendFormat("corr {0} {1} {2}\r\n",
        //        string.Join(" &\r\n", bkCol.Select(x => x.SynthesizedName)),//
        //        string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)),
        //        string.Join(" &\r\n", matIds)
        //        );

        //    //刪除所有圖形
        //    cmnd.AppendLine("gmana;");
        //    cmnd.AppendLine("all;");
        //    cmnd.AppendLine("close;");
        //    cmnd.AppendLine("nopr.");

        //    string fpath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
        //    proj.ExecuteCommand(string.Format("exec \"{0}\" 1", fpath), ws);
        //    #endregion

        //    # region Transform file
        //    //將檔案轉為二進位陣列
        //    if (File.Exists(gpath_trend))
        //    {
        //        this.Contents.Add(new RptOutput()
        //        {
        //            OType = MtbOType.GRAPH,
        //            OutputInByteArr = File.ReadAllBytes(gpath_trend),
        //            Tag = "Trend"
        //        });
        //    }
        //    if (File.Exists(gpath_corr))
        //    {
        //        this.Contents.Add(new RptOutput()
        //        {
        //            OType = MtbOType.GRAPH,
        //            OutputInByteArr = File.ReadAllBytes(gpath_corr),
        //            Tag = "Scatter"
        //        });
        //    }
        //    # endregion

        //    #region CreateCorrTable raw
        //    ////Create Correlation table            
        //    //DataTable corrTable = new DataTable();
        //    //corrTable.Columns.Add("ItemName", typeof(string));
        //    //corrTable.Columns.Add("CorrCoef", typeof(double));

        //    //var corrMat = ws.Matrices.Item(matIds[0]).GetData();
        //    //double[] subCorr = new double[varCols.Count];
        //    //double[] corr = corrMat as double[];
        //    //if (corr != null)
        //    //{
        //    //    Array.Copy(corr, 1, subCorr, 0, varCols.Count);
        //    //    string[] labNames = varCols.Select(x => x.Name).ToArray();
        //    //    DataRow dr;
        //    //    for (int i = 0; i < subCorr.Length; i++)
        //    //    {
        //    //        dr = corrTable.NewRow();
        //    //        dr[0] = labNames[i];
        //    //        dr[1] = subCorr[i];
        //    //        corrTable.Rows.Add(dr);
        //    //    }
        //    //}
        //    //this.Contents.Add(new RptOutput()
        //    //{
        //    //    OType = MtbOType.TABLE,
        //    //    OutputInByteArr = Tool.ConvertDataSetToByteArray(corrTable)
        //    //});
        //    #endregion

        //    #region CreateCorrTable revised
        //    //Create Correlation table            
        //    DataTable corrTable = new DataTable();
        //    corrTable.Columns.Add("ItemName", typeof(string));
        //    corrTable.Columns.Add("CorrCoef", typeof(double));//
            

        //    var corrMat = ws.Matrices.Item(matIds[0]).GetData();
        //    double[] subCorr = new double[varCols.Count];
        //    double[] corr = corrMat as double[];
        //    if (corr != null)
        //    {
        //        Array.Copy(corr, 1, subCorr, 0, varCols.Count);
        //        string[] labNames = varCols.Select(x => x.Name).ToArray();
        //        DataRow dr;
        //        for (int i = 0; i < subCorr.Length; i++)
        //        {
        //            dr = corrTable.NewRow();
        //            dr[0] = labNames[i];
        //            dr[1] = subCorr[i];
        //            corrTable.Rows.Add(dr);
        //        }
        //    }
        //    this.Contents.Add(new RptOutput()
        //    {
        //        OType = MtbOType.TABLE,
        //        OutputInByteArr = Tool.ConvertDataSetToByteArray(corrTable)
        //    });
        //    #endregion
        //}
        #endregion

        # region revised code 1
        //public override void Execute(Mtb.Project proj)
        //{
        //    # region read data
        //    // 將資料匯入 Minitab
        //    if (_rawdata == null || _rawdata.Rows.Count == 0)
        //    {
        //        throw new ArgumentNullException("查無對應資料");
        //    }

        //    _rptLst = new List<IRptOutput>(); //重新建立一個分析結果列舉
        //    proj.Commands.Delete();
        //    proj.Worksheets.Delete();
        //    Mtb.Worksheet ws = proj.Worksheets.Add(1); //新增工作表            
        //    Mtblib.Tools.MtbTools.InsertDataTableToMtbWs(_rawdata, ws); //匯入資料至 Minitab

        //    List<Mtb.Column> varCols = new List<Mtb.Column>(); //變數的欄位集合
        //    List<Mtb.Column> bkCol = new List<Mtb.Column>(); //斷絲率的集合
        //    Mtb.Column timeCol = null;

        //    for (int i = 0; i < _rawdata.Columns.Count; i++)
        //    {
        //        DataColumn col = _rawdata.Columns[i];
        //        switch (col.ColumnName)
        //        {
        //            case "TIMESTAMP":
        //                timeCol = ws.Columns.Item(col.ColumnName);
        //                break;
        //            case "RPT_TIMEHOUR":
        //            case "GROUP_ID":
        //                break;
        //            case "BK":
        //                bkCol.Add(ws.Columns.Item(col.ColumnName));
        //                break;
        //            default:
        //                varCols.Add(ws.Columns.Item(col.ColumnName));
        //                break;
        //        }
        //    }

        //    foreach (var col in bkCol)
        //    {
        //        if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
        //        {
        //            throw new ArgumentNullException(string.Format("[{0}]查無資料", col.Name));
        //        }
        //    }
        //    foreach (var col in varCols)
        //    {
        //        if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
        //        {
        //            throw new ArgumentNullException(string.Format("[{0}]查無資料", col.Name));
        //        }
        //    }
        //    # endregion

        //    # region Create command
        //    StringBuilder cmnd = new StringBuilder();
        //    /*
        //     * Create TSPlot
        //     * 該圖會將數據於多個平面中顯示(k*1的方式)，所以要先設定 Panel 的安排方式
        //     * 
        //     */
        //    cmnd.AppendLine("notitle");
        //    cmnd.AppendLine("brief 0");
        //    cmnd.AppendFormat("fdate {0};\r\n", timeCol.SynthesizedName);
        //    cmnd.AppendLine("format(dtyyyy-MM-dd hh:mm).");
        //    cmnd.AppendFormat("tsplot {0} ;\r\n",
        //        string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));//

        //    double gHeight = 4;
        //    if (varCols.Count > 3) gHeight = Math.Min(30, 4 + ((double)varCols.Count - 3) * 0.8);
        //    cmnd.AppendFormat("graph 8 {0};\r\n", gHeight);
        //    string gpath_trend = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("trend_{0}.jpg", _rawdata.TableName));
        //    cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath_trend);
        //    cmnd.AppendLine("repl;");
        //    cmnd.AppendLine("jpeg;");
        //    cmnd.AppendLine("panel;");
        //    cmnd.AppendFormat("rc {0} 1;\r\n", varCols.Count + bkCol.Count);
        //    cmnd.AppendLine("noal;");
        //    cmnd.AppendLine("label;");
        //    cmnd.AppendLine(" psize 5;");
        //    cmnd.AppendLine("scale 1;");
        //    cmnd.AppendLine(" psize 5;");
        //    cmnd.AppendLine("axlab 1;");
        //    cmnd.AppendLine("lshow;");
        //    cmnd.AppendLine(" angle 90;");
        //    cmnd.AppendLine("scale 2;");
        //    cmnd.AppendLine(" psize 5;");
        //    if (timeCol != null && (int)timeCol.DataType != 3) cmnd.AppendFormat("stamp {0};\r\n", timeCol.SynthesizedName);
        //    cmnd.AppendLine("symb;");
        //    cmnd.AppendLine("conn;");
        //    //cmnd.AppendFormat("foot \"建立時間: {0}\";\r\n", DateTime.Now);
        //    cmnd.AppendLine("nodt.");

        //    //Create Matrix Plot
        //    cmnd.AppendFormat("matrixplot {0};\r\n",
        //       string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));//

        //    double gWidth = 8;
        //    if (varCols.Count > 5) gWidth = Math.Min(30, 8 + ((double)varCols.Count - 5) * 0.8);
        //    cmnd.AppendFormat("graph {0} 5;\r\n", gWidth);
        //    string gpath_corr = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("corr_{0}.jpg", _rawdata.TableName));
        //    cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath_corr);
        //    cmnd.AppendLine("repl;");
        //    cmnd.AppendLine("jpeg;");
        //    cmnd.AppendLine("scale 1;");
        //    cmnd.AppendLine(" psize 5;");
        //    cmnd.AppendLine("nojitter;");
        //    cmnd.AppendLine("full;");
        //    cmnd.AppendLine("bound;");
        //    cmnd.AppendLine("noal;");
        //    cmnd.AppendLine("regr;");
        //    cmnd.AppendLine("symb;");
        //    cmnd.AppendLine("nodt.");

        //    //Calculate correlation coefficient 
        //    string[] matIds = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, 1, Mtblib.Tools.MtbVarType.Matrix);
        //    cmnd.AppendFormat("corr {0} {1} {2}\r\n",
        //        string.Join(" &\r\n", bkCol.Select(x => x.SynthesizedName)),//
        //        string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)),
        //        string.Join(" &\r\n", matIds)
        //        );

        //    //刪除所有圖形
        //    cmnd.AppendLine("gmana;");
        //    cmnd.AppendLine("all;");
        //    cmnd.AppendLine("close;");
        //    cmnd.AppendLine("nopr.");

        //    string fpath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
        //    proj.ExecuteCommand(string.Format("exec \"{0}\" 1", fpath), ws);
        //    #endregion

        //    # region Transform file
        //    //將檔案轉為二進位陣列
        //    if (File.Exists(gpath_trend))
        //    {
        //        this.Contents.Add(new RptOutput()
        //        {
        //            OType = MtbOType.GRAPH,
        //            OutputInByteArr = File.ReadAllBytes(gpath_trend),
        //            Tag = "Trend"
        //        });
        //    }
        //    if (File.Exists(gpath_corr))
        //    {
        //        this.Contents.Add(new RptOutput()
        //        {
        //            OType = MtbOType.GRAPH,
        //            OutputInByteArr = File.ReadAllBytes(gpath_corr),
        //            Tag = "Scatter"
        //        });
        //    }
        //    # endregion

        //    #region CreateCorrTable revised
        //    //Create Correlation table            
        //    DataTable corrTable = new DataTable();
        //    corrTable.Columns.Add("ItemName", typeof(string));
        //    corrTable.Columns.Add("CorrCoef", typeof(double));
        //    corrTable.Columns.Add("CorrCoef2", typeof(double));//
        //    //corrTable.Columns.Add("Column A", typeof(string));
        //    //corrTable.Columns.Add("Column B", typeof(double));
        //    //corrTable.Columns.Add("Column C", typeof(double));
        //    //corrTable.Rows.Add("D1",1.0,3.0);
        //    //corrTable.Rows.Add("D2", 1.02, 3.01);

        //    var corrMat = ws.Matrices.Item(matIds[0]).GetData();
        //    double[] subCorr = new double[varCols.Count];
        //    double[] corr = corrMat as double[];
        //    if (corr != null)
        //    {
        //        Array.Copy(corr, 1, subCorr, 0, varCols.Count);
        //        string[] labNames = varCols.Select(x => x.Name).ToArray();
        //        DataRow dr;
        //        for (int i = 0; i < subCorr.Length; i++)
        //        {
        //            dr = corrTable.NewRow();

        //            //object[] o = new object[subCorr.Length + 1];
        //            object[] o = new object[3];
        //            o[0] = labNames[i];
        //            o[1] = subCorr[i];
        //            o[2] = subCorr[i];
        //            corrTable.Rows.Add(o);
        //            //dr[0] = labNames[i];
        //            //dr[1] = subCorr[i];
        //            //dr[2] = subCorr[i];//
        //            //corrTable.Rows.Add(dr);

        //        }
        //    }
        //    this.Contents.Add(new RptOutput()
        //    {
        //        OType = MtbOType.TABLE,
        //        OutputInByteArr = Tool.ConvertDataSetToByteArray(corrTable)
        //    });
            #endregion

            #region CreateCorrTable raw
            ////Create Correlation table            
            //DataTable corrTable = new DataTable();
            //corrTable.Columns.Add("ItemName", typeof(string));
            //corrTable.Columns.Add("CorrCoef", typeof(double));

            //var corrMat = ws.Matrices.Item(matIds[0]).GetData();
            //double[] subCorr = new double[varCols.Count];
            //double[] corr = corrMat as double[];
            //if (corr != null)
            //{
            //    Array.Copy(corr, 1, subCorr, 0, varCols.Count);
            //    string[] labNames = varCols.Select(x => x.Name).ToArray();
            //    DataRow dr;
            //    for (int i = 0; i < subCorr.Length; i++)
            //    {
            //        dr = corrTable.NewRow();
            //        dr[0] = labNames[i];
            //        dr[1] = subCorr[i];
            //        corrTable.Rows.Add(dr);
            //    }
            //}
            //this.Contents.Add(new RptOutput()
            //{
            //    OType = MtbOType.TABLE,
            //    OutputInByteArr = Tool.ConvertDataSetToByteArray(corrTable)
            //});
            //#endregion

        //}
        #endregion 

        #region Revised Code 2
        public override void Execute(Mtb.Project proj)
        {
            # region read data
            // 將資料匯入 Minitab
            if (_rawdata == null || _rawdata.Rows.Count == 0)
            {
                throw new ArgumentNullException("查無對應資料");
            }

            _rptLst = new List<IRptOutput>(); //重新建立一個分析結果列舉
            proj.Commands.Delete();
            proj.Worksheets.Delete();
            Mtb.Worksheet ws = proj.Worksheets.Add(1); //新增工作表            
            Mtblib.Tools.MtbTools.InsertDataTableToMtbWs(_rawdata, ws); //匯入資料至 Minitab

            List<Mtb.Column> varCols = new List<Mtb.Column>(); //變數的欄位集合
            //List<Mtb.Column> bkCol = new List<Mtb.Column>(); //斷絲率的集合
            Mtb.Column timeCol = null;

            for (int i = 0; i < _rawdata.Columns.Count; i++)
            {
                DataColumn col = _rawdata.Columns[i];
                switch (col.ColumnName)
                {
                    case "TIMESTAMP":
                        timeCol = ws.Columns.Item(col.ColumnName);
                        break;
                    case "RPT_TIMEHOUR":
                    case "GROUP_ID":
                        break;
                    case "BK":
                    //    bkCol.Add(ws.Columns.Item(col.ColumnName));
                        break;
                    default:
                        varCols.Add(ws.Columns.Item(col.ColumnName));
                        break;
                }
            }

            //foreach (var col in bkCol)
            //{
            //    if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
            //    {
            //        throw new ArgumentNullException(string.Format("[{0}]查無資料", col.Name));
            //    }
            //}
            foreach (var col in varCols)
            {
                if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
                {
                    throw new ArgumentNullException(string.Format("[{0}]查無資料", col.Name));
                }
            }
            # endregion

            # region Create command
            StringBuilder cmnd = new StringBuilder();
            /*
             * Create TSPlot
             * 該圖會將數據於多個平面中顯示(k*1的方式)，所以要先設定 Panel 的安排方式
             * 
             */
            cmnd.AppendLine("notitle");
            cmnd.AppendLine("brief 0");
            cmnd.AppendFormat("fdate {0};\r\n", timeCol.SynthesizedName);
            cmnd.AppendLine("format(dtyyyy-MM-dd hh:mm).");
            cmnd.AppendFormat("tsplot {0} ;\r\n", 
                string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));//

            double gHeight = 4;
            if (varCols.Count > 3) gHeight = Math.Min(30, 4 + ((double)varCols.Count - 3) * 0.8);
            cmnd.AppendFormat("graph 8 {0};\r\n", gHeight);
            string gpath_trend = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("trend_{0}.jpg", _rawdata.TableName));
            cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath_trend);
            cmnd.AppendLine("repl;");
            cmnd.AppendLine("jpeg;");
            cmnd.AppendLine("panel;");
            cmnd.AppendFormat("rc {0} 1;\r\n", varCols.Count);//+ bkCol.Count
            cmnd.AppendLine("noal;");
            cmnd.AppendLine("label;");
            cmnd.AppendLine(" psize 5;");
            cmnd.AppendLine("scale 1;");
            cmnd.AppendLine(" psize 5;");
            cmnd.AppendLine("axlab 1;");
            cmnd.AppendLine("lshow;");
            cmnd.AppendLine(" angle 90;");
            cmnd.AppendLine("scale 2;");
            cmnd.AppendLine(" psize 5;");
            if (timeCol != null && (int)timeCol.DataType != 3) cmnd.AppendFormat("stamp {0};\r\n", timeCol.SynthesizedName);
            cmnd.AppendLine("symb;");
            cmnd.AppendLine("conn;");
            //cmnd.AppendFormat("foot \"建立時間: {0}\";\r\n", DateTime.Now);
            cmnd.AppendLine("nodt.");

            //Create Matrix Plot
            cmnd.AppendFormat("matrixplot {0};\r\n", 
               string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));//

            double gWidth = 8;
            if (varCols.Count > 5) gWidth = Math.Min(30, 8 + ((double)varCols.Count - 5) * 0.8);
            cmnd.AppendFormat("graph {0} 5;\r\n", gWidth);
            string gpath_corr = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("corr_{0}.jpg", _rawdata.TableName));
            cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath_corr);
            cmnd.AppendLine("repl;");
            cmnd.AppendLine("jpeg;");
            cmnd.AppendLine("scale 1;");
            cmnd.AppendLine(" psize 5;");
            cmnd.AppendLine("nojitter;");
            cmnd.AppendLine("full;");
            cmnd.AppendLine("bound;");
            cmnd.AppendLine("noal;");
            cmnd.AppendLine("regr;");
            cmnd.AppendLine("symb;");
            cmnd.AppendLine("nodt.");

            //Calculate correlation coefficient 
            string[] matIds = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, 1, Mtblib.Tools.MtbVarType.Matrix);
            cmnd.AppendFormat("corr {0} {1} \r\n", //{2}
                //string.Join(" &\r\n", bkCol.Select(x => x.SynthesizedName)),//
                string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)),
                string.Join(" &\r\n", matIds)
                );

            //刪除所有圖形
            cmnd.AppendLine("gmana;");
            cmnd.AppendLine("all;");
            cmnd.AppendLine("close;");
            cmnd.AppendLine("nopr.");

            string fpath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
            proj.ExecuteCommand(string.Format("exec \"{0}\" 1", fpath), ws);
            #endregion

            # region Transform file
            //將檔案轉為二進位陣列
            if (File.Exists(gpath_trend))
            {
                this.Contents.Add(new RptOutput()
                {
                    OType = MtbOType.GRAPH,
                    OutputInByteArr = File.ReadAllBytes(gpath_trend),
                    Tag = "Trend"
                });
            }
            if (File.Exists(gpath_corr))
            {
                this.Contents.Add(new RptOutput()
                {
                    OType = MtbOType.GRAPH,
                    OutputInByteArr = File.ReadAllBytes(gpath_corr),
                    Tag = "Scatter"
                });
            }
            # endregion

            #region CreateCorrTable
            //Create Correlation table            
            DataTable corrTable = new DataTable();
            corrTable.Columns.Add("ItemName", typeof(string));
            //corrTable.Columns.Add("CorrCoef", typeof(double));

            var corrMat = ws.Matrices.Item(matIds[0]).GetData();
            double[] subCorr = new double[varCols.Count];
            double[] corr = corrMat as double[];
            if (corr != null)
            {
                Array.Copy(corr, 1, subCorr, 0, varCols.Count);
                string[] labNames = varCols.Select(x => x.Name).ToArray();
                DataRow dr;
                for (int i = 0; i < labNames.Length; i++)
                {
                    corrTable.Columns.Add(labNames[i], typeof(double));
                }

                for (int i = 0; i < labNames.Length; i++)
                {
                    dr = corrTable.NewRow();
                    dr[0] = labNames[i];
                    for (int j = 0; j < subCorr.Length; j++)
                    {
                        dr[j + 1] = Math.Round((corr[labNames.Length * i + j]), 2);
                        //dr[j + 1] = corr[(subCorr.Length + 1) * (i + 1) + j + 1];
                    }
                    corrTable.Rows.Add(dr);
                }
            }
            //if (corr != null)
            //{
            //    Array.Copy(corr, 1, subCorr, 0, varCols.Count);
            //    string[] labNames = varCols.Select(x => x.Name).ToArray();
            //    DataRow dr;
            //    for (int i = 0; i < subCorr.Length; i++)
            //    {
            //        dr = corrTable.NewRow();
            //        dr[0] = labNames[i];
            //        dr[1] = subCorr[i];
            //        corrTable.Rows.Add(dr);
            //    }
            //}
            this.Contents.Add(new RptOutput()
            {
                OType = MtbOType.TABLE,
                OutputInByteArr = Tool.ConvertDataSetToByteArray(corrTable)
            });
            #endregion

        }

        #endregion

        #region Raw Code
        //public override void Execute(Mtb.Project proj)
        //{
        //    # region read data
        //    // 將資料匯入 Minitab
        //    if (_rawdata == null || _rawdata.Rows.Count == 0)
        //    {
        //        throw new ArgumentNullException("查無對應資料");
        //    }

        //    _rptLst = new List<IRptOutput>(); //重新建立一個分析結果列舉
        //    proj.Commands.Delete();
        //    proj.Worksheets.Delete();
        //    Mtb.Worksheet ws = proj.Worksheets.Add(1); //新增工作表            
        //    Mtblib.Tools.MtbTools.InsertDataTableToMtbWs(_rawdata, ws); //匯入資料至 Minitab

        //    List<Mtb.Column> varCols = new List<Mtb.Column>(); //變數的欄位集合
        //    List<Mtb.Column> bkCol = new List<Mtb.Column>(); //斷絲率的集合
        //    Mtb.Column timeCol = null;

        //    for (int i = 0; i < _rawdata.Columns.Count; i++)
        //    {
        //        DataColumn col = _rawdata.Columns[i];
        //        switch (col.ColumnName)
        //        {
        //            case "TIMESTAMP":
        //                timeCol = ws.Columns.Item(col.ColumnName);
        //                break;
        //            case "RPT_TIMEHOUR":
        //            case "GROUP_ID":
        //                break;
        //            case "BK":
        //                bkCol.Add(ws.Columns.Item(col.ColumnName));
        //                break;
        //            default:
        //                varCols.Add(ws.Columns.Item(col.ColumnName));
        //                break;
        //        }
        //    }

        //    foreach (var col in bkCol)
        //    {
        //        if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
        //        {
        //            throw new ArgumentNullException(string.Format("[{0}]查無資料", col.Name));
        //        }
        //    }
        //    foreach (var col in varCols)
        //    {
        //        if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
        //        {
        //            throw new ArgumentNullException(string.Format("[{0}]查無資料", col.Name));
        //        }
        //    }
        //    # endregion

        //    # region Create command
        //    StringBuilder cmnd = new StringBuilder();
        //    /*
        //     * Create TSPlot
        //     * 該圖會將數據於多個平面中顯示(k*1的方式)，所以要先設定 Panel 的安排方式
        //     * 
        //     */
        //    cmnd.AppendLine("notitle");
        //    cmnd.AppendLine("brief 0");
        //    cmnd.AppendFormat("fdate {0};\r\n", timeCol.SynthesizedName);
        //    cmnd.AppendLine("format(dtyyyy-MM-dd hh:mm).");
        //    cmnd.AppendFormat("tsplot {0} {1};\r\n", string.Join(" &\r\n", bkCol.Select(x => x.SynthesizedName)),
        //        string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));//

        //    double gHeight = 4;
        //    if (varCols.Count > 3) gHeight = Math.Min(30, 4 + ((double)varCols.Count - 3) * 0.8);
        //    cmnd.AppendFormat("graph 8 {0};\r\n", gHeight);
        //    string gpath_trend = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("trend_{0}.jpg", _rawdata.TableName));
        //    cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath_trend);
        //    cmnd.AppendLine("repl;");
        //    cmnd.AppendLine("jpeg;");
        //    cmnd.AppendLine("panel;");
        //    cmnd.AppendFormat("rc {0} 1;\r\n", varCols.Count + bkCol.Count);
        //    cmnd.AppendLine("noal;");
        //    cmnd.AppendLine("label;");
        //    cmnd.AppendLine(" psize 5;");
        //    cmnd.AppendLine("scale 1;");
        //    cmnd.AppendLine(" psize 5;");
        //    cmnd.AppendLine("axlab 1;");
        //    cmnd.AppendLine("lshow;");
        //    cmnd.AppendLine(" angle 90;");
        //    cmnd.AppendLine("scale 2;");
        //    cmnd.AppendLine(" psize 5;");
        //    if (timeCol != null && (int)timeCol.DataType != 3) cmnd.AppendFormat("stamp {0};\r\n", timeCol.SynthesizedName);
        //    cmnd.AppendLine("symb;");
        //    cmnd.AppendLine("conn;");
        //    //cmnd.AppendFormat("foot \"建立時間: {0}\";\r\n", DateTime.Now);
        //    cmnd.AppendLine("nodt.");

        //    //Create Matrix Plot
        //    cmnd.AppendFormat("matrixplot ({0})*({1});\r\n", string.Join(" &\r\n", bkCol.Select(x => x.SynthesizedName)),
        //       string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));//

        //    double gWidth = 8;
        //    if (varCols.Count > 5) gWidth = Math.Min(30, 8 + ((double)varCols.Count - 5) * 0.8);
        //    cmnd.AppendFormat("graph {0} 5;\r\n", gWidth);
        //    string gpath_corr = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("corr_{0}.jpg", _rawdata.TableName));
        //    cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath_corr);
        //    cmnd.AppendLine("repl;");
        //    cmnd.AppendLine("jpeg;");
        //    cmnd.AppendLine("scale 1;");
        //    cmnd.AppendLine(" psize 5;");
        //    cmnd.AppendLine("nojitter;");
        //    cmnd.AppendLine("full;");
        //    cmnd.AppendLine("bound;");
        //    cmnd.AppendLine("noal;");
        //    cmnd.AppendLine("regr;");
        //    cmnd.AppendLine("symb;");
        //    cmnd.AppendLine("nodt.");

        //    //Calculate correlation coefficient 
        //    string[] matIds = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, 1, Mtblib.Tools.MtbVarType.Matrix);
        //    cmnd.AppendFormat("corr {0} {1} {2}\r\n",
        //        string.Join(" &\r\n", bkCol.Select(x => x.SynthesizedName)),//
        //        string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)),
        //        string.Join(" &\r\n", matIds)
        //        );

        //    //刪除所有圖形
        //    cmnd.AppendLine("gmana;");
        //    cmnd.AppendLine("all;");
        //    cmnd.AppendLine("close;");
        //    cmnd.AppendLine("nopr.");

        //    string fpath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
        //    proj.ExecuteCommand(string.Format("exec \"{0}\" 1", fpath), ws);
        //    #endregion

        //    # region Transform file
        //    //將檔案轉為二進位陣列
        //    if (File.Exists(gpath_trend))
        //    {
        //        this.Contents.Add(new RptOutput()
        //        {
        //            OType = MtbOType.GRAPH,
        //            OutputInByteArr = File.ReadAllBytes(gpath_trend),
        //            Tag = "Trend"
        //        });
        //    }
        //    if (File.Exists(gpath_corr))
        //    {
        //        this.Contents.Add(new RptOutput()
        //        {
        //            OType = MtbOType.GRAPH,
        //            OutputInByteArr = File.ReadAllBytes(gpath_corr),
        //            Tag = "Scatter"
        //        });
        //    }
        //    # endregion

        //    #region CreateCorrTable
        //    //Create Correlation table            
        //    DataTable corrTable = new DataTable();
        //    corrTable.Columns.Add("ItemName", typeof(string));
        //    corrTable.Columns.Add("CorrCoef", typeof(double));

        //    var corrMat = ws.Matrices.Item(matIds[0]).GetData();
        //    double[] subCorr = new double[varCols.Count];
        //    double[] corr = corrMat as double[];
        //    if (corr != null)
        //    {
        //        Array.Copy(corr, 1, subCorr, 0, varCols.Count);
        //        string[] labNames = varCols.Select(x => x.Name).ToArray();
        //        DataRow dr;
        //        for (int i = 0; i < subCorr.Length; i++)
        //        {
        //            dr = corrTable.NewRow();
        //            dr[0] = labNames[i];
        //            dr[1] = subCorr[i];
        //            corrTable.Rows.Add(dr);
        //        }
        //    }
        //    this.Contents.Add(new RptOutput()
        //    {
        //        OType = MtbOType.TABLE,
        //        OutputInByteArr = Tool.ConvertDataSetToByteArray(corrTable)
        //    });
        //    #endregion

        //}
        #endregion 

    }
}
