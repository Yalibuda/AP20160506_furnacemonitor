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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dashboard.View.ConfigDialog
{
    /// <summary>
    /// Interaction logic for EditSPCMultivariateView.xaml
    /// </summary>
    public partial class EditSPCMultivariateView : Page, IAdminPage
    {
        public EditSPCMultivariateView()
        {
            InitializeComponent();
        }

        private void AvailableItemListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((ViewModel.ConfigDialog.EditSPCMultivariateViewModel)this.DataContext).AddClickedItem(e.OriginalSource);
        }

        private void SelectedItemListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((ViewModel.ConfigDialog.EditSPCMultivariateViewModel)this.DataContext).RemoveClickedItem(e.OriginalSource);
        }
    }
}
