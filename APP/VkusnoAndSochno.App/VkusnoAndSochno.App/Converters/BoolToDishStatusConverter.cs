using System.Globalization;

namespace VkusnoAndSochno.App.Converters
{
    public class BoolToDishStatusConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool)(value ?? false) ? "Снять с продажи" : "Вернуть в меню";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}