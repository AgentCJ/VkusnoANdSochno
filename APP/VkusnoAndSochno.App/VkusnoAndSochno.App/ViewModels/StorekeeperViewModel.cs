using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;
using CommunityToolkit.Maui.Storage;
using Xceed.Words.NET;
using Xceed.Document.NET;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class ReceiveItemDto : ObservableObject
    {
        public int IdIngredient { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        [ObservableProperty] private string quantityToAdd = string.Empty;
    }

    public partial class RequestItemDto : ObservableObject
    {
        public int IdIngredient { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        [ObservableProperty] private string requestedQuantity = string.Empty;
    }

    public partial class StorekeeperViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly UserSession _session;
        private int? _adjustIngredientId = null;

        [ObservableProperty] private ObservableCollection<StockBalanceDto> stockList = new();
        [ObservableProperty] private ObservableCollection<StockBalanceDto> filteredStockList = new();

        // Поля для поиска и сортировки
        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private string selectedSort = "По названию (А-Я)";

        public ObservableCollection<string> SortOptions { get; } = new()
        {
            "По названию (А-Я)",
            "По остатку (возр.)",
            "По остатку (убыв.)"
        };

        [ObservableProperty] private ObservableCollection<IngredientDto> ingredients = new();
        [ObservableProperty] private ObservableCollection<SupplierDto> suppliers = new();

        [ObservableProperty] private bool isAdjustVisible = false;
        [ObservableProperty] private string adjustProductName = string.Empty;
        [ObservableProperty] private string adjustFactQty = string.Empty;

        [ObservableProperty] private bool isReceiveSupplyVisible = false;
        [ObservableProperty] private ObservableCollection<ReceiveItemDto> receiveList = new();
        [ObservableProperty] private SupplierDto? selectedSupplierForReceive;

        [ObservableProperty] private bool isRequestVisible = false;
        [ObservableProperty] private ObservableCollection<RequestItemDto> requestList = new();
        [ObservableProperty] private SupplierDto? selectedSupplierForRequest;

        public StorekeeperViewModel(ApiService apiService, UserSession session)
        {
            _apiService = apiService;
            _session = session;
        }

        partial void OnSearchTextChanged(string value) => ApplyFilterAndSort();

        // КОМАНДА ДЛЯ ОТКРЫТИЯ МЕНЮ СОРТИРОВКИ
        [RelayCommand]
        public async Task ChangeSortAsync()
        {
            var result = await Shell.Current.DisplayActionSheet("Выберите сортировку", "Отмена", null, SortOptions.ToArray());
            if (!string.IsNullOrEmpty(result) && result != "Отмена")
            {
                SelectedSort = result;
                ApplyFilterAndSort();
            }
        }

        private void ApplyFilterAndSort()
        {
            var result = StockList.AsEnumerable();

            // Поиск
            if (!string.IsNullOrWhiteSpace(SearchText))
                result = result.Where(x => x.IngredientName.ToLower().Contains(SearchText.ToLower()));

            // Сортировка
            result = SelectedSort switch
            {
                "По остатку (возр.)" => result.OrderBy(x => x.Balance),
                "По остатку (убыв.)" => result.OrderByDescending(x => x.Balance),
                _ => result.OrderBy(x => x.IngredientName) // По умолчанию (и по названию А-Я)
            };

            FilteredStockList = new ObservableCollection<StockBalanceDto>(result);
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            try
            {
                var stock = await _apiService.GetBalanceAsync(_session.BranchId);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StockList = new ObservableCollection<StockBalanceDto>(stock);
                    ApplyFilterAndSort();
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
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка API", ex.Message, "ОК"); }
        }

        // --- ИНВЕНТАРИЗАЦИЯ ---
        [RelayCommand]
        public void OpenAdjust(StockBalanceDto item)
        {
            _adjustIngredientId = item.IdIngredient;
            AdjustProductName = item.IngredientName;
            AdjustFactQty = item.Balance.ToString("0.###");
            IsAdjustVisible = true;
        }

        [RelayCommand] public void CloseAdjust() => IsAdjustVisible = false;

        [RelayCommand]
        public async Task SaveAdjustAsync()
        {
            if (!decimal.TryParse(AdjustFactQty, out var factQty) || factQty < 0)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите корректное количество", "ОК"); return;
            }

            try
            {
                await _apiService.AdjustStockAsync(new { EmployeeId = _session.EmployeeId, BranchId = _session.BranchId, IngredientId = _adjustIngredientId, FactQuantity = factQty });
                CloseAdjust(); await LoadDataAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }

        // --- ОКНО ПОСТАВКИ ---
        [RelayCommand]
        public async Task OpenReceiveSupply()
        {
            var ings = await _apiService.GetIngredientsAsync();
            ReceiveList = new ObservableCollection<ReceiveItemDto>(ings.Select(i => new ReceiveItemDto { IdIngredient = i.IdIngredient, IngredientName = i.IngredientName, UnitOfMeasure = i.UnitOfMeasure }));
            SelectedSupplierForReceive = null; IsReceiveSupplyVisible = true;
        }

        [RelayCommand] public void CloseReceiveSupply() => IsReceiveSupplyVisible = false;

        [RelayCommand]
        public async Task ApplyReceiveSupplyAsync()
        {
            if (SelectedSupplierForReceive == null) { await Shell.Current.DisplayAlert("Ошибка", "Выберите поставщика", "ОК"); return; }
            var itemsToReceive = ReceiveList.Where(x => !string.IsNullOrWhiteSpace(x.QuantityToAdd) && decimal.TryParse(x.QuantityToAdd, out var q) && q > 0).ToList();
            if (itemsToReceive.Count == 0) { await Shell.Current.DisplayAlert("Ошибка", "Вы не указали количество", "ОК"); return; }

            try
            {
                foreach (var item in itemsToReceive)
                    await _apiService.RegisterSupplyAsync(new { EmployeeId = _session.EmployeeId, BranchId = _session.BranchId, SupplierId = SelectedSupplierForReceive.IdSupplier, IngredientId = item.IdIngredient, Quantity = decimal.Parse(item.QuantityToAdd), Comment = "Приемка" });

                CloseReceiveSupply(); await LoadDataAsync(); await Shell.Current.DisplayAlert("Успех", $"Принято {itemsToReceive.Count} позиций", "ОК");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }

        // --- ОКНО ЗАЯВКИ ---
        [RelayCommand]
        public async Task OpenRequest()
        {
            var ings = await _apiService.GetIngredientsAsync();
            RequestList = new ObservableCollection<RequestItemDto>(ings.Select(i => new RequestItemDto { IdIngredient = i.IdIngredient, IngredientName = i.IngredientName, UnitOfMeasure = i.UnitOfMeasure }));
            SelectedSupplierForRequest = null; IsRequestVisible = true;
        }

        [RelayCommand] public void CloseRequest() => IsRequestVisible = false;

        [RelayCommand]
        public async Task GenerateDocxAsync()
        {
            if (SelectedSupplierForRequest == null) { await Shell.Current.DisplayAlert("Ошибка", "Выберите поставщика", "ОК"); return; }
            var itemsToRequest = RequestList.Where(x => !string.IsNullOrWhiteSpace(x.RequestedQuantity) && decimal.TryParse(x.RequestedQuantity, out var q) && q > 0).ToList();
            if (itemsToRequest.Count == 0) { await Shell.Current.DisplayAlert("Ошибка", "Вы не указали количество", "ОК"); return; }

            try
            {
                using var stream = new MemoryStream();
                using (var document = DocX.Create(stream))
                {
                    var title = document.InsertParagraph("Заявка на поставку");
                    title.FontSize(22).Bold().Alignment = Alignment.center;
                    var info = document.InsertParagraph($"\nПоставщик: {SelectedSupplierForRequest.SupplierName}\nДата: {DateTime.Now:dd.MM.yyyy}\n").FontSize(12);
                    var table = document.AddTable(itemsToRequest.Count + 1, 3);
                    table.Design = TableDesign.TableGrid;
                    table.Rows[0].Cells[0].Paragraphs[0].Append("Наименование").Bold();
                    table.Rows[0].Cells[1].Paragraphs[0].Append("Количество").Bold();
                    table.Rows[0].Cells[2].Paragraphs[0].Append("Ед. изм.").Bold();
                    for (int i = 0; i < itemsToRequest.Count; i++)
                    {
                        table.Rows[i + 1].Cells[0].Paragraphs[0].Append(itemsToRequest[i].IngredientName);
                        table.Rows[i + 1].Cells[1].Paragraphs[0].Append(itemsToRequest[i].RequestedQuantity);
                        table.Rows[i + 1].Cells[2].Paragraphs[0].Append(itemsToRequest[i].UnitOfMeasure);
                    }
                    document.InsertTable(table);
                    document.Save();
                }
                stream.Position = 0;
                string safeSupplierName = string.Join("_", SelectedSupplierForRequest.SupplierName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"Заявка_{safeSupplierName}_{DateTime.Now:yyyyMMdd_HHmm}.docx";
                var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);
                if (fileSaverResult.IsSuccessful)
                {
                    await Shell.Current.DisplayAlert("Успех", $"Сохранено:\n{fileSaverResult.FilePath}", "ОК"); CloseRequest();
                }
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }
    }
}