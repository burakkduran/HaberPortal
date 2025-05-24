using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Uyg.API.DTOs;
using Uyg.API.Models;

namespace Uyg.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<AppUser> userManager, 
            IConfiguration configuration,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("GetUserInfo")]
        [Authorize]
        public async Task<ActionResult<UserInfoDTO>> GetUserInfo()
        {
            try
            {
                _logger.LogInformation("GetUserInfo endpoint'i çağrıldı");

                // Token'dan kullanıcı ID'sini al
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"Token'dan alınan UserId: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Token'dan UserId alınamadı");
                    return Unauthorized();
                }

                // Kullanıcıyı bul
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"Kullanıcı bulunamadı. UserId: {userId}");
                    return NotFound("Kullanıcı bulunamadı");
                }

                _logger.LogInformation($"Kullanıcı bulundu: {user.UserName}");

                // Kullanıcının rollerini al
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User"; // Eğer rol yoksa varsayılan olarak "User"

                _logger.LogInformation($"Kullanıcı rolü: {role}");

                // UserInfoDTO oluştur ve döndür
                var userInfo = new UserInfoDTO
                {
                    UserName = user.UserName,
                    Role = role
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserInfo endpoint'inde hata oluştu");
                return StatusCode(500, $"Bir hata oluştu: {ex.Message}");
            }
        }
    }
} 