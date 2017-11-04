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
    /// PropertyAnalysisView.xaml 的互動邏輯
    /// </summary>
    public partial class PropertyAnalysisView : Page, IFuncPage
    {
        public PropertyAnalysisView()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
           ((ViewModel.PropertyAnalysisViewModel)this.DataContext).Project = Minitab.Project;
        }
    }
}
