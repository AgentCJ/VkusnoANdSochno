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
            await _viewModel.LoadDashboardAsync(); // Загружаем сводку
            await _viewModel.GenerateReportAsync(); // Загружаем отчет по умолчанию
        }
    }
}