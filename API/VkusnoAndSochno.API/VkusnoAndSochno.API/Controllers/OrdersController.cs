using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VkusnoAndSochno.API.Models;

namespace VkusnoAndSochno.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly VkusnoDbContext _context;

        public OrdersController(VkusnoDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder(int employeeId, int branchId)
        {
            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<int>($"SELECT create_order({employeeId}, {branchId})")
                    .ToListAsync();
                return Ok(new { OrderId = result.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPost("{orderId}/add-item")]
        public async Task<IActionResult> AddItem(int orderId, int dishId, int quantity)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL add_order_item({0}, {1}, {2})", orderId, dishId, quantity);
                return Ok(new { Message = "Блюдо добавлено" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPost("{orderId}/close")]
        public async Task<IActionResult> CloseOrder(int orderId, string method)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL close_order({0}, {1})", orderId, method);
                return Ok(new { Message = "Заказ оплачен" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}