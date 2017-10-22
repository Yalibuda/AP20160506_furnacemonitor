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
    /// Interaction logic for ItemsCorrelationView.xaml
    /// </summary>
    public partial class ItemsCorrelationView : Page, IFuncPage
    {
        public ItemsCorrelationView()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            #region change columntext from xaml to xaml.cs
            //int numberOfColumns = 3;
            //string[] ItemNames = new string[] { "a", "b", "c" };
            //string headerName = "ItemName";
            //DataGridTextColumn firstDataGridTextColumn = new DataGridTextColumn();
            //firstDataGridTextColumn.Header = headerName;
            //firstDataGridTextColumn.Width = 80;
            //firstDataGridTextColumn.Binding = new System.Windows.Data.Binding(string.Format("[0]", 0));
            ////firstDataGridTextColumn.Binding = new System.Windows.Data.Binding("ItemNames");
            //corrGrid.Columns.Add(firstDataGridTextColumn);
            //for (int i = 0; i < numberOfColumns; i++)
            //{
            //    headerName = ItemNames[i];
            //    DataGridTextColumn tmpDataGridTextColumn = new DataGridTextColumn();
            //    tmpDataGridTextColumn.Header = headerName;
            //    tmpDataGridTextColumn.Width = 50;
            //    tmpDataGridTextColumn.Binding = new System.Windows.Data.Binding(string.Format("[{0}]", i + 1));
            //    corrGrid.Columns.Add(tmpDataGridTextColumn);
            //}
            #endregion  
            ((ViewModel.ItemsCorrelationViewModel)this.DataContext).Project = Minitab.Project;
        }
    }
}
