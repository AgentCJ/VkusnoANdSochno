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
            // Отключаем кэширование, чтобы данные на экране всегда были актуальными
            _httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true, NoStore = true };
        }

        //ЗАКАЗЫ
        public async Task<int> CreateOrderAsync(int employeeId, int branchId)
        {
            var response = await _httpClient.PostAsync($"orders/create?employeeId={employeeId}&branchId={branchId}", null);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
            return result!.OrderId;
        }

        public async Task AddItemAsync(int orderId, int dishId, int quantity)
        {
            var res = await _httpClient.PostAsync($"orders/{orderId}/add-item?dishId={dishId}&quantity={quantity}", null);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task RemoveItemAsync(int orderId, int dishId)
        {
            var res = await _httpClient.DeleteAsync($"orders/{orderId}/items/{dishId}");
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task CloseOrderAsync(int orderId, string method)
        {
            var res = await _httpClient.PostAsync($"orders/{orderId}/close?method={method}", null);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var res = await _httpClient.PostAsync($"orders/{orderId}/cancel", null);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task<List<OrderHistoryDto>> GetOrderHistoryAsync(int branchId)
        {
            var res = await _httpClient.GetFromJsonAsync<List<OrderHistoryDto>>($"orders/history/{branchId}");
            return res ?? new List<OrderHistoryDto>();
        }

        public async Task<List<ActiveOrderDto>> GetActiveOrdersAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<List<ActiveOrderDto>>("orders/active");
            return res ?? new List<ActiveOrderDto>();
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var res = await _httpClient.PutAsync($"orders/{orderId}/status?status={status}", null);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task<List<DishDto>> GetMenuAsync(int branchId)
        {
            var res = await _httpClient.GetFromJsonAsync<List<DishDto>>($"orders/menu/{branchId}");
            return res ?? new List<DishDto>();
        }

        // АВТОРИЗАЦИЯ
        public async Task<LoginResponse?> LoginAsync(string login, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", new { Login = login, Password = password });
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        // СКЛАД
        public async Task<List<StockBalanceDto>> GetBalanceAsync(int branchId)
        {
            var res = await _httpClient.GetFromJsonAsync<List<StockBalanceDto>>($"warehouse/balance/{branchId}?t={DateTime.Now.Ticks}");
            return res ?? new List<StockBalanceDto>();
        }

        public async Task<List<IngredientDto>> GetIngredientsAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<List<IngredientDto>>("warehouse/ingredients");
            return res ?? new List<IngredientDto>();
        }

        public async Task<List<SupplierDto>> GetSuppliersAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<List<SupplierDto>>("warehouse/suppliers");
            return res ?? new List<SupplierDto>();
        }

        public async Task RegisterSupplyAsync(object request)
        {
            var res = await _httpClient.PostAsJsonAsync("warehouse/supply", request);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task AddIngredientAsync(object request)
        {
            var res = await _httpClient.PostAsJsonAsync("warehouse/ingredient", request);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task UpdateIngredientAsync(int id, object request)
        {
            var res = await _httpClient.PutAsJsonAsync($"warehouse/ingredient/{id}", request);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task DeleteIngredientAsync(int id)
        {
            var res = await _httpClient.DeleteAsync($"warehouse/ingredient/{id}");
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task AdjustStockAsync(object request)
        {
            var res = await _httpClient.PostAsJsonAsync("warehouse/adjust", request);
            if (!res.IsSuccessStatusCode) throw new Exception(await res.Content.ReadAsStringAsync());
        }

        // ОТЧЕТЫ И ДАШБОРД
        public async Task<List<SalesReportDto>> GetSalesReportAsync(DateTime from, DateTime to)
        {
            var res = await _httpClient.GetAsync($"reports/sales?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
            return await res.Content.ReadFromJsonAsync<List<SalesReportDto>>() ?? new List<SalesReportDto>();
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<DashboardStatsDto>("reports/dashboard");
            return res ?? new DashboardStatsDto();
        }

        public async Task<DetailedReportDto> GetDetailedReportAsync(int branchId)
        {
            var res = await _httpClient.GetFromJsonAsync<DetailedReportDto>($"reports/detailed/{branchId}");
            return res ?? new DetailedReportDto();
        }

        // АДМИНИСТРАТОР
        public async Task<List<AdminUserDto>> GetUsersAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<List<AdminUserDto>>("admin/users");
            return res ?? new List<AdminUserDto>();
        }

        public async Task ToggleUserStatusAsync(int userId)
        {
            var res = await _httpClient.PutAsync($"admin/users/{userId}/toggle", null);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task<AdminMetadataDto> GetAdminMetadataAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<AdminMetadataDto>("admin/metadata");
            return res ?? new AdminMetadataDto();
        }

        public async Task CreateUserAsync(object request)
        {
            var res = await _httpClient.PostAsJsonAsync("admin/users", request);
            if (!res.IsSuccessStatusCode) throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task UpdateUserAsync(int id, object request)
        {
            var res = await _httpClient.PutAsJsonAsync($"admin/users/{id}", request);
            if (!res.IsSuccessStatusCode) throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task DeleteUserAsync(int id)
        {
            var res = await _httpClient.DeleteAsync($"admin/users/{id}");
            if (!res.IsSuccessStatusCode) throw new Exception(await res.Content.ReadAsStringAsync());
        }

        // ГРАФИК СМЕН
        public async Task<List<ShiftDto>> GetMyShiftsAsync(int employeeId)
        {
            var res = await _httpClient.GetFromJsonAsync<List<ShiftDto>>($"shifts/my/{employeeId}");
            return res ?? new List<ShiftDto>();
        }

        public async Task<List<BranchShiftDto>> GetBranchShiftsAsync(int branchId)
        {
            return await _httpClient.GetFromJsonAsync<List<BranchShiftDto>>($"shifts/branch/{branchId}") ?? new();
        }

        public async Task<List<EmployeeShortDto>> GetEmployeesForShiftsAsync(int branchId)
        {
            return await _httpClient.GetFromJsonAsync<List<EmployeeShortDto>>($"shifts/employees/{branchId}") ?? new();
        }

        public async Task CreateShiftAsync(object request)
        {
            var r = await _httpClient.PostAsJsonAsync("shifts", request);
            if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync());
        }

        public async Task UpdateShiftAsync(int id, object request)
        {
            var r = await _httpClient.PutAsJsonAsync($"shifts/{id}", request);
            if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync());
        }

        public async Task DeleteShiftAsync(int id)
        {
            var r = await _httpClient.DeleteAsync($"shifts/{id}");
            if (!r.IsSuccessStatusCode) throw new Exception(await r.Content.ReadAsStringAsync());
        }

        public async Task ToggleShiftStatusAsync(int id)
        {
            var res = await _httpClient.PutAsync($"shifts/{id}/toggle-status", null);
            if (!res.IsSuccessStatusCode) throw new Exception(await res.Content.ReadAsStringAsync());
        }

        // УПРАВЛЕНИЕ МЕНЮ
        public async Task<List<AdminDishDto>> GetAllDishesAsync()
        {
            var res = await _httpClient.GetFromJsonAsync<List<AdminDishDto>>("menu/all");
            return res ?? new List<AdminDishDto>();
        }

        public async Task AddDishAsync(object request)
        {
            var res = await _httpClient.PostAsJsonAsync("menu", request);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task UpdateDishAsync(int id, object request)
        {
            var res = await _httpClient.PutAsJsonAsync($"menu/{id}", request);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task DeleteDishAsync(int id)
        {
            var res = await _httpClient.DeleteAsync($"menu/{id}");
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task ToggleDishAsync(int dishId)
        {
            var res = await _httpClient.PutAsync($"menu/{dishId}/toggle", null);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        // РЕЦЕПТЫ
        public async Task<List<RecipeItemDto>> GetRecipeAsync(int dishId)
        {
            var res = await _httpClient.GetFromJsonAsync<List<RecipeItemDto>>($"recipes/{dishId}");
            return res ?? new List<RecipeItemDto>();
        }

        public async Task AddRecipeItemAsync(int dishId, object request)
        {
            var res = await _httpClient.PostAsJsonAsync($"recipes/{dishId}", request);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public async Task RemoveRecipeItemAsync(int dishId, int ingredientId)
        {
            var res = await _httpClient.DeleteAsync($"recipes/{dishId}/ingredients/{ingredientId}");
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }
    }

    // КЛАССЫ ДАННЫХ (DTO)

    public class LoginResponse
    {
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class CreateOrderResponse
    {
        public int OrderId { get; set; }
    }

    public partial class ActiveOrderDto : ObservableObject
    {
        public int IdOrder { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime OrderTime { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();

        [ObservableProperty]
        private string timeElapsed = "00:00";

        [ObservableProperty]
        private bool isOverdue = false;
    }

    public class OrderItemDto
    {
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public partial class OrderHistoryDto : ObservableObject
    {
        public int IdOrder { get; set; }
        public DateTime OrderTime { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;

        public List<OrderHistoryItemDto> Items { get; set; } = new();

        [ObservableProperty]
        private bool isExpanded = false;
    }

    public class OrderHistoryItemDto
    {
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAtSale { get; set; }
    }

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

    public class IngredientDto
    {
        public int IdIngredient { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
    }

    public class SupplierDto
    {
        public int IdSupplier { get; set; }
        public string SupplierName { get; set; } = string.Empty;
    }

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
        public bool IsAvailable { get; set; }
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

    public class ShiftDto
    {
        public int IdShift { get; set; }
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? HoursWorked { get; set; }
    }

    public class AdminDishDto
    {
        public int IdDish { get; set; }
        public string DishName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }

    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public string TopDish { get; set; } = string.Empty;
    }

    public class DetailedReportDto
    {
        public string Date { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public string TopDish { get; set; } = string.Empty;
        public string BottomDish { get; set; } = string.Empty;
        public int EmployeesOnShift { get; set; }
    }

    public class RecipeItemDto
    {
        public int IdIngredient { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
    }

    public class AdminUserDto
    {
        public int IdUser { get; set; }
        public string Login { get; set; } = string.Empty;
        public int IdEmployee { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int IdRole { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int IdBranch { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoleDto
    {
        public int IdRole { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class BranchDto
    {
        public int IdBranch { get; set; }
        public string BranchName { get; set; } = string.Empty;
    }

    public class PositionDto
    {
        public int IdPosition { get; set; }
        public string PositionName { get; set; } = string.Empty;
    }

    public class AdminMetadataDto
    {
        public List<RoleDto> Roles { get; set; } = new();
        public List<BranchDto> Branches { get; set; } = new();
        public List<PositionDto> Positions { get; set; } = new();
    }

    public class BranchShiftDto
    {
        public int IdShift { get; set; }
        public DateOnly ShiftDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int IdEmployee { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal? HoursWorked { get; set; }
        public bool IsToday => ShiftDate == DateOnly.FromDateTime(DateTime.Today);
    }

    public class EmployeeShortDto
    {
        public int IdEmployee { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}