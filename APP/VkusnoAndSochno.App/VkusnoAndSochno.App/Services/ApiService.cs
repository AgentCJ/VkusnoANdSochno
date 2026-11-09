using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VkusnoAndSochno.App.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "http://localhost:5212/api/";

        public ApiService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public async Task<int> CreateOrderAsync(int employeeId, int branchId)
        {
            var response = await _httpClient.PostAsync(
                $"orders/create?employeeId={employeeId}&branchId={branchId}", null);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
            return result!.OrderId;
        }

        public async Task AddItemAsync(int orderId, int dishId, int quantity)
        {
            var response = await _httpClient.PostAsync(
                $"orders/{orderId}/add-item?dishId={dishId}&quantity={quantity}", null);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public async Task CloseOrderAsync(int orderId, string method)
        {
            var response = await _httpClient.PostAsync(
                $"orders/{orderId}/close?method={method}", null);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public async Task<LoginResponse?> LoginAsync(string login, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", new { Login = login, Password = password });
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        public async Task<List<ActiveOrderDto>> GetActiveOrdersAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<ActiveOrderDto>>("orders/active");
            return response ?? new List<ActiveOrderDto>();
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var response = await _httpClient.PutAsync($"orders/{orderId}/status?status={status}", null);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public async Task<List<StockBalanceDto>> GetBalanceAsync(int branchId)
        {
            // ДОБАВЛЕНО: Параметр ?t=... обходит кэш браузера/Windows
            var res = await _httpClient.GetFromJsonAsync<List<StockBalanceDto>>(
                $"warehouse/balance/{branchId}?t={DateTime.Now.Ticks}");
            return res ?? new();
        }

        public async Task<List<IngredientDto>> GetIngredientsAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<List<IngredientDto>>("warehouse/ingredients");
            return res ?? new();
        }

        public async Task<List<SupplierDto>> GetSuppliersAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<List<SupplierDto>>("warehouse/suppliers");
            return res ?? new();
        }

        public async Task RegisterSupplyAsync(object request)
        {
            var response = await _httpClient.PostAsJsonAsync("warehouse/supply", request);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public async Task<List<SalesReportDto>> GetSalesReportAsync(DateTime from, DateTime to)
        {
            var fromStr = from.ToString("yyyy-MM-dd");
            var toStr = to.ToString("yyyy-MM-dd");

            var response = await _httpClient.GetAsync($"reports/sales?from={fromStr}&to={toStr}");

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new Exception($"Сервер вернул ошибку: {errorText}");
            }

            var res = await response.Content.ReadFromJsonAsync<List<SalesReportDto>>();
            return res ?? new();
        }

        public async Task<List<DishDto>> GetMenuAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<List<DishDto>>("orders/menu");
            return res ?? new();
        }

        public async Task RemoveItemAsync(int orderId, int dishId)
        {
            var response = await _httpClient.DeleteAsync($"orders/{orderId}/items/{dishId}");
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
        public async Task<List<AdminUserDto>> GetUsersAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<List<AdminUserDto>>("admin/users");
            return res ?? new();
        }

        public async Task ToggleUserStatusAsync(int userId)
        {
            var response = await _httpClient.PutAsync($"admin/users/{userId}/toggle", null);
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
        public async Task<List<ShiftDto>> GetMyShiftsAsync(int employeeId)
        {
            var res = await _httpClient.GetFromJsonAsync<List<ShiftDto>>($"shifts/my/{employeeId}");
            return res ?? new();
        }
        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<DashboardStatsDto>("reports/dashboard");
            return res ?? new();
        }

    }

    // классы данных

    public class LoginResponse
    {
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class CreateOrderResponse { public int OrderId { get; set; } }

    public class ActiveOrderDto
    {
        public int IdOrder { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderTime { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class StockBalanceDto
    {
        public int IdBranch { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int IdIngredient { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal MinRemaining { get; set; }
        public decimal Balance { get; set; }
    }

    public class IngredientDto { public int IdIngredient { get; set; } public string IngredientName { get; set; } = string.Empty; }
    public class SupplierDto { public int IdSupplier { get; set; } public string SupplierName { get; set; } = string.Empty; }

    public class SalesReportDto
    {
        public int IdBranch { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public long OrdersCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgOrder { get; set; }
    }

    public class DishDto
    {
        public int IdDish { get; set; }
        public string DishName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public partial class CartItemDto : ObservableObject
    {
        public int IdDish { get; set; }
        public string DishName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPrice))]
        private int quantity;

        public decimal TotalPrice => Price * Quantity;
    }
    public class AdminUserDto
    {
        public int IdUser { get; set; }
        public string Login { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
    public class ShiftDto
    {
        public int IdShift { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? HoursWorked { get; set; }
    }
    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public string TopDish { get; set; } = string.Empty;
    }

}