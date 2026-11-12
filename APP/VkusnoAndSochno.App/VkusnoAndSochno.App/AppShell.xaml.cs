using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App;

public partial class AppShell : Shell
{
    private string _previousRoute = "//LoginPage";

    public AppShell()
    {
        InitializeComponent();

        // Регистрация маршрутов для страниц, вызываемых программно
        Routing.RegisterRoute("MenuManagementPage", typeof(Views.MenuManagementPage));
        Routing.RegisterRoute("RecipePage", typeof(Views.RecipePage));
        Routing.RegisterRoute("OrderHistoryPage", typeof(Views.OrderHistoryPage));
        Routing.RegisterRoute("ProductManagementPage", typeof(Views.ProductManagementPage));
        Routing.RegisterRoute("ScheduleManagementPage", typeof(Views.ScheduleManagementPage));
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        var session = IPlatformApplication.Current?.Services.GetService<UserSession>();
        var currentRoute = CurrentState.Location.OriginalString;

        // Определяем текущую страницу для управления кнопками навигации
        bool isLoginPage = currentRoute.Contains("LoginPage");
        bool isTestHubPage = currentRoute.Contains("TestHubPage");
        bool isSchedulePage = currentRoute.Contains("MySchedulePage");
        bool isRecipePage = currentRoute.Contains("RecipePage");
        bool isMenuManagementPage = currentRoute.Contains("MenuManagementPage");
        bool isOrderHistoryPage = currentRoute.Contains("OrderHistoryPage");
        bool isProductManagementPage = currentRoute.Contains("ProductManagementPage");
        bool isScheduleManagementPage = currentRoute.Contains("ScheduleManagementPage");

        // Управление видимостью кнопки "Мой график"
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

        // Запоминаем предыдущий маршрут для корректного возврата
        if (!isSchedulePage && !isLoginPage && !isRecipePage && !isMenuManagementPage && !isOrderHistoryPage && !isProductManagementPage && !isScheduleManagementPage)
        {
            _previousRoute = currentRoute;
        }

        // Настройка текста кнопки выхода/назад в зависимости от экрана
        if (isSchedulePage || isRecipePage || isMenuManagementPage || isOrderHistoryPage || isProductManagementPage || isScheduleManagementPage)
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

        // Возврат на шаг назад из детализированных экранов
        if (currentRoute.Contains("RecipePage") || currentRoute.Contains("MenuManagementPage") || currentRoute.Contains("OrderHistoryPage") || currentRoute.Contains("ProductManagementPage") || currentRoute.Contains("ScheduleManagementPage"))
        {
            await GoToAsync("..");
            return;
        }

        // Возврат с графика на страницу, с которой был вызван
        if (currentRoute.Contains("MySchedulePage"))
        {
            await GoToAsync(_previousRoute);
            return;
        }

        // Возврат в меню тестирования ролей
        if (session?.RoleName == "test" && !currentRoute.Contains("TestHubPage") && !currentRoute.Contains("LoginPage"))
        {
            await GoToAsync("//TestHubPage");
            return;
        }

        // Стандартное закрытие приложения или выход из аккаунта
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