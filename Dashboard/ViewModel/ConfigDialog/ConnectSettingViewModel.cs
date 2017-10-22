using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Dashboard.ViewModel.ConfigDialog
{
    public class ConnectSettingViewModel : NotifyPropertyChanged
    {
        public ConnectSettingViewModel()
        {
            Load();
        }

        public string ServerName
        {
            get { return _srvName; }
            set
            {
                //_srvName = value;
                _srvName = "SFI-127";
                RaisePropertyChanged("ServerName");
            }
        }
        //private string _srvName = "";
        private string _srvName = "SFI-127";

        private void Load()
        {
            ServerName = DBServer.Default.ServerName;
        }

        public ICommand SaveCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        if (ServerName == null || ServerName == string.Empty)
                        {
                            MessageBox.Show("請輸入資料庫伺服器的位置");
                        }
                        DBServer.Default.ServerName = ServerName;
                        DBServer.Default.Save();
                        MessageBox.Show("請先關閉應用程式再重新啟動", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    );
            }
        }
    }
}
