using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.ViewModel
{
    /// <summary>
    /// 熔爐管制圖的資訊
    /// </summary>
    public class SPCItemInfo : NotifyPropertyChanged, Model.ISeletableItem
    {
        /// <summary>
        /// 取得熔爐項目INDEX(1或多個)的字串
        /// </summary>
        public string ItemList
        {
            get { return _itemList; }
            set { _itemList = value; RaisePropertyChanged("ItemList"); }
        }
        private string _itemList;

        /// <summary>
        /// 取得熔爐項目INDEX 的數值陣列
        /// </summary>
        public int IntItemList
        {
            get
            {
                int convertedInt;
                if (int.TryParse(ItemList, out convertedInt))
                {
                    return convertedInt;
                }
                else
                {
                    return -1;
                }

            }
        }

        /// <summary>
        /// 管制圖類型，例如 I 或 T2
        /// </summary>
        public string Flag
        {
            get { return _flag; }
            set { _flag = value; RaisePropertyChanged("Flag"); }
        }
        private string _flag;

        /// <summary>
        /// 取得管制圖的標題
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged("Title"); }
        }
        private string _title;

        /// <summary>
        /// 取得管制圖的描述文字
        /// </summary>
        public string Description
        {
            get { return _desc; }
            set { _desc = value; RaisePropertyChanged("Description"); }
        }
        private string _desc;

        /// <summary>
        /// 取得或指定管制圖項目是否被選取
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
        private bool _isSelected = false;
    }
}
