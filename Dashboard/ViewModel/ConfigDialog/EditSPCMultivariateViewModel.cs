using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LinearAlgebra = MathNet.Numerics.LinearAlgebra;

namespace Dashboard.ViewModel.ConfigDialog
{
    public class EditSPCMultivariateViewModel : NotifyPropertyChanged
    {

        public EditSPCMultivariateViewModel()
        {
            Load();
        }

        /// <summary>
        /// 取得頁面標題
        /// </summary>
        public string Title
        {
            get { return _title; }
            private set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }
        private string _title = "管理多變量參數組";

        /// <summary>
        /// 指定或取得廠別資訊
        /// </summary>
        public string Site
        {
            get { return _site; }
            set
            {
                _site = value;
                OnSiteChanged();
                Title = "管理多變量參數組 - " + Site;
                RaisePropertyChanged("Site");
            }
        }
        private string _site = "";

        /// <summary>
        /// 取得可使用編輯行為的清單
        /// </summary>
        public DataTable ActionList
        {
            get { return _actList; }
            private set
            {
                _actList = value;
                RaisePropertyChanged("ActionList");
            }
        }
        private DataTable _actList = null;

        /// <summary>
        /// 取得目前該頁面的行為
        /// </summary>
        public int ActionMode
        {
            get { return (int)_actMode; }
            set
            {
                _actMode = (UserActionMode)value;
                RaisePropertyChanged("ActionMode");
            }
        }
        private UserActionMode _actMode = UserActionMode.Add;

        /// <summary>
        /// 取得目前該頁面的行為描述文字
        /// </summary>
        public string ActionString
        {
            get { return _actString; }
            private set
            {
                _actString = value;
                RaisePropertyChanged("ActionString");
            }
        }
        private string _actString = "新增參數組";

        /// <summary>
        /// 取得可用於選取的熔爐項目集合
        /// </summary>
        public ObservableCollection<SPCItemInfo> FurnItemsSrc
        {
            get { return _furnItemSrc; }
            private set
            {
                _furnItemSrc = value;
                RaisePropertyChanged("FurnItemSrc");
            }
        }
        private ObservableCollection<SPCItemInfo> _furnItemSrc = new ObservableCollection<SPCItemInfo>();
        private void _furnItemSrc_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("FurnItemsSrc");
        }

        /// <summary>
        /// 取得選取的熔爐項目集合
        /// </summary>
        public ObservableCollection<SPCItemInfo> SelectedFurnItems
        {
            get { return _selectedFurnItems; }
            private set
            {
                _selectedFurnItems = value;
                RaisePropertyChanged("SelectedFurnItems");
            }
        }
        private ObservableCollection<SPCItemInfo> _selectedFurnItems = new ObservableCollection<SPCItemInfo>();
        private void _selectedFurnItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("SelectedFurnItems");
        }

        /// <summary>
        /// 取得目前使用者行為的步驟值
        /// </summary>
        public double Step
        {
            get { return _step; }
            private set
            {
                _step = value;
                RaisePropertyChanged("Step");
            }
        }
        private double _step = 1;

        /// <summary>
        /// 取得平均數向量
        /// </summary>
        public DataTable MeanVector
        {
            get { return _mvect; }
            private set
            {
                _mvect = value;
                RaisePropertyChanged("MeanVector");
            }
        }
        private DataTable _mvect = null;

        /// <summary>
        /// 取得共變異數矩陣
        /// </summary>
        public DataTable CovarianceMatrix
        {
            get { return _cov; }
            private set
            {
                _cov = value;
                RaisePropertyChanged("CovarianceMatrix");
            }
        }
        private DataTable _cov = null;

        /// <summary>
        /// 取得共變異數的樣本數
        /// </summary>
        public int? SampleSize
        {
            get { return _sampleSize > 0 ? (int?)_sampleSize : null; }
            set
            {
                if (value != null && value.ToString() != string.Empty)
                {
                    _sampleSize = (int)value;
                }
                else
                {
                    _sampleSize = -1;
                }
                RaisePropertyChanged("SampleSize");
            }
        }
        private int _sampleSize = -1;

        /// <summary>
        /// 取得新增參數組的套用時間
        /// </summary>
        public DateTime ApplyDate
        {
            get { return _applyDate; }
            private set
            {
                _applyDate = value;
                RaisePropertyChanged("ApplyDate");
            }
        }
        private DateTime _applyDate = DateTime.Now.Date;

        /// <summary>
        /// 取得試算的管制上限結果
        /// </summary>
        public string UCLText
        {
            get { return _uclText; }
            private set
            {
                _uclText = value;
                RaisePropertyChanged("UCLText");
            }
        }
        private string _uclText = "";

        /// <summary>
        /// 取得最新的各參數組資訊
        /// </summary>
        public DataTable ParaItemsSrc
        {
            get { return _paraItemsSrc; }
            private set
            {
                _paraItemsSrc = value;
                RaisePropertyChanged("ParaItemsSrc");
            }
        }
        private DataTable _paraItemsSrc = null;

        /// <summary>
        /// 取得欲編輯的參數組資訊 (CHARAT_PARA_INDEX)
        /// </summary>
        public string SelectedParaItemValue
        {
            get { return _selParamItemValue; }
            set
            {
                _selParamItemValue = value;
                OnSelectedParaItemChanged();
                RaisePropertyChanged("SelectedParaItemValue");
            }
        }
        private string _selParamItemValue;

        /// <summary>
        /// 指定或取得 Minitab Project
        /// </summary>
        public Mtb.Project Project { get; set; }

        /// <summary>
        /// 取得查詢的起始日期
        /// </summary>
        public virtual DateTime StartDate
        {
            get { return _startDate; }
            protected set
            {
                _startDate = value;
                RaisePropertyChanged("StartDateTime");
            }
        }
        private DateTime _startDate = DateTime.Now.AddDays(-1);
        /// <summary>
        /// 取得查詢的起始時間
        /// </summary>
        public virtual string StartTimeValue
        {
            get { return _startTimeValue; }
            protected set
            {
                _startTimeValue = value;
                RaisePropertyChanged("StartTimeValue");
            }
        }
        private string _startTimeValue = null;
        /// <summary>
        /// 取得查詢的結束日期
        /// </summary>
        public virtual DateTime EndDate
        {
            get { return _endDate; }
            protected set
            {
                _endDate = value;
                RaisePropertyChanged("EndDateTime");
            }
        }
        private DateTime _endDate = DateTime.Now;
        /// <summary>
        /// 取得查詢的結束時間
        /// </summary>
        public virtual string EndTimeValue
        {
            get { return _endTimeValue; }
            protected set
            {
                _endTimeValue = value;
                RaisePropertyChanged("EndTimeValue");
            }
        }
        private string _endTimeValue = null;


        private void Load()
        {
            _actList = new DataTable();
            _actList.Columns.Add("Name", typeof(string));
            _actList.Columns.Add("Value", typeof(UserActionMode));
            DataRow dr;
            dr = _actList.NewRow();
            dr["Name"] = "新增"; dr["Value"] = UserActionMode.Add;
            _actList.Rows.Add(dr);
            dr = _actList.NewRow();
            dr["Name"] = "修改"; dr["Value"] = UserActionMode.Edit;
            _actList.Rows.Add(dr);

            _furnItemSrc.CollectionChanged += _furnItemSrc_CollectionChanged;
            _selectedFurnItems.CollectionChanged += _selectedFurnItems_CollectionChanged;

            //設定時間區間
            DateTime datetime = DateTime.Now;
            EndDate = datetime.Date;
            StartDate = datetime.AddDays(-1).Date;
            EndTimeValue = string.Format("{0:HH:mm}", datetime);
            StartTimeValue = EndTimeValue;

        }

        private void OnSiteChanged()
        {
            if (Site == null || Site == string.Empty) return;
            //if (FurnItemsSrc == null) FurnItemsSrc = new ObservableCollection<SPCItemInfo>();
            FurnItemsSrc.Clear();
            DataTable dt = Database.DBQueryTool.GetFurnItemInfo(Site);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows.Cast<DataRow>())
                {
                    SPCItemInfo info = new SPCItemInfo()
                    {
                        ItemList = item["FURN_ITEM_INDEX"].ToString(),
                        Flag = "T2",
                        Description = item["ITEM_NAME"].ToString(),
                        Title = item["ITEM_NAME"].ToString()
                    };
                    FurnItemsSrc.Add(info);
                }
            }
        }

        private void OnActionChanged(object obj)
        {
            Step = 1;
            switch ((UserActionMode)ActionMode)
            {
                case UserActionMode.Add:
                    ActionString = "新增參數組";
                    break;
                case UserActionMode.BatchAdd:
                    break;
                case UserActionMode.Edit:
                    ActionString = "編輯參數組";
                    //載入最新的參數組清單
                    ParaItemsSrc = GetLatestParamList();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 處理滑鼠左鍵連點在可選/已選項
        /// </summary>
        public void AddClickedItem(object obj)
        {
            System.Windows.Controls.TextBlock itemTextBlock = obj as System.Windows.Controls.TextBlock;
            if (itemTextBlock == null) return;

            foreach (var item in FurnItemsSrc)
            {
                if (item.Description == itemTextBlock.Text)
                {
                    if (SelectedFurnItems.Count() > 0)
                    {
                        var a = SelectedFurnItems.Select(x => x.ItemList == item.ItemList);
                        a = a as IEnumerable<bool>;
                        if (!a.Contains(true))
                        {
                            SelectedFurnItems.Add(item);
                            return;
                        }
                    }
                    else
                    {
                        SelectedFurnItems.Add(item);
                        return;
                    }
                }
            }
        }

        public void RemoveClickedItem(object obj)
        {
            System.Windows.Controls.TextBlock itemTextBlock = obj as System.Windows.Controls.TextBlock;
            if (itemTextBlock == null) return;

            foreach (var item in FurnItemsSrc)
            {
                if (item.Description == itemTextBlock.Text)
                {
                    for (int i = SelectedFurnItems.Count; i-- > 0;)
                    {
                        SPCItemInfo info = SelectedFurnItems[i];
                        if (item.ItemList == info.ItemList)
                        {
                            SelectedFurnItems.RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 取得參數清單(只取最新的 APPLY_DATE )
        /// </summary>
        /// <returns></returns>
        private DataTable GetLatestParamList()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT DISTINCT CHART_PARA_INDEX,ITEM_LIST,APPLY_DATE FROM vw_chartparameter");
            query.AppendFormat("WHERE LAPPLYDATE = APPLY_DATE AND SITE_ID='{0}'\r\n", Site);
            query.AppendLine("ORDER BY ITEM_LIST,APPLY_DATE");
            DataTable dt1 = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString());
            DataTable dt2 = Database.DBQueryTool.GetFurnItemInfo(Site);
            DataTable dtFinal = new DataTable();
            dtFinal.Columns.Add("CHART_PARA_INDEX", typeof(string));
            dtFinal.Columns.Add("ITEM_LIST", typeof(string));
            dtFinal.Columns.Add("ITEM_NAMES", typeof(string));
            dtFinal.Columns.Add("APPLY_DATE", typeof(DateTime));
            var splitedItemList = dt1.AsEnumerable().Select(dr =>
                new
                {
                    Index = dr.Field<string>("CHART_PARA_INDEX"),
                    ItemList = dr.Field<string>("ITEM_LIST"),
                    ItemListArr = dr.Field<string>("ITEM_LIST").Split(','),
                    Applydate = dr.Field<DateTime>("APPLY_DATE")
                });
            foreach (var item in splitedItemList)
            {
                DataRow dr = dtFinal.NewRow();
                dr["CHART_PARA_INDEX"] = item.Index;
                dr["ITEM_LIST"] = item.ItemList;
                string[] itemNames = dt2.Select(
                    string.Format("FURN_ITEM_INDEX in ({0})", string.Join(",", item.ItemListArr.Select(x => "'" + x + "'"))))
                    .Select(x => x.Field<string>("ITEM_NAME")).ToArray();
                dr["ITEM_NAMES"] = string.Join(",", itemNames);
                dr["APPLY_DATE"] = item.Applydate;
                dtFinal.Rows.Add(dr);
            }
            if (dtFinal.Rows.Count <= 1) dtFinal.Rows.InsertAt(dtFinal.NewRow(), 0);
            return dtFinal;



        }

        private void OnSelectedParaItemChanged()
        {
            if (SelectedParaItemValue == null || SelectedParaItemValue == string.Empty) return;
            var info = ParaItemsSrc.AsEnumerable().Where(x => x["CHART_PARA_INDEX"].ToString() == SelectedParaItemValue)
                .Select(x => new
                {
                    ItemList = x.Field<string>("ITEM_LIST"),
                    ApplyDate = x.Field<DateTime>("APPLY_DATE")
                }).First();
            ParaSetInfo paraSet = GetParaDetail(info.ItemList);
            MeanVector = paraSet.Mean;
            CovarianceMatrix = paraSet.Cov;
            SampleSize = paraSet.SampleSize;
            ApplyDate = info.ApplyDate;

            Step = 2;
        }

        /// <summary>
        /// 取得指定熔爐組合的參數值
        /// </summary>
        /// <param name="itemList">熔爐 Index 組合</param>
        /// <returns></returns>
        private ParaSetInfo GetParaDetail(string itemList)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT CHART_PARA_INDEX,SITE_ID,ITEM_LIST,APPLY_DATE,P_FLAG,ROWNO,COLNO,VALUE FROM vw_chartparameter");
            query.AppendFormat("WHERE ITEM_LIST ='{0}' AND APPLY_DATE=LAPPLYDATE\r\n", itemList);
            query.AppendLine("ORDER BY APPLY_DATE DESC, RPT_DATE DESC,P_FLAG,ROWNO,COLNO");
            DataTable paraData = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString());
            //建立平均數向量和共變異數矩陣
            DataTable mean = new DataTable();
            DataTable cov = new DataTable();

            string[] itemNames = Database.DBQueryTool.GetFurnNameByItemList(itemList);

            foreach (var item in itemNames)
            {
                //建構 Mean vector 欄位
                DataColumn dc = new DataColumn(item, typeof(string));
                dc.AllowDBNull = true;
                mean.Columns.Add(dc);

                //建構 Covariance matrix 欄位
                dc = new DataColumn(item, typeof(string));
                dc.AllowDBNull = true;
                cov.Columns.Add(dc);

            }
            DataTable dt;
            ParaSetInfo paraSet = new ParaSetInfo();
            if (paraData != null && paraData.Rows.Count > 0)
            {
                #region 複製最新一筆參數值
                dt = paraData.Select("P_FLAG = 'MEAN'").CopyToDataTable();
                if (dt != null && dt.Rows.Count > 0)
                {
                    int r = dt.AsEnumerable().Select(x => x.Field<Byte>("ROWNO")).Max();
                    int c = dt.AsEnumerable().Select(x => x.Field<Byte>("COLNO")).Max();
                    var data = dt.AsEnumerable();
                    for (int rCnt = 0; rCnt < r; rCnt++)
                    {
                        DataRow dr = mean.NewRow();
                        for (int cCnt = 0; cCnt < c; cCnt++)
                        {
                            dr[cCnt] = data.Where(x => x.Field<Byte>("ROWNO") == rCnt + 1 && x.Field<Byte>("COLNO") == cCnt + 1)
                                .Select(x => x.Field<decimal>("VALUE")).First();
                        }
                        mean.Rows.Add(dr);
                    }
                    paraSet.Mean = mean;
                }

                dt = paraData.Select("P_FLAG='COV'").CopyToDataTable();
                if (dt != null && dt.Rows.Count > 0)
                {
                    int r = dt.AsEnumerable().Select(x => x.Field<Byte>("ROWNO")).Max();
                    int c = dt.AsEnumerable().Select(x => x.Field<Byte>("COLNO")).Max();
                    var data = dt.AsEnumerable();

                    for (int rCnt = 0; rCnt < r; rCnt++)
                    {
                        DataRow dr = cov.NewRow();
                        for (int cCnt = 0; cCnt < c; cCnt++)
                        {
                            dr[cCnt] = data.Where(x => x.Field<Byte>("ROWNO") == rCnt + 1 && x.Field<Byte>("COLNO") == cCnt + 1)
                                .Select(x => x.Field<decimal>("VALUE")).First();
                        }
                        cov.Rows.Add(dr);
                    }
                    paraSet.Cov = cov;

                    dt = paraData.Select("P_FLAG='N'").CopyToDataTable();
                    paraSet.SampleSize = Convert.ToInt32(dt.AsEnumerable().Select(x => x.Field<decimal>("VALUE")).FirstOrDefault());

                }
                #endregion

            }
            else
            {
                mean.Rows.Add(mean.NewRow());
                paraSet.Mean = mean;
                for (int i = 0; i < SelectedFurnItems.Count; i++)
                {
                    cov.Rows.Add(cov.NewRow());
                }
                paraSet.Cov = cov;
                paraSet.SampleSize = null;
            }
            paraSet.SubgroupSize = 1;

            return paraSet;


        }

        private void AddParaToDatabase(object obj)
        {
            ParaSetInfo paraSet = new ParaSetInfo()
            {
                Mean = MeanVector,
                Cov = CovarianceMatrix,
                SampleSize = SampleSize,
                SubgroupSize = 1
            };
            ParaValidation(paraSet);

            //確認時間是否有重複
            string itemList = string.Join(",", SelectedFurnItems.Select(x => x.IntItemList));
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT DISTINCT APPLY_DATE FROM vw_chartparameter");
            query.AppendFormat("WHERE ITEM_LIST ='{0}'\r\n", itemList);
            DataTable dateRecord = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString());
            if (dateRecord.AsEnumerable().Select(x => x.Field<DateTime>("APPLY_DATE")).ToArray().Contains(ApplyDate))
            {
                throw new Exception("資料庫中已有相同套用時間的參數組");
            }

            //建立上傳資料表
            DataTable uploadTable = new DataTable();
            uploadTable.Columns.Add("ITEM_LIST", typeof(string));
            uploadTable.Columns.Add("CHART_FLAG", typeof(string));
            uploadTable.Columns.Add("PARA_FLAG", typeof(string));
            uploadTable.Columns.Add("ROWNO", typeof(int));
            uploadTable.Columns.Add("COLNO", typeof(int));
            uploadTable.Columns.Add("VALUE", typeof(double));
            uploadTable.Columns.Add("APPLY_DATE", typeof(DateTime));

            DataRow dr;
            int r = CovarianceMatrix.Columns.Count;
            int c = CovarianceMatrix.Rows.Count;
            for (int cCnt = 0; cCnt < c; cCnt++)
            {
                dr = uploadTable.NewRow();
                dr["ITEM_LIST"] = itemList; dr["CHART_FLAG"] = "T2"; dr["PARA_FLAG"] = "MEAN"; dr["APPLY_DATE"] = ApplyDate;
                dr["ROWNO"] = 1;
                dr["COLNO"] = cCnt + 1;
                dr["VALUE"] = double.Parse(MeanVector.Rows[0][cCnt].ToString());
                uploadTable.Rows.Add(dr);
            }
            for (int rCnt = 0; rCnt < r; rCnt++)
            {
                for (int cCnt = 0; cCnt < c; cCnt++)
                {
                    dr = uploadTable.NewRow();
                    dr["ITEM_LIST"] = itemList; dr["CHART_FLAG"] = "T2"; dr["PARA_FLAG"] = "COV"; dr["APPLY_DATE"] = ApplyDate;
                    dr["ROWNO"] = rCnt + 1;
                    dr["COLNO"] = cCnt + 1;
                    dr["VALUE"] = double.Parse(CovarianceMatrix.Rows[rCnt][cCnt].ToString());
                    uploadTable.Rows.Add(dr);
                }
            }

            dr = uploadTable.NewRow();
            dr["ITEM_LIST"] = itemList; dr["CHART_FLAG"] = "T2"; dr["PARA_FLAG"] = "N"; dr["APPLY_DATE"] = ApplyDate;
            dr["ROWNO"] = 1; dr["COLNO"] = 1;
            dr["VALUE"] = SampleSize;
            uploadTable.Rows.Add(dr);

            dr = uploadTable.NewRow();
            dr["ITEM_LIST"] = itemList; dr["CHART_FLAG"] = "T2"; dr["PARA_FLAG"] = "SUBGP"; dr["APPLY_DATE"] = ApplyDate;
            dr["ROWNO"] = 1; dr["COLNO"] = 1;
            dr["VALUE"] = 1;
            uploadTable.Rows.Add(dr);

            using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand sqlCmnd = new SqlCommand("TRUNCATE TABLE UPLOAD_PARAMETER", conn))
                    {
                        //先清除暫存表上的資料
                        sqlCmnd.ExecuteNonQuery();
                    }
                    using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                    {
                        sqlBC.BatchSize = 1000;
                        sqlBC.BulkCopyTimeout = 6000;

                        //設定要寫入的資料庫
                        sqlBC.DestinationTableName = "UPLOAD_PARAMETER";

                        //對應資料行                            
                        sqlBC.ColumnMappings.Add("ITEM_LIST", "ITEM_LIST");
                        sqlBC.ColumnMappings.Add("CHART_FLAG", "CHART_FLAG");
                        sqlBC.ColumnMappings.Add("PARA_FLAG", "PARA_FLAG");
                        sqlBC.ColumnMappings.Add("APPLY_DATE", "APPLY_DATE");
                        sqlBC.ColumnMappings.Add("ROWNO", "ROWNO");
                        sqlBC.ColumnMappings.Add("COLNO", "COLNO");
                        sqlBC.ColumnMappings.Add("VALUE", "VALUE");

                        //開始寫入
                        sqlBC.WriteToServer(uploadTable);
                    }
                    using (SqlCommand sqlCmnd = new SqlCommand())
                    {
                        query.Clear();
                        //上傳主要參數資訊
                        query.AppendLine("INSERT INTO CHART_PARAMETER(SITE_ID, ITEM_LIST, FLAG, APPLY_DATE, RPT_DATE)");
                        query.AppendLine("SELECT * FROM (");
                        query.AppendFormat("SELECT DISTINCT SITE_ID ='{0}', ITEM_LIST, CHART_FLAG, APPLY_DATE, RPT_DATE= GETDATE() FROM UPLOAD_PARAMETER) AS T\r\n", Site);
                        //建立虛擬表格
                        query.AppendLine("IF OBJECT_ID('TEMPDB..#TMP') IS NOT NULL");
                        query.AppendLine("BEGIN");
                        query.AppendLine("DROP TABLE #TMP");
                        query.AppendLine("END");
                        query.AppendLine("CREATE TABLE #TMP(ID VARCHAR(32))");
                        //取得新增的流水號
                        query.AppendLine("INSERT INTO #TMP (ID)");
                        query.AppendLine("SELECT TOP(1) CHART_PARA_INDEX FROM CHART_PARAMETER");
                        query.AppendFormat("WHERE ITEM_LIST='{0}' AND APPLY_DATE='{1:yyyy-M-dd HH:mm}'\r\n", itemList, ApplyDate);
                        query.AppendLine("DECLARE @ID VARCHAR");
                        query.AppendLine("SET @ID = (SELECT TOP(1) ID  FROM #TMP)");
                        //將參數內容上傳至表格內
                        query.AppendLine("INSERT INTO PARAMETER_DETAIL(CHART_PARA_INDEX,FLAG, ROWNO,COLNO,VALUE)");
                        query.AppendLine("SELECT * FROM (SELECT ID=@ID, FLAG= PARA_FLAG, ROWNO, COLNO, VALUE FROM UPLOAD_PARAMETER) AS T");

                        sqlCmnd.Connection = conn;
                        sqlCmnd.CommandText = query.ToString();
                        sqlCmnd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("上傳時發生錯誤:\r\n{0}", ex.Message));
                }
            }

        }
        private void EditParaInDatabase(object obj)
        {
            ParaSetInfo paraSet = new ParaSetInfo()
            {
                Mean = MeanVector,
                Cov = CovarianceMatrix,
                SampleSize = SampleSize,
                SubgroupSize = 1
            };
            ParaValidation(paraSet);

            //判斷時間是否重複，並且不可比現有的應用時間(被編輯項除外)還要早
            string itemList = ParaItemsSrc.AsEnumerable().Where(x => x["CHART_PARA_INDEX"].ToString() == SelectedParaItemValue)
                .Select(x => x.Field<string>("ITEM_LIST")).First();
            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT DISTINCT CHART_PARA_INDEX, APPLY_DATE FROM vw_chartparameter");
            query.AppendFormat("WHERE ITEM_LIST ='{0}'\r\n", itemList);
            DataTable dateRecord = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString());
            if (dateRecord.Rows.Count > 1)//如果等於1，表示只有一筆參數組
            {
                int compare = DateTime.Compare(dateRecord.AsEnumerable()
                .Where(x => x["CHART_PARA_INDEX"].ToString() != SelectedParaItemValue)
                .Select(x => x.Field<DateTime>("APPLY_DATE"))
                .OrderByDescending(x => x).First(), ApplyDate);
                if (compare >= 0) throw new Exception("套用時間不可等於或早於該多變量管制圖的其他參數組套用時間");
            }

            //建立上傳資料表
            DataTable uploadTable = new DataTable();
            uploadTable.Columns.Add("ITEM_LIST", typeof(string));
            uploadTable.Columns.Add("CHART_FLAG", typeof(string));
            uploadTable.Columns.Add("PARA_FLAG", typeof(string));
            uploadTable.Columns.Add("ROWNO", typeof(int));
            uploadTable.Columns.Add("COLNO", typeof(int));
            uploadTable.Columns.Add("VALUE", typeof(double));
            uploadTable.Columns.Add("APPLY_DATE", typeof(DateTime));

            DataRow dr;
            int r = CovarianceMatrix.Columns.Count;
            int c = CovarianceMatrix.Rows.Count;
            for (int cCnt = 0; cCnt < c; cCnt++)
            {
                dr = uploadTable.NewRow();
                dr["ITEM_LIST"] = itemList; dr["CHART_FLAG"] = "T2"; dr["PARA_FLAG"] = "MEAN"; dr["APPLY_DATE"] = ApplyDate;
                dr["ROWNO"] = 1;
                dr["COLNO"] = cCnt + 1;
                dr["VALUE"] = double.Parse(MeanVector.Rows[0][cCnt].ToString());
                uploadTable.Rows.Add(dr);
            }
            for (int rCnt = 0; rCnt < r; rCnt++)
            {
                for (int cCnt = 0; cCnt < c; cCnt++)
                {
                    dr = uploadTable.NewRow();
                    dr["ITEM_LIST"] = itemList; dr["CHART_FLAG"] = "T2"; dr["PARA_FLAG"] = "COV"; dr["APPLY_DATE"] = ApplyDate;
                    dr["ROWNO"] = rCnt + 1;
                    dr["COLNO"] = cCnt + 1;
                    dr["VALUE"] = double.Parse(CovarianceMatrix.Rows[rCnt][cCnt].ToString());
                    uploadTable.Rows.Add(dr);
                }
            }

            dr = uploadTable.NewRow();
            dr["ITEM_LIST"] = itemList; dr["CHART_FLAG"] = "T2"; dr["PARA_FLAG"] = "N"; dr["APPLY_DATE"] = ApplyDate;
            dr["ROWNO"] = 1; dr["COLNO"] = 1;
            dr["VALUE"] = SampleSize;
            uploadTable.Rows.Add(dr);

            dr = uploadTable.NewRow();
            dr["ITEM_LIST"] = itemList; dr["CHART_FLAG"] = "T2"; dr["PARA_FLAG"] = "SUBGP"; dr["APPLY_DATE"] = ApplyDate;
            dr["ROWNO"] = 1; dr["COLNO"] = 1;
            dr["VALUE"] = 1;
            uploadTable.Rows.Add(dr);

            using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand sqlCmnd = new SqlCommand("TRUNCATE TABLE UPLOAD_PARAMETER", conn))
                    {
                        //先清除暫存表上的資料
                        sqlCmnd.ExecuteNonQuery();
                    }
                    using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                    {
                        sqlBC.BatchSize = 1000;
                        sqlBC.BulkCopyTimeout = 6000;

                        //設定要寫入的資料庫
                        sqlBC.DestinationTableName = "UPLOAD_PARAMETER";

                        //對應資料行                            
                        sqlBC.ColumnMappings.Add("ITEM_LIST", "ITEM_LIST");
                        sqlBC.ColumnMappings.Add("CHART_FLAG", "CHART_FLAG");
                        sqlBC.ColumnMappings.Add("PARA_FLAG", "PARA_FLAG");
                        sqlBC.ColumnMappings.Add("APPLY_DATE", "APPLY_DATE");
                        sqlBC.ColumnMappings.Add("ROWNO", "ROWNO");
                        sqlBC.ColumnMappings.Add("COLNO", "COLNO");
                        sqlBC.ColumnMappings.Add("VALUE", "VALUE");

                        //開始寫入
                        sqlBC.WriteToServer(uploadTable);
                    }
                    using (SqlCommand sqlCmnd = new SqlCommand())
                    {
                        query.Clear();
                        //更新 PARAMETER_DETAIL
                        query.AppendLine("UPDATE PARAMETER_DETAIL");
                        query.AppendLine("SET PARAMETER_DETAIL.VALUE = a.VALUE FROM(");
                        query.AppendFormat("SELECT CHART_PARA_INDEX='{0}',* FROM UPLOAD_PARAMETER", SelectedParaItemValue);
                        query.AppendLine(") a");
                        query.AppendLine("WHERE PARAMETER_DETAIL.CHART_PARA_INDEX = a.CHART_PARA_INDEX AND ");
                        query.AppendLine("PARAMETER_DETAIL.FLAG = a.PARA_FLAG AND PARAMETER_DETAIL.ROWNO = a.ROWNO AND PARAMETER_DETAIL.COLNO = a.COLNO");

                        //更新 CHART_PARAMETER
                        query.AppendLine("UPDATE CHART_PARAMETER");
                        query.AppendLine("SET APPLY_DATE = a.APPLY_DATE, RPT_DATE = GETDATE() FROM(");
                        query.AppendFormat("SELECT CHART_PARA_INDEX='{0}',* FROM UPLOAD_PARAMETER", SelectedParaItemValue);
                        query.AppendLine(") a");
                        query.AppendLine("WHERE CHART_PARAMETER.CHART_PARA_INDEX = a.CHART_PARA_INDEX");

                        sqlCmnd.Connection = conn;
                        sqlCmnd.CommandText = query.ToString();
                        sqlCmnd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("更新時發生錯誤:\r\n{0}", ex.Message));
                }
            }

        }

        /// <summary>
        /// 判斷individaul T2管制圖的參數是否合法? 如果合法不回發生任何事，若不合法則回傳例外。
        /// </summary>
        /// <param name="paraSet"></param>
        /// <returns></returns>
        private void ParaValidation(ParaSetInfo paraSet)
        {
            DataTable mean = paraSet.Mean;
            DataTable cov = paraSet.Cov;
            int? ssiz = paraSet.SampleSize;

            if (mean == null || cov == null) throw new ArgumentNullException("尚未輸入平均數向量或共變異數矩陣");
            if (SampleSize == null || SampleSize <= 0) throw new Exception("樣本數內容不正確");

            //確認輸入的數據是否正確(數量、共變異矩陣對稱、對角項大於0、正定等特性)
            int r = cov.Columns.Count;
            int c = cov.Rows.Count;
            double result;
            if (mean.Rows.Cast<DataRow>().Any(x => x.ItemArray.Any(y => !Double.TryParse(y.ToString(), out result)))) throw new Exception("平均數欄位內容必須為數字");
            if (cov.Rows.Cast<DataRow>().Any(x => x.ItemArray.Any(y => !Double.TryParse(y.ToString(), out result)))) throw new Exception("共變異數內容必須為數字");

            //將共變異數矩陣轉到Minitab中判斷是否合法
            double[] covArray = new double[r * c];

            for (int cCnt = 0; cCnt < c; cCnt++)
            {
                for (int rCnt = cCnt; rCnt < r; rCnt++)
                {
                    if (rCnt == cCnt)
                    {
                        double value = double.Parse(cov.Rows[rCnt][cCnt].ToString());
                        if (value <= 0)
                        {
                            throw new Exception("共變異數矩陣對角項小於等於0");
                        }
                        covArray[cCnt * r + rCnt] = value;
                    }
                    else
                    {
                        if (cov.Rows[rCnt][cCnt].ToString() != cov.Rows[cCnt][rCnt].ToString())
                        {
                            throw new Exception("共變異數矩陣不對稱");
                        }
                        double value = double.Parse(CovarianceMatrix.Rows[rCnt][cCnt].ToString());
                        covArray[cCnt * r + rCnt] = value;
                        covArray[rCnt * r + cCnt] = value;
                    }
                }
            }
            Mtb.Worksheet ws = Project.Worksheets.Add(1);
            Mtb.Matrix mat = ws.Matrices.Add(Quantity: 1);
            Mtb.Column col = ws.Columns.Add(Quantity: 1);
            mat.SetData(covArray, 1, 1);
            Project.ExecuteCommand(string.Format("Eigen {0} {1}.\r\n", mat.SynthesizedName, col.SynthesizedName), ws);
            double[] eigvalue = col.GetData();
            if (eigvalue.Any(x => x <= 0)) throw new Exception("共變異數矩陣不為正定");
            ws.Delete();
            Project.Commands.Delete();
        }

        public ICommand OnActionChangedCommand
        {
            get { return new Command.RelayCommand(OnActionChanged); }
        }
        public ICommand ConfirmFurnItemCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        if (SelectedFurnItems.Count < 2)
                        {
                            MessageBox.Show("請至少選擇兩個熔爐項", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        //取得要使用的項目與 INDEX
                        string itemList = string.Join(",", SelectedFurnItems.Select(x => x.IntItemList));

                        ParaSetInfo paraSet = GetParaDetail(itemList);
                        MeanVector = paraSet.Mean;
                        CovarianceMatrix = paraSet.Cov;
                        SampleSize = paraSet.SampleSize;
                        Step = 2;


                    });
            }
        }
        public ICommand PreStepCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        MeanVector = null;
                        CovarianceMatrix = null;
                        SampleSize = null;
                        Step = 1;
                    });
            }
        }
        public ICommand LimitCalcCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {

                        StringBuilder cmnd = new StringBuilder();
                        int p = 0;
                        int m = 0;
                        try
                        {
                            p = CovarianceMatrix.Columns.Count;
                            m = (int)SampleSize;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "樣本數或矩陣維度無法成功轉換成整數", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (m - p <= 0)
                        {
                            MessageBox.Show(string.Format("樣本數不可低於{0}，請重新確認。", p), "", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        cmnd.AppendLine("invCdf 0.9973 k1;");
                        cmnd.AppendFormat("F {0} {1}.\r\n", p, m - p);
                        cmnd.AppendFormat("let k1={0}*({1}-1)*({1}+1)/{1}/({1}-{0})*k1\r\n", p, m);
                        try
                        {
                            Mtb.Worksheet ws = Project.Worksheets.Add(1);
                            string fpath = Mtblib.Tools.MtbTools.BuildTemporaryMacro("mymacro.mtb", cmnd.ToString());
                            Project.ExecuteCommand(string.Format("Exec \"{0}\" 1", fpath), ws);
                            double ucl = ws.Constants.Item("k1").GetData();
                            ws.Delete();
                            Project.Commands.Delete();
                            int decplace = (int)Math.Floor(Math.Log10(ucl));
                            string format;
                            if (decplace < 0)
                            {
                                decplace = Math.Abs(decplace) + 2;
                                format = string.Concat("{0:F", decplace, "}");
                            }
                            else
                            {
                                format = "{0:F3}";
                            }
                            UCLText = string.Format(format, ucl);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        switch ((UserActionMode)ActionMode)
                        {
                            case UserActionMode.Add:
                                try
                                {
                                    AddParaToDatabase(param);
                                    MessageBox.Show("已成功新增參數組", "", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                                break;

                            case UserActionMode.Edit:
                                try
                                {
                                    EditParaInDatabase(param);
                                    MessageBox.Show("已成功編輯參數組", "", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                                break;
                            default:
                            case UserActionMode.BatchAdd:
                                break;
                        }
                    });
            }
        }
        public ICommand CalcParaCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        double miss = Mtblib.Tools.MtbTools.MISSINGVALUE;
                        string start = string.Format("{0:yyyy-MM-dd} {1}", StartDate.Date, StartTimeValue);
                        string end = string.Format("{0:yyyy-MM-dd} {1}", EndDate.Date, EndTimeValue);
                        int a = DateTime.Compare(DateTime.Parse(start), DateTime.Parse(end));
                        if (a >= 0)
                        {
                            MessageBox.Show("起迄時間不正確，開始時間應該早於結束時間", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        //取資料
                        string[] itemList = SelectedFurnItems.Select(x => x.ItemList).ToArray();
                        DataTable data = Database.DBQueryTool.GetPivotDataForMultivariateChart(Site, itemList, start, end);
                        if (data == null || data.Rows.Count == 0)
                        {
                            MessageBox.Show("區段時間內查無資料", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        //取出量測值的部分，建立一個資料矩陣
                        string[] itemName = data.Columns.Cast<DataColumn>().Select(x => x.ColumnName)
                            .Where(x => x != "TIMESTAMP" && x != "CHART_PARA_INDEX").ToArray();
                        var subData = data.DefaultView.ToTable(false, itemName);

                        var M = LinearAlgebra.Matrix<double>.Build;
                        var subDataRowEnumerable = subData.Rows.Cast<DataRow>()
                            .Select(x => x.ItemArray.Select(y => y == DBNull.Value ? miss : Convert.ToDouble(y)));
                        LinearAlgebra.Matrix<double> dataMat = M.DenseOfRows(subDataRowEnumerable);

                        Model.TSquareLimCalculation tcalc = new Model.TSquareLimCalculation();
                        Model.TsquareParameters tpara;
                        try //計算參數組
                        {
                            tpara = tcalc.Execute(dataMat, Project);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        //將計算結果填入 MeanVector 和 Covariance matrix
                        if (MeanVector == null)
                        {
                            DataTable dt = new DataTable();
                            foreach (var item in itemName)
                            {
                                DataColumn dc = new DataColumn(item, typeof(string));
                                dt.Columns.Add(dc);
                            }
                        }

                        MeanVector.Clear();
                        var meanAsEnumerable = tpara.Mean.EnumerateRows().Select(x => x.Enumerate());
                        foreach (var row in meanAsEnumerable)
                        {
                            MeanVector.Rows.Add(row.Select(x => (object)x).ToArray());
                        }

                        if (CovarianceMatrix == null)
                        {
                            DataTable dt = new DataTable();
                            foreach (var item in itemName)
                            {
                                DataColumn dc = new DataColumn(item, typeof(string));
                                dt.Columns.Add(dc);
                            }
                        }
                        CovarianceMatrix.Clear();
                        var covAsEnumerable = tpara.Covariance.EnumerateRows().Select(x => x.Enumerate());
                        foreach (var row in covAsEnumerable)
                        {
                            CovarianceMatrix.Rows.Add(row.Select(x => (object)x).ToArray());
                        }

                        SampleSize = (int)tpara.SampleSize;
                    });
            }
        }
        public ICommand AddSelectedItemCommand
        {
            get
            {
                return new Command.RelayCommand(param =>
                {
                    if (FurnItemsSrc == null) return;
                    foreach (var item in FurnItemsSrc.Where(x => x.IsSelected))
                    {
                        if (!SelectedFurnItems.Contains(item)) SelectedFurnItems.Add(item);
                    }
                });
            }
        }
        public ICommand RemoveSelectedItemCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        for (int i = SelectedFurnItems.Count; i-- > 0;)
                        {
                            SPCItemInfo item = SelectedFurnItems[i];
                            if (item.IsSelected) SelectedFurnItems.RemoveAt(i);
                        }
                    });
            }
        }

        /// <summary>
        /// 描述一個多變量管制圖的參數組
        /// </summary>
        private struct ParaSetInfo
        {
            public DataTable Mean;
            public DataTable Cov;
            public int? SampleSize;
            public int SubgroupSize;
        }


    }
        /// <summary>
        /// 編輯頁面中常用的行為列舉
        /// </summary>
        public enum UserActionMode
        {
            Add,
            BatchAdd,
            Edit
        }



    
}
