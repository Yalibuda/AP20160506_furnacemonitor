﻿using System;
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
    /// Interaction logic for RealtimeItemManagerView.xaml
    /// </summary>
    public partial class RealtimeItemManagerView : Page, IAdminPage
    {
        public RealtimeItemManagerView()
        {
            InitializeComponent();
        }

        private void AvailableItemListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((ViewModel.SPCItemSettingViewModel)this.DataContext).AddClickedItem(e.OriginalSource);
        }

        private void SelectedItemListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((ViewModel.SPCItemSettingViewModel)this.DataContext).AddClickedItem(e.OriginalSource);
        }
    }
}
