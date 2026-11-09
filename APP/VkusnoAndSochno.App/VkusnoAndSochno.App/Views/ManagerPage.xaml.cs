using VkusnoAndSochno.App.ViewModels;

namespace VkusnoAndSochno.App.Views
{
    public partial class ManagerPage : ContentPage
    {
        private readonly ManagerViewModel _viewModel;

        public ManagerPage(ManagerViewModel vm)
        {
            InitializeComponent();
            BindingContext = _viewModel = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDashboardAsync();
            await _viewModel.GenerateReportAsync();
        }

        private async void OnMenuManagementClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("MenuManagementPage");
        }
    }
}