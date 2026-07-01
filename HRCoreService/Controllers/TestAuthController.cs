using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace HRCoreService.Controllers
{
    [Route("api/v1/hr/[controller]")]
    [ApiController]
    public class TestAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TestAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //Đánh dấu phương thức này có thể truy cập mà không cần xác thực
        [AllowAnonymous]
        [HttpPost("generate-token")]
        public IActionResult GenerateToken(string role, Guid userId, Guid departmentId)
        {
            // 1. Tạo danh sách thông tin sẽ nhét vào Token
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Role, role), // <-- CHÌA KHÓA PHÂN QUYỀN NẰM Ở ĐÂY ("Admin", "Manager", "Employee")
        new Claim("DepartmentId", departmentId.ToString())
    };

            // 2. Đọc cấu hình trực tiếp từ appsettings.json
            var secretKey = _configuration["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing");
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];

            // 3. Ký và tạo Token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,          // Tự động lấy "UserAuthService"
                audience: audience,      // Tự động lấy "AllMicroservices"
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { Token = tokenString, Role = role });
        }
    }
}
