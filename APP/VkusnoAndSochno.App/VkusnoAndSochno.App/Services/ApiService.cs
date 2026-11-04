using System.Net.Http.Json;

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

    }
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

}