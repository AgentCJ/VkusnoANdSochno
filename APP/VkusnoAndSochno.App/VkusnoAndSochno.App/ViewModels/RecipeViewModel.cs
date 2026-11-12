using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    // Атрибуты QueryProperty используются для получения параметров навигации
    [QueryProperty(nameof(DishId), "dishId")]
    [QueryProperty(nameof(DishName), "dishName")]
    public partial class RecipeViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty] private int dishId;
        [ObservableProperty] private string dishName = string.Empty;

        [ObservableProperty] private ObservableCollection<RecipeItemDto> recipeItems = new();
        [ObservableProperty] private ObservableCollection<IngredientDto> ingredients = new();

        [ObservableProperty] private IngredientDto? selectedIngredient;
        [ObservableProperty] private string quantityText = string.Empty;
        [ObservableProperty] private bool isRefreshing;

        public RecipeViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsRefreshing = true;
            try
            {
                if (Ingredients.Count == 0)
                {
                    var ings = await _apiService.GetIngredientsAsync();
                    Ingredients = new ObservableCollection<IngredientDto>(ings);
                }

                var recipe = await _apiService.GetRecipeAsync(DishId);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    RecipeItems = new ObservableCollection<RecipeItemDto>(recipe);
                });
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
            finally { IsRefreshing = false; }
        }

        [RelayCommand]
        public async Task AddIngredientAsync()
        {
            if (SelectedIngredient == null || !decimal.TryParse(QuantityText, out var qty) || qty <= 0)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Выберите ингредиент и введите корректное количество", "ОК");
                return;
            }

            try
            {
                await _apiService.AddRecipeItemAsync(DishId, new { IdIngredient = SelectedIngredient.IdIngredient, Quantity = qty });
                QuantityText = string.Empty;
                SelectedIngredient = null;
                await LoadDataAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }

        [RelayCommand]
        public async Task RemoveIngredientAsync(RecipeItemDto item)
        {
            try
            {
                await _apiService.RemoveRecipeItemAsync(DishId, item.IdIngredient);
                await LoadDataAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }
    }
}