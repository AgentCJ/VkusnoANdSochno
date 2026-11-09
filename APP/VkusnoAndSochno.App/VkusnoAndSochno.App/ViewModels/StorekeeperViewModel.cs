using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class ReceiveItemDto : ObservableObject
    {
        public int IdIngredient { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;

        [ObservableProperty]
        private string quantityToAdd = string.Empty;
    }

    public partial class StorekeeperViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly UserSession _session;
        private int? _editingIngredientId = null;

        [ObservableProperty] private ObservableCollection<StockBalanceDto> stockList = new();
        [ObservableProperty] private ObservableCollection<IngredientDto> ingredients = new();
        [ObservableProperty] private ObservableCollection<SupplierDto> suppliers = new();

        [ObservableProperty] private bool isAddProductVisible = false;
        [ObservableProperty] private bool isReceiveSupplyVisible = false;
        [ObservableProperty] private bool isEditingMode = false; // Флаг для скрытия поля "Сейчас на складе"

        [ObservableProperty] private string formTitle = "Создание продукта";
        [ObservableProperty] private string actionButtonText = "💾 Сохранить";
        [ObservableProperty] private string newIngName = string.Empty;
        [ObservableProperty] private string newIngCost = string.Empty;
        [ObservableProperty] private string newIngMin = string.Empty;
        [ObservableProperty] private string newIngInitialQty = string.Empty;
        [ObservableProperty] private string selectedUnit = string.Empty;

        public ObservableCollection<string> UnitsOfMeasure { get; } = new() { "шт", "кг", "г", "л", "мл", "уп" };

        [ObservableProperty] private ObservableCollection<ReceiveItemDto> receiveList = new();
        [ObservableProperty] private SupplierDto? selectedSupplierForReceive;

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
                MainThread.BeginInvokeOnMainThread(() => StockList = new ObservableCollection<StockBalanceDto>(stock));

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
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка API", ex.Message, "ОК"); }
        }

        // --- ОКНО ПРОДУКТА ---
        [RelayCommand]
        public void OpenAddProduct()
        {
            _editingIngredientId = null;
            NewIngName = string.Empty; SelectedUnit = string.Empty; NewIngCost = string.Empty; NewIngMin = string.Empty; NewIngInitialQty = string.Empty;
            FormTitle = "Создание продукта";
            ActionButtonText = "💾 Сохранить";
            IsEditingMode = false;
            IsAddProductVisible = true;
        }

        [RelayCommand]
        public void EditProduct(StockBalanceDto item)
        {
            _editingIngredientId = item.IdIngredient;
            NewIngName = item.IngredientName;
            SelectedUnit = item.UnitOfMeasure;
            NewIngCost = item.CostPrice.ToString("0.##");
            NewIngMin = item.MinRemaining.ToString("0.###");
            NewIngInitialQty = "0";

            FormTitle = "Редактирование продукта";
            ActionButtonText = "💾 Обновить";
            IsEditingMode = true;
            IsAddProductVisible = true;
        }

        [RelayCommand]
        public void CloseAddProduct() => IsAddProductVisible = false;

        // Выбор единицы измерения
        [RelayCommand]
        public async Task SelectUnitAsync()
        {
            var result = await Shell.Current.DisplayActionSheet("Единица измерения", "Отмена", null, UnitsOfMeasure.ToArray());
            if (!string.IsNullOrEmpty(result) && result != "Отмена") SelectedUnit = result;
        }

        [RelayCommand]
        public async Task SaveProductAsync()
        {
            if (string.IsNullOrWhiteSpace(NewIngName) || string.IsNullOrWhiteSpace(SelectedUnit) ||
                !decimal.TryParse(NewIngCost, out var cost) || !decimal.TryParse(NewIngMin, out var min))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Заполните обязательные поля корректными данными", "ОК");
                return;
            }

            try
            {
                if (_editingIngredientId.HasValue)
                {
                    await _apiService.UpdateIngredientAsync(_editingIngredientId.Value, new { IngredientName = NewIngName, UnitOfMeasure = SelectedUnit, CostPrice = cost, MinRemaining = min });
                }
                else
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
                }

                CloseAddProduct();
                await LoadDataAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Внимание", ex.Message, "ОК"); }
        }

        [RelayCommand]
        public async Task DeleteProductAsync(StockBalanceDto item)
        {
            bool confirm = await Shell.Current.DisplayAlert("Удаление", $"Удалить '{item.IngredientName}'?", "Да", "Нет");
            if (!confirm) return;
            try { await _apiService.DeleteIngredientAsync(item.IdIngredient); await LoadDataAsync(); }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Внимание", ex.Message, "ОК"); }
        }

        // --- ОКНО МАССОВОЙ ПОСТАВКИ ---
        [RelayCommand]
        public async Task OpenReceiveSupply()
        {
            var ings = await _apiService.GetIngredientsAsync();
            var receiveData = ings.Select(i => new ReceiveItemDto { IdIngredient = i.IdIngredient, IngredientName = i.IngredientName, UnitOfMeasure = i.UnitOfMeasure }).ToList();

            ReceiveList = new ObservableCollection<ReceiveItemDto>(receiveData);
            SelectedSupplierForReceive = null;
            IsReceiveSupplyVisible = true;
        }

        [RelayCommand]
        public void CloseReceiveSupply() => IsReceiveSupplyVisible = false;

        [RelayCommand]
        public async Task ApplyReceiveSupplyAsync()
        {
            if (SelectedSupplierForReceive == null)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Выберите поставщика для этой поставки", "ОК");
                return;
            }

            var itemsToReceive = ReceiveList.Where(x => !string.IsNullOrWhiteSpace(x.QuantityToAdd) && decimal.TryParse(x.QuantityToAdd, out var q) && q > 0).ToList();

            if (itemsToReceive.Count == 0)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Вы не указали количество ни для одного продукта", "ОК");
                return;
            }

            try
            {
                foreach (var item in itemsToReceive)
                {
                    await _apiService.RegisterSupplyAsync(new
                    {
                        EmployeeId = _session.EmployeeId,
                        BranchId = _session.BranchId,
                        SupplierId = SelectedSupplierForReceive.IdSupplier,
                        IngredientId = item.IdIngredient,
                        Quantity = decimal.Parse(item.QuantityToAdd),
                        Comment = "Массовая приемка товара"
                    });
                }

                CloseReceiveSupply();
                await LoadDataAsync();
                await Shell.Current.DisplayAlert("Успех", $"Принято {itemsToReceive.Count} позиций на склад", "ОК");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }
    }
}