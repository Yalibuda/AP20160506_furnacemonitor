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

namespace Dashboard.ViewModel
{
    public class SPCItemSettingViewModel : NotifyPropertyChanged
    {
        public SPCItemSettingViewModel()
        {
            Load();
        }
        /// <summary>
        /// 設定或取得廠別資訊
        /// </summary>
        public string SITE_ID
        {
            get { return _site; }
            set
            {
                _site = value;
                OnSiteChanged();
                RaisePropertyChanged("SITE_ID");
            }
        }
        private string _site = "";

        /// <summary>
        /// 取得選擇的SPCITEMINFO 資料表，包含 ITEM_LIST, FLAG, SITE, TITLE
        /// </summary>
        public DataTable SpcItemsInfoTable
        {
            get;
            set;
        }

        /// <summary>
        /// 取得可選擇熔爐項的SPCInfo
        /// </summary>
        public ObservableCollection<SPCItemInfo> AvailableItems
        {
            get { return _avliableItems; }
            private set
            {
                _avliableItems = value;
                RaisePropertyChanged("AvailableItems");
            }
        }
        private ObservableCollection<SPCItemInfo> _avliableItems = null;
        private void AvailableItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("AvailableItems");
        }

        /// <summary>
        /// 取得已選擇熔爐項的SPCInfo，用於單變量管制圖
        /// </summary>
        public ObservableCollection<SPCItemInfo> SelectedItems
        {
            get { return _selItems; }
            private set
            {
                _selItems = value;
                RaisePropertyChanged("SelectedItems");
            }
        }
        private ObservableCollection<SPCItemInfo> _selItems = null;
        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("SelectedItems");
        }

        /// <summary>
        /// 取得已設定好的多變量管制圖資訊
        /// </summary>
        public ObservableCollection<SPCItemInfo> SelectedMulItems
        {
            get { return _selMulItems; }
            private set
            {
                _selMulItems = value;
                RaisePropertyChanged("SelectedMulItems");
            }
        }
        private ObservableCollection<SPCItemInfo> _selMulItems = null;
        private void SelectedMulItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("SelectedMulItems");
        }
        
        protected void Load()
        {
            SelectedMulItems = new ObservableCollection<SPCItemInfo>();
            SelectedMulItems.CollectionChanged += SelectedMulItems_CollectionChanged;
            AvailableItems = new ObservableCollection<SPCItemInfo>();
            AvailableItems.CollectionChanged += AvailableItems_CollectionChanged;
            SelectedItems = new ObservableCollection<SPCItemInfo>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        protected virtual void OnSiteChanged()
        {
            if (SITE_ID == null || SITE_ID == string.Empty) return;
            //重新讀取可選擇項
            SelectedMulItems.Clear();
            AvailableItems.Clear();
            SelectedItems.Clear();

            DataTable furnItemTable = Database.DBQueryTool.GetFurnItemInfo(SITE_ID);
            if (furnItemTable != null && furnItemTable.Rows.Count > 0)
            {
                foreach (DataRow item in furnItemTable.Rows.Cast<DataRow>())
                {
                    SPCItemInfo info = new SPCItemInfo()
                    {
                        ItemList = item["FURN_ITEM_INDEX"].ToString(),
                        Flag = "I",
                        Description = item["ITEM_NAME"].ToString(),
                        Title = item["ITEM_NAME"].ToString()
                    };
                    AvailableItems.Add(info);
                }
            }

            //載入資料庫中的紀錄
            SpcItemsInfoTable = Database.DBQueryTool.GetRealTimeSPCItems(SITE_ID);
            if (SpcItemsInfoTable != null && SpcItemsInfoTable.Rows.Count > 0)
            {
                //取得 I-Chart 的項目
                try
                {
                    var univariateItems = SpcItemsInfoTable.Rows.Cast<DataRow>().Where(x => x.Field<string>("FLAG") == "I").Select(x => x["ITEM_LIST"]); //取出單變量項目
                    foreach (var item in AvailableItems)
                    {
                        if (univariateItems.Contains(item.ItemList))
                        {
                            SelectedItems.Add(item);
                        }
                    }
                }
                catch
                {
                    //do nothing
                }
                //取的 T2 的項目
                try
                {
                    var selMulItems = SpcItemsInfoTable.Rows.Cast<DataRow>().Where(x => x.Field<string>("FLAG") == "T2"); //取出多變量項目
                    foreach (var item in selMulItems)
                    {
                        SPCItemInfo spcItem = new SPCItemInfo();
                        spcItem.ItemList = item["ITEM_LIST"].ToString();
                        spcItem.Title = item["TITLE"].ToString();
                        spcItem.Flag = item["FLAG"].ToString();
                        spcItem.Description = string.Join(",", Database.DBQueryTool.GetFurnNameByItemList(spcItem.ItemList));
                        SelectedMulItems.Add(spcItem);
                    }
                }
                catch
                {
                    //do nothing
                }
            }


        }

        private int Save()
        {
            //將SPCItemInfo 轉成資料表
            DataTable uploadTable = new DataTable();
            uploadTable.Columns.Add("ITEM_LIST", typeof(string));
            uploadTable.Columns.Add("FLAG", typeof(string));
            uploadTable.Columns.Add("SITE", typeof(string));
            uploadTable.Columns.Add("TITLE", typeof(string));

            DataRow dr;
            //加入單變量項目
            foreach (var item in SelectedItems)
            {
                dr = uploadTable.NewRow();
                dr["ITEM_LIST"] = item.ItemList;
                dr["FLAG"] = "I";
                dr["SITE"] = SITE_ID;
                dr["TITLE"] = item.Title;
                uploadTable.Rows.Add(dr);
            }
            //加入多變量項目
            foreach (var item in SelectedMulItems)
            {
                dr = uploadTable.NewRow();
                dr["ITEM_LIST"] = item.ItemList;
                dr["FLAG"] = "T2";
                dr["SITE"] = SITE_ID;
                dr["TITLE"] = item.Title;
                uploadTable.Rows.Add(dr);
            }
            //新增不存在於現有資料庫內的資料
            using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand sqlCmnd = new SqlCommand("TRUNCATE TABLE UPLOAD_SPC_ITEM_INFO", conn))
                    {
                        //先清除暫存表上的資料
                        sqlCmnd.ExecuteNonQuery();
                    }
                    using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                    {
                        sqlBC.BatchSize = 1000;
                        sqlBC.BulkCopyTimeout = 6000;

                        //設定要寫入的資料庫
                        sqlBC.DestinationTableName = "UPLOAD_SPC_ITEM_INFO";

                        //對應資料行                            
                        sqlBC.ColumnMappings.Add("ITEM_LIST", "ITEM_LIST");
                        sqlBC.ColumnMappings.Add("FLAG", "FLAG");
                        sqlBC.ColumnMappings.Add("SITE", "SITE");
                        sqlBC.ColumnMappings.Add("TITLE", "TITLE");


                        //開始寫入
                        sqlBC.WriteToServer(uploadTable);
                    }
                    using (SqlCommand sqlCmnd = new SqlCommand())
                    {
                        StringBuilder query = new StringBuilder();
                        //刪除項目
                        query.AppendLine("DELETE FROM SPC_ITEM_INFO FROM (");
                        query.AppendLine("SELECT ITEM_LIST,FLAG FROM SPC_ITEM_INFO");
                        query.AppendFormat("WHERE SITE='{0}'\r\n",SITE_ID);
                        query.AppendLine("EXCEPT");
                        query.AppendLine("SELECT ITEM_LIST,FLAG FROM UPLOAD_SPC_ITEM_INFO) a");
                        query.AppendLine("WHERE SPC_ITEM_INFO.ITEM_LIST = a.ITEM_LIST AND SPC_ITEM_INFO.FLAG = a.FLAG");

                        //新增項目
                        query.AppendLine("INSERT INTO SPC_ITEM_INFO(ITEM_LIST, FLAG, SITE)");
                        query.AppendLine("SELECT ITEM_LIST,FLAG,SITE FROM UPLOAD_SPC_ITEM_INFO");
                        query.AppendLine("EXCEPT");
                        query.AppendLine("SELECT ITEM_LIST,FLAG,SITE FROM SPC_ITEM_INFO");

                        //針對標題有更新的項目做變更
                        query.AppendLine("UPDATE SPC_ITEM_INFO");
                        query.AppendLine("SET TITLE=a.TITLE FROM (");
                        query.AppendLine("SELECT ITEM_LIST,FLAG,TITLE FROM UPLOAD_SPC_ITEM_INFO");
                        query.AppendLine("EXCEPT");
                        query.AppendLine("SELECT ITEM_LIST,FLAG,TITLE FROM SPC_ITEM_INFO");
                        query.AppendLine(") a");
                        query.AppendLine("WHERE a.ITEM_LIST = SPC_ITEM_INFO.ITEM_LIST");

                        sqlCmnd.Connection = conn;
                        sqlCmnd.CommandText = query.ToString();
                        int r = sqlCmnd.ExecuteNonQuery();
                        return r;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("上傳時發生錯誤:\r\n{0}", ex.Message));
                }
            }
        }

        public ICommand AddSelectedItemCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        IEnumerable<SPCItemInfo> itemsToAdd = AvailableItems.Where(x => x.IsSelected);
                        foreach (var item in itemsToAdd)
                        {
                            if (!SelectedItems.Contains(item)) SelectedItems.Add(item);
                        }
                    }
                    );
            }
        }
        public ICommand RemoveSelectedItemCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        for (int i = SelectedItems.Count; i-- > 0; )
                        {
                            SPCItemInfo item = SelectedItems[i];
                            if (item.IsSelected) SelectedItems.RemoveAt(i);
                        }
                    }
                    );
            }
        }
        public ICommand AddNewMultiSPCItemCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        SPCItemInfo item = new SPCItemInfo();
                        item.Flag = "T2";
                        int id = 0;
                        if (SelectedMulItems != null && SelectedMulItems.Count > 0)
                        {
                            var alltitles = SelectedMulItems.Select(x => x.Title).Distinct().ToArray();
                            string title = "多變量管制圖";
                            while (alltitles.Contains(title))
                            {
                                id++;
                                title = string.Format("{0}_{1}", "多變量管制圖", id);
                            }
                            item.Title = title;
                        }
                        else
                        {
                            item.Title = "多變量管制圖";
                        }
                        SelectedMulItems.Add(item);
                    });
            }


        }
        public ICommand DeleteMulItemCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        SPCItemInfo spcItem = param as SPCItemInfo;
                        if (spcItem != null)
                        {
                            for (int i = SelectedMulItems.Count; i-- > 0; )
                            {
                                if (object.Equals(SelectedMulItems[i], spcItem))
                                {
                                    SelectedMulItems.RemoveAt(i);
                                }
                            }
                        }
                    });
            }
        }
        public ICommand ShowSelMulItemDialogCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        SPCItemInfo spcItem = param as SPCItemInfo;
                        if (spcItem != null)
                        {
                            #region 將 SPCItemInfo 轉換成 ViewModel
                            SelectMulItemViewModel vm = new SelectMulItemViewModel();
                            vm.SITE_ID = this.SITE_ID;
                            vm.Title = spcItem.Title;
                            if (spcItem.ItemList != null && spcItem.ItemList != string.Empty)
                            {
                                string[] item_list = spcItem.ItemList.Split(',');
                                DataTable dt = new DataTable();
                                dt.Columns.Add("ITEM_LIST", typeof(string));
                                dt.Columns.Add("FLAG", typeof(string));
                                dt.Columns.Add("SITE", typeof(string));
                                dt.Columns.Add("TITLE", typeof(string));
                                DataRow dr;
                                foreach (var item in item_list)
                                {
                                    dr = dt.NewRow();
                                    dr["ITEM_LIST"] = item;
                                    dr["FLAG"] = "I";
                                    dr["SITE"] = this.SITE_ID;
                                    dt.Rows.Add(dr);
                                }
                                vm.SPCItemInfoTable = dt;
                            } 
                            #endregion

                            using (View.SelectMultivariateItemDialogView f = new View.SelectMultivariateItemDialogView(vm))
                            {
                                f.ShowInTaskbar = true;
                                f.ShowDialog();
                                SelectMulItemViewModel resultVm = (SelectMulItemViewModel)f.DataContext;
                                if (resultVm.SPCItemInfoTable != null && resultVm.SPCItemInfoTable.Rows.Count > 0)
                                {
                                    // probably need to sort the item list...
                                    spcItem.ItemList = string.Join(",", resultVm.SPCItemInfoTable.Rows.Cast<DataRow>()
                                        .Select(x => x.Field<string>("ITEM_LIST")).ToArray());
                                    spcItem.Title = resultVm.Title;
                                    spcItem.Description = string.Join(",", Database.DBQueryTool.GetFurnNameByItemList(spcItem.ItemList));
                                }
                                RaisePropertyChanged("SelectedMulItems");
                            }
                        }
                    }
                    );
            }
        }
        public ICommand SaveCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {                        
                        try
                        {
                            //檢查多變量項目資料否完備
                            foreach (var item in SelectedMulItems)
                            {
                                if (item.ItemList == null || item.ItemList == string.Empty) 
                                    throw new Exception(string.Format("{0}未設置熔爐項目",item.Title));
                            }
                            int effectedRows = Save();
                            MessageBox.Show(string.Format("更新即時項目完成，共{0}項", effectedRows)
                                , "", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        
                    }
                    );
            }
        }
    }
}
