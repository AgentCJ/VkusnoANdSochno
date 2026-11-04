using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class CookViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<ActiveOrderDto> activeOrders = new();

        [ObservableProperty]
        private bool isRefreshing;

        public CookViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadOrdersAsync()
        {
            IsRefreshing = true;
            try
            {
                var orders = await _apiService.GetActiveOrdersAsync();
                ActiveOrders.Clear();
                foreach (var order in orders)
                {
                    ActiveOrders.Add(order);
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", ex.Message, "ОК");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task StartCookingAsync(ActiveOrderDto order)
        {
            await UpdateStatusAsync(order, "готовится");
        }

        [RelayCommand]
        public async Task FinishCookingAsync(ActiveOrderDto order)
        {
            await UpdateStatusAsync(order, "выполнен");
        }

        private async Task UpdateStatusAsync(ActiveOrderDto order, string newStatus)
        {
            try
            {
                await _apiService.UpdateOrderStatusAsync(order.IdOrder, newStatus);
                await LoadOrdersAsync(); // Обновляем список на экране
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }
    }
}