using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    // Обработчик для кнопки выхода
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Выход", "Вы уверены, что хотите выйти из системы?", "Да", "Нет");
        if (answer)
        {
            // Возвращаемся на экран авторизации
            await GoToAsync("//LoginPage");
        }
    }
}