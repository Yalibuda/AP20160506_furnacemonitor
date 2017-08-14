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
    /// Interaction logic for SelectItemView.xaml
    /// </summary>
    public partial class SelectItemView : Window, IDisposable
    {
        public SelectItemView()
        {
            InitializeComponent();
        }
        public SelectItemView(ViewModel.SelectTrendItemViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
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
    }
}
