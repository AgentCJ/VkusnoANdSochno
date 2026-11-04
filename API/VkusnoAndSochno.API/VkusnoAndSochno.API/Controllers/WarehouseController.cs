using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VkusnoAndSochno.API.Models;

namespace VkusnoAndSochno.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly VkusnoDbContext _context;

        public WarehouseController(VkusnoDbContext context)
        {
            _context = context;
        }

        // Получить остатки (из представления v_stock_balance)
        [HttpGet("balance/{branchId}")]
        public async Task<IActionResult> GetBalance(int branchId)
        {
            try
            {
                // Явно указываем алиасы для колонок, чтобы они совпали с C#-классом
                var sql = @"
                    SELECT id_branch AS ""IdBranch"", 
                           branch_name AS ""BranchName"", 
                           id_ingredient AS ""IdIngredient"", 
                           ingredient_name AS ""IngredientName"",
                           unit_of_measure AS ""UnitOfMeasure"", 
                           min_remaining AS ""MinRemaining"",
                           balance AS ""Balance""
                    FROM v_stock_balance 
                    WHERE id_branch = {0}";

                var result = await _context.Database
                    .SqlQueryRaw<StockBalanceDto>(sql, branchId)
                    .ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Получить список всех ингредиентов (для выпадающего списка)
        [HttpGet("ingredients")]
        public async Task<IActionResult> GetIngredients()
        {
            var ingredients = await _context.Ingredients
                .Select(i => new { i.IdIngredient, i.IngredientName })
                .ToListAsync();
            return Ok(ingredients);
        }

        // Получить список поставщиков (для выпадающего списка)
        [HttpGet("suppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new { s.IdSupplier, s.SupplierName })
                .ToListAsync();
            return Ok(suppliers);
        }

        // Зарегистрировать поставку (вызываем процедуру register_supply)
        [HttpPost("supply")]
        public async Task<IActionResult> RegisterSupply([FromBody] SupplyRequest request)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "CALL register_supply({0}, {1}, {2}, {3}, {4}, {5})",
                    request.EmployeeId, request.BranchId, request.SupplierId,
                    request.IngredientId, request.Quantity, request.Comment);

                return Ok(new { Message = "Поставка успешно зарегистрирована" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }
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

    public class SupplyRequest
    {
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }
        public int SupplierId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; }
    }
}