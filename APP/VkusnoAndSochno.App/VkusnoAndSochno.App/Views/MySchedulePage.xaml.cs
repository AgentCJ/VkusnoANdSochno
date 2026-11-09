using VkusnoAndSochno.App.ViewModels;

namespace VkusnoAndSochno.App.Views
{
    public partial class MySchedulePage : ContentPage
    {
        private readonly MyScheduleViewModel _viewModel;

        public MySchedulePage(MyScheduleViewModel vm)
        {
            InitializeComponent();
            BindingContext = _viewModel = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadShiftsAsync();
        }
    }
}