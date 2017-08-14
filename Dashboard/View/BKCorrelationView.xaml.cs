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

namespace Dashboard.View
{
    /// <summary>
    /// Interaction logic for BKCorrelationView.xaml
    /// </summary>
    public partial class BKCorrelationView : Page, IFuncPage
    {
        public BKCorrelationView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((ViewModel.BKCorrelationViewModel)this.DataContext).Project = Minitab.Project;
        }
    }
}
