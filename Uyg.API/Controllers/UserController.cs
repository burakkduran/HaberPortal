using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Uyg.API.DTOs;
using Uyg.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace Uyg.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        ResultDto result = new ResultDto();
        public UserController(UserManager<AppUser> userManager, IMapper mapper, RoleManager<AppRole> roleManager, IConfiguration configuration, SignInManager<AppUser> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public List<UserDto> List()
        {
            var users = _userManager.Users.ToList();
            var userDtos = _mapper.Map<List<UserDto>>(users);
            return userDtos;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public UserDto GetById(string id)
        {
            var user = _userManager.Users.Where(s => s.Id == id).SingleOrDefault();
            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ResultDto> Add(RegisterDto dto)
        {
            var identityResult = await _userManager.CreateAsync(new() { UserName = dto.UserName, Email = dto.Email, FullName = dto.FullName, PhoneNumber = dto.PhoneNumber }, dto.Password);

            if (!identityResult.Succeeded)
            {
                result.Status = false;
                foreach (var item in identityResult.Errors)
                {
                    result.Message += "<p>" + item.Description + "<p>";
                }

                return result;
            }
            var user = await _userManager.FindByNameAsync(dto.UserName);
            var roleExist = await _roleManager.RoleExistsAsync(dto.Role);
            if (!roleExist)
            {
                var role = new AppRole { Name = dto.Role };
                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(user, dto.Role);
            result.Status = true;
            result.Message = "Kullanıcı Eklendi";
            return result;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultDto> Register(RegisterDto dto)
        {
            // Kullanıcı adı veya email kontrolü
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser != null)
            {
                result.Status = false;
                result.Message = "Bu kullanıcı adı zaten kullanılıyor!";
                return result;
            }

            existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                result.Status = false;
                result.Message = "Bu email adresi zaten kullanılıyor!";
                return result;
            }

            // Yeni kullanıcı oluştur
            var user = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                PhotoUrl = "profil.jpg"
            };

            var identityResult = await _userManager.CreateAsync(user, dto.Password);

            if (!identityResult.Succeeded)
            {
                result.Status = false;
                foreach (var item in identityResult.Errors)
                {
                    result.Message += "<p>" + item.Description + "<p>";
                }
                return result;
            }

            // User rolünü ekle
            var roleExist = await _roleManager.RoleExistsAsync("User");
            if (!roleExist)
            {
                var role = new AppRole { Name = "User" };
                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(user, "User");
            
            result.Status = true;
            result.Message = "Kayıt başarılı! Giriş yapabilirsiniz.";
            return result;
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ResultDto> Update(RegisterDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                result.Status = false;
                result.Message = "Kullanıcı Bulunamadı!";
                return result;
            }

            user.PhoneNumber = dto.PhoneNumber;
            user.FullName = dto.FullName;
            user.Email = dto.Email;

            // Rol güncelleme
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, dto.Role);

            await _userManager.UpdateAsync(user);
            result.Status = true;
            result.Message = "Kullanıcı Güncellendi";

            return result;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultDto> SignIn(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);

            if (user is null)
            {
                result.Status = false;
                result.Message = "Kullanıcı Bulunamadı!";
                return result;
            }
            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!isPasswordCorrect)
            {
                result.Status = false;
                result.Message = "Kullanıcı Adı veya Parola Geçersiz!";
                return result;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("JWTID", Guid.NewGuid().ToString()),
                new Claim("UserPhoto", user.PhotoUrl),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenerateJWT(authClaims);

            result.Status = true;
            result.Message = token;
            return result;
        }

        private string GenerateJWT(List<Claim> claims)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["AccessTokenExpiration"]));
            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var tokenObject = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: accessTokenExpiration,
                    claims: claims,
                    signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256)
                );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

            return token;
        }

        [HttpPost]
        [Authorize]
        public async Task<ResultDto> Upload(UploadDto dto)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
            {
                result.Status = false;
                result.Message = "Kullanıcı Bulunamadı!";
                return result;
            }

            // Sadece kendi profil fotoğrafını veya admin tüm kullanıcıların fotoğrafını değiştirebilir
            if (!User.IsInRole("Admin") && currentUserId != dto.UserId)
            {
                result.Status = false;
                result.Message = "Bu işlem için yetkiniz yok!";
                return result;
            }

            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot/Files/UserPhotos");
            string userPic = user.PhotoUrl;

            if (userPic != "profil.jpg")
            {
                var userPicUrl = Path.Combine(path, userPic);
                if (System.IO.File.Exists(userPicUrl))
                {
                    System.IO.File.Delete(userPicUrl);
                }
            }

            string data = dto.PicData;
            string base64 = data.Substring(data.IndexOf(',') + 1);
            base64 = base64.Trim('\0');
            byte[] imageBytes = Convert.FromBase64String(base64);
            string filePath = Guid.NewGuid().ToString() + dto.PicExt;

            var picPath = Path.Combine(path, filePath);
            System.IO.File.WriteAllBytes(picPath, imageBytes);

            user.PhotoUrl = filePath;
            await _userManager.UpdateAsync(user);

            result.Status = true;
            result.Message = "Profil Fotoğrafı Güncellendi";

            return result;
        }
    }
}

