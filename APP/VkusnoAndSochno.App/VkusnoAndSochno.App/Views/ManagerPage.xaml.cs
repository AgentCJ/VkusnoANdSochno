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
            // Автоматически формируем отчет при открытии экрана
            await _viewModel.GenerateReportAsync();
        }
    }
}