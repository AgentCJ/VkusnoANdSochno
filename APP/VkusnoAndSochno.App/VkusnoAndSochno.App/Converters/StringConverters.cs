using System.Globalization;

namespace VkusnoAndSochno.App.Converters
{
    // Проверяет, что строка пустая (для отображения Placeholder)
    public class IsStringNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => string.IsNullOrEmpty(value?.ToString());

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // Проверяет, что строка НЕ пустая (для отображения введенного текста)
    public class IsStringNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => !string.IsNullOrEmpty(value?.ToString());

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}