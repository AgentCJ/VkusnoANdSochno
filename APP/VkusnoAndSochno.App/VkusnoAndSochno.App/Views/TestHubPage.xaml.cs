namespace VkusnoAndSochno.App.Views
{
    public partial class TestHubPage : ContentPage
    {
        public TestHubPage()
        {
            InitializeComponent();
        }

        private async void OnCashierClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//CashierPage");
        private async void OnCookClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//CookPage");
        private async void OnStorekeeperClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//StorekeeperPage");
        private async void OnManagerClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//ManagerPage");

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}