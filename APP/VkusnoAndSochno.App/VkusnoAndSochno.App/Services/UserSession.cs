namespace VkusnoAndSochno.App.Services
{
    // Хранит информацию о текущем пользователе после авторизации
    public class UserSession
    {
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}