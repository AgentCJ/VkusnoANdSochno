using VkusnoAndSochno.App.ViewModels;

namespace VkusnoAndSochno.App.Views
{
    public partial class ScheduleManagementPage : ContentPage
    {
        private readonly ScheduleManagementViewModel _viewModel;

        public ScheduleManagementPage(ScheduleManagementViewModel vm)
        {
            InitializeComponent();
            BindingContext = _viewModel = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDataAsync();
        }
    }
}