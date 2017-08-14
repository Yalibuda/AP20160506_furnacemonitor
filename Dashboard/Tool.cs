using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using LinearAlgebra = MathNet.Numerics.LinearAlgebra;
using Word = Microsoft.Office.Interop.Word;

namespace Dashboard
{
    /// <summary>
    /// 工具箱物件
    /// </summary>
    public static class Tool
    {
        /// <summary>
        /// 將 Byte 陣列轉換為 Image
        /// </summary>
        /// <param name="binary">Byte 陣列</param>
        /// <returns></returns>
        public static Image BinaryToImage(byte[] binary)
        {
            if (binary == null || binary.Length == 0) { return null; }
            byte[] data = null;
            Image oImage = null;
            Bitmap oBitmap = null;
            //建立副本
            data = (byte[])binary.Clone();
            try
            {
                MemoryStream oMemoryStream = new MemoryStream(binary);
                //設定資料流位置
                oMemoryStream.Position = 0;
                oImage = System.Drawing.Image.FromStream(oMemoryStream);
                //建立副本
                oBitmap = new Bitmap(oImage);
            }
            catch
            {
                throw;
            }
            //return oImage;
            return oBitmap;
        }

        /// <summary>
        /// 將 Image 轉換為 Byte 陣列
        /// </summary>
        /// <param name="img"></param>
        /// <param name="imgFormat">指定影像格式</param>
        /// <returns></returns>
        public static byte[] ImageToBinary(Image img, System.Drawing.Imaging.ImageFormat imgFormat)
        {
            if (img == null) { return null; }
            byte[] data = null;
            using (MemoryStream oMemoryStream = new MemoryStream())
            {
                //建立副本
                using (Bitmap oBitmap = new Bitmap(img))
                {
                    //儲存圖片到 MemoryStream 物件，並且指定儲存影像之格式
                    oBitmap.Save(oMemoryStream, imgFormat);
                    //設定資料流位置
                    oMemoryStream.Position = 0;
                    //設定 buffer 長度
                    data = new byte[oMemoryStream.Length];
                    //將資料寫入 buffer
                    oMemoryStream.Read(data, 0, Convert.ToInt32(oMemoryStream.Length));
                    //將所有緩衝區的資料寫入資料流
                    oMemoryStream.Flush();
                }
            }
            return data;
        }

        /// <summary>
        /// 將二進位陣列轉換成 WPF 的 Image (System.Windows.Media.Imaging.BitmapImage)
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static System.Windows.Media.Imaging.BitmapImage BinaryToWPFImage(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0) return null;

            var img = new BitmapImage();
            byte[] imgData = buffer;
            using (var mem = new System.IO.MemoryStream(imgData))
            {
                mem.Position = 0;
                img.BeginInit();
                img.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = null;
                img.StreamSource = mem;
                img.EndInit();
            }
            img.Freeze();
            return img;
        }

        /// <summary>
        /// 將 DataTable 轉換成二進位陣列
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static byte[] ConvertDataSetToByteArray(DataTable dt)
        {
            byte[] binaryDataResult = null;
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter brFormatter = new BinaryFormatter();
                dt.RemotingFormat = SerializationFormat.Binary;
                brFormatter.Serialize(memStream, dt);
                binaryDataResult = memStream.ToArray();
            }
            return binaryDataResult;
        }

        /// <summary>
        /// 將二進位陣列轉換成 DataTable
        /// </summary>
        /// <param name="byteArrayData"></param>
        /// <returns></returns>
        public static DataTable BinaryToDataTable(byte[] byteArrayData)
        {
            if (byteArrayData == null || byteArrayData.Length == 0) { return null; }
            //DataSet tempDataSet = new DataSet();
            DataTable dt = null;
            // Deserializing into datatable    
            using (MemoryStream stream = new MemoryStream(byteArrayData))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                dt = (DataTable)bformatter.Deserialize(stream);
            }
            return dt;
        }

        /// <summary>
        /// 計算多變量管制圖的 Tsquare decomposition
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mean"></param>
        /// <param name="S"></param>
        /// <returns></returns>
        public static LinearAlgebra.Matrix<double> T2Decomposition(LinearAlgebra.Matrix<double> data, LinearAlgebra.Matrix<double> mean, LinearAlgebra.Matrix<double> S)
        {
            //mean 的 #column = #columns of data = #columns of covariance
            if (data.ColumnCount != mean.ColumnCount || data.ColumnCount != S.ColumnCount || mean.ColumnCount != S.ColumnCount)
            {
                throw new ArgumentException("矩陣的維度無法計算");
            }

            LinearAlgebra.Matrix<double> invS = S.Inverse();

            List<double> tsquare = new List<double>();
            for (int i = 0; i < data.RowCount; i++)
            {
                tsquare.Add(CalculateTSquare(data.Row(i).ToRowMatrix(), mean, invS));
            }
            List<double> decoValue = new List<double>();
            for (int c = 0; c < data.ColumnCount; c++)
            {
                LinearAlgebra.Matrix<double> subData = data.RemoveColumn(c);
                LinearAlgebra.Matrix<double> subMean = mean.RemoveColumn(c);
                LinearAlgebra.Matrix<double> subS = S.RemoveColumn(c).RemoveRow(c);
                LinearAlgebra.Matrix<double> invSubS = subS.Inverse();
                for (int r = 0; r < data.RowCount; r++)
                {
                    decoValue.Add(tsquare[r] - CalculateTSquare(subData.Row(r).ToRowMatrix(), subMean, invSubS));
                }
            }
            LinearAlgebra.Matrix<double> m = LinearAlgebra.Matrix<double>.Build.DenseOfColumnMajor(data.RowCount, data.ColumnCount, decoValue);
            return m;
        }

        /// <summary>
        /// 計算Tsquare
        /// </summary>
        /// <param name="item">a observation</param>
        /// <param name="mean">mean vector</param>
        /// <param name="invS">inverse of covariance matrix</param>
        /// <returns></returns>
        public static double CalculateTSquare(LinearAlgebra.Matrix<double> item, LinearAlgebra.Matrix<double> mean, LinearAlgebra.Matrix<double> invS)
        {
            if (item.ColumnCount != mean.ColumnCount || item.ColumnCount != invS.ColumnCount || mean.ColumnCount != invS.ColumnCount)
            {
                throw new ArgumentException("矩陣的維度無法計算");
            }

            LinearAlgebra.Matrix<double> diff = item - mean;
            var t2 = diff.Multiply(invS).Multiply(diff.Transpose());
            return t2.At(0, 0);
        }


        /// <summary>
        /// 將可簡單列舉的報表匯出成 MS Word 檔
        /// </summary>
        /// <param name="rpt"></param>
        /// <returns></returns>
        public static bool ExportMSWordReport(IEnumerable<Model.IReport> rpt)
        {
            Word.Application wdApp = null;
            try
            {                
                wdApp = new Word.Application();    
                wdApp.Visible = true;
                Word.Document doc = wdApp.Documents.Add();
                Word.Range rng;
                foreach (Model.IReport item in rpt)
                {
                    rng = doc.Paragraphs.Add().Range;
                    rng.InsertAfter(item.Title);
                    rng.Font.Size = 14;
                    rng.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    foreach (Model.IRptOutput output in item.Contents)
                    {
                        rng.InsertParagraphAfter();
                        rng = doc.Paragraphs[doc.Paragraphs.Count].Range;
                        rng.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        rng.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceSingle;
                        rng.Font.Size = 10;
                        if (output.OutputInByteArr == null) { continue; }
                        switch (output.OType)
                        {
                            case Dashboard.Model.MtbOType.GRAPH:
                                Image img = Tool.BinaryToImage(output.OutputInByteArr);
                                System.Windows.Clipboard.SetDataObject(img);
                                rng.Paste();
                                var shape = rng.InlineShapes[rng.InlineShapes.Count];
                                shape.Width = doc.PageSetup.PageWidth * 0.7f;
                                break;
                            case Dashboard.Model.MtbOType.PARAGRAPH:

                                rng.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                                rng.ParagraphFormat.LineSpacingRule = Word.WdLineSpacing.wdLineSpaceExactly;
                                rng.ParagraphFormat.LineSpacing = 12f;
                                rng.InsertAfter(System.Text.Encoding.UTF8.GetString(output.OutputInByteArr));
                                break;
                            case Dashboard.Model.MtbOType.TABLE:
                                DataTable dt = Tool.BinaryToDataTable(output.OutputInByteArr);
                                Word.Table tbl = CreateWordTableWithDataTable(dt, rng);
                                break;
                            case Dashboard.Model.MtbOType.TITLE:
                                break;
                            default:
                                break;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show( ex.HResult.ToString() + "-" +ex.Message,"", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;                
            }
            return true;
        }

        /// <summary>
        /// 於指定路徑中建立新檔名(包含路徑、副檔名)，如果有重複名稱則加入流水號 (1), (2), ...
        /// 
        /// </summary>
        /// <param name="folder">儲存路徑</param>
        /// <param name="fileName">檔案名稱，預設為 Data</param>
        /// <returns></returns>
        public static string CreateNewFileName(string folder, string extension, string fileName = "Data")
        {
            string fname = fileName + "." + extension;
            //判斷 folder 存在與否，如果沒有就新增資料夾
            //這裡會有潛在例外產生
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            //判斷是否有重複檔名
            string[] fileList = Directory.GetFiles(folder).Select(x => Path.GetFileName(x)).ToArray();
            int k = 0;
            while (fileList.Any(x => x == fname))
            {
                k = k + 1;
                fname = string.Format("{0} ({1}).{2}", fileName, k, extension);
            }
            return fname;

        }

        /// <summary>
        /// 將可簡單列舉的報表中的 DataTable 匯出成 CSV 檔
        /// </summary>
        /// <param name="datas">實作 Report 抽象類別的類別</param>
        /// <param name="folderPath">指定儲存CSV檔</param>
        /// <returns></returns>
        public static bool ExportDataTablesToCSVData(IEnumerable<DataTable> datas, string folderPath)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var dt in datas)
            {
                sb.Clear();
                if (dt != null && dt.Rows.Count > 0)
                {
                    IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                                      Select(column => column.ColumnName);
                    sb.AppendLine(string.Join(",", columnNames));
                    foreach (DataRow row in dt.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field => string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                        sb.AppendLine(string.Join(",", fields));
                    }

                    string fName = "";
                    if (dt.TableName != "") fName = dt.TableName + ".CSV";
                    else fName = Tool.CreateNewFileName(folderPath, "CSV");

                    try
                    {
                        File.WriteAllText( Path.Combine(folderPath, fName), sb.ToString(), Encoding.UTF8);
                        
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message + "\r\n 儲存資料失敗。", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                }
            }
            System.Windows.MessageBox.Show("資料儲存完成", "",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information);


            return true;
        }

        /// <summary>
        /// 將 DataTable 插入至指定的 Word Range 位置
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="oRange"></param>
        /// <returns></returns>
        public static Word.Table CreateWordTableWithDataTable(DataTable dt, Word.Range oRange)
        {
            int RowCount = dt.Rows.Count;
            int ColumnCount = dt.Columns.Count;
            Object[,] DataArray = new object[RowCount, ColumnCount];
            //int RowCount = 0; int ColumnCount = 0;
            int r = 0;
            for (int c = 0; c <= ColumnCount - 1; c++)
            {
                DataColumn dc = dt.Columns[c];
                object[] colValues;
                #region 對不同的資料型態轉換格式化
                if (dc.DataType == typeof(double))
                {
                    var o = dt.AsEnumerable().Select(x => Math.Round(x.Field<double>(dc.ColumnName), 4)).ToArray();
                    colValues = new object[o.Length];
                    o.CopyTo(colValues, 0);
                }
                else if (dc.DataType == typeof(DateTime))
                {
                    var o = dt.AsEnumerable().Select(x => string.Format("{0:yyyy-MM-dd hh:mm:ss}", x.Field<DateTime>(dc.ColumnName))).ToArray();
                    colValues = new object[o.Length];
                    o.CopyTo(colValues, 0);
                }
                else
                {
                    var o = dt.AsEnumerable().Select(x => x[dc.ColumnName]).ToArray();
                    colValues = new object[o.Length];
                    o.CopyTo(colValues, 0);
                }
                #endregion
                for (r = 0; r <= RowCount - 1; r++)
                {
                    DataArray[r, c] = colValues[r];
                } //end row loop
            } //end column loop

            StringBuilder oTemp = new StringBuilder();
            for (r = 0; r <= RowCount - 1; r++)
            {
                for (int c = 0; c <= ColumnCount - 1; c++)
                {
                    oTemp.Append(DataArray[r, c] + "\t");
                }
            }

            oRange.Text = oTemp.ToString();
            object Separator = Word.WdTableFieldSeparator.wdSeparateByTabs;
            object Format = Word.WdTableFormat.wdTableFormatWeb1;
            object ApplyBorders = true;
            object AutoFit = true;
            object AutoFitBehavior = Word.WdAutoFitBehavior.wdAutoFitContent;
            Word.Table tbl = oRange.ConvertToTable(Separator: Separator,
                NumRows: RowCount,
                NumColumns: ColumnCount,
                ApplyBorders: ApplyBorders, AutoFit: AutoFit, AutoFitBehavior: AutoFitBehavior);
            tbl.Rows.AllowBreakAcrossPages = 0;
            tbl.Rows.Alignment = Word.WdRowAlignment.wdAlignRowCenter;
            tbl.Rows.Add(tbl.Rows[1]);
            //gotta do the header row manually
            for (int c = 0; c <= ColumnCount - 1; c++)
            {
                tbl.Cell(1, c + 1).Range.Text = dt.Columns[c].ColumnName;
            }
            tbl.set_Style(Word.WdBuiltinStyle.wdStyleTableLightGrid);
            return tbl;
        }

        /// <summary>
        /// 將文字型別的 Limit 資料轉換成數值
        /// </summary>
        /// <param name="lclStr">管制下限</param>
        /// <param name="uclStr">管制上限</param>
        /// <param name="lslStr">規格下限</param>
        /// <param name="uslStr">規格上限</param>
        /// <returns></returns>
        public static LimitInformation LimitStringConverter(string lclStr, string uclStr, string lslStr, string uslStr)
        {
            double value;
            string itemValue;
            LimitInformation limits = new LimitInformation();
            //檢查 LCL 的值
            value = Mtblib.Tools.MtbTools.MISSINGVALUE;
            itemValue = lclStr;
            if (itemValue != "*" && itemValue != string.Empty && !double.TryParse(itemValue, out value))
                throw new Exception("LCL 必須為數值。");
            limits.LCL = value;

            //檢查 UCL 的值
            value = Mtblib.Tools.MtbTools.MISSINGVALUE;
            itemValue = uclStr;
            if (itemValue != "*" && itemValue != string.Empty && !double.TryParse(itemValue, out value))
                throw new Exception("UCL 必須為數值。");
            limits.UCL = value;

            //檢查 LSL 的值
            value = Mtblib.Tools.MtbTools.MISSINGVALUE;
            itemValue = lslStr;
            if (itemValue != "*" && itemValue != string.Empty && !double.TryParse(itemValue, out value))
                throw new Exception("LSL 必須為數值。");
            limits.LSL = value;

            //檢查 USL 的值
            value = Mtblib.Tools.MtbTools.MISSINGVALUE;
            itemValue = uslStr;
            if (itemValue != "*" && itemValue != string.Empty && !double.TryParse(itemValue, out value))
                throw new Exception("USL 必須為數值。");
            limits.USL = value;

            LimitValidationResult result = LimitValidation(limits.LCL, limits.UCL, limits.LSL, limits.USL);
            if (!result.Status)
            {
                throw new Exception(result.Message);
            }

            return limits;
        }
        /// <summary>
        /// 特徵值的界限結構 (LCL, UCL, LSL, USL)
        /// </summary>
        public struct LimitInformation
        {
            public double LCL;
            public double UCL;
            public double LSL;
            public double USL;
        }

        /// <summary>
        /// 確認規格內容是否合法
        /// </summary>
        /// <param name="lcl"></param>
        /// <param name="ucl"></param>
        /// <param name="lsl"></param>
        /// <param name="usl"></param>
        /// <returns></returns>
        public static LimitValidationResult LimitValidation(double lcl = 1.23456E+30, double ucl = 1.23456E+30, double lsl = 1.23456E+30, double usl = 1.23456E+30)
        {
            if (lcl == Mtblib.Tools.MtbTools.MISSINGVALUE && ucl == Mtblib.Tools.MtbTools.MISSINGVALUE &&
                lsl == Mtblib.Tools.MtbTools.MISSINGVALUE && usl == Mtblib.Tools.MtbTools.MISSINGVALUE)
                return new LimitValidationResult()
                {
                    Status = false,
                    Message = "至少填入一組界限值"
                };

            if ((lcl != Mtblib.Tools.MtbTools.MISSINGVALUE && ucl == Mtblib.Tools.MtbTools.MISSINGVALUE) ||
                (lcl != Mtblib.Tools.MtbTools.MISSINGVALUE && ucl == Mtblib.Tools.MtbTools.MISSINGVALUE))
                return new LimitValidationResult()
                {
                    Status = false,
                    Message = "管制界限不可為單邊"
                };

            if ((lcl != Mtblib.Tools.MtbTools.MISSINGVALUE && ucl != Mtblib.Tools.MtbTools.MISSINGVALUE) &&
                lcl >= ucl)
                return new LimitValidationResult()
                {
                    Status = false,
                    Message = "LCL >= UCL"
                };

            if ((lsl != Mtblib.Tools.MtbTools.MISSINGVALUE && usl != Mtblib.Tools.MtbTools.MISSINGVALUE) &&
                lsl >= usl)
                return new LimitValidationResult()
                {
                    Status = false,
                    Message = "LSL >= USL"
                };

            return new LimitValidationResult()
            {
                Status = true,
                Message = ""
            };
        }
        /// <summary>
        /// 規格內容檢查後的結果
        /// </summary>
        public struct LimitValidationResult
        {
            public bool Status;
            public string Message;

        }

    }
}
