using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class MenuManagementViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private int? _editingDishId = null;

        [ObservableProperty] private ObservableCollection<AdminDishDto> dishes = new();
        [ObservableProperty] private string newDishName = string.Empty;
        [ObservableProperty] private string newDishPrice = string.Empty;
        [ObservableProperty] private bool isRefreshing;

        [ObservableProperty] private bool isEditing = false;
        [ObservableProperty] private string formTitle = "Добавить новое блюдо";
        [ObservableProperty] private string actionButtonText = "Добавить в меню";

        public ObservableCollection<string> Categories { get; } = new()
        {
            "Бургеры", "Закуски", "Напитки", "Десерты", "Соусы"
        };

        [ObservableProperty] private string selectedCategory = string.Empty;

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
        public async Task SelectCategoryAsync()
        {
            var result = await Shell.Current.DisplayActionSheet("Выберите категорию", "Отмена", null, Categories.ToArray());
            if (!string.IsNullOrEmpty(result) && result != "Отмена")
            {
                SelectedCategory = result;
            }
        }

        [RelayCommand]
        public async Task SaveDishAsync()
        {
            if (string.IsNullOrWhiteSpace(NewDishName) || string.IsNullOrWhiteSpace(SelectedCategory) || !decimal.TryParse(NewDishPrice, out var price) || price <= 0)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Заполните название, выберите категорию и укажите корректную цену", "ОК");
                return;
            }

            try
            {
                if (IsEditing && _editingDishId.HasValue)
                {
                    await _apiService.UpdateDishAsync(_editingDishId.Value, new { DishName = NewDishName, Category = SelectedCategory, Price = price });
                }
                else
                {
                    await _apiService.AddDishAsync(new { DishName = NewDishName, Category = SelectedCategory, Price = price });
                }

                CancelEdit();
                await LoadDishesAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }

        [RelayCommand]
        public void EditDish(AdminDishDto dish)
        {
            _editingDishId = dish.IdDish;
            NewDishName = dish.DishName;
            SelectedCategory = dish.Category;
            NewDishPrice = dish.Price.ToString();

            IsEditing = true;
            FormTitle = "Редактировать блюдо";
            ActionButtonText = "Сохранить изменения";
        }

        [RelayCommand]
        public void CancelEdit()
        {
            _editingDishId = null;
            NewDishName = string.Empty;
            SelectedCategory = string.Empty;
            NewDishPrice = string.Empty;

            IsEditing = false;
            FormTitle = "Добавить новое блюдо";
            ActionButtonText = "Добавить в меню";
        }

        [RelayCommand]
        public async Task DeleteDishAsync(AdminDishDto dish)
        {
            bool confirm = await Shell.Current.DisplayAlert("Удаление", $"Вы уверены, что хотите удалить '{dish.DishName}'?", "Да", "Нет");
            if (!confirm) return;

            try
            {
                await _apiService.DeleteDishAsync(dish.IdDish);
                await LoadDishesAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Внимание", ex.Message, "ОК"); }
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

        [RelayCommand]
        public async Task GoToRecipeAsync(AdminDishDto dish)
        {
            await Shell.Current.GoToAsync($"RecipePage?dishId={dish.IdDish}&dishName={Uri.EscapeDataString(dish.DishName)}");
        }
    }
}