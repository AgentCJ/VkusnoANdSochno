using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class MyScheduleViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly UserSession _session;

        [ObservableProperty] private ObservableCollection<ShiftDto> shifts = new();
        [ObservableProperty] private bool isRefreshing;

        public MyScheduleViewModel(ApiService apiService, UserSession session)
        {
            _apiService = apiService;
            _session = session;
        }

        [RelayCommand]
        public async Task LoadShiftsAsync()
        {
            IsRefreshing = true;
            try
            {
                var data = await _apiService.GetMyShiftsAsync(_session.EmployeeId);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Shifts = new ObservableCollection<ShiftDto>(data);
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
    }
}