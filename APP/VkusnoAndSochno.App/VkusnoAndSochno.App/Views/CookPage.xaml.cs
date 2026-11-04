using VkusnoAndSochno.App.ViewModels;

namespace VkusnoAndSochno.App.Views
{
    public partial class CookPage : ContentPage
    {
        private readonly CookViewModel _viewModel;

        public CookPage(CookViewModel vm)
        {
            InitializeComponent();
            BindingContext = _viewModel = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadOrdersAsync(); // Автозагрузка при открытии
        }
    }
}