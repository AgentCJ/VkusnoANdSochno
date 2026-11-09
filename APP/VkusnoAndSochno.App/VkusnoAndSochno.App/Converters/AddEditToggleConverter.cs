using System.Globalization;

namespace VkusnoAndSochno.App.Converters
{
    public class AddEditToggleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool)(value ?? false) ? "❌ Отмена" : "➕ Новый продукт";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}