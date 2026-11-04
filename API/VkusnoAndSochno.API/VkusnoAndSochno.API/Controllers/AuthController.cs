using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VkusnoAndSochno.API.Models;

namespace VkusnoAndSochno.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly VkusnoDbContext _context;

        public AuthController(VkusnoDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Ищем пользователя по логину и паролю (сравниваем напрямую)
            var user = await _context.Users
                .Include(u => u.IdEmployeeNavigation)
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.Login == request.Login && u.PasswordHash == request.Password);

            if (user == null || user.IsActive == false)
                return Unauthorized(new { Message = "Неверный логин или пароль (или аккаунт заблокирован)" });

            return Ok(new
            {
                EmployeeId = user.IdEmployee,
                BranchId = user.IdEmployeeNavigation.IdBranch,
                RoleName = user.IdRoleNavigation.RoleName,
                Message = "Успешный вход"
            });
        }
    }

    public class LoginRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}