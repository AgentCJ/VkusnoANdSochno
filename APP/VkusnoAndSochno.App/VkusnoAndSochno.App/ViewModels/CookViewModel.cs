using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class CookViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly IDispatcherTimer _timer;

        [ObservableProperty]
        private ObservableCollection<ActiveOrderDto> activeOrders = new();

        [ObservableProperty]
        private bool isRefreshing;

        public CookViewModel(ApiService apiService)
        {
            _apiService = apiService;

            // Настраиваем таймер для обновления времени на экране каждую секунду
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += UpdateOrderTimers;
            _timer.Start();
        }

        private void UpdateOrderTimers(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            foreach (var order in ActiveOrders)
            {
                var elapsed = now - order.OrderTime;
                order.TimeElapsed = elapsed.ToString(@"mm\:ss");

                // Подсвечиваем красным, если заказ висит более 5 минут
                order.IsOverdue = elapsed.TotalMinutes >= 5;
            }
        }

        [RelayCommand]
        public async Task LoadOrdersAsync()
        {
            IsRefreshing = true;
            try
            {
                var orders = await _apiService.GetActiveOrdersAsync();

                // Обновляем коллекцию в главном потоке, чтобы избежать конфликтов с UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ActiveOrders.Clear();
                    foreach (var order in orders)
                    {
                        ActiveOrders.Add(order);
                    }
                    UpdateOrderTimers(null, EventArgs.Empty);
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК");
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
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }

        [RelayCommand]
        public async Task GoToHistoryAsync()
        {
            await Shell.Current.GoToAsync("OrderHistoryPage");
        }
    }
}