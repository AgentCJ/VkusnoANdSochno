using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VkusnoAndSochno.API.Models;

namespace VkusnoAndSochno.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly VkusnoDbContext _context;

        public MenuController(VkusnoDbContext context)
        {
            _context = context;
        }

        // Получение полного списка блюд с сортировкой по категориям
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDishes()
        {
            var dishes = await _context.Dishes
                .OrderBy(d => d.Category)
                .ThenBy(d => d.DishName)
                .Select(d => new { d.IdDish, d.DishName, d.Category, d.Price, d.IsActive })
                .ToListAsync();
            return Ok(dishes);
        }

        // POST: api/menu
        [HttpPost]
        public async Task<IActionResult> AddDish([FromBody] DishRequest request)
        {
            var dish = new Dish
            {
                DishName = request.DishName,
                Category = request.Category,
                Price = request.Price,
                IsActive = true
            };
            _context.Dishes.Add(dish);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Блюдо добавлено", Id = dish.IdDish });
        }

        // PUT: api/menu/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDish(int id, [FromBody] DishRequest request)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null) return NotFound("Блюдо не найдено");

            dish.DishName = request.DishName;
            dish.Category = request.Category;
            dish.Price = request.Price;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Блюдо обновлено" });
        }

        // DELETE: api/menu/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDish(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null) return NotFound("Блюдо не найдено");

            try
            {
                _context.Dishes.Remove(dish);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Блюдо удалено" });
            }
            catch (DbUpdateException)
            {
                // Ошибка внешнего ключа, если блюдо уже фигурирует в заказах
                return BadRequest("Невозможно удалить блюдо, так как оно уже есть в истории заказов. Используйте «Снять с продажи».");
            }
        }

        // Переключение доступности блюда (в меню/снято с продажи)
        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleDish(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null) return NotFound("Блюдо не найдено");

            dish.IsActive = !dish.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Статус изменен", IsActive = dish.IsActive });
        }
    }

    public class DishRequest
    {
        public string DishName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}