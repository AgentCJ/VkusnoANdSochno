using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class CashierViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private int _currentOrderId;

        [ObservableProperty]
        private string statusMessage = "Создайте заказ, чтобы начать";

        public CashierViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task CreateNewOrderAsync()
        {
            try
            {
                // ID 2 (кассир) и филиал 1 твоих тестовых данных
                _currentOrderId = await _apiService.CreateOrderAsync(2, 1);
                StatusMessage = $"Создан заказ №{_currentOrderId}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task AddBurgerAsync()
        {
            if (_currentOrderId == 0) { StatusMessage = "Сначала создайте заказ!"; return; }
            try
            {
                // ID блюда 1 (Гамбургер)
                await _apiService.AddItemAsync(_currentOrderId, 1, 1);
                StatusMessage = "Гамбургер добавлен (+150 ₽)";
            }
            catch (Exception ex) { StatusMessage = ex.Message; }
        }

        [RelayCommand]
        public async Task CloseOrderAsync()
        {
            if (_currentOrderId == 0) { StatusMessage = "Нет активного заказа"; return; }
            try
            {
                await _apiService.CloseOrderAsync(_currentOrderId, "карта");
                StatusMessage = $"Заказ №{_currentOrderId} оплачен картой";
            }
            catch (Exception ex) { StatusMessage = ex.Message; }
        }
    }
}