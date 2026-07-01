using HRCoreDB.Data;
using HRCoreDB.DTOs;
using HRCoreDB.Extensions;
using HRCoreDB.Entities;
using HRCoreService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HRCoreService.Controllers
{
    [Authorize]
    [Route("api/v1/hr/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        private readonly HRCoreDbContext _context;
        private readonly IEmailService _emailService;

        public ContractsController(HRCoreDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: api/v1/hr/contracts/types
        [HttpGet("types")]
        public async Task<IActionResult> GetContractTypes()
        {
            try
            {
                var types = await _context.ContractTypes.ToListAsync();
                var dtos = types.Select(ct => ct.ToDto());
                return Ok(new { statusCode = 200, message = "Lấy danh sách loại hợp đồng thành công", data = dtos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // GET: api/v1/hr/contracts
        // GET: api/v1/hr/contracts?employeeId={guid}
        [HttpGet]
        public async Task<IActionResult> GetContracts([FromQuery] Guid? employeeId)
        {
            try
            {
                var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Không xác định được danh tính người dùng" });
                }

                // Nếu không phải Admin hoặc HR, ép buộc employeeId về chính mình
                if (!User.IsInRole("Admin") && !User.IsInRole("HR"))
                {
                    employeeId = currentUserId;
                }

                var query = _context.Contracts
                    .Include(c => c.Employee)
                    .Include(c => c.ContractType)
                    .AsQueryable();

                if (employeeId.HasValue)
                {
                    query = query.Where(c => c.EmployeeId == employeeId.Value);
                }

                var contracts = await query.ToListAsync();
                var dtos = contracts.Select(c => c.ToDto());
                return Ok(new { statusCode = 200, message = "Lấy danh sách hợp đồng thành công", data = dtos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // GET: api/v1/hr/contracts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContractById(Guid id)
        {
            try
            {
                var contract = await _context.Contracts
                    .Include(c => c.Employee)
                    .Include(c => c.ContractType)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (contract == null)
                {
                    return NotFound(new { statusCode = 404, message = "Không tìm thấy hợp đồng" });
                }

                var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Không xác định được danh tính người dùng" });
                }

                // Nếu không phải Admin hoặc HR, và hợp đồng này không phải của chính mình -> Forbid
                if (!User.IsInRole("Admin") && !User.IsInRole("HR") && contract.EmployeeId != currentUserId)
                {
                    return Forbid();
                }

                return Ok(new { statusCode = 200, message = "Lấy thông tin hợp đồng thành công", data = contract.ToDto() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // POST: api/v1/hr/contracts
        [Authorize(Roles = "Admin, HR")]
        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check contract type exists
                var typeExists = await _context.ContractTypes.AnyAsync(ct => ct.Id == dto.ContractTypeId);
                if (!typeExists)
                {
                    return BadRequest(new { message = "Loại hợp đồng không tồn tại" });
                }

                // Check employee exists
                var employeeExists = await _context.Employees.AnyAsync(e => e.Id == dto.EmployeeId);
                if (!employeeExists)
                {
                    return BadRequest(new { message = "Nhân viên không tồn tại" });
                }

                // Check contract number uniqueness
                var numberExists = await _context.Contracts.AnyAsync(c => c.ContractNumber == dto.ContractNumber);
                if (numberExists)
                {
                    return BadRequest(new { message = "Số hợp đồng đã tồn tại" });
                }

                var entity = dto.ToEntity();
                _context.Contracts.Add(entity);
                await _context.SaveChangesAsync();

                // Fetch with includes for returning
                var createdContract = await _context.Contracts
                    .Include(c => c.Employee)
                    .Include(c => c.ContractType)
                    .FirstAsync(c => c.Id == entity.Id);

                if (createdContract.Employee != null)
                {
                    await _emailService.SendContractNotificationAsync(createdContract.Employee, createdContract);
                }

                return CreatedAtAction(nameof(GetContractById), new { id = createdContract.Id }, createdContract.ToDto());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // PUT: api/v1/hr/contracts/{id}
        [Authorize(Roles = "Admin, HR")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(Guid id, [FromBody] UpdateContractDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingContract = await _context.Contracts.FindAsync(id);
                if (existingContract == null)
                {
                    return NotFound(new { statusCode = 404, message = "Không tìm thấy hợp đồng để cập nhật" });
                }

                if (dto.ContractTypeId.HasValue)
                {
                    var typeExists = await _context.ContractTypes.AnyAsync(ct => ct.Id == dto.ContractTypeId.Value);
                    if (!typeExists)
                    {
                        return BadRequest(new { message = "Loại hợp đồng mới không tồn tại" });
                    }
                }

                if (dto.ContractNumber != null && dto.ContractNumber != existingContract.ContractNumber)
                {
                    var numberExists = await _context.Contracts.AnyAsync(c => c.ContractNumber == dto.ContractNumber && c.Id != id);
                    if (numberExists)
                    {
                        return BadRequest(new { message = "Số hợp đồng mới đã tồn tại ở hợp đồng khác" });
                    }
                }

                dto.ApplyTo(existingContract);
                await _context.SaveChangesAsync();

                var updated = await _context.Contracts
                    .Include(c => c.Employee)
                    .Include(c => c.ContractType)
                    .FirstAsync(c => c.Id == id);

                if (updated.Employee != null)
                {
                    await _emailService.SendContractNotificationAsync(updated.Employee, updated);
                }

                return Ok(new { statusCode = 200, message = "Cập nhật hợp đồng thành công", data = updated.ToDto() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // PUT: api/v1/hr/contracts/{id}/sign
        [HttpPut("{id}/sign")]
        public async Task<IActionResult> SignContract(Guid id)
        {
            try
            {
                var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
                {
                    return Unauthorized(new { statusCode = 401, message = "Không xác định được danh tính nhân viên" });
                }

                var contract = await _context.Contracts
                    .Include(c => c.Employee)
                    .Include(c => c.ContractType)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (contract == null)
                {
                    return NotFound(new { statusCode = 404, message = "Không tìm thấy hợp đồng" });
                }

                if (contract.EmployeeId != currentUserId)
                {
                    return Forbid(); // Trả về 403 Forbidden nếu không phải hợp đồng của họ
                }

                if (contract.Status != "Draft")
                {
                    return BadRequest(new { statusCode = 400, message = "Hợp đồng không ở trạng thái Bản nháp (Draft)" });
                }

                contract.Status = "Active";
                contract.SignDate = DateTime.UtcNow; // cập nhật ngày ký thực tế
                contract.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Gửi thông báo
                if (contract.Employee != null)
                {
                    await _emailService.SendContractNotificationAsync(contract.Employee, contract);
                }

                return Ok(new { statusCode = 200, message = "Ký hợp đồng thành công", data = contract.ToDto() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }

        // DELETE: api/v1/hr/contracts/{id}
        [Authorize(Roles = "Admin, HR")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(Guid id)
        {
            try
            {
                var existingContract = await _context.Contracts.FindAsync(id);
                if (existingContract == null)
                {
                    return NotFound(new { statusCode = 404, message = "Không tìm thấy hợp đồng" });
                }

                _context.Contracts.Remove(existingContract);
                await _context.SaveChangesAsync();

                return Ok(new { statusCode = 200, message = "Xóa hợp đồng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = ex.Message });
            }
        }
    }
}
