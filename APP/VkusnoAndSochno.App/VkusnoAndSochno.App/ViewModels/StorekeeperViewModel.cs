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

        [ObservableProperty]
        private ObservableCollection<StockBalanceDto> stockList = new();

        [ObservableProperty]
        private ObservableCollection<IngredientDto> ingredients = new();

        [ObservableProperty]
        private ObservableCollection<SupplierDto> suppliers = new();

        [ObservableProperty]
        private IngredientDto? selectedIngredient;

        [ObservableProperty]
        private SupplierDto? selectedSupplier;

        [ObservableProperty]
        private string quantityText = string.Empty;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        public StorekeeperViewModel(ApiService apiService, UserSession session)
        {
            _apiService = apiService;
            _session = session;
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            try
            {
                var stock = await _apiService.GetBalanceAsync(_session.BranchId);
                StockList.Clear();
                foreach (var item in stock) StockList.Add(item);

                if (Ingredients.Count == 0)
                {
                    var ings = await _apiService.GetIngredientsAsync();
                    foreach (var i in ings) Ingredients.Add(i);

                    var sups = await _apiService.GetSuppliersAsync();
                    foreach (var s in sups) Suppliers.Add(s);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК");
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