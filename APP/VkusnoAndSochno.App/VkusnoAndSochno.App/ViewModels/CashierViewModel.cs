using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class CashierViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly UserSession _session;

        private int _currentOrderId;

        [ObservableProperty]
        private string statusMessage = "Создайте заказ, чтобы начать";

        public CashierViewModel(ApiService apiService, UserSession session)
        {
            _apiService = apiService;
            _session = session;
        }


        [RelayCommand]
        public async Task CreateNewOrderAsync()
        {
            try
            {
                _currentOrderId = await _apiService.CreateOrderAsync(_session.EmployeeId, _session.BranchId);
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