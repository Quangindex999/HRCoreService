using HRCoreDB.DTOs;
using HRCoreDB.Extensions;
using HRCoreDB.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRCoreService.Controllers
{
    [Authorize] // Bắt buộc phải có Token mới được vào Controller này
    [Route("api/v1/hr/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;

        public EmployeesController(IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
        }

        // --- 1. API LẤY TOÀN BỘ NHÂN VIÊN ---
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null)
        {
            try
            {
                // Bóc tách thông tin từ Token
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
                var currentDeptId = User.FindFirstValue("DepartmentId");

                var allEmployees = await _employeeRepository.GetAllWithDepartmentAsync();

                // LỌC QUYỀN TRƯỞNG PHÒNG VÀ NHÂN VIÊN
                if (currentUserRole == "Manager")
                {
                    allEmployees = allEmployees.Where(e => e.DepartmentId.ToString() == currentDeptId).ToList();
                }
                else if (currentUserRole == "Employee")
                {
                    return StatusCode(403, new { message = "Forbidden: Bạn không có quyền xem danh sách nhân sự công ty." });
                }

                // LỌC SEARCH (tên, mã, email)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var kw = search.Trim().ToLower();
                    allEmployees = allEmployees.Where(e =>
                        e.FullName.ToLower().Contains(kw) ||
                        e.EmployeeCode.ToLower().Contains(kw) ||
                        (e.Email != null && e.Email.ToLower().Contains(kw))
                    ).ToList();
                }

                // LỌC STATUS
                if (!string.IsNullOrWhiteSpace(status))
                {
                    allEmployees = allEmployees.Where(e =>
                        string.Equals(e.Status, status, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                var total = allEmployees.Count();
                var dtos = allEmployees
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => e.ToDto());

                return Ok(new { statusCode = 200, message = "Lấy danh sách thành công", data = dtos, total });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { statusCode = 500, message = e.Message });
            }
        }

        // --- 2. API XEM HỒ SƠ CỦA 1 NHÂN VIÊN (HÀM MỚI BỔ SUNG) ---
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            // CHỐT CHẶN: Nhân viên chỉ được xem mình, xem người khác đá văng ngay
            if (currentUserRole == "Employee" && currentUserId != id.ToString())
            {
                return StatusCode(403, new { statusCode = 403, message = "Cảnh báo: Bạn chỉ được xem hồ sơ của chính mình!" });
            }

            var employee = await _employeeRepository.GetWithDepartmentAsync(id);
            if (employee == null)
            {
                return NotFound(new { statusCode = 404, message = "Không tìm thấy nhân viên" });
            }

            return Ok(new { statusCode = 200, message = "Lấy thông tin thành công", data = employee.ToDto() });
        }

        // --- 3. API TẠO MỚI (CHỈ ADMIN VÀ HR MỚI ĐƯỢC LÀM) ---
        [Authorize(Roles = "Admin, HR")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            var deptExists = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
            if (deptExists == null)
            {
                return NotFound(new { message = "Không tìm thấy phòng ban với ID đã được cung cấp" });
            }

            if (!await _employeeRepository.IsEmailUniqueAsync(dto.Email))
            {
                return BadRequest(new { message = "Email này đã được sử dụng" });
            }

            if (!await _employeeRepository.IsEmployeeCodeUniqueAsync(dto.EmployeeCode))
            {
                return BadRequest(new { message = "Mã nhân viên này đã tồn tại" });
            }

            var entity = dto.ToEntity();
            var result = await _employeeRepository.CreateAsync(entity);

            return CreatedAtAction(nameof(GetEmployeeById), new { id = result.Id }, result.ToDto());
        }

        // --- 4. API CẬP NHẬT (CHỈ ADMIN VÀ HR MỚI ĐƯỢC LÀM) ---
        [Authorize(Roles = "Admin, HR")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto dto)
        {
            var existingEmployee = await _employeeRepository.GetByIdAsync(id);
            if (existingEmployee == null)
            {
                return NotFound(new { statusCode = 404, message = $"Không tìm thấy nhân viên mã {id}" });
            }

            dto.ApplyTo(existingEmployee);

            await _employeeRepository.UpdateAsync(existingEmployee);
            return Ok(new { statusCode = 200, message = "Cap nhat nhan vien thanh cong", data = existingEmployee });
        }

        // --- 5. API XÓA (CHỈ ADMIN VÀ HR MỚI ĐƯỢC LÀM) ---
        [Authorize(Roles = "Admin, HR")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var existingEmployee = await _employeeRepository.GetByIdAsync(id);
            if (existingEmployee == null)
            {
                return NotFound(new { statusCode = 404, message = $"Không tìm thấy nhân viên mã {id}" });
            }
            await _employeeRepository.DeleteAsync(id);

            return Ok(new { statusCode = 200, message = "Xóa nhân viên thành công" });
        }

        // --- 6. API THÊM NHIỀU NHÂN VIÊN (BULK INSERT) ---
        [Authorize(Roles = "Admin, HR")]
        [HttpPost("bulk-create")]
        public async Task<IActionResult> BulkCreate([FromBody] List<CreateEmployeeDto> dtos)
        {
            if (dtos == null || !dtos.Any())
            {
                return BadRequest(new { message = "Danh sách gửi lên trống" });
            }

            var addedEmployees = new List<object>();
            var errorList = new List<string>();

            foreach (var dto in dtos)
            {
                // Kiểm tra phòng ban
                var deptExists = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
                if (deptExists == null)
                {
                    errorList.Add($"Email {dto.Email} thất bại: Không tìm thấy ID phòng ban.");
                    continue; // Bỏ qua, chạy tiếp người tiếp theo
                }

                // Kiểm tra trùng Email
                if (!await _employeeRepository.IsEmailUniqueAsync(dto.Email))
                {
                    errorList.Add($"Email {dto.Email} thất bại: Đã được sử dụng.");
                    continue;
                }

                // Kiểm tra trùng Mã nhân viên
                if (!await _employeeRepository.IsEmployeeCodeUniqueAsync(dto.EmployeeCode))
                {
                    errorList.Add($"Mã {dto.EmployeeCode} thất bại: Đã tồn tại.");
                    continue;
                }

                // Lưu vào Database
                var entity = dto.ToEntity();
                var result = await _employeeRepository.CreateAsync(entity);
                addedEmployees.Add(result.ToDto());
            }

            return Ok(new
            {
                statusCode = 200,
                message = $"Hoàn tất: Đã thêm {addedEmployees.Count} nhân viên, thất bại {errorList.Count} nhân viên.",
                data = addedEmployees,
                errors = errorList
            });
        }

        // --- 7. API SỬA NHIỀU NHÂN VIÊN (BULK UPDATE) ---
        [Authorize(Roles = "Admin, HR")]
        [HttpPut("bulk-update")]
        public async Task<IActionResult> BulkUpdate([FromBody] Dictionary<Guid, UpdateEmployeeDto> updates)
        {
            if (updates == null || !updates.Any())
            {
                return BadRequest(new { message = "Danh sách cập nhật trống" });
            }

            var updatedCount = 0;
            var errorList = new List<string>();

            foreach (var item in updates)
            {
                var id = item.Key;         // Lấy ID nhân viên
                var dto = item.Value;      // Lấy dữ liệu cần sửa

                var existingEmployee = await _employeeRepository.GetByIdAsync(id);
                if (existingEmployee == null)
                {
                    errorList.Add($"Không tìm thấy nhân viên mã {id}");
                    continue;
                }

                dto.ApplyTo(existingEmployee);

                await _employeeRepository.UpdateAsync(existingEmployee);
                updatedCount++;
            }

            return Ok(new
            {
                statusCode = 200,
                message = $"Hoàn tất: Cập nhật thành công {updatedCount} nhân viên.",
                errors = errorList
            });
        }

        // --- 8. API XÓA NHIỀU NHÂN VIÊN (BULK DELETE) ---
        [Authorize(Roles = "Admin, HR")]
        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> BulkDelete([FromBody] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest(new { message = "Danh sách ID trống" });
            }

            var deletedCount = 0;
            var errorList = new List<string>();

            foreach (var id in ids)
            {
                var existingEmployee = await _employeeRepository.GetByIdAsync(id);
                if (existingEmployee == null)
                {
                    errorList.Add($"Mã {id} không tồn tại hoặc đã bị xóa trước đó");
                    continue;
                }

                await _employeeRepository.DeleteAsync(id);
                deletedCount++;
            }

            return Ok(new
            {
                statusCode = 200,
                message = $"Đã xóa thành công {deletedCount} nhân viên.",
                errors = errorList
            });
        }

    }
}