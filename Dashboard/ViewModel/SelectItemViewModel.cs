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
    public class SelectItemViewModel: NotifyPropertyChanged
    {
        public SelectItemViewModel()
        {
            Load();
        }

        #region 屬性
       
        /// <summary>
        /// 取得可用於選取的項目資訊
        /// </summary>
        public ObservableCollection<ItemViewModel> AvailableItemsInListBox
        {
            get { return _availableItems; }
            protected set
            {
                _availableItems = value;
                RaisePropertyChanged("AvailableItemsInListBox");
            }
        }
        /// <summary>
        /// 取得已選取的項目
        /// </summary>
        public ObservableCollection<ItemViewModel> SelectedItemsInListBox
        {
            get { return _selectedItems; }
            protected set
            {
                _selectedItems = value;
                RaisePropertyChanged("SelectedItemsInListBox");
            }
        }

        /// <summary>
        /// 取得已選取的項目(字典)
        /// </summary>
        public Dictionary<int, string> SelectedItemList
        {
            get { return _selectedItemList; }
            protected set
            {
                _selectedItemList = value;
                RaisePropertyChanged("SelectedItemList");
            }
        }


        /// <summary>
        /// 取得可用的的項目(字典)
        /// </summary>
        public Dictionary<int, string> AvailableItemList
        {
            get { return _availableItemList; }
            set
            {
                _availableItemList = value;
                RaisePropertyChanged("SelectedItemList");
                //更新 AvailableItemInListBox 內容
                AvailableItemsInListBox.Clear();
                ItemViewModel itemViewModel; 
                foreach (KeyValuePair<int,string> item in _availableItemList)
                {
                    itemViewModel = new ItemViewModel();
                    itemViewModel.Item = item;
                    itemViewModel.IsSelected = false;
                    AvailableItemsInListBox.Add(itemViewModel);
                }
            }
        }

        /// <summary>
        /// 取得從 Available 項中被選擇的項目
        /// </summary>
        public IEnumerable<ItemViewModel> ItemToAddList
        {
            get { return AvailableItemsInListBox.Where(x => x.IsSelected); }
        }

        /// <summary>
        /// 取得從 Selected 項中被選擇的項目
        /// </summary>
        public IEnumerable<ItemViewModel> ItemToRemoveList
        {
            get { return SelectedItemsInListBox.Where(x => x.IsSelected); }
        }


        #endregion

        #region 方法
        protected virtual void Load()
        {
            SelectedItemsInListBox = new ObservableCollection<ItemViewModel>();
            SelectedItemsInListBox.CollectionChanged+=SelectedItems_CollectionChanged;
            AvailableItemsInListBox = new ObservableCollection<ItemViewModel>();
            AvailableItemsInListBox.CollectionChanged+=AvailableItems_CollectionChanged;

            if (AvailableItemList != null && AvailableItemList.Count > 0)
            {
                ItemViewModel itemViewModel;
                foreach (var item in AvailableItemList)
                {
                    itemViewModel = new ItemViewModel();
                    itemViewModel.Item = item;
                    itemViewModel.IsSelected = false;
                    AvailableItemsInListBox.Add(itemViewModel);
                }
            }


            /*
             * 從 SelectedItemList 還原各設定
             * 
             */
            if (SelectedItemList != null && SelectedItemList.Count > 0)
            {
                try
                {                    
                    foreach (var item in AvailableItemsInListBox)
                    {
                        if (SelectedItemList.Contains((KeyValuePair<int,string>)item.Item))
                        {
                            SelectedItemsInListBox.Add(item);
                        }
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
                if (!SelectedItemsInListBox.Contains(item)) SelectedItemsInListBox.Add(item);
            }
        }

        private void RemoveSelectedItem(object obj)
        {
            for (int i = SelectedItemsInListBox.Count; i-- > 0; )
            {
                ItemViewModel item = SelectedItemsInListBox[i];
                if (item.IsSelected) SelectedItemsInListBox.RemoveAt(i);
            }
        }        

        protected virtual void Save()
        {           
            //儲存最後結果
            if (SelectedItemsInListBox != null && SelectedItemsInListBox.Count > 0)
            {
                SelectedItemList = SelectedItemsInListBox.Select(x => (KeyValuePair<int, string>)x.Item).ToDictionary(x => x.Key, x => x.Value);
            }            

            //清除UI結果後關閉            
            SelectedItemsInListBox.Clear();
            CloseAction();
        }

        protected virtual bool CanSave()
        {
            //要檢查多變量選擇項是否都合法
            return true;
        }     


        #endregion

        #region 事件方法
        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("SelectedItems");
        }

        private void AvailableItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("AvailableItems");
        }

        #endregion

        #region 變數        
        private ObservableCollection<ItemViewModel> _availableItems = null;
        private ObservableCollection<ItemViewModel> _selectedItems = null;
        private Dictionary<int, string> _selectedItemList = null;
        private Dictionary<int, string> _availableItemList = null;
        
        #endregion

        #region Command
        /// <summary>
        /// 執行增加項目至已選擇項中
        /// </summary>
        public ICommand AddSelectedItemCommand
        {
            get { return new Command.RelayCommand(AddSelectedItem); }
        }
        /// <summary>
        /// 執行從已選擇項中移除項目
        /// </summary>
        public ICommand RemoveSelectedItemCommand
        {
            get { return new Command.RelayCommand(RemoveSelectedItem); }
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
        
        #endregion
    }

    public class ItemViewModel
    {
        public object Item { set; get; }

        public bool IsSelected { set; get; }
    }
}
