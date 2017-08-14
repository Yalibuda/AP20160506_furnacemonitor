using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Dashboard.ViewModel.ConfigDialog
{
    public class EditFurnItemViewModel : NotifyPropertyChanged
    {
        public EditFurnItemViewModel()
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
        private string _title = "管理熔爐項目";

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

        public string ActionString
        {
            get { return _actionString; }
            private set
            {
                _actionString = value;
                RaisePropertyChanged("ActionString");
            }
        }
        private string _actionString = "上傳檔案";

        //public string ActionMode
        //{
        //    get { return _actionMode; }
        //    set
        //    {
        //        _actionMode = value;
        //        RaisePropertyChanged("ActionMode");
        //    }
        //}
        //private string _actionMode = "Upload";

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

        //public bool IsUploadMode
        //{
        //    get
        //    {
        //        return _isUploadMode;
        //    }
        //    private set
        //    {
        //        _isUploadMode = value;
        //        RaisePropertyChanged("IsUploadMode");
        //    }
        //}
        //private bool _isUploadMode = true;

        //public bool IsEditMode
        //{
        //    get { return _isEditMode; }
        //    private set
        //    {
        //        _isEditMode = value;
        //        RaisePropertyChanged("IsEditMode");
        //    }
        //}
        //private bool _isEditMode = false;

        public DataTable FurnItemInfo
        {
            get { return _furnItemInfo; }
            private set
            {
                _furnItemInfo = value;
                RaisePropertyChanged("FurnItemInfo");
            }
        }
        private DataTable _furnItemInfo = null;

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


        private int UploadData()
        {
            DataTable dt;
            try
            {
                dt = Database.DBQueryTool.ReadCSVFile(SelectedFilePath);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    Console.WriteLine(dr["DEVICE"].GetType());
                    if (dr["DEVICE"] == null) dr["DEVICE"] = DBNull.Value;
                    if (dr["GROUP_ID"] == null) dr["GROUP_ID"] = DBNull.Value;
                }


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand sqlCmnd = new SqlCommand("TRUNCATE TABLE UPLOAD_FURN_ITEM_INFO", conn))
                    {
                        //先清除暫存表上的資料
                        sqlCmnd.ExecuteNonQuery();
                    }

                    using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                    {
                        sqlBC.BatchSize = 1000;
                        sqlBC.BulkCopyTimeout = 6000;

                        //設定要寫入的資料庫
                        sqlBC.DestinationTableName = "UPLOAD_FURN_ITEM_INFO";

                        //對應資料行
                        sqlBC.ColumnMappings.Add("TagName", "TAG_NAME");
                        sqlBC.ColumnMappings.Add("SITE_ID", "SITE_ID");
                        sqlBC.ColumnMappings.Add("DEVICE", "DEVICE");
                        sqlBC.ColumnMappings.Add("GROUP_ID", "GROUP_ID");
                        sqlBC.ColumnMappings.Add("ITEM_NAME", "ITEM_NAME");
                        sqlBC.ColumnMappings.Add("UNIT", "UNIT");

                        //開始寫入
                        sqlBC.WriteToServer(dt);
                    }

                    using (SqlCommand sqlCmnd = new SqlCommand())
                    {
                        StringBuilder query = new StringBuilder();
                        //新增
                        query.AppendLine("INSERT INTO FURN_ITEM_INFO(TAG_NAME, SITE_ID, DEVICE, GROUP_ID, ITEM_NAME, UNIT)");
                        query.AppendLine("SELECT * FROM (");
                        query.AppendLine("SELECT * FROM UPLOAD_FURN_ITEM_INFO");
                        query.AppendLine("EXCEPT");
                        query.AppendLine("SELECT TAG_NAME,SITE_ID,DEVICE,GROUP_ID,ITEM_NAME,UNIT FROM FURN_ITEM_INFO");
                        query.AppendLine(") a");
                        query.AppendLine("WHERE TAG_NAME NOT IN (SELECT TAG_NAME FROM FURN_ITEM_INFO)");
                        //更新
                        query.AppendLine("UPDATE FURN_ITEM_INFO");
                        query.AppendLine("SET ITEM_NAME = a.ITEM_NAME, UNIT = a.UNIT");
                        query.AppendLine("FROM (");
                        query.AppendLine("SELECT * FROM UPLOAD_FURN_ITEM_INFO");
                        query.AppendLine("EXCEPT");
                        query.AppendLine("SELECT TAG_NAME,SITE_ID,DEVICE,GROUP_ID,ITEM_NAME,UNIT FROM FURN_ITEM_INFO");
                        query.AppendLine(") a");
                        query.AppendLine("WHERE a.TAG_NAME = FURN_ITEM_INFO.TAG_NAME");


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

        private int EditData()
        {
            if (FurnItemInfo == null || FurnItemInfo.Rows.Count == 0) return 0;
            //擷取需要的部分
            DataTable dt = FurnItemInfo.Clone();
            dt.Columns.Remove("FURN_ITEM_INDEX");
            foreach (DataRow dr in FurnItemInfo.Rows)
            {
                DataRow newrow = dt.NewRow();
                newrow["TAG_NAME"] = dr["TAG_NAME"];
                newrow["SITE_ID"] = dr["SITE_ID"];
                newrow["DEVICE"] = dr["DEVICE"];
                newrow["GROUP_ID"] = dr["GROUP_ID"];
                newrow["ITEM_NAME"] = dr["ITEM_NAME"];
                newrow["UNIT"] = dr["UNIT"];
                dt.Rows.Add(newrow);
            }
            using (SqlConnection conn = new SqlConnection(Database.DBQueryTool.GetConnString()))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand sqlCmnd = new SqlCommand("TRUNCATE TABLE UPLOAD_FURN_ITEM_INFO", conn))
                    {
                        //先清除暫存表上的資料
                        sqlCmnd.ExecuteNonQuery();
                    }

                    using (SqlBulkCopy sqlBC = new SqlBulkCopy(conn))
                    {
                        sqlBC.BatchSize = 1000;
                        sqlBC.BulkCopyTimeout = 6000;

                        //設定要寫入的資料庫
                        sqlBC.DestinationTableName = "UPLOAD_FURN_ITEM_INFO";

                        //對應資料行
                        sqlBC.ColumnMappings.Add("TAG_NAME", "TAG_NAME");
                        sqlBC.ColumnMappings.Add("SITE_ID", "SITE_ID");
                        sqlBC.ColumnMappings.Add("DEVICE", "DEVICE");
                        sqlBC.ColumnMappings.Add("GROUP_ID", "GROUP_ID");
                        sqlBC.ColumnMappings.Add("ITEM_NAME", "ITEM_NAME");
                        sqlBC.ColumnMappings.Add("UNIT", "UNIT");

                        //開始寫入
                        sqlBC.WriteToServer(dt);
                    }

                    using (SqlCommand sqlCmnd = new SqlCommand())
                    {
                        StringBuilder query = new StringBuilder();
                        //更新
                        query.AppendLine("UPDATE FURN_ITEM_INFO");
                        query.AppendLine("SET ITEM_NAME = a.ITEM_NAME, UNIT = a.UNIT");
                        query.AppendLine("FROM (");
                        query.AppendLine("SELECT * FROM UPLOAD_FURN_ITEM_INFO");
                        query.AppendLine("EXCEPT");
                        query.AppendLine("SELECT TAG_NAME,SITE_ID,DEVICE,GROUP_ID,ITEM_NAME,UNIT FROM FURN_ITEM_INFO");
                        query.AppendLine(") a");
                        query.AppendLine("WHERE a.TAG_NAME = FURN_ITEM_INFO.TAG_NAME");
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

                //query.AppendLine("SELECT * FROM FURN_ITEM_INFO");
                //query.AppendFormat("WHERE SITE_ID='{0}'\r\n", Site);
                //query.AppendLine("ORDER BY FURN_ITEM_INDEX");

                //SqlDataAdapter da = new SqlDataAdapter(query.ToString(), conn);

                ////Update command
                //query.Clear();
                //query.AppendLine("UPDATE FURN_ITEM_INFO");
                //query.AppendLine("SET [ITEM_NAME] = @ITEM_NAME, [UNIT] = @UNIT");
                //query.AppendLine("WHERE [FURN_ITEM_INDEX]=@FURN_ITEM_INDEX");
                //da.UpdateCommand = new SqlCommand(query.ToString());
                //da.UpdateCommand.Parameters.Add("@ITEM_NAME", SqlDbType.VarChar, 32, "ITEM_NAME");
                //da.UpdateCommand.Parameters.Add("@UNIT", SqlDbType.VarChar, 32, "UNIT");
                //da.UpdateCommand.Parameters.Add("@FURN_ITEM_INDEX", SqlDbType.VarChar, 32, "FURN_ITEM_INDEX");
                //da.UpdateCommand.UpdatedRowSource = UpdateRowSource.None;

                ////Insert command
                //query.Clear();
                //query.AppendLine("INSERT INTO FURN_ITEM_INFO (TAG_NAME, SITE_ID, DEVICE, GROUP_ID, ITEM_NAME, UNIT)");
                //query.AppendLine("VALUES(@TAG_NAME, @SITE_ID, @DEVICE, @GROUP_ID, @ITEM_NAME, @UNIT)");
                //da.InsertCommand = new SqlCommand(query.ToString());
                //da.InsertCommand.Parameters.Add("@TAG_NAME", SqlDbType.VarChar, 50, "TAG_NAME");
                //da.InsertCommand.Parameters.Add("@SITE_ID", SqlDbType.VarChar, 10, "SITE_ID");
                //da.InsertCommand.Parameters.Add("@DEVICE", SqlDbType.VarChar, 32, "DEVICE");
                //da.InsertCommand.Parameters.Add("@GROUP_ID", SqlDbType.VarChar, 32, "GROUP_ID");
                //da.InsertCommand.Parameters.Add("@ITEM_NAME", SqlDbType.VarChar, 32, "ITEM_NAME");
                //da.InsertCommand.Parameters.Add("@UNIT", SqlDbType.VarChar, 32, "UNIT");
                //da.InsertCommand.UpdatedRowSource = UpdateRowSource.None;

                ////Delete command
                //query.Clear();
                //query.AppendLine("DELETE FROM FURN_ITEM_INFO");
                //query.AppendLine("WHERE [FURN_ITEM_INDEX]= @FURN_ITEM_INDEX");
                //da.DeleteCommand = new SqlCommand(query.ToString());
                //da.DeleteCommand.Parameters.Add("@FURN_ITEM_INDEX", SqlDbType.VarChar, 32, "FURN_ITEM_INDEX");
                //da.DeleteCommand.UpdatedRowSource = UpdateRowSource.None;

                //DataSet ds = new DataSet();
                //da.Fill(ds, "FURN_ITEM_INFO");
                //DataTable currentDataTabel = ds.Tables[0];
                //DataTable updatedRows = currentDataTabel.Clone();

                //IEqualityComparer<DataRow> comparer = DataRowComparer.Default;

                ////逐列檢查是否有更新
                //for (int i = 0; i < FurnItemInfo.Rows.Count; i++)
                //{
                //    DataRow dr1 = FurnItemInfo.Rows[i];
                //    DataRow dr2 = currentDataTabel.Rows[i];
                //    if (!comparer.Equals(dr1, dr2))
                //    {
                //        currentDataTabel.Rows[i].ItemArray = dr1.ItemArray.Clone() as object[];
                //    }
                //}
                //da.UpdateBatchSize = 1000;
                //updatedRowsCnt = da.Update(currentDataTabel);

            }
        }

        private void Load()
        {
            _actionList = new DataTable();
            _actionList.Columns.Add("Name", typeof(string));
            _actionList.Columns.Add("Value", typeof(UserActionMode));
            DataRow dr;

            dr = _actionList.NewRow();
            dr["Name"] = "上傳熔爐資料"; dr["Value"] = UserActionMode.Add;
            _actionList.Rows.Add(dr);
            dr = _actionList.NewRow();
            dr["Name"] = "編輯熔爐資料"; dr["Value"] = UserActionMode.Edit;
            _actionList.Rows.Add(dr);
        }

        private void OnSiteChanged()
        {
            if (Site == null && Site == string.Empty) FurnItemInfo = null;
            StringBuilder query = new StringBuilder();
            //query.AppendLine("SELECT * FROM VW_FURNACELIMIT");
            //query.AppendFormat("WHERE SITE_ID='{0}' AND APPLY_DATE IS NOT NULL\r\n", Site);
            //query.AppendLine("ORDER BY SITE_ID, FURN_ITEM_INDEX");
            query.AppendLine("SELECT * FROM FURN_ITEM_INFO");
            query.AppendFormat("WHERE SITE_ID='{0}'\r\n", Site);
            query.AppendLine("ORDER BY FURN_ITEM_INDEX");
            FurnItemInfo = Database.DBQueryTool.GetData(query.ToString(), Database.DBQueryTool.GetConnString());
            Title = "管理熔爐項目-" + Site;
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
                            switch ((UserActionMode)ActionMode)
                            {
                                case UserActionMode.Add:
                                    if (MessageBox.Show("是否確定要上傳資料?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                    {
                                        int rowsUpdated = UploadData();
                                        MessageBox.Show(string.Format("上傳資料完成，共{0}筆。", rowsUpdated), "",
                                            MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    break;
                                case UserActionMode.BatchAdd:
                                    break;
                                case UserActionMode.Edit:
                                    if (MessageBox.Show("是否確定要更新資料?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                    {
                                        int rowEdited = EditData();
                                        MessageBox.Show(string.Format("資料更新完成，共{0}筆。", rowEdited), "", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
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
                        switch ((UserActionMode)ActionMode)
                        {
                            case UserActionMode.Add:
                                ActionString = "上傳檔案";
                                break;
                            case UserActionMode.BatchAdd:
                                break;
                            case UserActionMode.Edit:
                                ActionString = "編輯資料";
                                break;
                            default:
                                break;
                        }
                        //if (ActionMode == "Upload")
                        //{
                        //    ActionString = "上傳檔案";
                        //    IsEditMode = false; IsUploadMode = true;

                        //}
                        //else if (ActionMode == "Edit")
                        //{
                        //    ActionString = "編輯資料";
                        //    IsEditMode = true; IsUploadMode = false;
                        //}
                    }
                    );
            }
        }

        public ICommand FileSelectCommand
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
                    }
                    );
            }
        }


    }

}
