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

            // Сохраняем данные в сессию
            _session.EmployeeId = result.EmployeeId;
            _session.BranchId = result.BranchId;
            _session.RoleName = result.RoleName;

            // Перенаправляем в зависимости от роли
            if (result.RoleName == "cashier")
            {
                await Shell.Current.GoToAsync("//CashierPage");
            }
            else if (result.RoleName == "cook")
            {
                await Shell.Current.GoToAsync("//CookPage");
            }
            else if (result.RoleName == "storekeeper")
            {
                await Shell.Current.GoToAsync("//StorekeeperPage");
            }
            else
            {
                await DisplayAlert("Успех", $"Вы вошли как {result.RoleName}. Экран в разработке.", "ОК");
            }

        }
    }
}