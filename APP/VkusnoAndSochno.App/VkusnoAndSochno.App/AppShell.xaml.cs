using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App;

public partial class AppShell : Shell
{
    private string _previousRoute = "//LoginPage";

    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("MenuManagementPage", typeof(Views.MenuManagementPage));

    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        var session = IPlatformApplication.Current?.Services.GetService<UserSession>();
        var currentRoute = CurrentState.Location.OriginalString;

        bool isLoginPage = currentRoute.Contains("LoginPage");
        bool isTestHubPage = currentRoute.Contains("TestHubPage");
        bool isSchedulePage = currentRoute.Contains("MySchedulePage");

        // 1. Управляем видимостью кнопки "Мой график"
        // Скрываем её на экране входа и в меню тестов
        if (isLoginPage || isTestHubPage)
        {
            if (ToolbarItems.Contains(ScheduleBtn))
                ToolbarItems.Remove(ScheduleBtn);
        }
        else
        {
            if (!ToolbarItems.Contains(ScheduleBtn))
                ToolbarItems.Add(ScheduleBtn);
        }

        // 2. Запоминаем маршрут для кнопки "Назад"
        if (!isSchedulePage && !isLoginPage)
        {
            _previousRoute = currentRoute;
        }

        // 3. Управляем текстом кнопки выхода
        if (isSchedulePage)
        {
            LogoutBtn.Text = "Назад";
        }
        else if (session?.RoleName == "test" && !isTestHubPage && !isLoginPage)
        {
            LogoutBtn.Text = "В меню теста";
        }
        else
        {
            LogoutBtn.Text = "Выйти";
        }
    }

    private async void OnScheduleClicked(object sender, EventArgs e)
    {
        await GoToAsync("//MySchedulePage");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var currentRoute = CurrentState.Location.OriginalString;
        var session = IPlatformApplication.Current?.Services.GetService<UserSession>();

        // Если мы на графике — кнопка работает как "Назад"
        if (currentRoute.Contains("MySchedulePage"))
        {
            await GoToAsync(_previousRoute);
            return;
        }

        // Если мы в тестовом режиме — возвращаем в меню теста
        if (session?.RoleName == "test" && !currentRoute.Contains("TestHubPage") && !currentRoute.Contains("LoginPage"))
        {
            await GoToAsync("//TestHubPage");
            return;
        }

        // Стандартная логика выхода/закрытия
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