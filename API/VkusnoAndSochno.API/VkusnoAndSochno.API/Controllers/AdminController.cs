using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VkusnoAndSochno.API.Models;

namespace VkusnoAndSochno.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly VkusnoDbContext _context;

        public AdminController(VkusnoDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            // Использование LINQ для объединения таблиц Пользователей, Сотрудников, Ролей и Филиалов
            var users = await (from u in _context.Users
                               join e in _context.Employees on u.IdEmployee equals e.IdEmployee
                               join r in _context.Roles on u.IdRole equals r.IdRole
                               join b in _context.Branches on e.IdBranch equals b.IdBranch
                               select new
                               {
                                   u.IdUser,
                                   u.Login,
                                   u.IdEmployee,
                                   EmployeeName = e.FullName,
                                   RoleName = r.RoleName,
                                   u.IdRole,
                                   BranchName = b.BranchName,
                                   e.IdBranch,
                                   u.IsActive
                               }).ToListAsync();

            return Ok(users);
        }

        // Получение справочников для выпадающих списков (роли, филиалы, должности)
        [HttpGet("metadata")]
        public async Task<IActionResult> GetMetadata()
        {
            var roles = await _context.Roles.Select(r => new { r.IdRole, r.RoleName }).ToListAsync();
            var branches = await _context.Branches.Select(b => new { b.IdBranch, b.BranchName }).ToListAsync();
            var positions = await _context.Positions.Select(p => new { p.IdPosition, p.PositionName }).ToListAsync();
            return Ok(new { Roles = roles, Branches = branches, Positions = positions });
        }

        // POST: api/admin/users
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] UserRequest request)
        {
            try
            {
                // Валидация входных данных
                if (string.IsNullOrWhiteSpace(request.Login) || request.Login.Length < 5)
                    return BadRequest("Логин должен содержать минимум 5 символов.");

                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 5)
                    return BadRequest("Пароль должен содержать минимум 5 символов.");

                if (string.IsNullOrWhiteSpace(request.EmployeeName) ||
                    request.EmployeeName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length != 3)
                    return BadRequest("Введите ФИО полностью (Фамилия Имя Отчество).");

                // Сначала создаем запись сотрудника
                var employee = new Employee
                {
                    FullName = request.EmployeeName.Trim(),
                    IdBranch = request.BranchId,
                    IdPosition = request.PositionId,
                    HireDate = DateOnly.FromDateTime(DateTime.Today),
                    Phone = request.Phone
                };
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // Затем создаем учетную запись для него
                var user = new User
                {
                    IdEmployee = employee.IdEmployee,
                    IdRole = request.RoleId,
                    Login = request.Login,
                    PasswordHash = request.Password,
                    IsActive = true
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Пользователь создан" });
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        // PUT: api/admin/users/{id}
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Login) || request.Login.Length < 5)
                    return BadRequest("Логин должен содержать минимум 5 символов.");

                if (!string.IsNullOrWhiteSpace(request.Password) && request.Password.Length < 5)
                    return BadRequest("Пароль должен содержать минимум 5 символов.");

                if (string.IsNullOrWhiteSpace(request.EmployeeName) ||
                    request.EmployeeName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length != 3)
                    return BadRequest("Введите ФИО полностью (Фамилия Имя Отчество).");

                // Поиск записи и включение связанных данных через Include (JOIN)
                var user = await _context.Users.Include(u => u.IdEmployeeNavigation).FirstOrDefaultAsync(u => u.IdUser == id);
                if (user == null) return NotFound("Пользователь не найден");

                // Обновление данных сотрудника
                user.IdEmployeeNavigation.FullName = request.EmployeeName.Trim();
                user.IdEmployeeNavigation.IdBranch = request.BranchId;
                user.IdEmployeeNavigation.Phone = request.Phone;

                // Обновление данных учетной записи
                user.Login = request.Login;
                user.IdRole = request.RoleId;
                if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = request.Password;

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Пользователь обновлен" });
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }

        // Переключение статуса активности (блокировка/разблокировка)
        [HttpPut("users/{id}/toggle")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Пользователь не найден");

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Статус изменен" });
        }

        // DELETE: api/admin/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.Include(u => u.IdEmployeeNavigation).FirstOrDefaultAsync(u => u.IdUser == id);
                if (user == null) return NotFound("Пользователь не найден");

                _context.Users.Remove(user);
                _context.Employees.Remove(user.IdEmployeeNavigation);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Пользователь удален" });
            }
            catch (DbUpdateException)
            {
                // Обработка ограничения внешнего ключа (сотрудник уже привязан к заказам)
                return BadRequest("Невозможно удалить: сотрудник имеет связанные заказы или смены.");
            }
            catch (Exception ex) { return BadRequest(ex.InnerException?.Message ?? ex.Message); }
        }
    }

    public class UserRequest
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public int PositionId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}