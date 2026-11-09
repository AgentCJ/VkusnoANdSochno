using VkusnoAndSochno.App.ViewModels;

namespace VkusnoAndSochno.App.Views
{
    public partial class RecipePage : ContentPage
    {
        private readonly RecipeViewModel _viewModel;

        public RecipePage(RecipeViewModel vm)
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