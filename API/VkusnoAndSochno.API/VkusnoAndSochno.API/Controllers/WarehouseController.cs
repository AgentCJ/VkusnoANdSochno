using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VkusnoAndSochno.API.Models;
using Npgsql;

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

        [HttpGet("balance/{branchId}")]
        public async Task<IActionResult> GetBalance(int branchId)
        {
            try
            {
                var sql = @"
                    SELECT {0} AS ""IdBranch"", i.id_ingredient AS ""IdIngredient"", i.ingredient_name AS ""IngredientName"",
                           i.unit_of_measure AS ""UnitOfMeasure"", i.min_remaining AS ""MinRemaining"",
                           i.cost_price AS ""CostPrice"", fn_ingredient_balance(i.id_ingredient, {0}) AS ""Balance""
                    FROM ""Ingredient"" i";

                var result = await _context.Database.SqlQueryRaw<StockBalanceDto>(sql, branchId).ToListAsync();
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, ex.Message); }
        }

        [HttpGet("ingredients")]
        public async Task<IActionResult> GetIngredients()
        {
            var ingredients = await _context.Ingredients
                .Select(i => new { i.IdIngredient, i.IngredientName, i.UnitOfMeasure })
                .ToListAsync();
            return Ok(ingredients);
        }

        [HttpGet("suppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new { s.IdSupplier, s.SupplierName })
                .ToListAsync();
            return Ok(suppliers);
        }

        [HttpPost("supply")]
        public async Task<IActionResult> RegisterSupply([FromBody] SupplyRequest request)
        {
            try
            {
                if (request.Quantity <= 0 || request.Quantity > 1000)
                    return BadRequest("Количество должно быть от 1 до 1000.");

                await _context.Database.ExecuteSqlRawAsync(
                    "CALL register_supply({0}, {1}, {2}, {3}, {4}, {5})",
                    request.EmployeeId, request.BranchId, request.SupplierId,
                    request.IngredientId, request.Quantity, request.Comment);
                return Ok(new { Message = "Поставка успешно зарегистрирована" });
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "22003")
            {
                return BadRequest("Введенное количество слишком велико для базы данных.");
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        [HttpPost("ingredient")]
        public async Task<IActionResult> AddIngredient([FromBody] IngredientRequest request)
        {
            try
            {
                if (request.IngredientName.Length > 100) return BadRequest("Название продукта не должно превышать 100 символов.");

                var exists = await _context.Ingredients
                    .AnyAsync(i => i.IngredientName.ToLower() == request.IngredientName.ToLower());

                if (exists) return BadRequest("Продукт с таким названием уже существует.");

                var ingredient = new Ingredient
                {
                    IngredientName = request.IngredientName,
                    UnitOfMeasure = request.UnitOfMeasure,
                    CostPrice = request.CostPrice,
                    MinRemaining = request.MinRemaining
                };
                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync();

                if (request.InitialQuantity > 0)
                {
                    var op = new WarehouseOperation
                    {
                        IdEmployee = request.EmployeeId,
                        IdBranch = request.BranchId,
                        OperationType = "приход",
                        Comment = "Начальный остаток при создании продукта",
                        OperationDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                    };
                    _context.WarehouseOperations.Add(op);
                    await _context.SaveChangesAsync();

                    _context.OperationItems.Add(new OperationItem
                    {
                        IdOperation = op.IdOperation,
                        IdIngredient = ingredient.IdIngredient,
                        Quantity = request.InitialQuantity
                    });
                    await _context.SaveChangesAsync();
                }

                return Ok(new { Message = "Продукт добавлен" });
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        [HttpPut("ingredient/{id}")]
        public async Task<IActionResult> UpdateIngredient(int id, [FromBody] IngredientRequest request)
        {
            try
            {
                if (request.IngredientName.Length > 100) return BadRequest("Название продукта не должно превышать 100 символов.");

                var ingredient = await _context.Ingredients.FindAsync(id);
                if (ingredient == null) return NotFound("Продукт не найден");

                var exists = await _context.Ingredients
                    .AnyAsync(i => i.IngredientName.ToLower() == request.IngredientName.ToLower() && i.IdIngredient != id);

                if (exists) return BadRequest("Продукт с таким названием уже существует.");

                ingredient.IngredientName = request.IngredientName;
                ingredient.UnitOfMeasure = request.UnitOfMeasure;
                ingredient.CostPrice = request.CostPrice;
                ingredient.MinRemaining = request.MinRemaining;

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Продукт обновлен" });
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        // Удаление продукта
        [HttpDelete("ingredient/{id}")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            try
            {
                var ingredient = await _context.Ingredients.FindAsync(id);
                if (ingredient == null) return NotFound("Продукт не найден");

                // Проверка, используется ли продукт в рецептах
                var usedInRecipes = await _context.RecipeIngredients.AnyAsync(ri => ri.IdIngredient == id);
                if (usedInRecipes)
                {
                    return BadRequest("Невозможно удалить продукт: он используется в рецептах блюд.");
                }

                // Удаление складских операций, связанных с продуктом
                var opItems = await _context.OperationItems.Where(oi => oi.IdIngredient == id).ToListAsync();
                _context.OperationItems.RemoveRange(opItems);

                var opIds = opItems.Select(oi => oi.IdOperation).Distinct().ToList();
                foreach (var opId in opIds)
                {
                    var hasOtherItems = await _context.OperationItems.AnyAsync(oi => oi.IdOperation == opId && oi.IdIngredient != id);
                    if (!hasOtherItems)
                    {
                        var op = await _context.WarehouseOperations.FindAsync(opId);
                        if (op != null) _context.WarehouseOperations.Remove(op);
                    }
                }

                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Продукт удален" });
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustStock([FromBody] AdjustRequest request)
        {
            try
            {
                if (request.FactQuantity < 0 || request.FactQuantity > 1000)
                    return BadRequest("Количество должно быть от 0 до 1000.");

                var currentBalance = await _context.Database
                    .SqlQueryRaw<decimal>("SELECT fn_ingredient_balance({0}, {1}) AS \"Value\"", request.IngredientId, request.BranchId)
                    .FirstOrDefaultAsync();

                var diff = request.FactQuantity - currentBalance;

                if (diff == 0) return Ok(new { Message = "Остаток не изменился" });

                var op = new WarehouseOperation
                {
                    IdEmployee = request.EmployeeId,
                    IdBranch = request.BranchId,
                    OperationType = diff > 0 ? "приход" : "списание",
                    Comment = "Корректировка (Инвентаризация)",
                    OperationDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                };
                _context.WarehouseOperations.Add(op);
                await _context.SaveChangesAsync();

                _context.OperationItems.Add(new OperationItem
                {
                    IdOperation = op.IdOperation,
                    IdIngredient = request.IngredientId,
                    Quantity = Math.Abs(diff)
                });
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Остаток скорректирован" });
            }
            catch (PostgresException pgEx) when (pgEx.SqlState == "22003")
            {
                return BadRequest("Введенное количество слишком велико для базы данных.");
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        public class AdjustRequest
        {
            public int EmployeeId { get; set; }
            public int BranchId { get; set; }
            public int IngredientId { get; set; }
            public decimal FactQuantity { get; set; }
        }
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

    public class SupplyRequest
    {
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }
        public int SupplierId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public string? Comment { get; set; }
    }

    public class IngredientRequest
    {
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal MinRemaining { get; set; }
        public decimal InitialQuantity { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
    }
}