using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.ViewModel.Converter
{
    /// <summary>
    /// 用於轉換項目編輯頁面中，有步驟對應關係控制項的可視性
    /// </summary>
    public class StepToVisibility : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                System.Windows.Visibility vis = System.Windows.Visibility.Collapsed;
                double step = (double)value;
                double targetStep = double.Parse(parameter.ToString());
                if (step >= targetStep) { vis = System.Windows.Visibility.Visible; }
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
