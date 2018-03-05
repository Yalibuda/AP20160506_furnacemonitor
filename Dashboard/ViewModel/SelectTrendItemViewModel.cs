using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dashboard.ViewModel
{
    public class SelectTrendItemViewModel : NotifyPropertyChanged
    {
        public SelectTrendItemViewModel()
        {
            Load();
        }

        #region 屬性
        /// <summary>
        /// 設定或取得廠別資訊
        /// </summary>
        public string SITE_ID { get; set; }
        private string _site = "";

        /// <summary>
        /// 取得已選取的多變量管制圖項目資訊
        /// </summary>
        public ObservableCollection<SPCItemInfo> SelectedMultivariateSPCItems
        {
            get { return _uiSelMulVarSPCItems; }
            protected set
            {
                _uiSelMulVarSPCItems = value;
                RaisePropertyChanged("SelectedMultivariateSPCItems");
            }
        }
        /// <summary>
        /// 取得可用於選取的熔爐單變量管制圖資訊
        /// </summary>
        public ObservableCollection<SPCItemInfo> AvailableUnivariateSPCItems
        {
            get { return _availableUnivariateSPCItems; }
            protected set
            {
                _availableUnivariateSPCItems = value;
                RaisePropertyChanged("AvailableUnivariateSPCItems");
            }
        }
        /// <summary>
        /// 取得已選取的多變量管制圖項目
        /// </summary>
        public ObservableCollection<SPCItemInfo> SelectedUnivariateSPCItems
        {
            get { return _uiSelUniVarSPCItems; }
            protected set
            {
                _uiSelUniVarSPCItems = value;
                RaisePropertyChanged("SelectedUnivariateSPCItems");
            }
        }

        /// <summary>
        /// 取得從 Available 項中被選擇的項目
        /// </summary>
        public IEnumerable<SPCItemInfo> ItemToAddList
        {
            get { return AvailableUnivariateSPCItems.Where(x => x.IsSelected); }
        }

        /// <summary>
        /// 取得從 Selected 項中被選擇的項目
        /// </summary>
        public IEnumerable<SPCItemInfo> ItemToRemoveList
        {
            get { return SelectedUnivariateSPCItems.Where(x => x.IsSelected); }
        }

        /// <summary>
        /// 取得選擇的SPCITEMINFO 資料表，包含 ITEM_LIST, FLAG, SITE, TITLE
        /// </summary>
        public DataTable SPCItemInfoTable
        {
            get
            {
                return _spcItemInfoTable;
            }
            set
            {
                _spcItemInfoTable = value;
            }
        }
        


        #endregion

        #region 方法
        protected virtual void Load()
        {
            if (SITE_ID == null || SITE_ID == string.Empty) return;

            SelectedMultivariateSPCItems = new ObservableCollection<SPCItemInfo>();
            SelectedMultivariateSPCItems.CollectionChanged += SelectedMultivariateSPCItems_CollectionChanged;
            AvailableUnivariateSPCItems = new ObservableCollection<SPCItemInfo>();
            AvailableUnivariateSPCItems.CollectionChanged += AvailableUnivariateSPCItems_CollectionChanged;
            SelectedUnivariateSPCItems = new ObservableCollection<SPCItemInfo>();
            SelectedUnivariateSPCItems.CollectionChanged += SelectedUnivariateSPCItme_CollectionChanged;

            DataTable availableItemsDataTable = Database.DBQueryTool.GetFurnItemInfo(SITE_ID);
            if (availableItemsDataTable != null && availableItemsDataTable.Rows.Count > 0)
            {
                foreach (DataRow item in availableItemsDataTable.Rows.Cast<DataRow>())
                {
                    SPCItemInfo info = new SPCItemInfo()
                    {
                        ItemList = item["FURN_ITEM_INDEX"].ToString(),
                        Flag = "I",
                        Description = item["ITEM_NAME"].ToString(),
                        Title = item["ITEM_NAME"].ToString()
                    };
                    AvailableUnivariateSPCItems.Add(info);
                }
            }

            /*
             * 從 SPCItemInfoTable 還原各設定
             * 
             */
            if (SPCItemInfoTable != null && SPCItemInfoTable.Rows.Count > 0)
            {
                try
                {
                    var selUniItemList = SPCItemInfoTable.Rows.Cast<DataRow>().Where(x => x.Field<string>("FLAG") == "I").Select(x => x["ITEM_LIST"]); //取出單變量項目
                    foreach (var item in AvailableUnivariateSPCItems)
                    {
                        if (selUniItemList.Contains(item.ItemList))
                        {
                            SelectedUnivariateSPCItems.Add(item);
                        }
                    }
                }
                catch
                {
                    //do nothing
                }

                try
                {
                    var selMulItems = SPCItemInfoTable.Rows.Cast<DataRow>().Where(x => x.Field<string>("FLAG") == "T2"); //取出多變量項目
                    foreach (var item in selMulItems)
                    {
                        SPCItemInfo spcItem = new SPCItemInfo();
                        spcItem.ItemList = item["ITEM_LIST"].ToString();
                        spcItem.Title = item["TITLE"].ToString();
                        spcItem.Flag = item["FLAG"].ToString();
                        spcItem.Description = string.Join(",", Database.DBQueryTool.GetFurnNameByItemList(spcItem.ItemList));
                        SelectedMultivariateSPCItems.Add(spcItem);
                    }
                }
                catch
                {
                    //do nothing
                }

            }


        }

        public void Initialize()
        {
            Load();
        }

        private void AddSelectedItem(object obj)
        {
            foreach (var item in ItemToAddList)
            {

                //if (SelectedUnivariateSPCItems.Count() > 0)
                //{
                //    var a = SelectedUnivariateSPCItems.Select(x => x.ItemList);
                //    a = a as IEnumerable<string>;
                //    if (!a.Contains(item.ItemList))
                //    {
                //        SelectedUnivariateSPCItems.Add(item);
                //    }
                //}
                //else
                //{
                //    SelectedUnivariateSPCItems.Add(item);
                //}
                if (!SelectedUnivariateSPCItems.Contains(item)) SelectedUnivariateSPCItems.Add(item); // original one

            }
        }

        private void RemoveSelectedItem(object obj)
        {
            for (int i = SelectedUnivariateSPCItems.Count; i-- > 0;)
            {
                SPCItemInfo item = SelectedUnivariateSPCItems[i];
                if (item.IsSelected) SelectedUnivariateSPCItems.RemoveAt(i);
            }
        }

        private void AddNewMultiSPCItem(object obj)
        {
            SPCItemInfo item = new SPCItemInfo();
            item.Flag = "T2";
            int id = 0;
            if (SelectedMultivariateSPCItems != null && SelectedMultivariateSPCItems.Count > 0)
            {
                var alltitles = SelectedMultivariateSPCItems.Select(x => x.Title).Distinct().ToArray();
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
            SelectedMultivariateSPCItems.Add(item);
        }

        protected virtual void Save()
        {
            Console.WriteLine("Closing and saving");
            DataTable dt = new DataTable();
            dt.Columns.Add("ITEM_LIST", typeof(string));
            dt.Columns.Add("FLAG", typeof(string));
            dt.Columns.Add("SITE", typeof(string));
            dt.Columns.Add("TITLE", typeof(string));

            DataRow dr;
            if (SelectedMultivariateSPCItems != null)
            {

                foreach (SPCItemInfo item in SelectedMultivariateSPCItems)
                {
                    if (item.ItemList == null || item.ItemList == string.Empty)
                    {
                        System.Windows.MessageBox.Show(string.Format("請設定[{0}]中的項目", item.Title), 
                            "", 
                            System.Windows.MessageBoxButton.OK, 
                            System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                    dr = dt.NewRow();
                    dr["ITEM_LIST"] = item.ItemList;
                    dr["FLAG"] = item.Flag;
                    dr["SITE"] = SITE_ID;
                    dr["TITLE"] = item.Title;
                    dt.Rows.Add(dr);
                }
            }


            if (SelectedUnivariateSPCItems != null)
            {
                foreach (SPCItemInfo item in SelectedUnivariateSPCItems)
                {
                    dr = dt.NewRow();
                    dr["ITEM_LIST"] = item.IntItemList;
                    dr["FLAG"] = item.Flag;
                    dr["SITE"] = SITE_ID;
                    dr["TITLE"] = item.Title;
                    dt.Rows.Add(dr);
                }
            }

            SPCItemInfoTable = dt;

            //清除UI結果後關閉
            SelectedMultivariateSPCItems.Clear();
            SelectedUnivariateSPCItems.Clear();
            CloseAction();
        }

        protected virtual bool CanSave()
        {
            //要檢查多變量選擇項是否都合法
            return true;
        }

        private void ShowSelMulItemDialog(object obj)
        {
            SPCItemInfo spcItem = obj as SPCItemInfo;
            if (spcItem != null)
            {
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
                    RaisePropertyChanged("SelectedMultivariateSPCItems");
                }
            }
        }

        private void DeleteMulItem(object obj)
        {
            SPCItemInfo spcItem = obj as SPCItemInfo;
            if (spcItem != null)
            {
                for (int i = SelectedMultivariateSPCItems.Count; i-- > 0; )
                {
                    if (object.Equals(SelectedMultivariateSPCItems[i], spcItem))
                    {
                        SelectedMultivariateSPCItems.RemoveAt(i);
                    }
                }
            }
        }


        #endregion

        #region 事件方法
        private void SelectedUnivariateSPCItme_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("SelectedUnivariateSPCItme");
        }

        private void AvailableUnivariateSPCItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("AvailableUnivariateSPCItems");
        }

        private void SelectedMultivariateSPCItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("SelectedMultivariateSPCItems");
        }

        /// <summary>
        /// 處理滑鼠左鍵連點在可選/已選項
        /// </summary>
        public void AddClickedItem(object obj)
        {
            System.Windows.Controls.TextBlock itemTextBlock = obj as System.Windows.Controls.TextBlock;
            if (itemTextBlock == null) return;
            DataTable availableItemsDataTable = Database.DBQueryTool.GetFurnItemInfo(SITE_ID);

            foreach (DataRow row in availableItemsDataTable.Rows.Cast<DataRow>())
            {
                if (row["ITEM_NAME"].ToString() == itemTextBlock.Text)
                {
                    SPCItemInfo info = new SPCItemInfo()
                    {
                        ItemList = row["FURN_ITEM_INDEX"].ToString(),
                        Flag = "I",
                        Description = row["ITEM_NAME"].ToString(),
                        Title = row["ITEM_NAME"].ToString()
                    };
                    if (SelectedUnivariateSPCItems.Count() > 0)
                    {
                        var a = SelectedUnivariateSPCItems.Select(x => x.ItemList == info.ItemList);
                        a = a as IEnumerable<bool>;
                        if (!a.Contains(true))
                        {
                            SelectedUnivariateSPCItems.Add(info);
                            return;
                        }
                    }
                    else
                    {
                        SelectedUnivariateSPCItems.Add(info);
                        return;
                    }
                }
            }
        }

        public void RemoveClickedItem(object obj)
        {
            System.Windows.Controls.TextBlock itemTextBlock = obj as System.Windows.Controls.TextBlock;
            DataTable availableItemsDataTable = Database.DBQueryTool.GetFurnItemInfo(SITE_ID);
            foreach (DataRow row in availableItemsDataTable.Rows.Cast<DataRow>())
            {
                if (row["ITEM_NAME"].ToString() == itemTextBlock.Text)
                {
                    SPCItemInfo info = new SPCItemInfo()
                    {
                        ItemList = row["FURN_ITEM_INDEX"].ToString(),
                        Flag = "I",
                        Description = row["ITEM_NAME"].ToString(),
                        Title = row["ITEM_NAME"].ToString()
                    };

                    for (int i = SelectedUnivariateSPCItems.Count; i-- > 0;)
                    {
                        SPCItemInfo item = SelectedUnivariateSPCItems[i];
                        if (item.ItemList == info.ItemList)
                        {
                            SelectedUnivariateSPCItems.RemoveAt(i);
                            return;
                        }
                    }
                }
            }

        }
        #endregion

        #region 變數
        private ObservableCollection<SPCItemInfo> _uiSelMulVarSPCItems = null; //用於反映UI上的選擇結果
        private ObservableCollection<SPCItemInfo> _availableUnivariateSPCItems = null;
        private ObservableCollection<SPCItemInfo> _uiSelUniVarSPCItems = null; //用於反映UI上的選擇結果
        private DataTable _spcItemInfoTable = null;
        #endregion

        #region Command
        /// <summary>
        /// 執行增加項目至單變量管制圖選擇項中
        /// </summary>
        public ICommand AddSelectedItemCommand
        {
            get { return new Command.RelayCommand(AddSelectedItem); }
        }
        /// <summary>
        /// 執行從單變量管制圖選擇項中移除項目
        /// </summary>
        public ICommand RemoveSelectedItemCommand
        {
            get { return new Command.RelayCommand(RemoveSelectedItem); }
        }
        /// <summary>
        /// 執行新增多變量管制圖項目
        /// </summary>
        public ICommand AddNewMultiSPCItemCommand
        {
            get { return new Command.RelayCommand(AddNewMultiSPCItem); }
        }
        /// <summary>
        /// 執行儲存設定結果
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param => Save(),
                    param => CanSave()
                    );
            }
        }
        /// <summary>
        /// 執行顯示編輯多變量管制圖對話框
        /// </summary>
        public ICommand ShowSelMulItemDialogCommand
        {
            get { return new Command.RelayCommand(ShowSelMulItemDialog); }
        }
        /// <summary>
        /// 執行刪除多變量管制圖項目
        /// </summary>
        public ICommand DeleteMulItemCommand
        {
            get
            {
                return new Command.RelayCommand(DeleteMulItem);
            }
        }
        #endregion


    }

}
