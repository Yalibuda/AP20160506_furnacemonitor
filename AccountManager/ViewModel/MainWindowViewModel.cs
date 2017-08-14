using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AccountManager.ViewModel
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {
        public MainWindowViewModel()
        {

        }

        public string SelectedFilePath
        {
            get
            {
                return _selectedFilePath;
            }
            set
            {
                _selectedFilePath = value;
                RaisePropertyChanged("SelectedFilePath");
            }
        }
        private string _selectedFilePath = "";

        public string SrvName
        {
            get { return _srvName; }
            set
            {
                _srvName = value;
                RaisePropertyChanged("SrvName");
            }
        }
        private string _srvName = DBServer.Default.ServerName;

        public string PortNum
        {
            get { return _portNum; }
            set
            {
                _portNum = value;
                RaisePropertyChanged("PortNum");
            }
        }
        private string _portNum = DBServer.Default.Port;

        public string DBName
        {
            get { return _dbName; }
            set
            {
                _dbName = value;
                RaisePropertyChanged("PortNum");
            }
        }
        private string _dbName = DBServer.Default.DBName;

        public string UID
        {
            get { return _uid; }
            set
            {
                _uid = value;
                RaisePropertyChanged("UID");
            }
        }
        private string _uid = DBServer.Default.UserID;

        public string PSW
        {
            get { return _psw; }
            set
            {
                _psw = value;
                RaisePropertyChanged("PSW");
            }
        }
        private string _psw = DBServer.Default.Password;

        public ICommand BrowseFileCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                        openFileDialog.Filter = "CSV(逗號分隔)(*.csv)|*.csv";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            SelectedFilePath = openFileDialog.FileName;
                        }
                    }
                    );
            }
        }
        public ICommand DownloadSampleCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                        saveFileDialog.Filter = "逗號分隔文字檔|*.csv";
                        saveFileDialog.Title = "儲存上傳範例檔";
                        if (saveFileDialog.ShowDialog() == true && saveFileDialog.FileName != "")
                        {
                            string fpath = saveFileDialog.FileName;
                            DataTable dt =
                            DBTool.GetData("select top 1 * from UPLOAD_USER_ACC", DBTool.GetConnString()).Clone();
                            StringBuilder sb = new StringBuilder();
                            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                                              Select(column => column.ColumnName);
                            sb.AppendLine(string.Join(",", columnNames));
                            File.WriteAllText(fpath, sb.ToString());
                            MessageBox.Show("資料下載完成。", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    );
            }
        }
        public ICommand UploadFileCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        DataTable dt;
                        try
                        {
                            dt = DBTool.ReadCSVFile(SelectedFilePath);
                            if (dt == null || dt.Rows.Count == 0)
                            {
                                throw new Exception("上傳檔案無內容。");
                            }

                            //檢查欄位名稱是否正確
                            DataColumn[] dcs = dt.Columns.Cast<DataColumn>().ToArray();
                            string[] colNames = new string[] { "USERNAME", "PASSWORD", "FIRSTNAME", "LASTNAME", "SITE_ID", "ACC_ROLE" };
                            if (!dcs.Any(x => colNames.Contains(x.ColumnName)) ||
                                dcs.Length != 6 ||
                                dcs.Select(x => x.ColumnName).Distinct().Count() != 6)
                            {
                                throw new Exception("欄位名稱未依定義命名");
                            }


                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                DataRow dr = dt.Rows[i];
                                if (dr.ItemArray.Any(x => x == null || x == DBNull.Value))
                                {
                                    throw new Exception("欄位內容不可為空");
                                }

                                if (dr["ACC_ROLE"].ToString().ToUpper() != "ADMIN" && dr["ACC_ROLE"].ToString().ToUpper() != "GUEST")
                                {
                                    throw new Exception("ACC_ROLE 內容只能是\"Admin\"或\"Guest\"");
                                }
                            }

                            int r = Model.AccManagerTool.UploadUsers(dt);
                            MessageBox.Show(string.Format("上傳完成，共影響{0}列資料", r), "", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }


                    }
                    );
            }
        }
        public ICommand UpdateConnStrCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        DBServer.Default.ServerName = _srvName;
                        DBServer.Default.Port = _portNum;
                        DBServer.Default.DBName = _dbName;
                        DBServer.Default.UserID = _uid;
                        DBServer.Default.Password = _psw;
                        DBServer.Default.Save();
                        MessageBox.Show("設定修改完成", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    );
            }
        }
    }
}
