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
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Menu = new ObservableCollection<DishDto>(dishes);
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
                StatusMessage = $"Заказ №{_currentOrderId} создан. Добавьте блюда.";
            }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        [RelayCommand]
        public async Task AddToCartAsync(DishDto dish)
        {
            if (!IsOrderCreated) { StatusMessage = "Сначала создайте заказ!"; return; }

            if (!dish.IsAvailable)
            {
                StatusMessage = "Невозможно добавить: закончились ингредиенты!";
                return;
            }

            try
            {
                await _apiService.AddItemAsync(_currentOrderId, dish.IdDish, 1);

                var existingItem = Cart.FirstOrDefault(c => c.IdDish == dish.IdDish);
                if (existingItem != null) { existingItem.Quantity++; }
                else { Cart.Add(new CartItemDto { IdDish = dish.IdDish, DishName = dish.DishName, Price = dish.Price, Quantity = 1 }); }

                RecalculateTotal();
                StatusMessage = $"Добавлено: {dish.DishName}";
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
                StatusMessage = $"Убавлено: {item.DishName}";
            }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        [RelayCommand]
        public async Task RemoveFromCartAsync(CartItemDto item)
        {
            if (!IsOrderCreated) return;
            bool confirm = await Shell.Current.DisplayAlert("Удаление", $"Удалить '{item.DishName}' из заказа?\n(Требуется подтверждение менеджера)", "Да", "Отмена");
            if (!confirm) return;

            try
            {
                await _apiService.RemoveItemAsync(_currentOrderId, item.IdDish);
                Cart.Remove(item);
                RecalculateTotal();
                StatusMessage = $"Удалено: {item.DishName}";
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
                await LoadMenuAsync(); // Обновляем меню, т.к. остатки списались
            }
            catch (Exception ex) { StatusMessage = $"Ошибка: {ex.Message}"; }
        }

        private void RecalculateTotal() => TotalAmount = Cart.Sum(item => item.TotalPrice);
    }
}