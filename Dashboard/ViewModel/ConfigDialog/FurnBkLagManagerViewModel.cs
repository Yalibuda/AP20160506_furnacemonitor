using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dashboard.ViewModel.ConfigDialog
{
    public class FurnBkLagManagerViewModel : NotifyPropertyChanged
    {
        public FurnBkLagManagerViewModel()
        {

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
        /// 取得熔爐項與拉絲製程的時間
        /// </summary>
        public DataTable FurnBkLagInfo
        {
            get { return _furnBkLagInfo; }
            private set
            {
                _furnBkLagInfo = value;
                RaisePropertyChanged("FurnBkLagInfo");
            }
        }
        private DataTable _furnBkLagInfo = null;


        private void OnSiteChanged()
        {
            if (Site == null || Site == string.Empty) return;
            //if (FurnItemsSrc == null) FurnItemsSrc = new ObservableCollection<SPCItemInfo>();
            if (FurnBkLagInfo == null)
            {
                FurnBkLagInfo = new DataTable();

            }
            FurnBkLagInfo.Clear();
            StringBuilder query = new StringBuilder();
            query.AppendLine("select a.TAG_NAME,a.FURN_ITEM_INDEX, a.ITEM_NAME, LAGHOUR = ISNULL(b.LAGHOUR,0) from FURN_ITEM_INFO a ");
            query.AppendLine("left join FURN_BK_LAG_INFO b on a.FURN_ITEM_INDEX = b.FURN_ITEM_INDEX");
            query.AppendFormat("where a.SITE_ID = '{0}'", Site);
            FurnBkLagInfo = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString());
        }

        public ICommand UpdateCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        //判斷上傳資料內容是否都合法，才能上傳
                        DataGrid dg = param as DataGrid;
                        var errors = (from c in
                                          (from object i in dg.ItemsSource
                                           select dg.ItemContainerGenerator.ContainerFromItem(i))
                                      where c != null
                                      select System.Windows.Controls.Validation.GetHasError(c)).FirstOrDefault(x => x);
                        if (errors)
                        {
                            MessageBox.Show("延遲時間必須為非負整數值", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        try
                        {
                            using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
                            {
                                conn.Open();
                                using (SqlCommand sqlCmnd = new SqlCommand("TRUNCATE TABLE UPLOAD_FURN_BK_LAG_INFO", conn))
                                {
                                    //先清除暫存表上的資料
                                    sqlCmnd.ExecuteNonQuery();
                                }

                                using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                                {
                                    sqlBC.BatchSize = 1000;
                                    sqlBC.BulkCopyTimeout = 6000;

                                    //設定要寫入的資料庫
                                    sqlBC.DestinationTableName = "UPLOAD_FURN_BK_LAG_INFO";

                                    //對應資料行                           
                                    sqlBC.ColumnMappings.Add("TAG_NAME", "TAG_NAME");
                                    sqlBC.ColumnMappings.Add("FURN_ITEM_INDEX", "FURN_ITEM_INDEX");
                                    sqlBC.ColumnMappings.Add("ITEM_NAME", "ITEM_NAME");
                                    sqlBC.ColumnMappings.Add("LAGHOUR", "LAGHOUR");

                                    //開始寫入
                                    sqlBC.WriteToServer(FurnBkLagInfo);
                                }

                                int effectedRows = 0;
                                using (SqlCommand sqlCmnd = new SqlCommand())
                                {
                                    StringBuilder query = new StringBuilder();
                                    query.AppendLine("insert FURN_BK_LAG_INFO (FURN_ITEM_INDEX, LAGHOUR)");
                                    query.AppendLine("select FURN_ITEM_INDEX, LAGHOUR from UPLOAD_FURN_BK_LAG_INFO");
                                    query.AppendLine("where FURN_ITEM_INDEX in (");
                                    query.AppendLine("select FURN_ITEM_INDEX from UPLOAD_FURN_BK_LAG_INFO");
                                    query.AppendLine("except ");
                                    query.AppendLine("select FURN_ITEM_INDEX from FURN_BK_LAG_INFO)");
                                    query.AppendLine("update FURN_BK_LAG_INFO");
                                    query.AppendLine("set LAGHOUR = a.LAGHOUR from (");
                                    query.AppendLine("select FURN_ITEM_INDEX, LAGHOUR from UPLOAD_FURN_BK_LAG_INFO");
                                    query.AppendLine("except ");
                                    query.AppendLine("select FURN_ITEM_INDEX, LAGHOUR from FURN_BK_LAG_INFO) a");
                                    query.AppendLine("where FURN_BK_LAG_INFO.FURN_ITEM_INDEX = a.FURN_ITEM_INDEX");
                                    sqlCmnd.Connection = conn;
                                    sqlCmnd.CommandText = query.ToString();
                                    effectedRows = sqlCmnd.ExecuteNonQuery();

                                }
                                MessageBox.Show(string.Format("更新延遲時間完成，共影響{0}筆。", effectedRows), "", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    });
            }
        }
    }
}
