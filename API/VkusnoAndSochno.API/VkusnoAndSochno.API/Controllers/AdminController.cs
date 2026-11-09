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

        // Получить список всех пользователей
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await (from u in _context.Users
                               join e in _context.Employees on u.IdEmployee equals e.IdEmployee
                               join r in _context.Roles on u.IdRole equals r.IdRole
                               join b in _context.Branches on e.IdBranch equals b.IdBranch
                               select new
                               {
                                   u.IdUser,
                                   u.Login,
                                   EmployeeName = e.FullName,
                                   RoleName = r.RoleName,
                                   BranchName = b.BranchName,
                                   u.IsActive
                               }).ToListAsync();

            return Ok(users);
        }

        // Заблокировать / Разблокировать пользователя
        [HttpPut("users/{id}/toggle")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Пользователь не найден");

            // Меняем статус на противоположный
            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Статус изменен", IsActive = user.IsActive });
        }
    }
}