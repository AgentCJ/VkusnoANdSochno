using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class StorekeeperViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly UserSession _session;

        [ObservableProperty] private ObservableCollection<StockBalanceDto> stockList = new();
        [ObservableProperty] private ObservableCollection<IngredientDto> ingredients = new();
        [ObservableProperty] private ObservableCollection<SupplierDto> suppliers = new();
        [ObservableProperty] private IngredientDto? selectedIngredient;
        [ObservableProperty] private SupplierDto? selectedSupplier;
        [ObservableProperty] private string quantityText = string.Empty;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isRefreshing;

        public StorekeeperViewModel(ApiService apiService, UserSession session)
        {
            _apiService = apiService;
            _session = session;
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsRefreshing = true;
            try
            {
                var stock = await _apiService.GetBalanceAsync(_session.BranchId);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StockList = new ObservableCollection<StockBalanceDto>(stock);
                });

                if (Ingredients.Count == 0)
                {
                    var ings = await _apiService.GetIngredientsAsync();
                    var sups = await _apiService.GetSuppliersAsync();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Ingredients = new ObservableCollection<IngredientDto>(ings);
                        Suppliers = new ObservableCollection<SupplierDto>(sups);
                    });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка API", ex.Message, "ОК");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task RegisterSupplyAsync()
        {
            if (SelectedIngredient == null || SelectedSupplier == null || !decimal.TryParse(QuantityText, out var qty) || qty <= 0)
            {
                StatusMessage = "Выберите ингредиент, поставщика и введите корректное количество.";
                return;
            }

            try
            {
                var request = new
                {
                    EmployeeId = _session.EmployeeId,
                    BranchId = _session.BranchId,
                    SupplierId = SelectedSupplier.IdSupplier,
                    IngredientId = SelectedIngredient.IdIngredient,
                    Quantity = qty,
                    Comment = "Поставка через АРМ кладовщика"
                };

                await _apiService.RegisterSupplyAsync(request);
                StatusMessage = $"Успешно: {SelectedIngredient.IngredientName} +{qty}";
                QuantityText = string.Empty;

                await LoadDataAsync(); // Обновляем таблицу остатков
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }
    }
}