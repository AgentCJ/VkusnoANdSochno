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
        [ObservableProperty] private ObservableCollection<CartItemDto> cart = new();
        [ObservableProperty] private decimal totalAmount = 0;
        [ObservableProperty] private bool isOrderCreated = false;

        [ObservableProperty] private string globalQuantity = "1";

        public CashierViewModel(ApiService apiService, UserSession session)
        {
            _apiService = apiService;
            _session = session;
        }

        [RelayCommand]
        public async Task LoadMenuAsync()
        {
            try
            {
                var dishes = await _apiService.GetMenuAsync(_session.BranchId);
                MainThread.BeginInvokeOnMainThread(() => Menu = new ObservableCollection<DishDto>(dishes));
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
                "Количество",
                "Сколько позиций добавить за раз?",
                "ОК", "Отмена",
                initialValue: GlobalQuantity,
                keyboard: Keyboard.Numeric);

            if (!string.IsNullOrWhiteSpace(result) && int.TryParse(result, out var qty) && qty > 0)
            {
                GlobalQuantity = result;
            }
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

                // Сбрасываем количество обратно на 1 после добавления
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

        // ОБНОВЛЕНО: Удаление с выбором количества
        [RelayCommand]
        public async Task RemoveFromCartAsync(CartItemDto item)
        {
            if (!IsOrderCreated) return;

            string result = await Shell.Current.DisplayPromptAsync(
                "Удаление из чека",
                $"Сколько '{item.DishName}' убрать? (В чеке: {item.Quantity})",
                "Убрать", "Отмена",
                initialValue: item.Quantity.ToString(),
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(result) || !int.TryParse(result, out var qtyToRemove) || qtyToRemove <= 0) return;

            try
            {
                if (qtyToRemove >= item.Quantity)
                {
                    // Если ввели число больше или равное, удаляем позицию полностью
                    await _apiService.RemoveItemAsync(_currentOrderId, item.IdDish);
                    Cart.Remove(item);
                }
                else
                {
                    // Иначе отнимаем нужное количество
                    await _apiService.AddItemAsync(_currentOrderId, item.IdDish, -qtyToRemove);
                    item.Quantity -= qtyToRemove;
                }
                RecalculateTotal();
                StatusMessage = $"Убрано: {item.DishName} (x{qtyToRemove})";
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
        public async Task GoToHistoryAsync()
        {
            await Shell.Current.GoToAsync("OrderHistoryPage");
        }

        private void RecalculateTotal() => TotalAmount = Cart.Sum(item => item.TotalPrice);
    }
}