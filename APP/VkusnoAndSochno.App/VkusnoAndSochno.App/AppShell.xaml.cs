using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App;

public partial class AppShell : Shell
{
    private string _previousRoute = "//LoginPage";

    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("MenuManagementPage", typeof(Views.MenuManagementPage));
        Routing.RegisterRoute("RecipePage", typeof(Views.RecipePage));
        Routing.RegisterRoute("OrderHistoryPage", typeof(Views.OrderHistoryPage));
        Routing.RegisterRoute("ProductManagementPage", typeof(Views.ProductManagementPage)); // ДОБАВЛЕНО
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        var session = IPlatformApplication.Current?.Services.GetService<UserSession>();
        var currentRoute = CurrentState.Location.OriginalString;

        bool isLoginPage = currentRoute.Contains("LoginPage");
        bool isTestHubPage = currentRoute.Contains("TestHubPage");
        bool isSchedulePage = currentRoute.Contains("MySchedulePage");
        bool isRecipePage = currentRoute.Contains("RecipePage");
        bool isMenuManagementPage = currentRoute.Contains("MenuManagementPage");
        bool isOrderHistoryPage = currentRoute.Contains("OrderHistoryPage");
        bool isProductManagementPage = currentRoute.Contains("ProductManagementPage"); // ДОБАВЛЕНО

        // 1. Управляем видимостью кнопки "Мой график"
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
        if (!isSchedulePage && !isLoginPage && !isRecipePage && !isMenuManagementPage && !isOrderHistoryPage && !isProductManagementPage)
        {
            _previousRoute = currentRoute;
        }

        // 3. Управляем текстом кнопки выхода
        if (isSchedulePage || isRecipePage || isMenuManagementPage || isOrderHistoryPage || isProductManagementPage)
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

        // 1. Если мы на рецепте, в управлении меню, истории ИЛИ создании продуктов — возвращаемся на шаг назад
        if (currentRoute.Contains("RecipePage") || currentRoute.Contains("MenuManagementPage") || currentRoute.Contains("OrderHistoryPage") || currentRoute.Contains("ProductManagementPage"))
        {
            await GoToAsync("..");
            return;
        }

        // 2. Если мы на графике — кнопка работает как "Назад"
        if (currentRoute.Contains("MySchedulePage"))
        {
            await GoToAsync(_previousRoute);
            return;
        }

        // 3. Если мы в тестовом режиме — возвращаем в меню теста
        if (session?.RoleName == "test" && !currentRoute.Contains("TestHubPage") && !currentRoute.Contains("LoginPage"))
        {
            await GoToAsync("//TestHubPage");
            return;
        }

        // 4. Стандартная логика выхода/закрытия
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