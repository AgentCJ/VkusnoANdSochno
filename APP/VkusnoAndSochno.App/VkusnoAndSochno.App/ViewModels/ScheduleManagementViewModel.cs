using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;
using CommunityToolkit.Maui.Storage;
using Xceed.Words.NET;
using Xceed.Document.NET;

namespace VkusnoAndSochno.App.ViewModels
{
    public class WeekDayDto
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public string DateString { get; set; } = string.Empty;
        public ObservableCollection<BranchShiftDto> Shifts { get; set; } = new();
    }

    public partial class ScheduleManagementViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly UserSession _session;
        private int? _editingShiftId = null;

        [ObservableProperty] private ObservableCollection<BranchShiftDto> shifts = new();
        [ObservableProperty] private ObservableCollection<WeekDayDto> weekDays = new();
        [ObservableProperty] private ObservableCollection<EmployeeShortDto> employees = new();

        [ObservableProperty] private DateTime selectedWeekDate = DateTime.Today;
        [ObservableProperty] private string weekDisplay = string.Empty;

        [ObservableProperty] private bool isFormVisible = false;
        [ObservableProperty] private string formTitle = "Новая смена";

        [ObservableProperty] private EmployeeShortDto? selectedEmployee;
        [ObservableProperty] private DateTime selectedDate = DateTime.Today;

        [ObservableProperty] private string startTimeText = "10:00";
        [ObservableProperty] private string endTimeText = "22:00";

        private string[] _dayNames = { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };

        public ScheduleManagementViewModel(ApiService apiService, UserSession session)
        {
            _apiService = apiService;
            _session = session;
            UpdateWeekDisplay();
        }

        partial void OnSelectedWeekDateChanged(DateTime value) => ApplyWeekFilter();

        private void UpdateWeekDisplay()
        {
            int diff = (7 + (SelectedWeekDate.DayOfWeek - System.DayOfWeek.Monday)) % 7;
            var startOfWeek = SelectedWeekDate.AddDays(-1 * diff).Date;
            var endOfWeek = startOfWeek.AddDays(6);
            WeekDisplay = $"Неделя: с {startOfWeek:dd.MM.yyyy} по {endOfWeek:dd.MM.yyyy}";
        }

        private void ApplyWeekFilter()
        {
            UpdateWeekDisplay();

            int diff = (7 + (SelectedWeekDate.DayOfWeek - System.DayOfWeek.Monday)) % 7;
            var startOfWeek = SelectedWeekDate.AddDays(-1 * diff).Date;

            WeekDays.Clear();

            for (int i = 0; i < 7; i++)
            {
                var currentDate = startOfWeek.AddDays(i);
                var dateOnly = DateOnly.FromDateTime(currentDate);

                var dayShifts = Shifts
                    .Where(s => s.ShiftDate == dateOnly)
                    .OrderBy(s => s.StartTime)
                    .ToList();

                WeekDays.Add(new WeekDayDto
                {
                    Date = currentDate,
                    DayName = _dayNames[i],
                    DateString = currentDate.ToString("dd.MM"),
                    Shifts = new ObservableCollection<BranchShiftDto>(dayShifts)
                });
            }
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            try
            {
                var data = await _apiService.GetBranchShiftsAsync(_session.BranchId);
                Shifts = new ObservableCollection<BranchShiftDto>(data);
                ApplyWeekFilter();

                if (Employees.Count == 0)
                {
                    var emps = await _apiService.GetEmployeesForShiftsAsync(_session.BranchId);
                    Employees = new ObservableCollection<EmployeeShortDto>(emps);
                }
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }

        [RelayCommand]
        public void OpenCreateForm()
        {
            _editingShiftId = null;
            FormTitle = "Новая смена";
            SelectedEmployee = null;
            SelectedDate = DateTime.Today;
            StartTimeText = "10:00";
            EndTimeText = "22:00";
            IsFormVisible = true;
        }

        [RelayCommand]
        public void EditShift(BranchShiftDto shift)
        {
            _editingShiftId = shift.IdShift;
            FormTitle = "Редактирование смены";
            SelectedEmployee = Employees.FirstOrDefault(e => e.IdEmployee == shift.IdEmployee);
            SelectedDate = shift.ShiftDate.ToDateTime(TimeOnly.MinValue);
            StartTimeText = shift.StartTime.ToString("HH:mm");
            EndTimeText = shift.EndTime.ToString("HH:mm");
            IsFormVisible = true;
        }

        [RelayCommand] public void CloseForm() => IsFormVisible = false;

        [RelayCommand]
        public async Task SaveShiftAsync()
        {
            if (SelectedEmployee == null)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Выберите сотрудника", "ОК");
                return;
            }

            if (!TimeOnly.TryParseExact(StartTimeText, "HH:mm", out var newStart) ||
                !TimeOnly.TryParseExact(EndTimeText, "HH:mm", out var newEnd))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите время в формате ЧЧ:ММ (например, 10:00)", "ОК");
                return;
            }

            if (newStart >= newEnd)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Время начала должно быть меньше времени окончания", "ОК");
                return;
            }

            var newDate = DateOnly.FromDateTime(SelectedDate);

            // Проверка на пересечение смен у сотрудника в этот день
            var employeeShifts = Shifts.Where(s =>
                s.IdEmployee == SelectedEmployee.IdEmployee &&
                s.ShiftDate == newDate &&
                s.IdShift != (_editingShiftId ?? 0)).ToList();

            bool hasOverlap = employeeShifts.Any(s => newStart < s.EndTime && newEnd > s.StartTime);

            if (hasOverlap)
            {
                await Shell.Current.DisplayAlert("Внимание", "У сотрудника уже есть смена, которая пересекается с этим временем.", "ОК");
                return;
            }

            var request = new
            {
                BranchId = _session.BranchId,
                EmployeeId = SelectedEmployee.IdEmployee,
                ShiftDate = newDate,
                StartTime = newStart,
                EndTime = newEnd
            };

            try
            {
                if (_editingShiftId.HasValue)
                    await _apiService.UpdateShiftAsync(_editingShiftId.Value, request);
                else
                    await _apiService.CreateShiftAsync(request);

                CloseForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }

        [RelayCommand]
        public async Task DeleteShiftAsync(BranchShiftDto shift)
        {
            bool confirm = await Shell.Current.DisplayAlert("Удаление", "Удалить эту смену?", "Да", "Нет");
            if (!confirm) return;

            try
            {
                await _apiService.DeleteShiftAsync(shift.IdShift);
                await LoadDataAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }

        [RelayCommand]
        public async Task ToggleStatusAsync(BranchShiftDto shift)
        {
            try
            {
                await _apiService.ToggleShiftStatusAsync(shift.IdShift);
                await LoadDataAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }

        [RelayCommand]
        public async Task ExportToDocxAsync()
        {
            if (!Shifts.Any())
            {
                await Shell.Current.DisplayAlert("Ошибка", "Нет смен для экспорта на этой неделе", "ОК"); return;
            }

            try
            {
                using var stream = new MemoryStream();
                using (var document = DocX.Create(stream))
                {
                    var title = document.InsertParagraph($"График смен ({WeekDisplay})");
                    title.FontSize(20).Bold().Alignment = Alignment.center;

                    var exportShifts = WeekDays.SelectMany(d => d.Shifts).ToList();

                    var table = document.AddTable(exportShifts.Count + 1, 5);
                    table.Design = TableDesign.TableGrid;

                    table.Rows[0].Cells[0].Paragraphs[0].Append("Дата").Bold();
                    table.Rows[0].Cells[1].Paragraphs[0].Append("Сотрудник").Bold();
                    table.Rows[0].Cells[2].Paragraphs[0].Append("Начало").Bold();
                    table.Rows[0].Cells[3].Paragraphs[0].Append("Конец").Bold();
                    table.Rows[0].Cells[4].Paragraphs[0].Append("Статус").Bold();

                    for (int i = 0; i < exportShifts.Count; i++)
                    {
                        var s = exportShifts[i];
                        table.Rows[i + 1].Cells[0].Paragraphs[0].Append(s.ShiftDate.ToString("dd.MM.yyyy"));
                        table.Rows[i + 1].Cells[1].Paragraphs[0].Append(s.EmployeeName);
                        table.Rows[i + 1].Cells[2].Paragraphs[0].Append(s.StartTime.ToString("HH:mm"));
                        table.Rows[i + 1].Cells[3].Paragraphs[0].Append(s.EndTime.ToString("HH:mm"));
                        table.Rows[i + 1].Cells[4].Paragraphs[0].Append(s.Status);
                    }
                    document.InsertTable(table);
                    document.Save();
                }
                stream.Position = 0;

                string fileName = $"График_смен_{SelectedWeekDate:yyyyMMdd}.docx";
                var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);

                if (fileSaverResult.IsSuccessful)
                    await Shell.Current.DisplayAlert("Успех", $"Сохранено:\n{fileSaverResult.FilePath}", "ОК");
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }
    }
}