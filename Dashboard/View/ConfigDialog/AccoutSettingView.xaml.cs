using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dashboard.View.ConfigDialog
{
    /// <summary>
    /// Interaction logic for AccoutSettingView.xaml
    /// </summary>
    public partial class AccoutSettingView : Window
    {
        public AccoutSettingView()
        {
            InitializeComponent();
            ((ViewModel.NotifyPropertyChanged)this.DataContext).CloseAction = new Action(() => this.Close()); //讓VM可以與對話框關閉行為連動
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IconHelper.RemoveIcon(this);
        }
    }
}
