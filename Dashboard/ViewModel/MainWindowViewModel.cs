using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dashboard.ViewModel
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            CurrentPage = new View.RealTimeSPCView();
            IsRealTimeSelected = true;
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            private set
            {
                _isAdmin = value;
                RaisePropertyChanged("IsAdmin");
            }
        }
        private bool _isAdmin = false;

        public bool ShowLogIn
        {
            get { return _showLogin; }
            private set
            {
                _showLogin = value;
                RaisePropertyChanged("ShowLogIn");
            }
        }
        private bool _showLogin = true;

        public bool ShowLogOut
        {
            get { return _showLogout; }
            set
            {
                _showLogout = value;
                RaisePropertyChanged("ShowLogOut");
            }
        }
        private bool _showLogout = false;

        public string UserName
        {
            get { return _username; }
            set
            {
                _username = value;
                RaisePropertyChanged("UserName");
            }
        }
        private string _username = "guest";

        public Page CurrentPage
        {
            get { return _currentPage; }
            private set
            {
                _currentPage = value;
                RaisePropertyChanged("CurrentPage");
            }
        }
        private Page _currentPage = null;

        public bool IsRealTimeSelected
        {
            get { return _isRealTimeSelected; }
            private set
            {
                _isRealTimeSelected = value;
                RaisePropertyChanged("IsRealTimeSelected");
            }
        }
        private bool _isRealTimeSelected = false;

        public bool IsSPCSelected
        {
            get { return _isSPCSelected; }
            private set
            {
                _isSPCSelected = value;
                RaisePropertyChanged("IsSPCSelected");
            }
        }
        private bool _isSPCSelected = false;

        public bool IsCorrSelected
        {
            get { return _isCorrSelected; }
            private set
            {
                _isCorrSelected = value;
                RaisePropertyChanged("IsCorrSelected");
            }
        }
        private bool _isCorrSelected = false;

        public bool IsPosiDiffSelected
        {
            get { return _isPosiDiffSelected; }
            private set
            {
                _isPosiDiffSelected = value;
                RaisePropertyChanged("IsCorrSelected");
            }
        }
        private bool _isPosiDiffSelected = false;

        public bool IsPropertySelected
        {
            get { return _isPropertySelected; }
            private set
            {
                _isPropertySelected = value;
                RaisePropertyChanged("IsPropertySelected");
            }

        }
        private bool _isPropertySelected = false;

        private string _site = "";
        private string _dataFolderPath = "";

        /// <summary>
        /// 切換主要功能的要處理的工作
        /// </summary>
        /// <param name="obj"></param>
        private void TreeviewSelectedItemChanged(Object obj)
        {
            TreeViewItem tvItem = obj as TreeViewItem;
            if (tvItem != null)
            {
                if (tvItem.Name != "Export" && tvItem.Name != "ExportData")
                {
                    if (CurrentPage.DataContext != null && CurrentPage.DataContext is BasicPage)
                    {
                        ((BasicPage)CurrentPage.DataContext).StopCurrentWork();
                    }
                }

                switch (tvItem.Name)
                {
                    case "RealTime":
                        IsRealTimeSelected = true;
                        CurrentPage = new View.RealTimeSPCView();
                        break;
                    case "SPC":
                        IsSPCSelected = true;
                        CurrentPage = new View.TrendAnalysisView();
                        break;
                    case "Correlation":
                        IsCorrSelected = true;
                        CurrentPage = new View.BKCorrelationView();
                        break;
                    case "DiffCompare":
                        IsPosiDiffSelected = true;
                        CurrentPage = new View.PosiDiffTestView();
                        break;
                    case "PropTrend":
                        IsPropertySelected = true;
                        CurrentPage = new View.PropertyAnalysisView();
                        break;
                    //case "Export":
                    //    BasicPage page = CurrentPage.DataContext as BasicPage;
                    //    if (page == null) return;
                    //    if (page.IsBusy)
                    //    {
                    //        System.Windows.MessageBox.Show("工作正在運行中，請稍後匯出。", "Minitab Dashboard",
                    //            System.Windows.MessageBoxButton.OK,
                    //            System.Windows.MessageBoxImage.Warning);
                    //        return;
                    //    }
                    //    List<Model.IReport> RptItems
                    //        = page.ReportItems;
                    //    if (RptItems != null && RptItems.Count > 0)
                    //    {
                    //        Tool.ExportMSWordReport(RptItems);
                    //    }
                    //    break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 取消所有主要功能相關的工作選擇
        /// </summary>
        private void CancelAllTreeViewSelection()
        {
            IsRealTimeSelected = false;
            IsSPCSelected = false;
            IsCorrSelected = false;
            IsPosiDiffSelected = false;

        }

        public ICommand EditItemCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        CancelAllTreeViewSelection();
                        View.ConfigDialog.EditFurnItemView f = new View.ConfigDialog.EditFurnItemView();
                        ((ViewModel.ConfigDialog.EditFurnItemViewModel)f.DataContext).Site = _site;
                        CurrentPage = f;
                    }
                    );
            }
        }
        public ICommand EditUniVarCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        CancelAllTreeViewSelection();
                        Page page = new View.ConfigDialog.EditSPCUnivariateView();
                        ((ViewModel.ConfigDialog.EditSPCUnivariateViewModel)page.DataContext).Site = _site;
                        CurrentPage = page;
                    });
            }
        }
        public ICommand EditMulVarCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        CancelAllTreeViewSelection();
                        Page page = new View.ConfigDialog.EditSPCMultivariateView();
                        ((ViewModel.ConfigDialog.EditSPCMultivariateViewModel)page.DataContext).Site = _site;
                        ((ViewModel.ConfigDialog.EditSPCMultivariateViewModel)page.DataContext).Project = Minitab.Project;
                        CurrentPage = page;
                    });
            }
        }
        public ICommand EditFurnBkLagCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        CancelAllTreeViewSelection();
                        Page page = new View.ConfigDialog.FurnBKLagManagerView();
                        ((ViewModel.ConfigDialog.FurnBkLagManagerViewModel)page.DataContext).Site = _site;
                        //((ViewModel.ConfigDialog.EditSPCMultivariateViewModel)page.DataContext).Project = Minitab.Project;
                        CurrentPage = page;
                    });
            }
        }
        public ICommand EditRealTimeCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        CancelAllTreeViewSelection();
                        Page page = new View.ConfigDialog.RealtimeItemManagerView();
                        ((ViewModel.SPCItemSettingViewModel)page.DataContext).SITE_ID = _site;
                        //((ViewModel.ConfigDialog.EditSPCMultivariateViewModel)page.DataContext).Project = Minitab.Project;
                        CurrentPage = page;
                    });
            }
        }
        public ICommand DBSettingCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        CancelAllTreeViewSelection();
                        CurrentPage = new View.ConfigDialog.ConnectSettingView();
                    });
            }
        }
        public ICommand AboutCommand
        {
            get
            {
                return new Command.RelayCommand
                (
                    param =>
                    {
                        //CancelAllTreeViewSelection();
                        AboutDialog f = new AboutDialog();
                        f.ShowDialog();
                    },
                    param =>
                    {
                        return true;
                    }
                );
            }
        }
        public ICommand LoginCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        View.ConfigDialog.LoginView f = new View.ConfigDialog.LoginView();
                        f.ShowDialog();

                        _account = ((ConfigDialog.LoginViewModel)f.DataContext).Account;
                        if (_account != null && _account.Count() > 0)
                        {
                            _site = _account.Select(x => x.Site).FirstOrDefault(); //目前編輯功能的設計會默認一個廠區(讓A廠管理者不能跨廠上傳至B廠)

                            if (_account.Any(x => x.Role == "Admin"))
                            {
                                IsAdmin = true;
                            }
                            else
                            {
                                IsAdmin = false;
                            }
                            ShowLogIn = false;
                            ShowLogOut = true;
                            UserName = _account.First().FirstName + " " + _account.First().LastName;
                        }



                    }
                    );
            }
        }
        public ICommand AccountSettingCommand
        {
            get
            {
                return new ViewModel.Command.RelayCommand(
                    param =>
                    {
                        View.ConfigDialog.AccoutSettingView f = new View.ConfigDialog.AccoutSettingView();
                        ((ConfigDialog.LoginViewModel)f.DataContext).Account = _account;
                        f.ShowDialog();
                    });
            }
        }
        public ICommand LogoutCommand
        {
            get
            {
                return new Command.RelayCommand(
                    param =>
                    {
                        IsAdmin = false;
                        ShowLogIn = true;
                        ShowLogOut = false;
                        _site = "";
                        System.Windows.MessageBox.Show("登出完成", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        //如果使用者剛好在需要權限的頁面時，則離開該頁面
                        if (CurrentPage is View.ConfigDialog.IAdminPage)
                        {
                            CurrentPage = new View.TrendAnalysisView();
                        }

                    });
            }
        }
        public ICommand TreeviewSelectedItemChangedCommand
        {
            get
            {
                return new Command.RelayCommand(TreeviewSelectedItemChanged);
            }
        }
        public ICommand ExportReportCommand
        {
            get
            {
                return new Command.RelayCommand(param =>
                {
                    BasicPage page = CurrentPage.DataContext as BasicPage;
                    if (page == null) return;
                    if (page.IsBusy)
                    {
                        System.Windows.MessageBox.Show("工作正在運行中，請稍後匯出。", "Minitab Dashboard",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                    List<Model.IReport> RptItems
                        = page.ReportItems;
                    if (RptItems != null && RptItems.Count > 0)
                    {
                        if (RptItems.All(x => x == null))
                        {
                            return;
                        }
                        Tool.ExportMSWordReport(RptItems.Where(x => x != null));
                    }

                });
            }
        }
        public ICommand ExportDataCommand
        {
            get
            {
                return new Command.RelayCommand(param =>
                {
                    BasicPage page = CurrentPage.DataContext as BasicPage;
                    if (page == null) return;
                    page = CurrentPage.DataContext as BasicPage;
                    if (page == null) return;
                    if (page.IsBusy)
                    {
                        System.Windows.MessageBox.Show("工作正在運行中，請稍後匯出。", "Minitab Dashboard",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                    List<Model.IReport> RptItems = page.ReportItems;
                    if (RptItems != null && RptItems.Count > 0) //
                    {
                        //過濾掉 null item
                        if (RptItems.All(x => x == null)) //全部為 null
                        {
                            return;
                        }
                        else if (RptItems.Any(x => x == null)) //表示至少有一 item 為 null
                        {
                            RptItems = RptItems.Where(x => x != null).ToList(); //把非 null 留下
                        }

                        //過濾掉無法輸入資料的 item
                        if (RptItems.Select(x => x.RawData).All(x => x == null || x.Rows.Count == 0)) //如果全部都沒有資料就跳過不做
                        {
                            return;
                        }

                        //匯出資料
                        try
                        {
                            System.Data.DataTable[] dts = RptItems.Where(x => x.RawData != null && x.RawData.Rows.Count > 0)
                                .Select(x => x.RawData.Copy()).ToArray();
                            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                            {
                                if (_dataFolderPath != "") fbd.SelectedPath = _dataFolderPath;
                                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                                {
                                    _dataFolderPath = fbd.SelectedPath;
                                    Tool.ExportDataTablesToCSVData(dts, _dataFolderPath);
                                }
                            }
                        }
                        catch
                        {
                            //do nothing
                        }

                    }

                });
            }
        }

        private IEnumerable<ConfigDialog.AccountInfo> _account;

    }
    public enum FunctionMode
    {
        RealTime,
        TrendAnalysis,
        BkCorrelation,
        PositionDifference
    }
}
