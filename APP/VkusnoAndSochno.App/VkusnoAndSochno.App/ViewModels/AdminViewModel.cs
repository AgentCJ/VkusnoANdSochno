using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using VkusnoAndSochno.App.Services;

namespace VkusnoAndSochno.App.ViewModels
{
    public partial class AdminViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private int? _editingUserId = null;

        [ObservableProperty] private ObservableCollection<AdminUserDto> users = new();
        [ObservableProperty] private bool isRefreshing;

        [ObservableProperty] private ObservableCollection<RoleDto> roles = new();
        [ObservableProperty] private ObservableCollection<BranchDto> branches = new();
        [ObservableProperty] private ObservableCollection<PositionDto> positions = new();

        [ObservableProperty] private bool isFormVisible = false;
        [ObservableProperty] private string formTitle = "Новый пользователь";

        [ObservableProperty] private string inputName = string.Empty;
        [ObservableProperty] private string inputPhone = string.Empty;
        [ObservableProperty] private string inputLogin = string.Empty;
        [ObservableProperty] private string inputPassword = string.Empty;

        [ObservableProperty] private RoleDto? selectedRole;
        [ObservableProperty] private BranchDto? selectedBranch;
        [ObservableProperty] private PositionDto? selectedPosition;

        public AdminViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsRefreshing = true;
            try
            {
                var data = await _apiService.GetUsersAsync();
                Users = new ObservableCollection<AdminUserDto>(data);

                if (Roles.Count == 0)
                {
                    var meta = await _apiService.GetAdminMetadataAsync();
                    Roles = new ObservableCollection<RoleDto>(meta.Roles);
                    Branches = new ObservableCollection<BranchDto>(meta.Branches);
                    Positions = new ObservableCollection<PositionDto>(meta.Positions);
                }
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
            finally { IsRefreshing = false; }
        }

        [RelayCommand]
        public void OpenCreateForm()
        {
            _editingUserId = null;
            FormTitle = "Новый пользователь";
            InputName = string.Empty; InputPhone = string.Empty; InputLogin = string.Empty; InputPassword = string.Empty;
            SelectedRole = null; SelectedBranch = null; SelectedPosition = null;
            IsFormVisible = true;
        }

        [RelayCommand]
        public void EditUser(AdminUserDto user)
        {
            _editingUserId = user.IdUser;
            FormTitle = "Редактирование пользователя";
            InputName = user.EmployeeName;
            InputLogin = user.Login;
            InputPassword = string.Empty;

            SelectedRole = Roles.FirstOrDefault(r => r.IdRole == user.IdRole);
            SelectedBranch = Branches.FirstOrDefault(b => b.IdBranch == user.IdBranch);

            IsFormVisible = true;
        }

        [RelayCommand] public void CloseForm() => IsFormVisible = false;

        [RelayCommand]
        public async Task SaveUserAsync()
        {
            // Проверка на заполненность всех полей
            if (string.IsNullOrWhiteSpace(InputName) || string.IsNullOrWhiteSpace(InputLogin) ||
                SelectedRole == null || SelectedBranch == null || SelectedPosition == null)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Заполните все обязательные поля", "ОК"); return;
            }

            // Проверка ФИО (ровно 3 слова, только буквы и дефис)
            var nameParts = InputName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length != 3)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите ФИО полностью (Фамилия Имя Отчество)", "ОК"); return;
            }
            foreach (var part in nameParts)
            {
                if (!Regex.IsMatch(part, @"^[а-яА-ЯёЁa-zA-Z\-]+$"))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "ФИО должно содержать только буквы", "ОК"); return;
                }
            }

            // Проверка длины логина
            if (InputLogin.Length < 5)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Логин должен содержать минимум 5 символов", "ОК"); return;
            }

            // Проверка пароля
            if (_editingUserId == null && string.IsNullOrWhiteSpace(InputPassword))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите пароль для нового пользователя", "ОК"); return;
            }
            if (!string.IsNullOrWhiteSpace(InputPassword) && InputPassword.Length < 5)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Пароль должен содержать минимум 5 символов", "ОК"); return;
            }

            var request = new
            {
                EmployeeName = InputName,
                Phone = InputPhone,
                BranchId = SelectedBranch.IdBranch,
                PositionId = SelectedPosition.IdPosition,
                Login = InputLogin,
                Password = InputPassword,
                RoleId = SelectedRole.IdRole
            };

            try
            {
                if (_editingUserId.HasValue)
                    await _apiService.UpdateUserAsync(_editingUserId.Value, request);
                else
                    await _apiService.CreateUserAsync(request);

                CloseForm();
                await LoadDataAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }

        [RelayCommand]
        public async Task DeleteUserAsync(AdminUserDto user)
        {
            bool confirm = await Shell.Current.DisplayAlert("Удаление", $"Удалить пользователя {user.EmployeeName}?", "Да", "Нет");
            if (!confirm) return;

            try
            {
                await _apiService.DeleteUserAsync(user.IdUser);
                await LoadDataAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Внимание", ex.Message, "ОК"); }
        }

        [RelayCommand]
        public async Task ToggleUserStatusAsync(AdminUserDto user)
        {
            try
            {
                await _apiService.ToggleUserStatusAsync(user.IdUser);
                await LoadDataAsync();
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Ошибка", ex.Message, "ОК"); }
        }
    }
}