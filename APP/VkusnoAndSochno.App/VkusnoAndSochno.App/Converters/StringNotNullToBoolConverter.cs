using System.Globalization;

namespace VkusnoAndSochno.App.Converters
{
    // Возвращает true, если значение не равно null
    public class StringNotNullToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value != null;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}