using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VkusnoAndSochno.API.Models;

namespace VkusnoAndSochno.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly VkusnoDbContext _context;

        public RecipesController(VkusnoDbContext context)
        {
            _context = context;
        }

        // Получение списка ингредиентов, входящих в блюдо
        [HttpGet("{dishId}")]
        public async Task<IActionResult> GetRecipe(int dishId)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.IdDish == dishId);
            if (recipe == null) return Ok(new List<object>()); // Если рецепт не создан

            var items = await (from ri in _context.RecipeIngredients
                               join i in _context.Ingredients on ri.IdIngredient equals i.IdIngredient
                               where ri.IdRecipe == recipe.IdRecipe
                               select new
                               {
                                   ri.IdIngredient,
                                   i.IngredientName,
                                   ri.Quantity,
                                   i.UnitOfMeasure
                               }).ToListAsync();

            return Ok(items);
        }

        // Добавление или обновление ингредиента в рецепте
        [HttpPost("{dishId}")]
        public async Task<IActionResult> AddIngredient(int dishId, [FromBody] RecipeItemRequest request)
        {
            try
            {
                // Проверяем, существует ли рецепт для блюда, если нет - создаем
                var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.IdDish == dishId);
                if (recipe == null)
                {
                    var dish = await _context.Dishes.FindAsync(dishId);
                    if (dish == null) return NotFound("Блюдо не найдено");

                    recipe = new Recipe { IdDish = dishId, Description = $"Рецепт для {dish.DishName}" };
                    _context.Recipes.Add(recipe);
                    await _context.SaveChangesAsync();
                }

                // Проверяем наличие ингредиента в текущем рецепте
                var existingItem = await _context.RecipeIngredients
                    .FirstOrDefaultAsync(ri => ri.IdRecipe == recipe.IdRecipe && ri.IdIngredient == request.IdIngredient);

                if (existingItem != null)
                {
                    existingItem.Quantity = request.Quantity;
                }
                else
                {
                    _context.RecipeIngredients.Add(new RecipeIngredient
                    {
                        IdRecipe = recipe.IdRecipe,
                        IdIngredient = request.IdIngredient,
                        Quantity = request.Quantity
                    });
                }

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Ингредиент добавлен в рецепт" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        // Удаление ингредиента из рецепта
        [HttpDelete("{dishId}/ingredients/{ingredientId}")]
        public async Task<IActionResult> RemoveIngredient(int dishId, int ingredientId)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.IdDish == dishId);
            if (recipe == null) return NotFound();

            var item = await _context.RecipeIngredients
                .FirstOrDefaultAsync(ri => ri.IdRecipe == recipe.IdRecipe && ri.IdIngredient == ingredientId);

            if (item == null) return NotFound("Ингредиент не найден в рецепте");

            _context.RecipeIngredients.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Ингредиент удален" });
        }
    }

    public class RecipeItemRequest
    {
        public int IdIngredient { get; set; }
        public decimal Quantity { get; set; }
    }
}