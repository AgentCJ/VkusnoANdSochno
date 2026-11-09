using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class ManagerViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        // --- Свойства для Дашборда ---
        [ObservableProperty] private decimal todayRevenue;
        [ObservableProperty] private int todayOrdersCount;
        [ObservableProperty] private string topDish = "-";

        // --- Свойства для Отчетов ---
        [ObservableProperty] private DateTime fromDate = DateTime.Today.AddDays(-7);
        [ObservableProperty] private DateTime toDate = DateTime.Today;
        [ObservableProperty] private ObservableCollection<SalesReportDto> reportData = new();
        [ObservableProperty] private string totalSummary = string.Empty;
        [ObservableProperty] private bool isRefreshing;

        public ManagerViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        // --- Метод загрузки дашборда ---
        [RelayCommand]
        public async Task LoadDashboardAsync()
        {
            try
            {
                var stats = await _apiService.GetDashboardStatsAsync();
                TodayRevenue = stats.TotalRevenue;
                TodayOrdersCount = stats.OrderCount;
                TopDish = stats.TopDish;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка API (Дашборд)", ex.Message, "ОК");
            }
        }

        // --- Метод формирования отчета ---
        [RelayCommand]
        public async Task GenerateReportAsync()
        {
            if (FromDate > ToDate)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Дата начала не может быть позже даты окончания", "ОК");
                return;
            }

            IsRefreshing = true;
            try
            {
                var data = await _apiService.GetSalesReportAsync(FromDate, ToDate);

                ReportData.Clear();
                decimal totalRevenue = 0;

                foreach (var item in data)
                {
                    ReportData.Add(item);
                    totalRevenue += item.TotalRevenue;
                }

                TotalSummary = $"Общая выручка за период: {totalRevenue:N2} ₽";
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка API (Отчет)", ex.Message, "ОК");
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}