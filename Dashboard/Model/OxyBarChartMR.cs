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
using OxyPlot.Axes;
using OxyPlot.Annotations;
using System.ComponentModel;
using System.Windows;//

using System.IO;
using System.Threading;

namespace Dashboard.Model
{
    public class OxyBarChartMR : OxyReport
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

            #region
            Model = new PlotModel();
            Model.Title = Title;

            #region x-axis setting

            //var xAxis = new DateTimeAxis
            //{
            //    StringFormat = "yyyy/MM/dd",
            //    Title = XTitle,
            //    MinorIntervalType = DateTimeIntervalType.Days,
            //    IntervalType = DateTimeIntervalType.Days,
            //    IsZoomEnabled = false,
            //    TitleFontSize = 14,
            //};
            //Model.Axes.Add(xAxis);
            CategoryAxis categoriesAxis = new CategoryAxis()
            {
                Position = AxisPosition.Bottom,
                IsZoomEnabled = false,
            };
            ColumnSeries barChart = new ColumnSeries();
            barChart.FillColor = OxyColor.FromRgb(121, 168, 225); // the rgb of minitab default
            #endregion

            #region y-axis setting
            LinearAxis yAxis = new LinearAxis()
            {
                IsZoomEnabled = false,
                Position = AxisPosition.Left,
            };
            Model.Axes.Add(yAxis);
            #endregion

            #region add each data point to bar chart items
            foreach (DataRow row in _rawdata.Rows)
            {
                int emptyIndex = row[0].ToString().IndexOf(" ");
                categoriesAxis.ActualLabels.Add(row[0].ToString().Substring(0, emptyIndex));
                barChart.Items.Add(new ColumnItem(Convert.ToDouble(row[1])));
            };
            #endregion

            #region mouse click on bar
            barChart.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == OxyMouseButton.Left)
                {
                    //if ((int)Math.Round(e.HitTestResult.Index) != indexOfNearestBarChart) IndexOfNearestBarChart = (int)Math.Round(e.HitTestResult.Index);
                    e.Handled = true;//
                }
                else { }
            };
            #endregion

            #region add warning line at y = 10
            Model.Axes.Add(categoriesAxis);
            Model.Series.Add(barChart);
            var la = new LineAnnotation { Type = LineAnnotationType.Horizontal, Y = Warningline, Color = OxyColors.Red };
            Model.Annotations.Add(la);
            #endregion

            #endregion

            #region fail method, sometimes successful, interesting
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
            #endregion

            //將檔案轉為二進位陣列
            this.OxyContents.Add(new OxyRptOutput()
            {
                OxyOType = OxyOType.GRAPH,
                OutputInByteArr = bytes,
                Tag = "Bar",
            });

            this.OxyContents.Add(new OxyRptOutput()
            {
                OxyOType = OxyOType.TABLE,
                OutputInByteArr = Tool.ConvertDataSetToByteArray(statTable),
            });
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private double warningline = 10;

        public double Warningline
        {
            get
            {
                return warningline;
            }
            set
            {
                warningline = value;
            }
        }

        public DataTable statTable;

        public int N
        {
            get
            {
                return _rawdata.Rows.Count;
            }
        }

        public double stDev
        {
            get
            {
                double stdev;
                if (_rawdata.Rows.Count > 1) stdev = (double)_rawdata.Compute("Stdev(Value)", string.Empty);
                else stdev = 0;
                return Math.Round(stdev, 3);
            }
        }

        public double Max
        {
            get
            {
                if (_rawdata.Rows.Count == 0) return 0;
                else
                {
                    double maxAccountLevel = double.MinValue;
                    foreach (DataRow dr in _rawdata.Rows)
                    {
                        double accountLevel = dr.Field<double>("Value");
                        maxAccountLevel = Math.Max(maxAccountLevel, accountLevel);
                    }
                    return Math.Round(maxAccountLevel, 2);
                }
            }
        }

        public double Min
        {
            get
            {
                if (_rawdata.Rows.Count == 0) return 0;
                else
                {
                    double minAccountLevel = double.MaxValue;
                    foreach (DataRow dr in _rawdata.Rows)
                    {
                        double accountLevel = dr.Field<double>("Value");
                        minAccountLevel = Math.Min(minAccountLevel, accountLevel);
                    }
                    return Math.Round(minAccountLevel, 2);
                }
            }
        }

        public double Mean
        {
            get
            {
                if (_rawdata.Rows.Count == 0) return 0;
                else
                {
                    double avg = _rawdata.AsEnumerable().Average(r =>
                    r.Field<double>("Value"));
                    return Math.Round(avg, 2);
                }

            }
        }

    }
}
