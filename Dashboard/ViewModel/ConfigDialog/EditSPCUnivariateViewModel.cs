using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dashboard.ViewModel.ConfigDialog
{
    public class EditSPCUnivariateViewModel : NotifyPropertyChanged
    {
        public EditSPCUnivariateViewModel()
        {
            Load();
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }
        private string _title = "單一熔爐項的界限管理";

        public string Description
        {
            get { return _desc; }
            set
            {
                _desc = value;
                RaisePropertyChanged("Description");
            }
        }
        private string _desc = "Limit information...";

        /// <summary>
        /// 取得可用的熔爐項資料
        /// </summary>
        public DataTable FurnItemsSource
        {
            get { return _furnItemsSource; }
            private set
            {
                _furnItemsSource = value;
                RaisePropertyChanged("FurnItemsSource");
            }
        }
        private DataTable _furnItemsSource = null;

        /// <summary>
        /// 取得特定熔爐項目的界限資訊
        /// </summary>
        public DataTable FurnItemLimSrc
        {
            get { return _furnItemLimSrc; }
            private set
            {
                _furnItemLimSrc = value;
                RaisePropertyChanged("FurnItemLimSrc");
            }
        }
        private DataTable _furnItemLimSrc = null;

        /// <summary>
        /// 在單項上傳時，被選擇的熔爐項(TAG_NAME)
        /// </summary>
        public string SelectedItemValue
        {
            get { return _selectedItemValue; }
            set
            {
                _selectedItemValue = value;
                OnSelectedItemChanged();
                RaisePropertyChanged("SelectedItemValue");
            }
        }
        private string _selectedItemValue = "";

        /// <summary>
        /// 在單項上傳時的 LCL~USL 等 TextBox 是否可以使用
        /// </summary>
        public bool IsEditable
        {
            get { return _isEditable; }
            private set
            {
                _isEditable = value;
                RaisePropertyChanged("IsEditable");
            }
        }
        private bool _isEditable = false;

        /// <summary>
        /// 取得或指定廠別
        /// </summary>
        public string Site
        {
            get { return _site; }
            set
            {
                _site = value;
                OnSiteChanged();
                RaisePropertyChanged("Site");
            }

        }
        private string _site = "";

        /// <summary>
        /// 取得可使用動作的清單
        /// </summary>
        public DataTable ActionList
        {
            get
            {
                return _actionList;
            }
            private set
            {
                _actionList = value;
                RaisePropertyChanged("ActionList");
            }
        }
        private DataTable _actionList;

        /// <summary>
        /// 取得已選擇動作的文字描述
        /// </summary>
        public string ActionString
        {
            get { return _actionString; }
            private set
            {
                _actionString = value;
                RaisePropertyChanged("ActionString");
            }
        }
        private string _actionString = "新增界限";
        /// <summary>
        /// 取得已選擇動作的模式(Add, BatchAdd, Eidt)
        /// </summary>
        public string ActionMode
        {
            get { return _actionMode; }
            set
            {
                _actionMode = value;
                RaisePropertyChanged("ActionMode");
            }
        }
        private string _actionMode = "Add";

        /// <summary>
        /// 取得是否為單一新增模式
        /// </summary>
        public bool IsSingleAddMode
        {
            get { return _isSingleAddMode; }
            private set
            {
                _isSingleAddMode = value;
                RaisePropertyChanged("IsSingleAddMode");
            }
        }
        private bool _isSingleAddMode = true;

        /// <summary>
        /// 取得是否為批次新增模式
        /// </summary>
        public bool IsBatchAddMode
        {
            get { return _isBatchAddMode; }
            private set
            {
                _isBatchAddMode = value;
                RaisePropertyChanged("IsBatchAddMode");
            }
        }
        private bool _isBatchAddMode = false;

        /// <summary>
        /// 取得是否為編輯模式
        /// </summary>
        public bool IsEditMode
        {
            get { return _isEditMode; }
            private set
            {
                _isEditMode = value;
                RaisePropertyChanged("IsEditMode");
            }
        }
        private bool _isEditMode = false;

        /// <summary>
        /// 取得選取的檔案路徑
        /// </summary>
        public string SelectedFilePath
        {
            get { return _selectedFilePath; }
            private set
            {
                _selectedFilePath = value;
                RaisePropertyChanged("SelectedFilePath");
            }
        }
        private string _selectedFilePath = "";

        /// <summary>
        /// 取得輸入的LCL值
        /// </summary>
        public string InputedLCL
        {
            get { return _inputedLCL; }
            set
            {
                _inputedLCL = value;
                RaisePropertyChanged("InputedLCL");
            }
        }
        private string _inputedLCL;
        /// <summary>
        /// 取得輸入的UCL值
        /// </summary>
        public string InputedUCL
        {
            get { return _inputedUCL; }
            set
            {
                _inputedUCL = value;
                RaisePropertyChanged("InputedUCL");
            }
        }
        private string _inputedUCL;
        /// <summary>
        /// 取得輸入的LSL值
        /// </summary>
        public string InputedLSL
        {
            get { return _inputedLSL; }
            set
            {
                _inputedLSL = value;
                RaisePropertyChanged("InputedLSL");
            }
        }
        private string _inputedLSL;
        /// <summary>
        /// 取得輸入的USL值
        /// </summary>
        public string InputedUSL
        {
            get { return _inputedUSL; }
            set
            {
                _inputedUSL = value;
                RaisePropertyChanged("InputedUSL");
            }
        }
        private string _inputedUSL;
        /// <summary>
        /// 取得輸入的應用時間
        /// </summary>
        public DateTime InputedApplyDate
        {
            get { return _inputedApplyDate; }
            set
            {
                _inputedApplyDate = value;
                RaisePropertyChanged("InputedApplyDate");
            }
        }
        private DateTime _inputedApplyDate = DateTime.Now;

        private void Load()
        {
            _actionList = new DataTable();
            _actionList.Columns.Add("Name", typeof(string));
            _actionList.Columns.Add("Value", typeof(string));
            DataRow dr;

            dr = _actionList.NewRow();
            dr["Name"] = "單項新增"; dr["Value"] = "Add";
            _actionList.Rows.Add(dr);
            dr = _actionList.NewRow();
            dr["Name"] = "批次新增"; dr["Value"] = "BatchAdd";
            _actionList.Rows.Add(dr);
            dr = _actionList.NewRow();
            dr["Name"] = "編輯界限"; dr["Value"] = "Edit";
            _actionList.Rows.Add(dr);

        }
        /// <summary>
        /// 當選擇的廠別變更的時候
        /// </summary>
        private void OnSiteChanged()
        {
            if (Site == null && Site == string.Empty) FurnItemsSource = null;
            StringBuilder query = new StringBuilder();
            //query.AppendLine("SELECT * FROM VW_FURNACELIMIT");
            //query.AppendFormat("WHERE SITE_ID='{0}' AND APPLY_DATE IS NOT NULL\r\n", Site);
            //query.AppendLine("ORDER BY SITE_ID, FURN_ITEM_INDEX");
            query.AppendLine("SELECT TAG_NAME, ITEM_NAME FROM FURN_ITEM_INFO");
            query.AppendFormat("WHERE SITE_ID='{0}'\r\n", Site);
            query.AppendLine("ORDER BY FURN_ITEM_INDEX");
            FurnItemsSource = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString());
            Title = "單一熔爐項的界限管理-" + Site;
        }
        /// <summary>
        /// 當選擇的熔爐項變更的時候
        /// </summary>
        private void OnSelectedItemChanged()
        {
            if (Site != null && Site != string.Empty &&
                SelectedItemValue != null && SelectedItemValue != string.Empty &&
                FurnItemsSource.AsEnumerable().Select(x => x["TAG_NAME"].ToString()).Contains(SelectedItemValue))
            {
                IsEditable = true;
                //載入該項目最新的一筆(最新的 APPLY DATE + RPT_DATE)
                StringBuilder query = new StringBuilder();
                query.AppendLine("SELECT * FROM vw_furnacelimitrecord");
                query.AppendFormat("WHERE TAG_NAME='{0}' AND SITE_ID='{1}'\r\n", SelectedItemValue, Site);
                query.AppendLine("ORDER BY APPLY_DATE DESC, RPT_DATE DESC");
                DataTable dt = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString());
                if (dt != null && dt.Rows.Count > 0)
                {
                    InputedLCL = dt.Rows[0]["LCL"].ToString();
                    InputedUCL = dt.Rows[0]["UCL"].ToString();
                    InputedLSL = dt.Rows[0]["LSL"].ToString();
                    InputedUSL = dt.Rows[0]["USL"].ToString();
                    if (dt.Rows[0]["APPLY_DATE"] is DateTime)
                    {
                        InputedApplyDate = (DateTime)dt.Rows[0]["APPLY_DATE"];
                    }
                    else
                    {
                        InputedApplyDate = DateTime.Now;
                    }
                    if (IsEditMode)
                    {
                        DataRow[] drs = dt.Select("FURN_ITEM_LIM_INDEX IS NOT NULL");
                        if (drs != null && drs.Length > 0)
                        {
                            FurnItemLimSrc = drs.CopyToDataTable();
                        }
                        else
                        {
                            FurnItemLimSrc = null;
                        }
                    }
                }



            }
            else
            {
                IsEditable = false;
                InputedLCL = "";
                InputedLSL = "";
                InputedUCL = "";
                InputedUSL = "";
                InputedApplyDate = DateTime.Now;
                FurnItemLimSrc = null;
            }
        }

        /// <summary>
        /// 新增界限至資料庫
        /// </summary>
        /// <param name="obj"></param>
        private void AddLimit(object obj)
        {
            double
                lsl = Mtblib.Tools.MtbTools.MISSINGVALUE,
                usl = Mtblib.Tools.MtbTools.MISSINGVALUE,
                lcl = Mtblib.Tools.MtbTools.MISSINGVALUE,
                ucl = Mtblib.Tools.MtbTools.MISSINGVALUE;
            DateTime? applyTime = null;

            #region 單次輸入值的確認與取得
            //確認 LCL 內容正確，並且取得數值
            double value = Mtblib.Tools.MtbTools.MISSINGVALUE;
            string inputedValue;
            inputedValue = InputedLCL;
            if (inputedValue.Trim() != "*" && inputedValue != string.Empty)
            {
                if (!double.TryParse(inputedValue, out value))
                {
                    throw new Exception(string.Format("LCL 欄位的內容必須為數值，若為遺失值則留白。"));
                }
            }
            lcl = value;
            //確認 UCL 內容正確，並且取得數值
            inputedValue = InputedUCL;
            if (inputedValue.Trim() != "*" && inputedValue != string.Empty)
            {
                if (!double.TryParse(inputedValue, out value))
                {
                    throw new Exception(string.Format("UCL 欄位的內容必須為數值，若為遺失值則留白。"));
                }
            }
            ucl = value;
            //確認 LSL 內容正確，並且取得數值
            inputedValue = InputedLSL;
            if (inputedValue.Trim() != "*" && inputedValue != string.Empty)
            {
                if (!double.TryParse(inputedValue, out value))
                {
                    throw new Exception(string.Format("LSL 欄位的內容必須為數值，若為遺失值則留白。"));
                }
            }
            lsl = value;
            //確認 USL 內容正確，並且取得數值
            inputedValue = InputedUSL;
            if (inputedValue.Trim() != "*" && inputedValue != string.Empty)
            {
                if (!double.TryParse(inputedValue, out value))
                {
                    throw new Exception(string.Format("USL 欄位的內容必須為數值，若為遺失值則留白。"));
                }
            }
            usl = value;

            //取得套用界限值的時間
            applyTime = InputedApplyDate;
            #endregion


            // 確認界限之間的關係
            if (lcl == Mtblib.Tools.MtbTools.MISSINGVALUE && ucl == Mtblib.Tools.MtbTools.MISSINGVALUE &&
                lsl == Mtblib.Tools.MtbTools.MISSINGVALUE && usl == Mtblib.Tools.MtbTools.MISSINGVALUE)
            {
                throw new ArgumentNullException("所有界限欄位均無資料，請填入資料後上傳。");
            }
            if (lcl < Mtblib.Tools.MtbTools.MISSINGVALUE && ucl < Mtblib.Tools.MtbTools.MISSINGVALUE)
            {
                if (lcl >= ucl) throw new Exception("LCL 不可大於或等於 UCL。");
            }
            if ((lcl == Mtblib.Tools.MtbTools.MISSINGVALUE && ucl < Mtblib.Tools.MtbTools.MISSINGVALUE) ||
                (lcl < Mtblib.Tools.MtbTools.MISSINGVALUE && ucl == Mtblib.Tools.MtbTools.MISSINGVALUE))
            {
                throw new Exception("管制界限必須要雙邊");
            }
            if (lsl < Mtblib.Tools.MtbTools.MISSINGVALUE && usl < Mtblib.Tools.MtbTools.MISSINGVALUE)
            {
                if (lsl >= usl) throw new Exception("LSL 不可大於或等於 USL。");
            }

            // 確認應用時間
            if (applyTime == null) throw new Exception("應用時間不可為空。");

            // 取得該項目現有的所有界限紀錄
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT * FROM vw_furnacelimitrecord");
            query.AppendFormat("WHERE SITE_ID='{0}' AND TAG_NAME = '{1}'\r\n", Site, SelectedItemValue);
            query.AppendFormat("ORDER BY FURN_ITEM_INDEX, APPLY_DATE DESC, RPT_DATE DESC");

            DataTable dt1 = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString()); //取得DB中的內容           
            DataRow[] drs;

            //判斷是否有相同的界限值，若有，則直接忽略
            drs = dt1.Select(string.Format("APPLY_DATE='{0}' AND LCL={1} AND UCL={2} AND LSL={3} AND USL={4}\r\n",
                applyTime, lcl, ucl, lsl, usl));
            if (drs != null && drs.Length > 0)
            {
                throw new Exception("已有完全相同的界限設定值。");
            }

            //判斷是否有相同時間的設定值，若有，丟出例外。
            drs = dt1.Select(string.Format("APPLY_DATE='{0}'\r\n", applyTime));
            if (drs != null && drs.Length > 0) throw new Exception("已有相同應用時間的界限值，應用時間不可重複。");


            //開始上傳
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("TAG_NAME", typeof(string));
            dt2.Columns.Add("LCL", typeof(double));
            dt2.Columns.Add("UCL", typeof(double));
            dt2.Columns.Add("LSL", typeof(double));
            dt2.Columns.Add("USL", typeof(double));
            dt2.Columns.Add("APPLY_DATE", typeof(DateTime));


            DataRow dr = dt2.NewRow();
            dr["TAG_NAME"] = SelectedItemValue;
            if (lcl < Mtblib.Tools.MtbTools.MISSINGVALUE) dr["LCL"] = lcl;
            if (ucl < Mtblib.Tools.MtbTools.MISSINGVALUE) dr["UCL"] = ucl;
            if (lsl < Mtblib.Tools.MtbTools.MISSINGVALUE) dr["LSL"] = lsl;
            if (usl < Mtblib.Tools.MtbTools.MISSINGVALUE) dr["USL"] = usl;
            dr["APPLY_DATE"] = applyTime;
            dt2.Rows.Add(dr);
            using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand sqlCmnd = new SqlCommand("TRUNCATE TABLE UPLOAD_FURN_ITEM_LIMIT_INFO", conn))
                    {
                        //先清除暫存表上的資料
                        sqlCmnd.ExecuteNonQuery();
                    }

                    using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                    {
                        sqlBC.BatchSize = 1000;
                        sqlBC.BulkCopyTimeout = 6000;

                        //設定要寫入的資料庫
                        sqlBC.DestinationTableName = "UPLOAD_FURN_ITEM_LIMIT_INFO";

                        //對應資料行                            
                        sqlBC.ColumnMappings.Add("TAG_NAME", "TAG_NAME");
                        sqlBC.ColumnMappings.Add("LCL", "LCL");
                        sqlBC.ColumnMappings.Add("UCL", "UCL");
                        sqlBC.ColumnMappings.Add("LSL", "LSL");
                        sqlBC.ColumnMappings.Add("USL", "USL");
                        sqlBC.ColumnMappings.Add("APPLY_DATE", "APPLY_DATE");

                        //開始寫入
                        sqlBC.WriteToServer(dt2);
                    }

                    using (SqlCommand sqlCmnd = new SqlCommand())
                    {
                        query.Clear();
                        query.AppendLine("INSERT INTO FURN_ITEM_LIMIT_INFO(FURN_ITEM_INDEX, LCL, UCL, LSL, USL, APPLY_DATE, RPT_DATE)");
                        query.AppendLine("SELECT * FROM (SELECT A.FURN_ITEM_INDEX, B.LCL, B.UCL, B.LSL, B.USL, B.APPLY_DATE,RPTDATE=GETDATE() FROM FURN_ITEM_INFO A");
                        query.AppendLine("INNER JOIN UPLOAD_FURN_ITEM_LIMIT_INFO B");
                        query.AppendLine("ON A.TAG_NAME = B.TAG_NAME) AS T");
                        sqlCmnd.Connection = conn;
                        sqlCmnd.CommandText = query.ToString();
                        int r = sqlCmnd.ExecuteNonQuery();
                    }


                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("上傳時發生錯誤:\r\n{0}", ex.Message));
                }

            }



        }

        /// <summary>
        /// 批次新增界限至資料庫
        /// </summary>
        /// <param name="obj"></param>
        private int BatchAddLimit(object obj)
        {
            int effectedRows = 0;
            DataTable dtSrc = Database.DBQueryTool.ReadCSVFile(SelectedFilePath);
            if (dtSrc == null || dtSrc.Rows.Count == 0) throw new Exception("檔案無內容");

            //因為用OLE取得的 DataTable 可能因資料而產生不適合的資料欄位，
            //又因 DataTable 填入資料後無法改變資料型別，所以另外複製做為
            //上傳程序使用。
            DataTable dt = dtSrc.Clone();
            dt.Columns["LCL"].DataType = typeof(double); dt.Columns["LCL"].AllowDBNull = true;
            dt.Columns["UCL"].DataType = typeof(double); dt.Columns["UCL"].AllowDBNull = true;
            dt.Columns["LSL"].DataType = typeof(double); dt.Columns["LSL"].AllowDBNull = true;
            dt.Columns["USL"].DataType = typeof(double); dt.Columns["USL"].AllowDBNull = true;

            //逐筆檢查資料的合法性
            //LCL~USL 須為數值，且大小關係要正確
            string itemName;
            DateTime appDate;
            foreach (DataRow dr in dtSrc.Rows)
            {
                itemName = dr["TAG_NAME"].ToString();
                if (itemName == "") throw new Exception("TAG_NAME 不可為空");

                //檢查套用時間
                if (!DateTime.TryParse(dr["APPLY_DATE"].ToString(), out appDate))
                    throw new Exception(string.Format("[{0}] 的 APPLY_DATE 非日期資料。", itemName));

                //檢查界限資訊
                Tool.LimitInformation limits = Tool.LimitStringConverter(
                dr["LCL"].ToString(), dr["UCL"].ToString(), dr["LSL"].ToString(), dr["USL"].ToString());

                //將資訊回填至表格中
                DataRow newdr = dt.NewRow();
                newdr["TAG_NAME"] = itemName;
                if (limits.LCL < Mtblib.Tools.MtbTools.MISSINGVALUE) newdr["LCL"] = limits.LCL;
                if (limits.UCL < Mtblib.Tools.MtbTools.MISSINGVALUE) newdr["UCL"] = limits.UCL;
                if (limits.LSL < Mtblib.Tools.MtbTools.MISSINGVALUE) newdr["LSL"] = limits.LSL;
                if (limits.USL < Mtblib.Tools.MtbTools.MISSINGVALUE) newdr["USL"] = limits.USL;
                newdr["APPLY_DATE"] = appDate;
                dt.Rows.Add(newdr);
            }

            using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
            {
                conn.Open();
                using (SqlCommand sqlCmnd = new SqlCommand("TRUNCATE TABLE UPLOAD_FURN_ITEM_LIMIT_INFO", conn))
                {
                    //先清除暫存表上的資料
                    sqlCmnd.ExecuteNonQuery();
                }
                using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                {
                    sqlBC.BatchSize = 1000;
                    sqlBC.BulkCopyTimeout = 6000;

                    //設定要寫入的資料庫
                    sqlBC.DestinationTableName = "UPLOAD_FURN_ITEM_LIMIT_INFO";

                    //對應資料行                            
                    sqlBC.ColumnMappings.Add("TAG_NAME", "TAG_NAME");
                    sqlBC.ColumnMappings.Add("LCL", "LCL");
                    sqlBC.ColumnMappings.Add("UCL", "UCL");
                    sqlBC.ColumnMappings.Add("LSL", "LSL");
                    sqlBC.ColumnMappings.Add("USL", "USL");
                    sqlBC.ColumnMappings.Add("APPLY_DATE", "APPLY_DATE");

                    //開始寫入
                    sqlBC.WriteToServer(dt);
                }
                using (SqlCommand sqlCmnd = new SqlCommand())
                {
                    StringBuilder query = new StringBuilder();
                    query.Clear();
                    query.AppendLine("INSERT INTO FURN_ITEM_LIMIT_INFO(FURN_ITEM_INDEX, LCL, UCL, LSL, USL, APPLY_DATE, RPT_DATE)");
                    query.AppendLine("SELECT * FROM (SELECT A.FURN_ITEM_INDEX, B.LCL, B.UCL, B.LSL, B.USL, B.APPLY_DATE,RPTDATE=GETDATE() FROM FURN_ITEM_INFO A");
                    //query.AppendLine("INNER JOIN UPLOAD_FURN_ITEM_LIMIT_INFO B");
                    query.AppendLine("INNER JOIN (");
                    query.AppendLine("SELECT * FROM UPLOAD_FURN_ITEM_LIMIT_INFO");
                    query.AppendLine("EXCEPT");
                    query.AppendLine("SELECT TAG_NAME, LCL, UCL, LSL, USL, APPLY_DATE FROM vw_furnacelimitrecord");
                    query.AppendLine("EXCEPT");
                    query.AppendLine("(SELECT a.TAG_NAME, a.LCL, a.UCL, a.LSL, a.USL, a.APPLY_DATE FROM UPLOAD_FURN_ITEM_LIMIT_INFO a ");
                    query.AppendLine("INNER JOIN vw_furnacelimitrecord b ON a.TAG_NAME=b.TAG_NAME AND a.APPLY_DATE = b.APPLY_DATE)");
                    query.AppendLine(") B");
                    query.AppendLine("ON A.TAG_NAME = B.TAG_NAME) AS T");
                    sqlCmnd.Connection = conn;
                    sqlCmnd.CommandText = query.ToString();
                    effectedRows = sqlCmnd.ExecuteNonQuery();
                }
            }
            return effectedRows;
        }

        /// <summary>
        /// 編輯資料庫內的界限
        /// </summary>
        /// <param name="obj"></param>
        private int EditLimit(object obj)
        {
            int effectedRows = 0;
            if (FurnItemLimSrc == null || FurnItemLimSrc.Rows.Count == 0) return 0;
            //檢查每一列都是合法的資料(時間、上下界均為數值)
            const double missingvalue = Mtblib.Tools.MtbTools.MISSINGVALUE;
            double lsl = missingvalue, usl = missingvalue, lcl = missingvalue, ucl = missingvalue, value = missingvalue;
            DateTime applyDate;
            foreach (DataRow dr in FurnItemLimSrc.Rows)
            {
                if (!DateTime.TryParse(dr["APPLY_DATE"].ToString(), out applyDate)) throw new Exception("套用時間欄位必須填入時間格式資料");
                Tool.LimitInformation limits = Tool.LimitStringConverter(
                    dr["LCL"].ToString(), dr["UCL"].ToString(), dr["LSL"].ToString(), dr["USL"].ToString());
            }
            //套用時間不可重複
            if (FurnItemLimSrc.AsEnumerable().Select(x => x.Field<DateTime>("APPLY_DATE")).Distinct().Count() < FurnItemLimSrc.Rows.Count)
            {
                throw new Exception("套用時間欄位中有重複的資料");
            }

            //建立上傳資料
            DataTable uploadTable = new DataTable();
            uploadTable.Columns.Add("FURN_ITEM_LIM_INDEX", typeof(string));
            uploadTable.Columns.Add("LCL", typeof(double));
            uploadTable.Columns.Add("UCL", typeof(double));
            uploadTable.Columns.Add("LSL", typeof(double));
            uploadTable.Columns.Add("USL", typeof(double));
            uploadTable.Columns.Add("APPLY_DATE", typeof(DateTime));
            foreach (DataRow dr in FurnItemLimSrc.Rows)
            {
                DataRow newDr = uploadTable.NewRow();
                newDr["FURN_ITEM_LIM_INDEX"] = dr["FURN_ITEM_LIM_INDEX"];
                newDr["LCL"] = dr["LCL"];
                newDr["UCL"] = dr["UCL"];
                newDr["LSL"] = dr["LSL"];
                newDr["USL"] = dr["USL"];
                newDr["APPLY_DATE"] = dr["APPLY_DATE"];
                uploadTable.Rows.Add(newDr);
            }

            //將資料上傳至 UPLOAD_UPDATE_FURN_ITEM_LIMIT_INFO
            using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
            {
                conn.Open();

                using (SqlCommand sqlCmnd = new SqlCommand("TRUNCATE TABLE UPLOAD_UPDATE_FURN_ITEM_LIMIT_INFO", conn))
                {
                    //先清除暫存表上的資料
                    sqlCmnd.ExecuteNonQuery();
                }
                using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                {
                    sqlBC.BatchSize = 1000;
                    sqlBC.BulkCopyTimeout = 6000;

                    //設定要寫入的資料庫
                    sqlBC.DestinationTableName = "UPLOAD_UPDATE_FURN_ITEM_LIMIT_INFO";

                    //對應資料行                            
                    sqlBC.ColumnMappings.Add("FURN_ITEM_LIM_INDEX", "FURN_ITEM_LIM_INDEX");
                    sqlBC.ColumnMappings.Add("LCL", "LCL");
                    sqlBC.ColumnMappings.Add("UCL", "UCL");
                    sqlBC.ColumnMappings.Add("LSL", "LSL");
                    sqlBC.ColumnMappings.Add("USL", "USL");
                    sqlBC.ColumnMappings.Add("APPLY_DATE", "APPLY_DATE");

                    //開始寫入
                    sqlBC.WriteToServer(uploadTable);
                }
                using (SqlCommand sqlCmnd = new SqlCommand())
                {
                    StringBuilder query = new StringBuilder();
                    query.Clear();
                    query.AppendLine("UPDATE FURN_ITEM_LIMIT_INFO ");
                    query.AppendLine("SET LCL = a.LCL, UCL= a.UCL, LSL= a.LSL, USL=a.USL, APPLY_DATE = a.APPLY_DATE,RPT_DATE=GETDATE()");
                    query.AppendLine("FROM (");
                    query.AppendLine("SELECT * FROM UPLOAD_UPDATE_FURN_ITEM_LIMIT_INFO");
                    query.AppendLine("EXCEPT");
                    query.AppendLine("SELECT FURN_ITEM_LIM_INDEX, LCL, UCL, LSL, USL, APPLY_DATE FROM FURN_ITEM_LIMIT_INFO");
                    query.AppendLine(") a");
                    query.AppendLine("WHERE a.FURN_ITEM_LIM_INDEX =  FURN_ITEM_LIMIT_INFO.FURN_ITEM_LIM_INDEX");
                    sqlCmnd.Connection = conn;
                    sqlCmnd.CommandText = query.ToString();
                    effectedRows = sqlCmnd.ExecuteNonQuery();
                }
            }
            return effectedRows;
        }

        public ICommand OpenFileDialogCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                        if (openFileDialog.ShowDialog() == true)
                        {
                            SelectedFilePath = openFileDialog.FileName;
                        }
                    });
            }
        }

        public ICommand GetSampleFileCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                        saveFileDialog.Filter = "逗號分隔文字檔|*.csv";
                        saveFileDialog.Title = "儲存批次上傳檔格式";
                        if (saveFileDialog.ShowDialog() == true && saveFileDialog.FileName != "")
                        {
                            string fpath = saveFileDialog.FileName;
                            DataTable dt =
                            Database.DBQueryTool.GetData(
                            "select * from UPLOAD_FURN_ITEM_LIMIT_INFO",
                            Database.DBQueryTool.GetConnString()).Clone();
                            StringBuilder sb = new StringBuilder();
                            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                                              Select(column => column.ColumnName);
                            sb.AppendLine(string.Join(",", columnNames));
                            File.WriteAllText(fpath, sb.ToString());
                            MessageBox.Show("資料下載完成。", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        }



                    });
            }
        }

        public ICommand SelectionChangedCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        if (ActionMode == "Add")
                        {
                            ActionString = "新增界限";
                            IsSingleAddMode = true; IsBatchAddMode = false; IsEditMode = false;

                        }
                        else if (ActionMode == "BatchAdd")
                        {
                            ActionString = "批次新增界限";
                            IsBatchAddMode = true; IsSingleAddMode = false; IsEditMode = false;
                        }
                        else if (ActionMode == "Edit")
                        {
                            ActionString = "編輯界限";
                            IsSingleAddMode = false; IsBatchAddMode = false; IsEditMode = true;
                        }
                    });
            }
        }

        public ICommand ActionCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        try
                        {
                            if (ActionMode == "Add")
                            {
                                AddLimit(param);
                                MessageBox.Show("資料上傳完成", "", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else if (ActionMode == "BatchAdd")
                            {
                                BatchAddLimit(param);
                                MessageBox.Show(string.Format("資料批次上傳完成，成功上傳{0}筆", 0), "", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else if (ActionMode == "Edit")
                            {
                                int r = EditLimit(param);
                                MessageBox.Show(string.Format("資料編輯完成，共{0}筆。", r), "", MessageBoxButton.OK, MessageBoxImage.Information);
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    });
            }
        }
    }
}
