
using System.Globalization;
using System.Windows.Data;

namespace Code_Generatore.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null || parameter == null)
                return false;

            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
                throw new ArgumentException("Value must be a boolean.", nameof(value));

            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if (boolValue)
            {
                return Enum.Parse(targetType, parameter.ToString());
            }

            return Binding.DoNothing;
        }
    }
}
