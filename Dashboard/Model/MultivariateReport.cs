using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LinearAlgebra = MathNet.Numerics.LinearAlgebra;

namespace Dashboard.Model
{
    public class MultivariateReport : Report
    {
        private DataTable _paraTable = null;
        //private double[] _mean = null;
        //private double[] _cov = null;
        //private double _sample = MISSINGVALUE;
        //private double _subgroup = 1;

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

        private List<TsquareParameters> _parameters = null;
        /// <summary>
        /// 指定或取得參數資料表(包含MEAN, COV, SAMPLE, SUBGROUP SIZE) 
        /// 輸入的表格欄位應包含(CHART_PARA_INDEX, SITE, FLAG, ROWNO, COLNO, VALUE, ITEM_LIST)
        /// </summary>
        public DataTable Parameters
        {
            get
            {
                return _paraTable;
            }
            set
            {
                _paraTable = value;

                if (_paraTable == null || _paraTable.Rows.Count == 0)
                {
                    _parameters = null;
                    return;
                }

                var rows = _paraTable.AsEnumerable();
                _parameters = new List<TsquareParameters>();

                var index = rows.Select(x => x.Field<string>("CHART_PARA_INDEX")).Distinct().ToArray();
                foreach (var id in index)
                {
                    TsquareParameters para = new TsquareParameters();
                    para.ParaID = id;

                    try //取得 Location
                    {
                        var m = LinearAlgebra.Matrix<double>.Build;
                        int colno = rows.Where(x => x["FLAG"].ToString() == "MEAN" && x["CHART_PARA_INDEX"].ToString() == id)
                            .Select(x => x.Field<Byte>("COLNO")).Max();
                        LinearAlgebra.Matrix<double> meanvector = m.Dense(1, colno,
                            rows.Where(x => x["FLAG"].ToString() == "MEAN" && x["CHART_PARA_INDEX"].ToString() == id)
                            .Select(x => Convert.ToDouble(x.Field<decimal>("VALUE"))).ToArray());
                        para.Mean = meanvector;
                    }
                    catch
                    {
                        para.Mean = null;
                    }

                    try //取得 Covariance
                    {
                        var m = LinearAlgebra.Matrix<double>.Build;
                        int rowno = rows.Where(x => x["FLAG"].ToString() == "COV" && x["CHART_PARA_INDEX"].ToString() == id)
                            .Select(x => x.Field<Byte>("ROWNO")).Max();
                        int colno = rows.Where(x => x["FLAG"].ToString() == "COV" && x["CHART_PARA_INDEX"].ToString() == id)
                            .Select(x => x.Field<Byte>("COLNO")).Max();
                        LinearAlgebra.Matrix<double> covariance = m.Dense(rowno, colno,
                            rows.Where(x => x["FLAG"].ToString() == "COV" && x["CHART_PARA_INDEX"].ToString() == id)
                            .OrderBy(x => x.Field<Byte>("COLNO")).ThenBy(x => x.Field<Byte>("ROWNO"))
                            .Select(x => Convert.ToDouble(x.Field<decimal>("VALUE"))).ToArray());
                        para.Covariance = covariance;
                        para.SampleSize = rows.Where(x => x["FLAG"].ToString() == "N" && x["CHART_PARA_INDEX"].ToString() == id)
                        .Select(x => Convert.ToDouble(x.Field<decimal>("VALUE"))).First();
                    }
                    catch
                    {
                        para.Covariance = null;
                        para.SampleSize = MISSINGVALUE;
                    }

                    try
                    {
                        para.SubgroupSize = rows.Where(x => x["FLAG"].ToString() == "SUBGP" && x["CHART_PARA_INDEX"].ToString() == id)
                            .Select(x => Convert.ToDouble(x.Field<decimal>("VALUE"))).First();
                    }
                    catch
                    {
                        para.SubgroupSize = 1;
                    }

                    _parameters.Add(para);
                }


            }
        }

        public override void Execute(Mtb.Project proj)
        {
            // 將資料匯入 Minitab
            if (_rawdata == null || _rawdata.Rows.Count == 0)
            {
                return;
            }
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

            foreach (var col in varCols)
            {
                if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
                {
                    throw new ArgumentNullException(string.Format("[{0}]查無資料-多變量管制圖", col.Name));
                }
            }

            StringBuilder cmnd = new StringBuilder();
            List<TsquareParameters> tmpParaList = new List<TsquareParameters>(); //用於計算 decomposition             
            string[] colIds = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, 4, Mtblib.Tools.MtbVarType.Column); //Plot point, CL, UCL and Test column

            // 指定繪圖時需要的欄位變數
            Mtb.Column
                pplotCol = ws.Columns.Item(colIds[0]),
                clCol = ws.Columns.Item(colIds[1]),
                uclCol = ws.Columns.Item(colIds[2]),
                oocCol = ws.Columns.Item(colIds[3]),
                timeCol = ws.Columns.Item("TIMESTAMP");


            // 計算 T2 plot points            
            if (_parameters == null || _parameters.Count == 0)
            {
                //自己算的時候需要的變數和子命令
                #region Phase I
                cmnd.AppendLine("macro");
                cmnd.AppendLine("myt2calculator x.1-x.p;");
                cmnd.AppendLine("variance cov ssiz;");
                cmnd.AppendLine("meanvect location; ");
                cmnd.AppendLine("ppoint t2;");
                cmnd.AppendLine("climit cl ucl;");
                cmnd.AppendLine("test ooc;");
                cmnd.AppendLine("siglevel alpha.");

                cmnd.AppendLine("mcolumn x.1-x.p t2");
                cmnd.AppendLine("mcolumn loc.1-loc.p xx.1-xx.p tmp.1-tmp.3 ooc ucl cl");
                cmnd.AppendLine("mconstant ssiz m alpha conf a1 a2 kk");
                cmnd.AppendLine("mmatrix location diff cov tdiff invCov");
                cmnd.AppendLine("default alpha = 0.0027");

                cmnd.AppendLine("rnmiss x.1-x.p tmp.1");
                cmnd.AppendLine("copy x.1-x.p xx.1-xx.p;");
                cmnd.AppendLine("exclude;");
                cmnd.AppendFormat("where \"tmp.1>0\".\r\n");
                cmnd.AppendLine("cova xx.1-xx.p cov"); // Get Covariance matrix
                cmnd.AppendLine("let ssiz = n(xx.1)"); // Get the sample size of covariance calculation, ALSO..THAT IS THE NUMBER OF NONMISSING OBSERVATIONS
                cmnd.AppendLine("stat x.1-x.p;");
                cmnd.AppendLine("mean loc.1-loc.p."); //Get mean vector.                

                /*
                 * Check if there is enough nonmissing observation to calculate control limit under 
                 * Tsquare command (m>p+1, where m=#observation, include missing obs, p=#items)
                 * If no, you still need to draw plot points on graph..
                 * The trick is we given parameters (Mean & Covariance) and given a fake sample size, then
                 * you get t-sq value, becasue we don't need the CL & UCL from Minitab. 
                 * (WE USE REGULAR COVARIANCE INSTEAD OF COVARIANCE BY Sullivan & Woodall)
                 * 
                 */
                cmnd.AppendLine("if(ssiz < p+1)");
                cmnd.AppendLine("let kk = p+1");
                cmnd.AppendLine("else");
                cmnd.AppendLine("copy ssiz kk\r\n");
                cmnd.AppendLine("endif");

                cmnd.AppendLine("tsquare x.1-x.p 1;");
                cmnd.AppendLine("mu loc.1-loc.p;");
                cmnd.AppendLine("sigma cov;");
                cmnd.AppendLine("number kk;");
                cmnd.AppendLine("sampsize tmp.1;"); //Get subgroup size.
                cmnd.AppendLine("ppoint t2."); //Get Tsquare value.



                cmnd.AppendLine("copy loc.1-loc.p location"); //Copy column to mean vector 

                cmnd.AppendLine("let m = sum(tmp.1)"); //Get the actual sample size (include missing observations)

                cmnd.AppendLine("if (m <= p+1)"); // Check if there is enough obs to calc contril limit...
                cmnd.AppendLine("let cl[m]=miss()"); //no center line
                cmnd.AppendLine("let ucl[m]=miss()"); //no control limit
                cmnd.AppendLine("set ooc"); //no ooc
                cmnd.AppendLine("(0)m");
                cmnd.AppendLine("end");

                cmnd.AppendLine("else");

                cmnd.AppendLine("let conf = 1-alpha");
                cmnd.AppendLine("let a1 = p/2");
                cmnd.AppendLine("let a2 = (m-p-1)/2");

                // Calculate UCL   
                cmnd.AppendLine("set tmp.1");
                cmnd.AppendLine("(conf)m");
                cmnd.AppendLine("end");
                cmnd.AppendLine("invcdf tmp.1 ucl;");
                cmnd.AppendLine(" beta a1 a2.");
                cmnd.AppendLine("let ucl = (m-1)**2/m*ucl"); //Get upper control limit
                // Calculate CL
                cmnd.AppendLine("set tmp.1");
                cmnd.AppendLine("(0.5)m");
                cmnd.AppendLine("end");
                cmnd.AppendLine("invcdf tmp.1 cl;");
                cmnd.AppendLine(" beta a1 a2.");
                cmnd.AppendLine("let cl = (m-1)**2/m*cl"); //Get center line
                cmnd.AppendLine("let ooc = if(t2>ucl and t2<>MISS(),1,0)"); //Get OOC info
                cmnd.AppendLine("endif");

                //刪除所有圖形
                cmnd.AppendLine("gmana;");
                cmnd.AppendLine("all;");
                cmnd.AppendLine("close;");
                cmnd.AppendLine("nopr.");


                cmnd.AppendLine("endmacro");

                string macPath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mytsquare.mac", cmnd.ToString());

                string[] matIds = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, 2, Mtblib.Tools.MtbVarType.Matrix);//紀錄Mean vecot & Covariance matrix                
                string[] constIds = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, 1, Mtblib.Tools.MtbVarType.Constant); // Sample size of Covariance matrix

                cmnd.Clear();
                cmnd.AppendLine("notitle");
                cmnd.AppendLine("brief 0");
                cmnd.AppendFormat("%\"{0}\" {1};\r\n", macPath, string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));
                cmnd.AppendFormat("variance {0} {1};\r\n", matIds[0], constIds[0]);
                cmnd.AppendFormat("meanvect {0};\r\n", matIds[1]);
                cmnd.AppendFormat("ppoint {0};\r\n", pplotCol.SynthesizedName);
                cmnd.AppendFormat("climit {0} {1};\r\n", clCol.SynthesizedName, uclCol.SynthesizedName);
                cmnd.AppendFormat("test {0}.\r\n", oocCol.SynthesizedName);
                

                string path = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
                proj.ExecuteCommand(string.Format("exec \"{0}\" 1", path), ws);

                #endregion

                //取得參數組
                TsquareParameters tmpPara = new TsquareParameters();
                Mtb.Matrix mat;
                mat = ws.Matrices.Item(matIds[1]);
                tmpPara.Mean = LinearAlgebra.Matrix<double>.Build.DenseOfColumnMajor(1, mat.ColumnCount, mat.GetData());
                mat = ws.Matrices.Item(matIds[0]);
                tmpPara.Covariance = LinearAlgebra.Matrix<double>.Build.DenseOfColumnMajor(mat.RowCount, mat.ColumnCount, mat.GetData());
                tmpPara.SampleSize = ws.Constants.Item(constIds[0]).GetData();
                tmpPara.SubgroupSize = 1;
                tmpPara.ParaID = null;
                tmpParaList.Add(tmpPara);
            }
            else
            {
                //有指定參數的時候
                #region Phase II
                int nParas = ws.Columns.Item("CHART_PARA_INDEX").GetNumDistinctRows(); //宣告多組參數組
                cmnd.AppendLine("macro");
                cmnd.AppendLine("myt2calculator x.1-x.p;");
                cmnd.AppendFormat("variance cov.1-cov.{0} ssiz.1-ssiz.{0};\r\n", nParas);
                cmnd.AppendFormat("meanvect mvect.1-mvect.{0};\r\n", nParas);
                cmnd.AppendLine("ppoint t2;");
                cmnd.AppendLine("climit cl ucl;");
                cmnd.AppendLine("test ooc;");
                cmnd.AppendLine("siglevel alpha.");

                cmnd.AppendLine("mcolumn x.1-x.p xx.1-xx.p xxx.1-xxx.p loc.1-loc.p");
                cmnd.AppendLine("mcolumn tmp t2 ucl cl ooc tmpT2 tmpUCL tmpCL tmpOOC");
                cmnd.AppendFormat("mmatrix mvect.1-mvect.{0} cov.1-cov.{0}\r\n", nParas);
                cmnd.AppendFormat("mconstant m histm alpha conf a1 a2 ssiz.1-ssiz.{0} kk\r\n", nParas);
                cmnd.AppendLine("default alpha = 0.0027");

                var _chartparaindex = _rawdata.AsEnumerable()
                    .Select(x => x.Field<string>("CHART_PARA_INDEX")).ToArray();
                var _distinctIds = _chartparaindex.Distinct().ToArray();

                for (int i = 0; i < _distinctIds.Length; i++) //對每個參數與對應的資料計算 Tsquare 和管制界限
                {
                    string id = _distinctIds[i];
                    cmnd.AppendLine("copy x.1-x.p xx.1-xx.p;");
                    cmnd.AppendLine("include;");
                    cmnd.AppendFormat("rows {0}.\r\n",
                        string.Join(" &\r\n",
                        _chartparaindex.Select((x, rowid) => new { Value = x, RowId = rowid + 1 })
                        .Where(x => x.Value == id).Select(x => x.RowId).ToArray()));

                    #region Set parameter information into Minitab macro
                    TsquareParameters para = new TsquareParameters();
                    para.Mean = null; para.Covariance = null; para.SampleSize = MISSINGVALUE; para.SubgroupSize = 1;
                    if (id != null)
                    {
                        para = _parameters.Where(x => x.ParaID == id).First();
                    }

                    if (para.Mean != null) //把已知的 Mean vector 寫入 Minitab
                    {
                        cmnd.AppendLine("read loc.1-loc.p");
                        cmnd.AppendFormat("{0}\r\n", string.Join(" &\r\n", para.Mean.Enumerate()));
                        cmnd.AppendLine("end");
                    }
                    else
                    {
                        cmnd.AppendLine("stat xx.1-xx.p;");
                        cmnd.AppendLine("mean loc.1-loc.p.");
                    }


                    if (para.Covariance != null) //把已知的 Covariance 寫入 Minitab
                    {
                        cmnd.AppendFormat("read {1} {2} cov.{0} \r\n", i + 1, para.Covariance.RowCount, para.Covariance.ColumnCount);
                        List<LinearAlgebra.Vector<double>> valuesByRow = para.Covariance.EnumerateRows().ToList();
                        for (int r = 0; r < para.Covariance.RowCount; r++)
                        {
                            cmnd.AppendFormat("{0}\r\n", string.Join(" &\r\n", valuesByRow[r]));
                        }
                        cmnd.AppendLine("end");
                        cmnd.AppendFormat("let ssiz.{0}={1}\r\n", i + 1, para.SampleSize);
                    }
                    else
                    {
                        cmnd.AppendLine("rnmiss xx.1-xx.p tmp");
                        cmnd.AppendLine("copy xx.1-xx.p xxx.1-xxx.p;");
                        cmnd.AppendLine("exclud;");
                        cmnd.AppendFormat("where \"tmp>0\".\r\n");
                        cmnd.AppendFormat("cova xxx.1-xxx.p cov.{0}\r\n", i + 1);
                        cmnd.AppendFormat("let ssiz.{0} = count(xxx.1)\r\n", i + 1);
                    }
                    #endregion

                    //Tsquare command                    
                    cmnd.AppendFormat("if(ssiz.{0} < p+1)\r\n", i + 1);
                    cmnd.AppendLine("let kk = p+1");
                    cmnd.AppendLine("else");
                    cmnd.AppendFormat("copy ssiz.{0} kk\r\n", i + 1);
                    cmnd.AppendLine("endif");
                    if (id == null && _chartparaindex.Where(x => x == id).Count() == 1) //如果只有一個觀測值且沒有參數設定
                    {
                        cmnd.AppendLine("let tmp=1");
                        cmnd.AppendLine("let tmpT2=miss()");                        
                    }
                    else
                    {
                        cmnd.AppendFormat("tsquare xx.1-xx.p {0};\r\n", para.SubgroupSize);
                        cmnd.AppendLine(" mu loc.1-loc.p;");
                        cmnd.AppendFormat(" sigma cov.{0};\r\n", i + 1);
                        cmnd.AppendLine(" number kk;");
                        cmnd.AppendLine("sampsize tmp;"); //Get subgroup size.
                        cmnd.AppendLine("ppoint tmpT2."); //Get Tsquare value.
                    }


                    cmnd.AppendFormat("copy loc.1-loc.p mvect.{0}\r\n", i + 1); //Copy column to mean vector 


                    /*
                     * Get the value of m which use to calculate UCL, CL.
                     * In phase II, m is the sample size of the data used to calculate historical 
                     * covariance matrix.
                     * In phase I, m is the number of observations.
                     * 
                     */ 
                    cmnd.AppendLine("let m = sum(tmp)"); //Get the actual sample size (include missing data)
                    
                    if (para.Covariance != null)
                    {
                        cmnd.AppendFormat("let histm = ssiz.{0}\r\n", i + 1);
                    }
                    else
                    {
                        cmnd.AppendLine("copy m histm");
                    }

                    
                    // Calculate control limits
                    if (id == null && _chartparaindex.Where(x => x == id).Count() == 1)
                    {
                        cmnd.AppendLine("let tmpUCL=miss()");
                        cmnd.AppendLine("let tmpCL=miss()");
                    }
                    else
                    {
                        cmnd.AppendLine("let conf = 1-alpha");
                        cmnd.AppendLine("let a1 = p");
                        cmnd.AppendLine("let a2 = histm");
                        #region Get UCL
                        cmnd.AppendLine("set tmp");
                        cmnd.AppendLine("(conf)m");
                        cmnd.AppendLine("end");

                        if (id == null) //phase I case
                        {
                            cmnd.AppendLine("invcdf tmp tmpUCL;");
                            cmnd.AppendLine(" beta a1 a2.");
                            cmnd.AppendLine("let tmpUCL = (m-1)**2/m*tmpUCL"); //Get upper control limit
                        }
                        else //phase II case
                        {
                            cmnd.AppendLine("invcdf tmp tmpUCL;");
                            cmnd.AppendLine(" f a1 a2.");
                            cmnd.AppendLine("let tmpUCL = p*(histm+1)*(histm-1)/histm/(histm-p)*tmpUCL"); //Get upper control limit
                        }
                        #endregion
                        #region Get CL
                        cmnd.AppendLine("set tmp");
                        cmnd.AppendLine("(0.5)m");
                        cmnd.AppendLine("end");
                        if (id == null)
                        {
                            cmnd.AppendLine("invcdf tmp tmpCL;");
                            cmnd.AppendLine(" beta a1 a2.");
                            cmnd.AppendLine("let tmpCL = (m-1)**2/m*tmpCL"); //Get upper control limit
                        }
                        else
                        {
                            cmnd.AppendLine("invcdf tmp tmpCL;");
                            cmnd.AppendLine(" f a1 a2.");
                            cmnd.AppendLine("let tmpCL =  p*(histm+1)*(histm-1)/histm/(histm-p)*tmpCL"); //Get center line
                        }
                        #endregion
                    }

                    cmnd.AppendLine("let tmpOOC = if(tmpT2>tmpUCL AND tmpT2<>MISS(),1,0)"); //Get OOC info

                    if (i == 0)
                    {
                        cmnd.AppendLine("copy tmpT2 tmpUCL tmpCL tmpOOC t2 ucl cl ooc");
                    }
                    else
                    {
                        cmnd.AppendLine("stack (t2 ucl cl ooc) (tmpT2 tmpUCL tmpCL tmpOOC) (t2 ucl cl ooc)");
                    }
                }
                //刪除所有圖形
                cmnd.AppendLine("gmana;");
                cmnd.AppendLine("all;");
                cmnd.AppendLine("close;");
                cmnd.AppendLine("nopr.");

                cmnd.AppendLine("endmacro");

                string macPath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mytsquare.mac", cmnd.ToString());

                string[] matMeanIds = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, nParas, Mtblib.Tools.MtbVarType.Matrix);
                string[] matCovIds = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, nParas, Mtblib.Tools.MtbVarType.Matrix);
                string[] constIds = Mtblib.Tools.MtbTools.CreateVariableStrArray(ws, 1 * nParas, Mtblib.Tools.MtbVarType.Constant);

                cmnd.Clear();
                cmnd.AppendLine("notitle");
                cmnd.AppendLine("brief 0");
                cmnd.AppendFormat("%\"{0}\" {1};\r\n", macPath, string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));
                cmnd.AppendFormat("variance {0} {1};\r\n", string.Join(" &\r\n", matCovIds), string.Join(" &\r\n", constIds));
                cmnd.AppendFormat("meanvect {0};\r\n", string.Join(" &\r\n", matMeanIds));
                cmnd.AppendFormat("ppoint {0};\r\n", pplotCol.SynthesizedName);
                cmnd.AppendFormat("climit {0} {1};\r\n", clCol.SynthesizedName, uclCol.SynthesizedName);
                cmnd.AppendFormat("test {0}.\r\n", oocCol.SynthesizedName);

                string path = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
                proj.ExecuteCommand(string.Format("exec \"{0}\" 1", path), ws);
                #endregion

                //取得參數組
                TsquareParameters tmpPara;
                Mtb.Matrix mat;
                for (int i = 0; i < _distinctIds.Length; i++)
                {
                    tmpPara = new TsquareParameters();
                    tmpPara.ParaID = _distinctIds[i];
                    mat = ws.Matrices.Item(matMeanIds[i]);
                    tmpPara.Mean = LinearAlgebra.Matrix<double>.Build.DenseOfColumnMajor(mat.RowCount, mat.ColumnCount, mat.GetData());
                    mat = ws.Matrices.Item(matCovIds[i]);
                    tmpPara.Covariance = LinearAlgebra.Matrix<double>.Build.DenseOfColumnMajor(mat.RowCount, mat.ColumnCount, mat.GetData());
                    tmpPara.SampleSize = ws.Constants.Item(constIds[i]).GetData();
                    tmpParaList.Add(tmpPara);
                }

            }

            //繪圖
            //會使用 TIMESTAMP, PPOINT, CL, UCL, TEST
            #region 繪製 T2 Control Chart (TSPLOT)
            string gpath = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("tsquare_{0}.jpg", _rawdata.TableName));
            cmnd.Clear();
            cmnd.AppendFormat("fdate {0};\r\n", timeCol.SynthesizedName);
            cmnd.AppendLine("format(dtyyyy-MM-dd hh:mm).");
            cmnd.AppendFormat("tsplot {0} {1} {2};\r\n", pplotCol.SynthesizedName, uclCol.SynthesizedName, clCol.SynthesizedName);
            cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath);
            cmnd.AppendLine("repl;");
            cmnd.AppendLine("jpeg;");
            cmnd.AppendLine("over;");
            cmnd.AppendFormat("symb {0};\r\n", oocCol.SynthesizedName);
            cmnd.AppendLine("type &");
            double[] oocData = oocCol.GetData();
            if (oocData.Any(x => x == 0)) cmnd.AppendLine("6 &");
            if (oocData.Any(x => x == 1)) cmnd.AppendLine("12 &");
            cmnd.AppendLine("0 0 0 0;");
            cmnd.AppendLine("size 1;");
            cmnd.AppendLine("color &");
            if (oocData.Any(x => x == 0)) cmnd.AppendLine("1 &"); //r17 color 64
            if (oocData.Any(x => x == 1)) cmnd.AppendLine("2 &");
            cmnd.AppendLine(";");
            cmnd.AppendLine("conn;");
            cmnd.AppendLine("type 1 1 1;");
            cmnd.AppendLine("color 1 2 120;"); //r17 conn:64 cl: 9, climit:8
            //cmnd.AppendLine("graph;");
            //cmnd.AppendLine("color 22;");
            cmnd.AppendLine("nole;");
            cmnd.AppendFormat("stamp {0};\r\n", timeCol.SynthesizedName);
            cmnd.AppendLine("scale 1;");
            cmnd.AppendFormat("tick 1:{0}/{1};\r\n", pplotCol.RowCount,
                pplotCol.RowCount > 35 ? Math.Ceiling((double)pplotCol.RowCount / 35) : 1);
            cmnd.AppendLine("axla 1;");
            cmnd.AppendLine("adis 0;");
            cmnd.AppendLine("axla 2;");
            cmnd.AppendLine("adis 0;");
            string ttlString = string.Join(",", varnames);
            if (ttlString.Length > 23) ttlString = ttlString.Substring(0, 20) + "...";
            cmnd.AppendLine("graph 8 4;");
            cmnd.AppendFormat("title \"T2管制圖 {0}\";\r\n", ttlString);
            cmnd.AppendFormat("footn \"更新時間: {0}\";\r\n", DateTime.Now);
            cmnd.AppendFormat("ZTag \"{0}\";\r\n", "_T2CHART");
            cmnd.AppendLine(".");

            //刪除所有圖形
            cmnd.AppendLine("gmana;");
            cmnd.AppendLine("all;");
            cmnd.AppendLine("close;");
            cmnd.AppendLine("nopr.");

            string t2MacroPath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("myt2macro.mtb", cmnd.ToString());
            proj.ExecuteCommand(string.Format("exec \"{0}\" 1", t2MacroPath), ws);
            #endregion

            //將檔案轉為二進位陣列
            this.Contents.Add(new RptOutput()
            {
                OType = MtbOType.GRAPH,
                OutputInByteArr = File.ReadAllBytes(gpath)
            });


            //計算 Decomposition
            if (oocData.Any(x => x == 1))
            {
                DataTable tmpDataTable = _rawdata.Copy();
                tmpDataTable.Columns.Add("OOC", typeof(int));
                for (int r = 0; r < tmpDataTable.Rows.Count; r++)
                {
                    DataRow dr = tmpDataTable.Rows[r];
                    dr["OOC"] = oocData[r];
                }

                //建立 Decomposition 的表格
                DataTable decoTable = new DataTable();
                decoTable.Columns.Add("TIMESTAMP", typeof(DateTime));
                foreach (var item in varnames)
                {
                    decoTable.Columns.Add(item, typeof(double));
                }

                //將OOC的項目值取出                
                var subData = tmpDataTable.Select("OOC=1").CopyToDataTable();
                var oocParaSet = subData.AsEnumerable().Select(dr => dr.Field<string>("CHART_PARA_INDEX")).Distinct().ToArray(); //OOC 的參數組有哪些
                foreach (var item in tmpParaList)
                {
                    if (oocParaSet.Contains(item.ParaID)) //該參數組的觀測值是OOC
                    {
                        //把對應的觀測值轉換成 List<double[]>，其中每個 double[] 是每個 row 的數據
                        var subsubData = subData.Select(string.Format("CHART_PARA_INDEX {0}", item.ParaID == null ? "IS NULL" : "='" + item.ParaID + "'"))
                            .CopyToDataTable();
                        var obsArray = subsubData.DefaultView.ToTable(false, varnames.ToArray())
                            .AsEnumerable().Select(x => x.ItemArray.Select(o => Convert.ToDouble(o)).ToArray()).ToList();
                        LinearAlgebra.Matrix<double> obs = LinearAlgebra.Matrix<double>.Build.DenseOfRowArrays(obsArray);
                        LinearAlgebra.Matrix<double> t2deco = Tool.T2Decomposition(obs, item.Mean, item.Covariance);
                        var t2decoByRow = t2deco.EnumerateRows().ToArray();
                        for (int r = 0; r < t2decoByRow.Count(); r++)
                        {
                            DataRow dr = decoTable.NewRow();
                            object[] o = new object[1 + t2deco.ColumnCount];
                            t2decoByRow[r].ToArray().CopyTo(o, 1);
                            o[0] = subsubData.Rows[r].Field<DateTime>("TIMESTAMP");
                            decoTable.Rows.Add(o);
                        }
                    }
                }

                cmnd.Clear();
                //LinearAlgebra.Matrix<double> obs = LinearAlgebra.Matrix<double>.Build.DenseOfRowArrays(subData);
                this.Contents.Add(new RptOutput()
                {
                    OType = MtbOType.TABLE,
                    OutputInByteArr = Tool.ConvertDataSetToByteArray(decoTable)
                });
            }
            else // 沒有 OOC 就不做
            {
                this.Contents.Add(new RptOutput()
                {
                    OType = MtbOType.TABLE,
                    OutputInByteArr = null
                });
            }
            //Console.ReadKey();
        }       

    }

    /// <summary>
    /// 多變量參數組
    /// </summary>
    public struct TsquareParameters
    {
        /// <summary>
        /// 參數組的 ID (CHART_PARA_INDEX)
        /// </summary>
        public string ParaID;
        /// <summary>
        /// 平均數向量 1*p
        /// </summary>
        public LinearAlgebra.Matrix<double> Mean;
        /// <summary>
        /// 共變異數矩陣 p*p
        /// </summary>
        public LinearAlgebra.Matrix<double> Covariance;
        /// <summary>
        /// 估計共變異數的樣本數
        /// </summary>
        public double SampleSize;
        /// <summary>
        /// 該圖形的合理分群
        /// </summary>
        public double SubgroupSize;
    }
}
