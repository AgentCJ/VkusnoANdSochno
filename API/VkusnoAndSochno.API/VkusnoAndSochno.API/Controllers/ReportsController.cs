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

        // GET: api/reports/sales?from=2023-10-01&to=2023-10-31
        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesReport(DateTime from, DateTime to)
        {
            try
            {
                // Решение проблемы: принудительно указываем Npgsql, что это UTC
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
    }

    public class SalesReportDto
    {
        public int IdBranch { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public long OrdersCount { get; set; } // В PostgreSQL COUNT возвращает bigint
        public decimal TotalRevenue { get; set; }
        public decimal AvgOrder { get; set; }
    }
}