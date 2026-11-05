using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        var session = IPlatformApplication.Current?.Services.GetService<UserSession>();
        var currentRoute = CurrentState.Location.OriginalString;

        // Определяем, нужно ли показывать кнопку
        bool shouldShowBackButton = (session?.RoleName == "test") &&
                                    !currentRoute.Contains("TestHubPage") &&
                                    !currentRoute.Contains("LoginPage");

        // Управляем коллекцией ToolbarItems
        if (shouldShowBackButton)
        {
            // Добавляем, если её ещё нет
            if (!ToolbarItems.Contains(BackToTestMenuBtn))
            {
                ToolbarItems.Add(BackToTestMenuBtn);
            }
        }
        else
        {
            // Удаляем, если она есть
            if (ToolbarItems.Contains(BackToTestMenuBtn))
            {
                ToolbarItems.Remove(BackToTestMenuBtn);
            }
        }
    }

    private async void OnBackToTestMenuClicked(object sender, EventArgs e)
    {
        await GoToAsync("//TestHubPage");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var currentRoute = CurrentState.Location.OriginalString;

        if (currentRoute.Contains("LoginPage") || currentRoute.Contains("TestHubPage"))
        {
            bool answer = await DisplayAlert("Выход", "Вы уверены, что хотите закрыть приложение?", "Да", "Нет");
            if (answer)
            {
                Application.Current?.Quit();
            }
        }
        else
        {
            bool answer = await DisplayAlert("Выход", "Вы уверены, что хотите выйти из аккаунта?", "Да", "Нет");
            if (answer)
            {
                await GoToAsync("//LoginPage");
            }
        }
    }
}