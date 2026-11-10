using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class CashierViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly UserSession _session;
        private int _currentOrderId;

        [ObservableProperty] private string statusMessage = "Нажмите 'Новый заказ' для начала работы";
        [ObservableProperty] private ObservableCollection<DishDto> menu = new();
        [ObservableProperty] private ObservableCollection<DishDto> filteredMenu = new();
        [ObservableProperty] private ObservableCollection<CartItemDto> cart = new();
        [ObservableProperty] private decimal totalAmount = 0;
        [ObservableProperty] private bool isOrderCreated = false;
        [ObservableProperty] private string globalQuantity = "1";

        // Поиск и Сортировка
        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private string selectedSort = "По умолчанию";

        public ObservableCollection<string> SortOptions { get; } = new()
        {
            "По умолчанию",
            "По названию (А-Я)",
            "По цене (возр.)",
            "По цене (убыв.)",
            "По категории"
        };

        public CashierViewModel(ApiService apiService, UserSession session)
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
            var result = Menu.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                result = result.Where(x => x.DishName.ToLower().Contains(SearchText.ToLower()));

            result = SelectedSort switch
            {
                "По названию (А-Я)" => result.OrderBy(x => x.DishName),
                "По цене (возр.)" => result.OrderBy(x => x.Price),
                "По цене (убыв.)" => result.OrderByDescending(x => x.Price),
                "По категории" => result.OrderBy(x => x.Category).ThenBy(x => x.DishName),
                _ => result
            };

            FilteredMenu = new ObservableCollection<DishDto>(result);
        }

        [RelayCommand]
        public async Task LoadMenuAsync()
        {
            try
            {
                var dishes = await _apiService.GetMenuAsync(_session.BranchId);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Menu = new ObservableCollection<DishDto>(dishes);
                    ApplyFilterAndSort();
                });
            }
            catch (Exception ex) { StatusMessage = $"Ошибка меню: {ex.Message}"; }
        }

        [RelayCommand]
        public async Task CreateNewOrderAsync()
        {
            try
            {
                _currentOrderId = await _apiService.CreateOrderAsync(_session.EmployeeId, _session.BranchId);
                Cart.Clear();
                TotalAmount = 0;
                IsOrderCreated = true;
                GlobalQuantity = "1";
                StatusMessage = $"Заказ №{_currentOrderId} создан.";
            }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        [RelayCommand]
        public async Task ChangeQuantityAsync()
        {
            string result = await Shell.Current.DisplayPromptAsync(
                "Количество", "Сколько позиций добавить за раз?", "ОК", "Отмена",
                initialValue: GlobalQuantity, keyboard: Keyboard.Numeric);

            if (!string.IsNullOrWhiteSpace(result) && int.TryParse(result, out var qty) && qty > 0)
                GlobalQuantity = result;
        }

        [RelayCommand]
        public async Task CancelOrderAsync()
        {
            if (!IsOrderCreated) return;
            bool confirm = await Shell.Current.DisplayAlert("Отмена", "Вы уверены, что хотите отменить текущий заказ?", "Да", "Нет");
            if (!confirm) return;

            try
            {
                await _apiService.CancelOrderAsync(_currentOrderId);
                IsOrderCreated = false;
                Cart.Clear();
                TotalAmount = 0;
                StatusMessage = $"Заказ №{_currentOrderId} отменен.";
            }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        [RelayCommand]
        public async Task AddToCartAsync(DishDto dish)
        {
            if (!IsOrderCreated) { StatusMessage = "Сначала создайте заказ!"; return; }
            if (!dish.IsAvailable) { StatusMessage = "Закончились ингредиенты!"; return; }

            if (!int.TryParse(GlobalQuantity, out var qtyToAdd) || qtyToAdd <= 0) qtyToAdd = 1;

            try
            {
                await _apiService.AddItemAsync(_currentOrderId, dish.IdDish, qtyToAdd);

                var existingItem = Cart.FirstOrDefault(c => c.IdDish == dish.IdDish);
                if (existingItem != null) { existingItem.Quantity += qtyToAdd; }
                else { Cart.Add(new CartItemDto { IdDish = dish.IdDish, DishName = dish.DishName, Price = dish.Price, Quantity = qtyToAdd }); }

                RecalculateTotal();
                StatusMessage = $"Добавлено: {dish.DishName} (x{qtyToAdd})";
                GlobalQuantity = "1";
            }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        [RelayCommand]
        public async Task DecreaseQuantityAsync(CartItemDto item)
        {
            if (!IsOrderCreated || item.Quantity <= 0) return;
            try
            {
                if (item.Quantity > 1)
                {
                    await _apiService.AddItemAsync(_currentOrderId, item.IdDish, -1);
                    item.Quantity--;
                }
                else
                {
                    await _apiService.RemoveItemAsync(_currentOrderId, item.IdDish);
                    Cart.Remove(item);
                }
                RecalculateTotal();
            }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        [RelayCommand]
        public async Task RemoveFromCartAsync(CartItemDto item)
        {
            if (!IsOrderCreated) return;

            string result = await Shell.Current.DisplayPromptAsync(
                "Удаление", $"Сколько '{item.DishName}' убрать? (В чеке: {item.Quantity})", "Убрать", "Отмена",
                initialValue: item.Quantity.ToString(), keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(result) || !int.TryParse(result, out var qtyToRemove) || qtyToRemove <= 0) return;

            try
            {
                if (qtyToRemove >= item.Quantity)
                {
                    await _apiService.RemoveItemAsync(_currentOrderId, item.IdDish);
                    Cart.Remove(item);
                }
                else
                {
                    await _apiService.AddItemAsync(_currentOrderId, item.IdDish, -qtyToRemove);
                    item.Quantity -= qtyToRemove;
                }
                RecalculateTotal();
            }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        [RelayCommand]
        public async Task PayOrderAsync()
        {
            if (!IsOrderCreated) return;
            if (Cart.Count == 0) { StatusMessage = "Чек пуст!"; return; }

            try
            {
                await _apiService.CloseOrderAsync(_currentOrderId, "карта");
                StatusMessage = $"Заказ №{_currentOrderId} оплачен!";
                IsOrderCreated = false;
                Cart.Clear();
                TotalAmount = 0;
                await LoadMenuAsync();
            }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        [RelayCommand]
        public async Task GoToHistoryAsync() => await Shell.Current.GoToAsync("OrderHistoryPage");

        private void RecalculateTotal() => TotalAmount = Cart.Sum(item => item.TotalPrice);
    }
}