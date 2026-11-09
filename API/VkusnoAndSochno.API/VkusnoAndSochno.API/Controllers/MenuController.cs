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

        // Получить ВСЕ блюда (включая снятые с продажи) для управляющего
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

        // Добавить новое блюдо
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

        // Переключить статус (снять/вернуть в продажу)
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