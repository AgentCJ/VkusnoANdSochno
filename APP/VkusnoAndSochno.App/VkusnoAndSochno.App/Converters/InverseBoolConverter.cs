using System.Globalization;

namespace VkusnoAndSochno.App.Converters
{
    // Инвертирует логическое значение (true становится false и наоборот)
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => !(bool)(value ?? false);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}