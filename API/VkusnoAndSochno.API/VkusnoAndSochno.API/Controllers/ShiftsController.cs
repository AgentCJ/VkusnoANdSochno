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

        // Получение графика конкретного сотрудника
        [HttpGet("my/{employeeId}")]
        public async Task<IActionResult> GetMyShifts(int employeeId)
        {
            var shifts = await (from se in _context.ShiftEmployees
                                join s in _context.Shifts on se.IdShift equals s.IdShift
                                where se.IdEmployee == employeeId
                                orderby s.ShiftDate descending
                                select new { s.IdShift, s.ShiftDate, s.StartTime, s.EndTime, s.Status, se.HoursWorked }).ToListAsync();
            return Ok(shifts);
        }

        // Получение всех смен филиала (для управляющего)
        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetBranchShifts(int branchId)
        {
            var shifts = await (from s in _context.Shifts
                                join se in _context.ShiftEmployees on s.IdShift equals se.IdShift
                                join e in _context.Employees on se.IdEmployee equals e.IdEmployee
                                where s.IdBranch == branchId
                                orderby s.ShiftDate descending, s.StartTime
                                select new
                                {
                                    s.IdShift,
                                    s.ShiftDate,
                                    s.StartTime,
                                    s.EndTime,
                                    s.Status,
                                    se.IdEmployee,
                                    EmployeeName = e.FullName,
                                    se.HoursWorked
                                }).ToListAsync();
            return Ok(shifts);
        }

        // Получение списка сотрудников филиала для выпадающего списка
        [HttpGet("employees/{branchId}")]
        public async Task<IActionResult> GetEmployees(int branchId)
        {
            var emps = await _context.Employees
                .Where(e => e.IdBranch == branchId)
                .Select(e => new { e.IdEmployee, e.FullName })
                .ToListAsync();
            return Ok(emps);
        }

        // Создание новой смены
        [HttpPost]
        public async Task<IActionResult> CreateShift([FromBody] ShiftRequest request)
        {
            try
            {
                // Проверка на пересечение смен у сотрудника в этот день
                var existingShifts = await _context.Shifts
                    .Where(s => s.ShiftDate == request.ShiftDate &&
                                _context.ShiftEmployees.Any(se => se.IdShift == s.IdShift && se.IdEmployee == request.EmployeeId))
                    .ToListAsync();

                if (existingShifts.Any(s => request.StartTime < s.EndTime && request.EndTime > s.StartTime))
                {
                    return BadRequest("У сотрудника уже есть смена, которая пересекается с этим временем.");
                }

                var shift = new Shift
                {
                    IdBranch = request.BranchId,
                    ShiftDate = request.ShiftDate,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Status = "запланирована"
                };
                _context.Shifts.Add(shift);
                await _context.SaveChangesAsync();

                var shiftEmp = new ShiftEmployee
                {
                    IdShift = shift.IdShift,
                    IdEmployee = request.EmployeeId,
                    HoursWorked = (decimal)(request.EndTime - request.StartTime).TotalHours
                };
                _context.ShiftEmployees.Add(shiftEmp);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Смена создана" });
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        // Редактирование существующей смены
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShift(int id, [FromBody] ShiftRequest request)
        {
            try
            {
                var shift = await _context.Shifts.FindAsync(id);
                if (shift == null) return NotFound("Смена не найдена");

                // Проверка пересечения, исключая текущую смену
                var existingShifts = await _context.Shifts
                    .Where(s => s.IdShift != id &&
                                s.ShiftDate == request.ShiftDate &&
                                _context.ShiftEmployees.Any(se => se.IdShift == s.IdShift && se.IdEmployee == request.EmployeeId))
                    .ToListAsync();

                if (existingShifts.Any(s => request.StartTime < s.EndTime && request.EndTime > s.StartTime))
                {
                    return BadRequest("У сотрудника уже есть смена, которая пересекается с этим временем.");
                }

                shift.ShiftDate = request.ShiftDate;
                shift.StartTime = request.StartTime;
                shift.EndTime = request.EndTime;

                var shiftEmp = await _context.ShiftEmployees.FirstOrDefaultAsync(se => se.IdShift == id);
                if (shiftEmp != null)
                {
                    shiftEmp.IdEmployee = request.EmployeeId;
                    shiftEmp.HoursWorked = (decimal)(request.EndTime - request.StartTime).TotalHours;
                }

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Смена обновлена" });
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        // Переключение статуса смены (запланирована / отработана)
        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleShiftStatus(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return NotFound("Смена не найдена");

            shift.Status = shift.Status == "отработана" ? "запланирована" : "отработана";
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Статус изменен" });
        }

        // Удаление смены
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            try
            {
                var shiftEmp = await _context.ShiftEmployees.FirstOrDefaultAsync(se => se.IdShift == id);
                if (shiftEmp != null) _context.ShiftEmployees.Remove(shiftEmp);

                var shift = await _context.Shifts.FindAsync(id);
                if (shift != null) _context.Shifts.Remove(shift);

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Смена удалена" });
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }
    }

    public class ShiftRequest
    {
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public DateOnly ShiftDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}