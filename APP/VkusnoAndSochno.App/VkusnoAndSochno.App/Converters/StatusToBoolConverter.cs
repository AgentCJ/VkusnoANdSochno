using System.Globalization;

namespace VkusnoAndSochno.App.Converters
{
    // Сравнивает текущий статус с переданным параметром
    public class StatusToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}