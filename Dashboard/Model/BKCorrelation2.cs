using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinearAlgebra = MathNet.Numerics.LinearAlgebra;

namespace Dashboard.Model
{
    public class BKCorrelation2 : BkCorrelation
    {
        public override void Execute(Mtb.Project proj)
        {
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
            List<Mtb.Column> bkCols = new List<Mtb.Column>(); //斷絲率的集合
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
                    case "SITE_ID":
                        break;

                    default:                        
                        if (col.ColumnName.Substring(0, 2) == "BK")
                        {
                            bkCols.Add(ws.Columns.Item(col.ColumnName));                            
                        }
                        else
                        {
                            varCols.Add(ws.Columns.Item(col.ColumnName));
                        }
                        break;
                }
            }

            foreach (var col in bkCols)
            {
                if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
                {
                    throw new ArgumentNullException(string.Format("[{0}]查無資料", col.Name));
                }
            }
            foreach (var col in varCols)
            {
                if ((int)col.DataType == 3 || col.MissingCount == col.RowCount)
                {
                    throw new ArgumentNullException(string.Format("[{0}]查無資料", col.Name));
                }
            }


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
            cmnd.AppendFormat("tsplot {0} {1};\r\n", string.Join(" &\r\n", bkCols.Select(x => x.SynthesizedName)),
                string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));

            double gHeight = 3;
            //if (varCols.Count > 3) gHeight = Math.Min(30, 4 + ((double)varCols.Count - 3) * 0.8);
            cmnd.AppendFormat("graph 8 {0};\r\n", gHeight);
            string gpath_trend = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("trend_{0}.jpg", _rawdata.TableName));
            cmnd.AppendFormat("gsave \"{0}\";\r\n", gpath_trend);
            cmnd.AppendLine("repl;");
            cmnd.AppendLine("jpeg;");
            //cmnd.AppendLine("panel;");
            //cmnd.AppendFormat("rc {0} 1;\r\n", varCols.Count + bkCols.Count);
            //cmnd.AppendLine("noal;");
            //cmnd.AppendLine("label;");
            //cmnd.AppendLine(" psize 5;");
            cmnd.AppendLine("scale 1;");
            cmnd.AppendLine(" psize 8;");
            cmnd.AppendLine("grid 1;");
            cmnd.AppendLine("axlab 1;");
            cmnd.AppendLine("lshow;");
            cmnd.AppendLine(" angle 90;");
            cmnd.AppendLine("grid 2;");
            cmnd.AppendLine("scale 2;");
            cmnd.AppendLine(" psize 8;");
            if (timeCol != null && (int)timeCol.DataType != 3) cmnd.AppendFormat("stamp {0};\r\n", timeCol.SynthesizedName);
            cmnd.AppendLine("symb;");
            cmnd.AppendLine("conn;");
            //cmnd.AppendFormat("foot \"建立時間: {0}\";\r\n", DateTime.Now);
            cmnd.AppendLine("nodt.");

            //Create Matrix Plot
            cmnd.AppendFormat("matrixplot ({0})*({1});\r\n", string.Join(" &\r\n", bkCols.Select(x => x.SynthesizedName)),
               string.Join(" &\r\n", varCols.Select(x => x.SynthesizedName)));

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
            cmnd.AppendFormat("corr {0} {1} {2}\r\n", string.Join(" &\r\n", bkCols.Select(x => x.SynthesizedName)),
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


            //將檔案轉為二進位陣列
            string _root = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab", string.Format("trend_{0}", _rawdata.TableName));
            string _file;
            for (int i = 0; i < bkCols.Count + varCols.Count; i++)
            {
                _file = _root + (i + 1).ToString("D3") + ".jpg";
                if (File.Exists(_file))
                {
                    this.Contents.Add(new RptOutput()
                    {
                        OType = MtbOType.GRAPH,
                        OutputInByteArr = File.ReadAllBytes(_file),
                        Tag = "Trend"
                    });

                }
            }

            //if (File.Exists(gpath_trend))
            //{
            //    this.Contents.Add(new RptOutput()
            //    {
            //        OType = MtbOType.GRAPH,
            //        OutputInByteArr = File.ReadAllBytes(gpath_trend),
            //        Tag = "Trend"
            //    });
            //}
            if (File.Exists(gpath_corr))
            {
                this.Contents.Add(new RptOutput()
                {
                    OType = MtbOType.GRAPH,
                    OutputInByteArr = File.ReadAllBytes(gpath_corr),
                    Tag = "Scatter"
                });
            }



            //Create Correlation table            
            DataTable corrTable = new DataTable();
            corrTable.Columns.Add("熔爐項目", typeof(string));
            foreach (Mtb.Column col in bkCols)
            {
                corrTable.Columns.Add(col.Name, typeof(double));
            }


            //corrTable.Columns.Add("CorrCoef", typeof(double));

            var corrMat = ws.Matrices.Item(matIds[0]).GetData(); //取出 correlation matrix

            var mBuilder = LinearAlgebra.Matrix<double>.Build;
            LinearAlgebra.Matrix<double> corM = mBuilder.DenseOfColumnMajor(bkCols.Count + varCols.Count, bkCols.Count + varCols.Count, corrMat);
            LinearAlgebra.Matrix<double> subCorrMat = corM.SubMatrix(bkCols.Count, varCols.Count, 0, bkCols.Count);

            string[] labNames = varCols.Select(x => x.Name).ToArray();
            List<object> itemArray;
            DataRow dr;
            for (int i = 0; i < varCols.Count; i++)
            {
                itemArray = new List<object>();
                itemArray.Add(labNames[i]);
                itemArray.AddRange(subCorrMat.Row(i).ToArray().Select(x => (object)Math.Round(x, 3)).ToArray());
                //dr = corrTable.NewRow();
                //dr.ItemArray = itemArray;
                corrTable.Rows.Add(itemArray.ToArray());
            }


            //double[] subCorr = new double[varCols.Count];
            //double[] corr = corrMat as double[];
            //if (corr != null)
            //{
            //    Array.Copy(corr, 1, subCorr, 0, varCols.Count);
            //    
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
                OutputInByteArr = Tool.ConvertDataSetToByteArray(corrTable),
                Tag = "Table"
            });
        }
        /// <summary>
        /// 用於從外部加入報表內容，特別為 BKCorrelation2 加入的方法
        /// </summary>
        /// <param name="rpt"></param>
        public void AddContent(IRptOutput rpt)
        {
            if (this.Contents == null)
            {
                this._rptLst = new List<IRptOutput>();
            }
            this.Contents.Add(rpt);
        }
    }

}
