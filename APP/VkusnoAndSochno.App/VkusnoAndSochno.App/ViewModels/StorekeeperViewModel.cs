using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;
using CommunityToolkit.Maui.Storage;
using Xceed.Words.NET;
using Xceed.Document.NET;

namespace VkusnoAndSochno.App.ViewModels
{
    // Класс для окна "Принять поставку"
    public partial class ReceiveItemDto : ObservableObject
    {
        public int IdIngredient { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;

        [ObservableProperty]
        private string quantityToAdd = string.Empty;
    }

    // Класс для окна "Заявка поставщику"
    public partial class RequestItemDto : ObservableObject
    {
        public int IdIngredient { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;

        [ObservableProperty]
        private string requestedQuantity = string.Empty;
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
        [ObservableProperty] private bool isRequestVisible = false;
        [ObservableProperty] private bool isEditingMode = false;

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

        [ObservableProperty] private ObservableCollection<RequestItemDto> requestList = new();
        [ObservableProperty] private SupplierDto? selectedSupplierForRequest;

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
            FormTitle = "Создание продукта"; ActionButtonText = "💾 Сохранить";
            IsEditingMode = false; IsAddProductVisible = true;
        }

        [RelayCommand]
        public void EditProduct(StockBalanceDto item)
        {
            _editingIngredientId = item.IdIngredient;
            NewIngName = item.IngredientName; SelectedUnit = item.UnitOfMeasure;
            NewIngCost = item.CostPrice.ToString("0.##"); NewIngMin = item.MinRemaining.ToString("0.###");
            NewIngInitialQty = "0";
            FormTitle = "Редактирование продукта"; ActionButtonText = "💾 Обновить";
            IsEditingMode = true; IsAddProductVisible = true;
        }

        [RelayCommand] public void CloseAddProduct() => IsAddProductVisible = false;

        [RelayCommand]
        public async Task SelectUnitAsync()
        {
            var result = await Shell.Current.DisplayActionSheet("Единица измерения", "Отмена", null, UnitsOfMeasure.ToArray());
            if (!string.IsNullOrEmpty(result) && result != "Отмена") SelectedUnit = result;
        }

        [RelayCommand]
        public async Task SaveProductAsync()
        {
            if (string.IsNullOrWhiteSpace(NewIngName) || string.IsNullOrWhiteSpace(SelectedUnit) || !decimal.TryParse(NewIngCost, out var cost) || !decimal.TryParse(NewIngMin, out var min))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Заполните обязательные поля корректными данными", "ОК"); return;
            }

            try
            {
                if (_editingIngredientId.HasValue)
                    await _apiService.UpdateIngredientAsync(_editingIngredientId.Value, new { IngredientName = NewIngName, UnitOfMeasure = SelectedUnit, CostPrice = cost, MinRemaining = min });
                else
                {
                    decimal.TryParse(NewIngInitialQty, out var initialQty);
                    await _apiService.AddIngredientAsync(new { IngredientName = NewIngName, UnitOfMeasure = SelectedUnit, CostPrice = cost, MinRemaining = min, InitialQuantity = initialQty, BranchId = _session.BranchId, EmployeeId = _session.EmployeeId });
                }
                CloseAddProduct(); await LoadDataAsync();
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

        // --- ОКНО ЗАЯВКИ ПОСТАВЩИКУ ---
        [RelayCommand]
        public async Task OpenRequest()
        {
            var ings = await _apiService.GetIngredientsAsync();
            RequestList = new ObservableCollection<RequestItemDto>(ings.Select(i => new RequestItemDto { IdIngredient = i.IdIngredient, IngredientName = i.IngredientName, UnitOfMeasure = i.UnitOfMeasure }));
            SelectedSupplierForRequest = null; IsRequestVisible = true;
        }

        [RelayCommand] public void CloseRequest() => IsRequestVisible = false;

        // ГЕНЕРАЦИЯ DOCX
        [RelayCommand]
        public async Task GenerateDocxAsync()
        {
            if (SelectedSupplierForRequest == null) { await Shell.Current.DisplayAlert("Ошибка", "Выберите поставщика", "ОК"); return; }

            var itemsToRequest = RequestList.Where(x => !string.IsNullOrWhiteSpace(x.RequestedQuantity) && decimal.TryParse(x.RequestedQuantity, out var q) && q > 0).ToList();

            if (itemsToRequest.Count == 0) { await Shell.Current.DisplayAlert("Ошибка", "Вы не указали количество ни для одного продукта", "ОК"); return; }

            try
            {
                using var stream = new MemoryStream();

                using (var document = DocX.Create(stream))
                {
                    var title = document.InsertParagraph("Заявка на поставку");
                    title.FontSize(22).Bold().Alignment = Alignment.center;

                    var info = document.InsertParagraph($"\nПоставщик: {SelectedSupplierForRequest.SupplierName}\nДата формирования: {DateTime.Now:dd.MM.yyyy}\n");
                    info.FontSize(12);

                    var table = document.AddTable(itemsToRequest.Count + 1, 3);
                    table.Design = TableDesign.TableGrid;

                    table.Rows[0].Cells[0].Paragraphs[0].Append("Наименование продукта").Bold();
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
                    await Shell.Current.DisplayAlert("Успех", $"Заявка сохранена по пути:\n{fileSaverResult.FilePath}", "ОК");
                    CloseRequest();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка генерации", ex.Message, "ОК");
            }
        }
    }
}