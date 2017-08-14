using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.ViewModel.Converter
{
    /// <summary>
    /// 用於轉換項目編輯頁面中，有步驟對應關係控制項的可控制性
    /// </summary>
    public class StepToEnable : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool isEnable = false;
                double step = (double)value;
                double targetStep = double.Parse(parameter.ToString());
                if (step == targetStep) isEnable = true;
                return isEnable;
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
