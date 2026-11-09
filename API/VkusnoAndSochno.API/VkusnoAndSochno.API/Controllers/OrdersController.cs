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
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
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
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
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
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            var orders = await (from o in _context.Orders
                                where o.Status == "оплачен" || o.Status == "готовится"
                                orderby o.OrderTime
                                select new
                                {
                                    o.IdOrder,
                                    o.Status,
                                    o.TotalAmount,
                                    o.OrderTime,
                                    Items = (from oi in _context.OrderItems
                                             join d in _context.Dishes on oi.IdDish equals d.IdDish
                                             where oi.IdOrder == o.IdOrder
                                             select new { d.DishName, oi.Quantity }).ToList()
                                }).ToListAsync();
            return Ok(orders);
        }

        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE \"Order\" SET status = {0} WHERE id_order = {1}", status, orderId);

                if (result == 0) return NotFound("Заказ не найден");
                return Ok(new { Message = $"Статус изменен на '{status}'" });
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        // ОБНОВЛЕННЫЙ МЕТОД: Проверяет наличие ингредиентов
        [HttpGet("menu/{branchId}")]
        public async Task<IActionResult> GetMenu(int branchId)
        {
            var dishes = await _context.Dishes.Where(d => d.IsActive).ToListAsync();
            var result = new List<object>();

            foreach (var d in dishes)
            {
                bool isAvailable = true;
                var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.IdDish == d.IdDish);

                if (recipe != null)
                {
                    var recipeItems = await _context.RecipeIngredients.Where(ri => ri.IdRecipe == recipe.IdRecipe).ToListAsync();
                    foreach (var ri in recipeItems)
                    {
                        // ИСПРАВЛЕНИЕ ЗДЕСЬ: добавляем AS "Value", чтобы EF Core понял, куда класть число
                        var balance = await _context.Database
                            .SqlQueryRaw<decimal>("SELECT fn_ingredient_balance({0}, {1}) AS \"Value\"", ri.IdIngredient, branchId)
                            .FirstOrDefaultAsync();

                        if (balance < ri.Quantity)
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                }
                result.Add(new { d.IdDish, d.DishName, d.Category, d.Price, IsAvailable = isAvailable });
            }
            return Ok(result);
        }

        [HttpDelete("{orderId}/items/{dishId}")]
        public async Task<IActionResult> RemoveItem(int orderId, int dishId)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM \"OrderItem\" WHERE id_order = {0} AND id_dish = {1}", orderId, dishId);
                return Ok(new { Message = "Позиция удалена" });
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }
    }
}