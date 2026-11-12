using System.Globalization;

namespace VkusnoAndSochno.App.Converters
{
    // Меняет текст кнопки в зависимости от того, открыта форма добавления или редактирования
    public class AddEditToggleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (bool)(value ?? false) ? "Отмена" : "Новый продукт";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}