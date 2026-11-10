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

            // Настраиваем таймер, который тикает каждую секунду
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += UpdateOrderTimers;
            _timer.Start();
        }

        // Этот метод вызывается каждую секунду
        private void UpdateOrderTimers(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            foreach (var order in ActiveOrders)
            {
                // Считаем, сколько времени прошло с момента создания
                var elapsed = now - order.OrderTime;

                // Форматируем в минуты и секунды (например, "05:23")
                order.TimeElapsed = elapsed.ToString(@"mm\:ss");

                // Если прошло больше 5 минут — делаем красным
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

                // Обновляем коллекцию в UI-потоке, чтобы таймер не ломал приложение
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ActiveOrders.Clear();
                    foreach (var order in orders)
                    {
                        ActiveOrders.Add(order);
                    }
                    // Сразу обновляем время для новых заказов
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
                await LoadOrdersAsync(); // Обновляем список на экране
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }

        // НОВЫЙ МЕТОД: Переход в историю
        [RelayCommand]
        public async Task GoToHistoryAsync()
        {
            await Shell.Current.GoToAsync("OrderHistoryPage");
        }
    }
}