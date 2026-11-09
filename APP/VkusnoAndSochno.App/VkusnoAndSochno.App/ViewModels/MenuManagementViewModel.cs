using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class MenuManagementViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty] private ObservableCollection<AdminDishDto> dishes = new();
        [ObservableProperty] private string newDishName = string.Empty;
        [ObservableProperty] private string newDishCategory = string.Empty;
        [ObservableProperty] private string newDishPrice = string.Empty;
        [ObservableProperty] private bool isRefreshing;

        public MenuManagementViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadDishesAsync()
        {
            IsRefreshing = true;
            try
            {
                var data = await _apiService.GetAllDishesAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Dishes = new ObservableCollection<AdminDishDto>(data);
                });
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
            finally { IsRefreshing = false; }
        }

        [RelayCommand]
        public async Task AddDishAsync()
        {
            if (string.IsNullOrWhiteSpace(NewDishName) || !decimal.TryParse(NewDishPrice, out var price) || price <= 0)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Заполните название и корректную цену", "ОК");
                return;
            }

            try
            {
                await _apiService.AddDishAsync(new { DishName = NewDishName, Category = NewDishCategory, Price = price });
                NewDishName = string.Empty;
                NewDishCategory = string.Empty;
                NewDishPrice = string.Empty;
                await LoadDishesAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }

        [RelayCommand]
        public async Task ToggleDishAsync(AdminDishDto dish)
        {
            try
            {
                await _apiService.ToggleDishAsync(dish.IdDish);
                await LoadDishesAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }
    }
}