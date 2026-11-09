using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class AdminViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty] private ObservableCollection<AdminUserDto> users = new();
        [ObservableProperty] private bool isRefreshing;

        public AdminViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadUsersAsync()
        {
            IsRefreshing = true;
            try
            {
                var data = await _apiService.GetUsersAsync();

                // Заставляем UI обновиться
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Users = new ObservableCollection<AdminUserDto>(data);
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
        public async Task ToggleUserStatusAsync(AdminUserDto user)
        {
            try
            {
                await _apiService.ToggleUserStatusAsync(user.IdUser);
                await LoadUsersAsync(); // Перезагружаем список, чтобы обновить кнопку
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }
    }
}