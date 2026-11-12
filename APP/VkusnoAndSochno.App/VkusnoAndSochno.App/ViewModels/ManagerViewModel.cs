using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;
using CommunityToolkit.Maui.Storage;
using Xceed.Words.NET;
using Xceed.Document.NET;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class ManagerViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty] private decimal todayRevenue;
        [ObservableProperty] private int todayOrdersCount;
        [ObservableProperty] private string topDish = "-";

        [ObservableProperty] private DateTime fromDate = DateTime.Today.AddDays(-7);
        [ObservableProperty] private DateTime toDate = DateTime.Today;
        [ObservableProperty] private ObservableCollection<SalesReportDto> reportData = new();
        [ObservableProperty] private string totalSummary = string.Empty;
        [ObservableProperty] private bool isRefreshing;

        public ManagerViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

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
                await Shell.Current.DisplayAlert("Ошибка API", ex.Message, "ОК");
            }
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

        [RelayCommand]
        public async Task GenerateDetailedDocxAsync()
        {
            try
            {
                var report = await _apiService.GetDetailedReportAsync(1);

                using var stream = new MemoryStream();
                using (var document = DocX.Create(stream))
                {
                    var title = document.InsertParagraph("Детальный отчет за сегодня");
                    title.FontSize(22).Bold().Alignment = Alignment.center;

                    var dateStr = document.InsertParagraph($"Дата: {report.Date}\n\n").FontSize(14).Alignment = Alignment.center;

                    var table = document.AddTable(6, 2);
                    table.Design = TableDesign.TableGrid;

                    table.Rows[0].Cells[0].Paragraphs[0].Append("Показатель").Bold();
                    table.Rows[0].Cells[1].Paragraphs[0].Append("Значение").Bold();

                    table.Rows[1].Cells[0].Paragraphs[0].Append("Количество выполненных заказов");
                    table.Rows[1].Cells[1].Paragraphs[0].Append(report.OrdersCount.ToString());

                    table.Rows[2].Cells[0].Paragraphs[0].Append("Выручка");
                    table.Rows[2].Cells[1].Paragraphs[0].Append($"{report.TotalRevenue:N2} ₽");

                    table.Rows[3].Cells[0].Paragraphs[0].Append("Навар (чистая прибыль)");
                    table.Rows[3].Cells[1].Paragraphs[0].Append($"{report.TotalProfit:N2} ₽");

                    table.Rows[4].Cells[0].Paragraphs[0].Append("Самое популярное блюдо");
                    table.Rows[4].Cells[1].Paragraphs[0].Append(report.TopDish);

                    table.Rows[5].Cells[0].Paragraphs[0].Append("Менее популярное блюдо");
                    table.Rows[5].Cells[1].Paragraphs[0].Append(report.BottomDish);

                    document.InsertTable(table);
                    document.InsertParagraph($"\nСотрудников на смене: {report.EmployeesOnShift}").FontSize(14).Bold();
                    document.Save();
                }
                stream.Position = 0;

                string fileName = $"Отчет_Управляющего_{DateTime.Now:yyyyMMdd}.docx";
                var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);

                if (fileSaverResult.IsSuccessful)
                {
                    await Shell.Current.DisplayAlert("Успех", $"Отчет сохранен:\n{fileSaverResult.FilePath}", "ОК");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }
    }
}