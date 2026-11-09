using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly UserSession _session;

        public LoginPage(ApiService apiService, UserSession session)
        {
            InitializeComponent();
            _apiService = apiService;
            _session = session;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            ErrorLabel.Text = string.Empty;
            var login = LoginEntry.Text?.Trim();
            var pass = PasswordEntry.Text?.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
            {
                ErrorLabel.Text = "Введите логин и пароль";
                return;
            }

            var result = await _apiService.LoginAsync(login, pass);

            if (result == null)
            {
                ErrorLabel.Text = "Неверный логин или пароль!";
                return;
            }

            _session.EmployeeId = result.EmployeeId;
            _session.BranchId = result.BranchId;
            _session.RoleName = result.RoleName;

            if (result.RoleName == "cashier")
                await Shell.Current.GoToAsync("//CashierPage");
            else if (result.RoleName == "cook")
                await Shell.Current.GoToAsync("//CookPage");
            else if (result.RoleName == "storekeeper")
                await Shell.Current.GoToAsync("//StorekeeperPage");
            else if (result.RoleName == "manager")
                await Shell.Current.GoToAsync("//ManagerPage");
            else if (result.RoleName == "test")
                await Shell.Current.GoToAsync("//TestHubPage");
            else if (result.RoleName == "administrator")
                await Shell.Current.GoToAsync("//AdminPage");
            else
                await DisplayAlert("Успех", $"Вы вошли как {result.RoleName}. Экран в разработке.", "ОК");
        }

        // НОВЫЙ ОБРАБОТЧИК ДЛЯ КНОПКИ ТЕСТА
        private async void OnTestModeClicked(object sender, EventArgs e)
        {
            // Заполняем сессию тестовыми данными
            _session.EmployeeId = 1;
            _session.BranchId = 1;
            _session.RoleName = "test";

            // Сразу переходим в хаб выбора ролей
            await Shell.Current.GoToAsync("//TestHubPage");
        }
    }
}