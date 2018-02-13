// --------------------------------------------------------------------------------------------------------------------
// <copyright>
//   The MIT License (MIT)
//
//   Copyright (c) 2012 Oystein Bjorke
//
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using OxyPlot;
using OxyPlot.Series;

using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.IO.Compression;

namespace Dashboard.Model
{
    public class OxyPieChart : OxyReport
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

        public override void Execute(Mtb.Project project)
        {
            // not be used in the class
        }

        public override void Execute()
        {
            if (_rawdata == null || _rawdata.Rows.Count == 0)
            {
                throw new ArgumentNullException("查無對應資料");
            }

            _oxyrptLst = new List<IOxyRptOutput>();

            #region create oxy code
            // This data would be used in bar chart too, so move it to view model.
                // Here we only handle the PlotModel

                //// Initialize in View Model
                //_rptLst = new List<IOxyRptOutput>(); //重新建立一個分析結果列舉
                //DataTable distinctPlant = new DataTable();

                //// search different plant
                //distinctPlant.Columns.Add("Plant", typeof(int));
                //foreach(DataRow row in RawData.Rows)
                //{
                //    if (!(distinctPlant.Rows.Find(row[0])==row[0]))
                //    {
                //        DataRow addrow = new DataRow();
                //        addrow[0] = row[0];
                //        distinctPlant.Rows.Add(addrow);
                //    }
                //};
                //DataTable ComputeResult = new DataTable();
                //ComputeResult.Columns.Add("Plant", typeof(int));
                //ComputeResult.Columns.Add("Max MR", typeof(int));
                //foreach (DataRow row in distinctPlant.Rows)
                //{
                //    DataRow dt = new DataRow();
                //    dt[0] = row;
                //    DataTable tmpdt= new DataTable();
                //    tmpdt.Rows.Add(RawData.Rows.Find(row));

                //};

                //RawData.Columns.Add("Plant", typeof(int));
                //RawData.Columns.Add("Max MR", typeof(int));
            Model = new PlotModel();

            Model.IsLegendVisible = true;

            Model.Title = Title;
            PieSeries pieSeries = new PieSeries();

            //RawData.DefaultView.Sort = "Plant DESC";
            //int countRangeOver10 = 0;
            //int countRangeUnover10 = 0;
            int tmpIndex = 0;
            foreach (DataRow row in _rawdata.Rows)
            {
                tmpIndex++;
                if (tmpIndex == 1)
                {
                    pieSeries.Slices.Add(new PieSlice((string)row[0], Convert.ToDouble(row[1])) { IsExploded = false, Fill = OxyColors.PaleVioletRed });
                }
                else
                {
                    pieSeries.Slices.Add(new PieSlice((string)row[0], Convert.ToDouble(row[1])) { IsExploded = true });
                }
            };
            Model.Series.Add(pieSeries);   


                //查無資料 construct
            #endregion

            #region Thread to solve
            //var stream = new MemoryStream();
            var stream = new MemoryStream();
            var pngExporter = new OxyPlot.Wpf.PngExporter { Width = 600, Height = 400, Background = OxyColors.White };
            var thread = new Thread(()
            =>
            {
                pngExporter.Export(this.Model, stream);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            byte[] bytes = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, bytes.Length); //can't write into bytes, length too long? bytes.Length
            stream.Close();
            //stream.Seek(0, SeekOrigin.Begin);
            #endregion


            // untry method , but it seems not for this problem
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
            //    {
            //        ds.Write(data, 0, data.Length);
            //        ds.Flush();
            //    }
            //    return ms.ToArray();
            //}

            Console.WriteLine("");
            //pngExporter.
            //將檔案轉為二進位陣列
            this.OxyContents.Add(new OxyRptOutput()
            {
                OxyOType = OxyOType.GRAPH,
                OutputInByteArr = bytes,
                Tag = "Pie",
            });

        }

        #region 變數 
        public int AreaCount
        {
            get
            {
                _areaCount = 0;
                for (int i = 0; i < (_rawdata.Rows.Count - 1); i++)
                {
                    //int a=0;
                    //a +=
                    //(1 == 2) ? 0 : 1;
                    if (i==0 &&_rawdata.Rows[i]["Area"].ToString() != null) _areaCount += 1;
                    _areaCount +=
                        (string.Compare(_rawdata.Rows[i]["Area"].ToString(), _rawdata.Rows[i + 1]["Area"].ToString()) == 0) ? 0 : 1;
                    //(string.Compare(_rawdata.Rows[i]["Area"],_rawdata.Rows[i+1]["Area"])) ?? 0:1;
                    //if (!(_rawdata.Rows[i]["Area"] == _rawdata.Rows[i + 1]["Area"])) _areaCount++;
                }
                return _areaCount;
            }
        }
        private int _areaCount;

        //public override string Title 
        //{
        //    get { return title; }
        //    set { title = value; }
        //}
        //private string title;
        #endregion

    }
}
