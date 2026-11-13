using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class ProductManagementViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly UserSession _session;

        // Список для отображения существующих продуктов
        [ObservableProperty] private ObservableCollection<StockBalanceDto> products = new();

        [ObservableProperty] private string newIngName = string.Empty;
        [ObservableProperty] private string newIngCost = string.Empty;
        [ObservableProperty] private string newIngMin = string.Empty;
        [ObservableProperty] private string newIngInitialQty = string.Empty;
        [ObservableProperty] private string selectedUnit = string.Empty;
        [ObservableProperty] private string statusMessage = string.Empty;

        public ObservableCollection<string> UnitsOfMeasure { get; } = new() { "шт", "кг", "г", "л", "мл", "уп" };

        public ProductManagementViewModel(ApiService apiService, UserSession session)
        {
            _apiService = apiService;
            _session = session;
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            try
            {
                var data = await _apiService.GetBalanceAsync(_session.BranchId);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Products = new ObservableCollection<StockBalanceDto>(data);
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка API", ex.Message, "ОК");
            }
        }

        [RelayCommand]
        public async Task SelectUnitAsync()
        {
            var result = await Shell.Current.DisplayActionSheet("Единица измерения", "Отмена", null, UnitsOfMeasure.ToArray());
            if (!string.IsNullOrEmpty(result) && result != "Отмена")
                SelectedUnit = result;
        }

        [RelayCommand]
        public async Task SaveProductAsync()
        {
            if (string.IsNullOrWhiteSpace(NewIngName) || string.IsNullOrWhiteSpace(SelectedUnit) ||
                !decimal.TryParse(NewIngCost, out var cost) || !decimal.TryParse(NewIngMin, out var min))
            {
                StatusMessage = "Заполните все обязательные поля корректными данными.";
                return;
            }

            // Проверка длины названия
            if (NewIngName.Length > 100)
            {
                StatusMessage = "Название продукта не должно превышать 100 символов.";
                return;
            }

            try
            {
                decimal.TryParse(NewIngInitialQty, out var initialQty);

                await _apiService.AddIngredientAsync(new
                {
                    IngredientName = NewIngName,
                    UnitOfMeasure = SelectedUnit,
                    CostPrice = cost,
                    MinRemaining = min,
                    InitialQuantity = initialQty,
                    BranchId = _session.BranchId,
                    EmployeeId = _session.EmployeeId
                });

                StatusMessage = "Продукт успешно добавлен!";

                NewIngName = string.Empty; SelectedUnit = string.Empty; NewIngCost = string.Empty; NewIngMin = string.Empty; NewIngInitialQty = string.Empty;

                // Обновляем список
                await LoadDataAsync();
            }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        [RelayCommand]
        public async Task DeleteProductAsync(StockBalanceDto product)
        {
            bool confirm = await Shell.Current.DisplayAlert("Удаление", $"Удалить '{product.IngredientName}' из системы?", "Да", "Нет");
            if (!confirm) return;

            try
            {
                await _apiService.DeleteIngredientAsync(product.IdIngredient);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                // Если продукт используется в рецептах, БД не даст его удалить
                await Shell.Current.DisplayAlert("Внимание", ex.Message, "ОК");
            }
        }
    }
}