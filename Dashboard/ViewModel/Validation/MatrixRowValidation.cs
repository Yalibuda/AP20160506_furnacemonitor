using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashboard.ViewModel.Validation
{
    public class MatrixRowValidation:System.Windows.Controls.ValidationRule
    {
        public override System.Windows.Controls.ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            Console.WriteLine("aaaa");
            return System.Windows.Controls.ValidationResult.ValidResult;
        }
    }
}
