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
    }

    public class CreateOrderResponse { public int OrderId { get; set; } }
}