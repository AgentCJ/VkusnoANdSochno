using VkusnoAndSochno.App.ViewModels;

namespace VkusnoAndSochno.App.Views
{
    public partial class MenuManagementPage : ContentPage
    {
        private readonly MenuManagementViewModel _viewModel;

        public MenuManagementPage(MenuManagementViewModel vm)
        {
            InitializeComponent();
            BindingContext = _viewModel = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDishesAsync();
        }
    }
}