using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algebra = MathNet.Numerics.LinearAlgebra;
using Stat = MathNet.Numerics.Statistics;

namespace Dashboard.Model
{
    public class TSquareLimCalculation
    {
        public TSquareLimCalculation()
        {

        }

        /// <summary>
        /// 計算穩定狀態下的參數組(單一觀測值)，使用一般的 Variance-Covariance Matrix 去計算，而非 Sullivan-Woodall 
        /// </summary>
        /// <param name="data">欲使用的資料表</param>
        /// <returns></returns>
        public TsquareParameters Execute(Algebra.Matrix<double> data, Mtb.Project proj)
        {
            //確認基本資訊能否計算
            int p = data.ColumnCount;
            int m = data.RowCount;
            if (m - p <= 0)
            {
                throw new Exception(string.Format("樣本數不可低於{0}，請重新確認。", p));
            }

            //組合管制界線指令並計算 Phase1 的 limit
            double ucl;
            StringBuilder cmnd = new StringBuilder();
            cmnd.AppendLine("invCdf 0.9973 k1;");
            cmnd.AppendFormat("Beta {0} {1}.\r\n", (double)p / 2, (double)(m - p - 1) / 2);
            cmnd.AppendFormat("let k1=(({0}-1)**2)/{0}*k1\r\n", m);
            try
            {
                Mtb.Worksheet ws = proj.Worksheets.Add(1);
                string fpath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
                proj.ExecuteCommand(string.Format("Exec \"{0}\" 1", fpath), ws);
                ucl = ws.Constants.Item("k1").GetData();
                ws.Delete();
                proj.Commands.Delete();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }


            double miss = Mtblib.Tools.MtbTools.MISSINGVALUE;
            var M = Algebra.Matrix<double>.Build;

            //計算 Mean vector & Covariance matrix
            IEnumerable<Algebra.Vector<double>> cols = data.EnumerateColumns();
            var mean = M.DenseOfColumnMajor(1, data.ColumnCount, cols.Select(x => x.Where(o => o < miss).Average()));
            var subData = M.DenseOfRows((data.EnumerateRows().Where(x => x.All(y => y < miss)).ToArray()));
            var diff = subData - M.Dense(subData.RowCount, 1, (i, j) => 1).Multiply(mean);
            var cov = diff.Transpose().Multiply(diff) / (subData.RowCount - 1);
            var invS = cov.Inverse();

            TsquareParameters tsquarePara = new TsquareParameters()
            {
                Mean = mean,
                Covariance = cov,
                SampleSize = m,
                SubgroupSize = 1
            };

            //計算出資料的Tsquare value
            List<double> t2 = new List<double>();
            for (int i = 0; i < data.RowCount; i++)
            {
                if (data.Row(i).ToArray().All(x => x < miss))
                {
                    t2.Add(Tool.CalculateTSquare(data.Row(i).ToRowMatrix(), mean, invS));
                }
                else
                {
                    t2.Add(miss);
                }

            }

            if (t2.Any(x => x >= ucl && x<miss))
            {
                int[] oocRow = t2.Select((x, i) => new { Tsquare = x, Index = i }).Where(x => x.Tsquare >= ucl && x.Tsquare < miss).Select(x => x.Index).OrderByDescending(x => x).ToArray();
                subData = data.Clone();
                //逐列移除OOC的資料列
                for (int i = 0; i < oocRow.Length; i++)
                {
                    subData = subData.RemoveRow(oocRow[i]);
                }
                tsquarePara = Execute(subData, proj);
            }

            return tsquarePara;
        }


    }
}
