using VkusnoAndSochno.App.ViewModels;

namespace VkusnoAndSochno.App.Views
{
    public partial class CashierPage : ContentPage
    {
        private readonly CashierViewModel _viewModel;

        public CashierPage(CashierViewModel vm)
        {
            InitializeComponent();
            BindingContext = _viewModel = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadMenuAsync();
        }
    }
}