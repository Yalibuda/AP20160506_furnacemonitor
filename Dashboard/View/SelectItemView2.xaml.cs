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
    /// SelectItemView2.xaml 的互動邏輯
    /// </summary>
    public partial class SelectItemView2 : Window, IDisposable
    {
        public SelectItemView2()
        {
            InitializeComponent();
        }
        public SelectItemView2(ViewModel.SelectItemViewModel vm)
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
            ((ViewModel.SelectItemViewModel)this.DataContext).Initialize();
            ((ViewModel.SelectItemViewModel)this.DataContext).CloseAction = new Action(() => this.Close());
        }
    }
}
