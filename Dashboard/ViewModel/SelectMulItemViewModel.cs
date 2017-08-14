using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.ViewModel
{
    /// <summary>
    /// 此類別透過繼承 SelectTrendItemViewModel 和 Override 部分方法，在階層邏輯(選擇項目--&gt; 選擇多變量管制圖項目)
    /// 下達到編輯每一個多變量管制圖項目的目的
    /// </summary>
    public class SelectMulItemViewModel : SelectTrendItemViewModel
    {

        public SelectMulItemViewModel()
        {

        }

        /// <summary>
        /// 覆寫 Save 方法，把原本 Univariate 選項轉為 Multivariate 項目
        /// </summary>
        protected override void Save()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ITEM_LIST", typeof(string));
            dt.Columns.Add("FLAG", typeof(string));
            dt.Columns.Add("SITE", typeof(string));
            dt.Columns.Add("TITLE", typeof(string));

            DataRow dr;

            if (SelectedUnivariateSPCItems != null && SelectedUnivariateSPCItems.Count > 1)
            {
                foreach (SPCItemInfo item in SelectedUnivariateSPCItems)
                {
                    dr = dt.NewRow();
                    dr["ITEM_LIST"] = item.IntItemList;
                    dr["FLAG"] = "T2";
                    dr["SITE"] = SITE_ID;
                    dr["TITLE"] = item.Title;
                    dt.Rows.Add(dr);
                }
                dt = dt.Rows.Cast<DataRow>().OrderBy(x => Convert.ToInt16(x["ITEM_LIST"])).CopyToDataTable();
            }
            else
            {
                System.Windows.MessageBox.Show("請輸入至少二項變數做為多變量管制圖", "",
                  System.Windows.MessageBoxButton.OK,
                  System.Windows.MessageBoxImage.Warning);
                return;
            }
            SPCItemInfoTable = dt;

            //清除UI結果後關閉
            SelectedMultivariateSPCItems.Clear();
            SelectedUnivariateSPCItems.Clear();
            CloseAction();

        }

        public string Title { get; set; }

    }
}
