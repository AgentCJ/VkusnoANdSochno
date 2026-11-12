using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.Views
{
    public partial class OrderHistoryPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly UserSession _session;
        public ObservableCollection<OrderHistoryDto> HistoryList { get; set; } = new();

        public OrderHistoryPage(ApiService apiService, UserSession session)
        {
            InitializeComponent();
            _apiService = apiService;
            _session = session;
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                var data = await _apiService.GetOrderHistoryAsync(_session.BranchId);
                HistoryList.Clear();
                foreach (var item in data) HistoryList.Add(item);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadHistoryAsync();
        }

        private void OnOrderTapped(object sender, TappedEventArgs e)
        {
            // Разворачиваем/сворачиваем детали заказа при клике
            if (sender is Border border && border.BindingContext is OrderHistoryDto order)
            {
                order.IsExpanded = !order.IsExpanded;
            }
        }
    }
}