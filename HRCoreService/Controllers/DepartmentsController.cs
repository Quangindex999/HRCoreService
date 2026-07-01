using HRCoreDB.DTOs;
using HRCoreDB.Extensions;
using HRCoreDB.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRCoreService.Controllers
{
    [Authorize]
    [Route("api/v1/hr/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public DepartmentsController(IDepartmentRepository repository, IEmployeeRepository employeeRepository)
        {
            _departmentRepository = repository;
            _employeeRepository = employeeRepository;
        }

        // GET: api/v1/hr/department/tree (API lấy toàn bộ phòng ban)
        [HttpGet("tree")]
        public async Task<IActionResult> GetTree()
        {
            var departments = await _departmentRepository.GetTreeAsync();
            var dtos = departments.Select(d => d.ToDto());
            return Ok(new
            {
                statusCode = 200,
                message = "Lấy sơ đồ phòng ban thành công",
                data = dtos
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
        {
            //kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await _departmentRepository.IsCodeUniqueAsync(dto.Code))
            {
                return BadRequest(new
                {
                    message = "Mã phòng ban đã tồn tại. Vui lòng chọn mã khác."
                });
            }

            var entity = dto.ToEntity();
            var result = await _departmentRepository.CreateAsync(entity);

            return CreatedAtAction(nameof(GetTree), new { id = result.Id }, result.ToDto());

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto dto)
        {
            //kiểm tra xem phòng ban có tồn tại không
            var existingDepartment = await _departmentRepository.GetByIdAsync(id);
            if (existingDepartment == null)
            {
                return NotFound(new { StatusCode = 404, message = $"Không tìm thấy phòng ban mã {id}" });
            }

            //chống lỗi tự động nhận diện mã phòng ban khi cập nhật
            if (dto.ParentId == id)
            {
                return BadRequest(new { statusCode = 400, message = "Phòng ban không thể tự thay đổi phòng ban cha" });
            }

            //cập nhật dữ liệu
            dto.ApplyTo(existingDepartment);

            await _departmentRepository.UpdateAsync(existingDepartment);
            return Ok(new { statusCode = 200, message = "Cập nhật phòng ban thành công", data = existingDepartment });
        }

        [Authorize(Roles = "Admin, HR")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            // 1. Kiểm tra xem phòng ban có tồn tại không
            var existingDepartment = await _departmentRepository.GetByIdAsync(id);
            if (existingDepartment == null)
            {

                return NotFound(new { statusCode = 404, message = $"Không tìm thấy phòng ban mã {id}" });
            }

            // 2. Kiểm tra xem phòng ban đang có nhân viên nào không
            var allEmployees = await _employeeRepository.GetAllAsync();
            var isDepartmentOccupied = allEmployees.Any(e => e.DepartmentId == id);

            if (isDepartmentOccupied)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Không thể xóa! Đang có nhân viên trực thuộc phòng ban này. Vui lòng chuyển nhân viên sang phòng khác trước."
                });
            }

            // 3. Tiến hành xóa an toàn
            await _departmentRepository.DeleteAsync(id);
            return Ok(new { statusCode = 200, message = "Xóa phòng ban thành công" });
        }
    }
}
