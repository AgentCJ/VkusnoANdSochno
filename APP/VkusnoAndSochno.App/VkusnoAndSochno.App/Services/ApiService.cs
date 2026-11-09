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
            _httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true, NoStore = true };
        }

        public async Task<int> CreateOrderAsync(int employeeId, int branchId) => (await (await _httpClient.PostAsync($"orders/create?employeeId={employeeId}&branchId={branchId}", null)).Content.ReadFromJsonAsync<CreateOrderResponse>())!.OrderId;
        public async Task AddItemAsync(int orderId, int dishId, int quantity) { var r = await _httpClient.PostAsync($"orders/{orderId}/add-item?dishId={dishId}&quantity={quantity}", null); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task CloseOrderAsync(int orderId, string method) { var r = await _httpClient.PostAsync($"orders/{orderId}/close?method={method}", null); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task<LoginResponse?> LoginAsync(string login, string password) { var r = await _httpClient.PostAsJsonAsync("auth/login", new { Login = login, Password = password }); if (r.StatusCode == System.Net.HttpStatusCode.Unauthorized) return null; r.EnsureSuccessStatusCode(); return await r.Content.ReadFromJsonAsync<LoginResponse>(); }
        public async Task<List<ActiveOrderDto>> GetActiveOrdersAsync() => await _httpClient.GetFromJsonAsync<List<ActiveOrderDto>>("orders/active") ?? new();
        public async Task UpdateOrderStatusAsync(int orderId, string status) { var r = await _httpClient.PutAsync($"orders/{orderId}/status?status={status}", null); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task<List<StockBalanceDto>> GetBalanceAsync(int branchId) => await _httpClient.GetFromJsonAsync<List<StockBalanceDto>>($"warehouse/balance/{branchId}?t={DateTime.Now.Ticks}") ?? new();
        public async Task<List<IngredientDto>> GetIngredientsAsync() => await _httpClient.GetFromJsonAsync<List<IngredientDto>>("warehouse/ingredients") ?? new();
        public async Task<List<SupplierDto>> GetSuppliersAsync() => await _httpClient.GetFromJsonAsync<List<SupplierDto>>("warehouse/suppliers") ?? new();
        public async Task RegisterSupplyAsync(object request) { var r = await _httpClient.PostAsJsonAsync("warehouse/supply", request); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task AddIngredientAsync(object request) { var r = await _httpClient.PostAsJsonAsync("warehouse/ingredient", request); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task UpdateIngredientAsync(int id, object request) { var r = await _httpClient.PutAsJsonAsync($"warehouse/ingredient/{id}", request); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task DeleteIngredientAsync(int id) { var r = await _httpClient.DeleteAsync($"warehouse/ingredient/{id}"); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task<List<SalesReportDto>> GetSalesReportAsync(DateTime from, DateTime to) { var r = await _httpClient.GetAsync($"reports/sales?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}"); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); return await r.Content.ReadFromJsonAsync<List<SalesReportDto>>() ?? new(); }
        public async Task<List<DishDto>> GetMenuAsync(int branchId) => await _httpClient.GetFromJsonAsync<List<DishDto>>($"orders/menu/{branchId}") ?? new();
        public async Task RemoveItemAsync(int orderId, int dishId) { var r = await _httpClient.DeleteAsync($"orders/{orderId}/items/{dishId}"); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task<List<AdminUserDto>> GetUsersAsync() => await _httpClient.GetFromJsonAsync<List<AdminUserDto>>("admin/users") ?? new();
        public async Task ToggleUserStatusAsync(int userId) { var r = await _httpClient.PutAsync($"admin/users/{userId}/toggle", null); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task<List<ShiftDto>> GetMyShiftsAsync(int employeeId) => await _httpClient.GetFromJsonAsync<List<ShiftDto>>($"shifts/my/{employeeId}") ?? new();
        public async Task<List<AdminDishDto>> GetAllDishesAsync() => await _httpClient.GetFromJsonAsync<List<AdminDishDto>>("menu/all") ?? new();
        public async Task AddDishAsync(object request) { var r = await _httpClient.PostAsJsonAsync("menu", request); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task UpdateDishAsync(int id, object request) { var r = await _httpClient.PutAsJsonAsync($"menu/{id}", request); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task DeleteDishAsync(int id) { var r = await _httpClient.DeleteAsync($"menu/{id}"); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task ToggleDishAsync(int dishId) { var r = await _httpClient.PutAsync($"menu/{dishId}/toggle", null); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task<DashboardStatsDto> GetDashboardStatsAsync() => await _httpClient.GetFromJsonAsync<DashboardStatsDto>("reports/dashboard") ?? new();
        public async Task<List<RecipeItemDto>> GetRecipeAsync(int dishId) => await _httpClient.GetFromJsonAsync<List<RecipeItemDto>>($"recipes/{dishId}") ?? new();
        public async Task AddRecipeItemAsync(int dishId, object request) { var r = await _httpClient.PostAsJsonAsync($"recipes/{dishId}", request); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
        public async Task RemoveRecipeItemAsync(int dishId, int ingredientId) { var r = await _httpClient.DeleteAsync($"recipes/{dishId}/ingredients/{ingredientId}"); if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync()); }
    }

    // --- DTO КЛАССЫ ---
    public class LoginResponse { public int EmployeeId { get; set; } public int BranchId { get; set; } public string RoleName { get; set; } = string.Empty; public string Message { get; set; } = string.Empty; }
    public class CreateOrderResponse { public int OrderId { get; set; } }
    public class ActiveOrderDto { public int IdOrder { get; set; } public string Status { get; set; } = string.Empty; public decimal TotalAmount { get; set; } public DateTime OrderTime { get; set; } public List<OrderItemDto> Items { get; set; } = new(); }
    public class OrderItemDto { public string DishName { get; set; } = string.Empty; public int Quantity { get; set; } }

    public class StockBalanceDto
    {
        public int IdBranch { get; set; }
        public int IdIngredient { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal MinRemaining { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Balance { get; set; }
        public bool IsLowStock => Balance < MinRemaining;
    }

    public class IngredientDto { public int IdIngredient { get; set; } public string IngredientName { get; set; } = string.Empty; public string UnitOfMeasure { get; set; } = string.Empty; }
    public class SupplierDto { public int IdSupplier { get; set; } public string SupplierName { get; set; } = string.Empty; }
    public class SalesReportDto { public int IdBranch { get; set; } public string BranchName { get; set; } = string.Empty; public long OrdersCount { get; set; } public decimal TotalRevenue { get; set; } public decimal AvgOrder { get; set; } }
    public class DishDto { public int IdDish { get; set; } public string DishName { get; set; } = string.Empty; public string Category { get; set; } = string.Empty; public decimal Price { get; set; } public bool IsAvailable { get; set; } }
    public partial class CartItemDto : ObservableObject { public int IdDish { get; set; } public string DishName { get; set; } = string.Empty; public decimal Price { get; set; } [ObservableProperty][NotifyPropertyChangedFor(nameof(TotalPrice))] private int quantity; public decimal TotalPrice => Price * Quantity; }
    public class AdminUserDto { public int IdUser { get; set; } public string Login { get; set; } = string.Empty; public string EmployeeName { get; set; } = string.Empty; public string RoleName { get; set; } = string.Empty; public string BranchName { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class ShiftDto { public int IdShift { get; set; } public DateTime ShiftDate { get; set; } public TimeSpan StartTime { get; set; } public TimeSpan EndTime { get; set; } public string Status { get; set; } = string.Empty; public decimal? HoursWorked { get; set; } }
    public class AdminDishDto { public int IdDish { get; set; } public string DishName { get; set; } = string.Empty; public string Category { get; set; } = string.Empty; public decimal Price { get; set; } public bool IsActive { get; set; } }
    public class DashboardStatsDto { public decimal TotalRevenue { get; set; } public int OrderCount { get; set; } public string TopDish { get; set; } = string.Empty; }
    public class RecipeItemDto { public int IdIngredient { get; set; } public string IngredientName { get; set; } = string.Empty; public decimal Quantity { get; set; } public string UnitOfMeasure { get; set; } = string.Empty; }
}