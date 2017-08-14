using Dashboard.ViewModel.ConfigDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.ViewModel.Converter
{
    /// <summary>
    /// 用於轉換項目編輯頁面中，行為模式變更時，控制項的可視性
    /// </summary>
    public class ActionToVisibility : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                System.Windows.Visibility vis = System.Windows.Visibility.Collapsed;
                UserActionMode currentMode = (UserActionMode)value;
                UserActionMode targetMode = (UserActionMode)parameter;
                if ((int)currentMode == (int)targetMode)
                {
                    vis = System.Windows.Visibility.Visible;
                }
                return vis;
            }
            catch
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
