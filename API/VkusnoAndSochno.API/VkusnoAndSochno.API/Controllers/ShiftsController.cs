using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VkusnoAndSochno.API.Models;

namespace VkusnoAndSochno.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftsController : ControllerBase
    {
        private readonly VkusnoDbContext _context;

        public ShiftsController(VkusnoDbContext context)
        {
            _context = context;
        }

        // Получить график смен для конкретного сотрудника
        [HttpGet("my/{employeeId}")]
        public async Task<IActionResult> GetMyShifts(int employeeId)
        {
            var shifts = await (from se in _context.ShiftEmployees
                                join s in _context.Shifts on se.IdShift equals s.IdShift
                                where se.IdEmployee == employeeId
                                orderby s.ShiftDate descending
                                select new
                                {
                                    s.IdShift,
                                    s.ShiftDate,
                                    s.StartTime,
                                    s.EndTime,
                                    s.Status,
                                    se.HoursWorked
                                }).ToListAsync();

            return Ok(shifts);
        }
    }
}