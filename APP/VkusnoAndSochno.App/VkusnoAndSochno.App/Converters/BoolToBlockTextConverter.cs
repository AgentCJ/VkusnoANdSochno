using System.Globalization;

namespace VkusnoAndSochno.App.Converters
{
    public class BoolToBlockTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isActive = (bool)(value ?? false);
            return isActive ? "Блокировать" : "Разблокировать";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}