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
using LinearAlgebra = MathNet.Numerics.LinearAlgebra;
using System.Threading;
using System.Security.AccessControl;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                Minitab.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            try
            {
                System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(Database.DBQueryTool.GetConnString());
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            string tmpDir = System.IO.Path.Combine(Environment.GetEnvironmentVariable("tmp"), "Minitab");
            if (!System.IO.Directory.Exists(tmpDir))
            {
                System.IO.Directory.CreateDirectory(tmpDir);
            }
            //Minitab.App.UserInterface.Visible = true;
            //Minitab.App.UserInterface.DisplayAlerts = false;
            //Minitab.Project.Worksheets.Open(@"D:\Dropbox\Workspace\03.PFG\06.Dataset\熔爐資料\multivar_test_V16.mtw");
            //Mtb.Worksheet ws = Minitab.Project.ActiveWorksheet;

            //StringBuilder cmnd = new StringBuilder();
            //Minitab.Project.ExecuteCommand("copy c2-c4 m1");
            //double[] colarray = ws.Matrices.Item("m1").GetData();
            //var M = LinearAlgebra.Matrix<double>.Build;
            //LinearAlgebra.Matrix<double> data = M.DenseOfColumnMajor(ws.Columns.Item("c2").RowCount, 3, colarray);
            //Model.TSquareLimCalculation tcalc = new Model.TSquareLimCalculation();
            //Model.TsquareParameters tpara = tcalc.Execute(data, Minitab.Project);
            //Console.WriteLine(tpara.Mean.ToString());
            //Console.WriteLine(tpara.Covariance.ToString());
            //Console.WriteLine(tpara.SampleSize.ToString());



        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Minitab.Quit();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }

    public class LeftMarginMultiplierConverter : IValueConverter
    {
        public double Length { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
                return new Thickness(0);
            return new Thickness(Length * item.GetDepth(), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        {
            TreeViewItem parent;
            while ((parent = GetParent(item)) != null)
            {
                return GetDepth(parent) + 1;
            }
            return 0;
        }

        private static TreeViewItem GetParent(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }
    }

}
