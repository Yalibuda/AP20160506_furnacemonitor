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

namespace Dashboard.View
{
    /// <summary>
    /// SelectMultivariateItemDialogView.xaml 的互動邏輯
    /// </summary>
    public partial class SelectMultivariateItemDialogView : Window, IDisposable
    {
        public SelectMultivariateItemDialogView(ViewModel.SelectMulItemViewModel viewmodel)
        {
            InitializeComponent();
            this.DataContext = viewmodel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((ViewModel.SelectMulItemViewModel)this.DataContext).Initialize();
            ((ViewModel.SelectMulItemViewModel)this.DataContext).CloseAction = new Action(() => this.Close());
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IconHelper.RemoveIcon(this);
        }

        public void Dispose()
        {
            GC.Collect();
        }

        private void AvailableItemListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((ViewModel.SelectMulItemViewModel)this.DataContext).AddClickedItem(e.OriginalSource);
        }

        private void SelectedItemListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((ViewModel.SelectMulItemViewModel)this.DataContext).RemoveClickedItem(e.OriginalSource);
        }
    }
}
