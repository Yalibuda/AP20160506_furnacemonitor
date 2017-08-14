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
    /// SelectTrendItemDialog.xaml 的互動邏輯
    /// </summary>
    public partial class SelectTrendItemDialogView : Window, IDisposable
    {    
        public SelectTrendItemDialogView()
        {           
            InitializeComponent();
        }

        public SelectTrendItemDialogView(ViewModel.SelectTrendItemViewModel viewmodel)
        {
            InitializeComponent();
            this.DataContext = viewmodel;
            

        }

        public void Dispose()
        {
            GC.Collect();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {           
            ((ViewModel.SelectTrendItemViewModel)this.DataContext).Initialize();
            ((ViewModel.SelectTrendItemViewModel)this.DataContext).CloseAction = new Action(() => this.Close());
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IconHelper.RemoveIcon(this);
        }
       
    }
}
