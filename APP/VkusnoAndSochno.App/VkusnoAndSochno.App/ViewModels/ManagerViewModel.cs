using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class ManagerViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private DateTime fromDate = DateTime.Today.AddDays(-7); // По умолчанию неделя назад

        [ObservableProperty]
        private DateTime toDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<SalesReportDto> reportData = new();

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string totalSummary = string.Empty;

        public ManagerViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

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
                await Shell.Current.DisplayAlert("Ошибка API", ex.Message, "ОК");
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}