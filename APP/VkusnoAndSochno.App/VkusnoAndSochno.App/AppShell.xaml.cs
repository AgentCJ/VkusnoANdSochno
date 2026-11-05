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
        // Получаем текущий маршрут (на какой странице мы находимся)
        var currentRoute = CurrentState.Location.OriginalString;

        if (currentRoute.Contains("LoginPage"))
        {
            // Если мы на экране входа — предлагаем закрыть программу
            bool answer = await DisplayAlert("Выход", "Вы уверены, что хотите закрыть приложение?", "Да", "Нет");
            if (answer)
            {
                Application.Current?.Quit(); // Закрывает приложение целиком
            }
        }
        else
        {
            // Если мы внутри аккаунта — предлагаем выйти из него
            bool answer = await DisplayAlert("Выход", "Вы уверены, что хотите выйти из аккаунта?", "Да", "Нет");
            if (answer)
            {
                // Возвращаемся на экран авторизации
                await GoToAsync("//LoginPage");
            }
        }
    }
}