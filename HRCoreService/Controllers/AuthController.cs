using HRCoreDB.DTOs;
using HRCoreDB.Entities;
using HRCoreDB.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HRCoreService.Controllers
{
    [Route("api/v1/hr/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const int Pbkdf2Iterations = 100_000;
        private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Admin",
            "HR",
            "Manager",
            "Employee"
        };

        private readonly IEmployeeRepository _employeeRepository;
        private readonly IConfiguration _configuration;

        public AuthController(IEmployeeRepository employeeRepository, IConfiguration configuration)
        {
            _employeeRepository = employeeRepository;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = dto.Username.Trim();
            var employee = await _employeeRepository.GetByUsernameAsync(username);

            if (employee is null ||
                string.IsNullOrWhiteSpace(employee.PasswordHash) ||
                !VerifyPassword(dto.Password, employee.PasswordHash))
            {
                return Unauthorized(new { statusCode = 401, message = "Tên đăng nhập hoặc mật khẩu không đúng" });
            }

            if (!string.Equals(employee.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new { statusCode = 403, message = "Tài khoản nhân viên đã bị vô hiệu hóa" });
            }

            var token = GenerateJwtToken(employee);

            return Ok(new
            {
                statusCode = 200,
                message = "Đăng nhập thành công",
                token,
                user = ToAccountDto(employee)
            });
        }

        [Authorize(Roles = "Admin, HR")]
        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!ValidRoles.Contains(dto.Role))
            {
                return BadRequest(new { statusCode = 400, message = "Role không hợp lệ. Chỉ chấp nhận Admin, HR, Manager, Employee" });
            }

            var employee = await _employeeRepository.GetWithDepartmentAsync(dto.EmployeeId);
            if (employee is null)
            {
                return NotFound(new { statusCode = 404, message = "Không tìm thấy nhân viên" });
            }

            if (!string.IsNullOrWhiteSpace(employee.Username))
            {
                return BadRequest(new { statusCode = 400, message = "Nhân viên này đã có tài khoản" });
            }

            var username = dto.Username.Trim();
            var existingUsername = await _employeeRepository.GetByUsernameAsync(username);
            if (existingUsername is not null)
            {
                return BadRequest(new { statusCode = 400, message = "Username đã được sử dụng" });
            }

            employee.Username = username;
            employee.PasswordHash = HashPassword(dto.Password);
            employee.Role = NormalizeRole(dto.Role);

            await _employeeRepository.UpdateAsync(employee);

            return Ok(new
            {
                statusCode = 200,
                message = "Cấp tài khoản thành công",
                data = ToAccountDto(employee)
            });
        }

        [Authorize(Roles = "Admin, HR")]
        [HttpGet("employees-without-account")]
        public async Task<IActionResult> GetEmployeesWithoutAccount()
        {
            var employees = await _employeeRepository.GetAllWithDepartmentAsync();
            var data = employees
                .Where(e => string.IsNullOrWhiteSpace(e.Username))
                .Select(ToAccountDto);

            return Ok(new
            {
                statusCode = 200,
                message = "Lấy danh sách nhân viên chưa có tài khoản thành công",
                data
            });
        }

        [Authorize(Roles = "Admin, HR")]
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            var employees = await _employeeRepository.GetAllWithDepartmentAsync();
            var data = employees
                .Where(e => !string.IsNullOrWhiteSpace(e.Username))
                .Select(ToAccountDto);

            return Ok(new { statusCode = 200, message = "Lấy danh sách tài khoản thành công", data });
        }

        [Authorize(Roles = "Admin, HR")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (employee is null)
                return NotFound(new { statusCode = 404, message = "Không tìm thấy nhân viên" });

            if (string.IsNullOrWhiteSpace(employee.Username))
                return BadRequest(new { statusCode = 400, message = "Nhân viên này chưa có tài khoản" });

            employee.PasswordHash = HashPassword(dto.NewPassword);
            await _employeeRepository.UpdateAsync(employee);

            return Ok(new { statusCode = 200, message = "Đặt lại mật khẩu thành công" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("change-role")]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!ValidRoles.Contains(dto.Role))
                return BadRequest(new { statusCode = 400, message = "Role không hợp lệ. Chỉ chấp nhận Admin, HR, Manager, Employee" });

            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
            if (employee is null)
                return NotFound(new { statusCode = 404, message = "Không tìm thấy nhân viên" });

            if (string.IsNullOrWhiteSpace(employee.Username))
                return BadRequest(new { statusCode = 400, message = "Nhân viên này chưa có tài khoản" });

            employee.Role = NormalizeRole(dto.Role);
            await _employeeRepository.UpdateAsync(employee);

            return Ok(new { statusCode = 200, message = "Đổi role thành công", data = ToAccountDto(employee) });
        }

        private string GenerateJwtToken(Employee employee)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new(ClaimTypes.Role, employee.Role),
                new("DepartmentId", employee.DepartmentId.ToString()),
                new("EmployeeCode", employee.EmployeeCode),
                new("FullName", employee.FullName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static AccountDto ToAccountDto(Employee employee) => new()
        {
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FullName = employee.FullName,
            Email = employee.Email,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name ?? string.Empty,
            Username = employee.Username,
            Role = employee.Role
        };

        private static string NormalizeRole(string role)
        {
            return ValidRoles.First(validRole => string.Equals(validRole, role, StringComparison.OrdinalIgnoreCase));
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Pbkdf2Iterations,
                numBytesRequested: 32);

            return $"PBKDF2${Pbkdf2Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedPasswordHash)
        {
            var parts = storedPasswordHash.Split('$');
            if (parts.Length != 4 || parts[0] != "PBKDF2")
            {
                return storedPasswordHash == password;
            }

            if (!int.TryParse(parts[1], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: iterations,
                numBytesRequested: expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
