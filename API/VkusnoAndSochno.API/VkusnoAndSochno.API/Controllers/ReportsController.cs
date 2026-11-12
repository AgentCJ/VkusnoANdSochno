using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VkusnoAndSochno.API.Models;

namespace VkusnoAndSochno.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly VkusnoDbContext _context;

        public ReportsController(VkusnoDbContext context)
        {
            _context = context;
        }

        // Формирование отчета по продажам за период с использованием функции БД
        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport(DateTime from, DateTime to)
        {
            try
            {
                var utcFrom = DateTime.SpecifyKind(from.Date, DateTimeKind.Utc);
                var utcTo = DateTime.SpecifyKind(to.Date, DateTimeKind.Utc);

                var sql = @"
                    SELECT id_branch AS ""IdBranch"", 
                           branch_name AS ""BranchName"", 
                           orders_count AS ""OrdersCount"", 
                           total_revenue AS ""TotalRevenue"", 
                           avg_order AS ""AvgOrder""
                    FROM report_sales({0}::date, {1}::date)";

                var result = await _context.Database
                    .SqlQueryRaw<SalesReportDto>(sql, utcFrom, utcTo)
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // Сводка для дашборда (выручка, количество заказов, топ-блюдо)
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var today = DateTime.Today;

                var stats = await _context.Orders
                    .Where(o => o.Status == "выполнен" && o.OrderTime.Date == today)
                    .GroupBy(o => 1)
                    .Select(g => new
                    {
                        TotalRevenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .FirstOrDefaultAsync();

                var topDish = await (from oi in _context.OrderItems
                                     join o in _context.Orders on oi.IdOrder equals o.IdOrder
                                     join d in _context.Dishes on oi.IdDish equals d.IdDish
                                     where o.Status == "выполнен" && o.OrderTime.Date == today
                                     group oi by d.DishName into g
                                     orderby g.Sum(x => x.Quantity) descending
                                     select g.Key).FirstOrDefaultAsync();

                return Ok(new
                {
                    TotalRevenue = stats?.TotalRevenue ?? 0,
                    OrderCount = stats?.OrderCount ?? 0,
                    TopDish = topDish ?? "Нет данных"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        // Детальный отчет за сегодня (выручка, навар, топ/анти-топ, сотрудники)
        [HttpGet("detailed/{branchId}")]
        public async Task<IActionResult> GetDetailedReport(int branchId)
        {
            try
            {
                var today = DateTime.Today;

                var orders = await _context.Orders
                    .Where(o => o.Status == "выполнен" && o.OrderTime.Date == today && o.IdBranch == branchId)
                    .ToListAsync();

                int ordersCount = orders.Count;
                decimal revenue = orders.Sum(o => o.TotalAmount);

                var dishStats = await (from oi in _context.OrderItems
                                       join o in _context.Orders on oi.IdOrder equals o.IdOrder
                                       join d in _context.Dishes on oi.IdDish equals d.IdDish
                                       where o.Status == "выполнен" && o.OrderTime.Date == today && o.IdBranch == branchId
                                       group oi by d.DishName into g
                                       select new { DishName = g.Key, TotalQty = g.Sum(x => x.Quantity) })
                                       .OrderByDescending(x => x.TotalQty)
                                       .ToListAsync();

                string topDish = dishStats.FirstOrDefault()?.DishName ?? "Нет данных";
                string bottomDish = dishStats.LastOrDefault()?.DishName ?? "Нет данных";

                // Расчет количества сотрудников на смене
                int employeesOnShift = await (from se in _context.ShiftEmployees
                                              join s in _context.Shifts on se.IdShift equals s.IdShift
                                              where s.ShiftDate == DateOnly.FromDateTime(today) && s.IdBranch == branchId
                                              select se.IdEmployee).Distinct().CountAsync();

                // Расчет себестоимости проданных блюд для вычисления навара
                decimal totalCost = 0;
                var soldItems = await (from oi in _context.OrderItems
                                       join o in _context.Orders on oi.IdOrder equals o.IdOrder
                                       where o.Status == "выполнен" && o.OrderTime.Date == today && o.IdBranch == branchId
                                       select new { oi.IdDish, oi.Quantity }).ToListAsync();

                foreach (var item in soldItems)
                {
                    var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.IdDish == item.IdDish);
                    if (recipe != null)
                    {
                        var ingredients = await _context.RecipeIngredients.Where(ri => ri.IdRecipe == recipe.IdRecipe).ToListAsync();
                        foreach (var ing in ingredients)
                        {
                            var ingInfo = await _context.Ingredients.FindAsync(ing.IdIngredient);
                            if (ingInfo != null) totalCost += (ingInfo.CostPrice * ing.Quantity) * item.Quantity;
                        }
                    }
                }

                decimal profit = revenue - totalCost;

                return Ok(new
                {
                    Date = today.ToString("dd.MM.yyyy"),
                    OrdersCount = ordersCount,
                    TotalRevenue = revenue,
                    TotalProfit = profit,
                    TopDish = topDish,
                    BottomDish = bottomDish,
                    EmployeesOnShift = employeesOnShift
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }
    }

    public class SalesReportDto
    {
        public int IdBranch { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public long OrdersCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgOrder { get; set; }
    }
}