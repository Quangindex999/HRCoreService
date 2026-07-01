using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HRCoreDB.Entities;

namespace HRCoreDB.Repositories
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        /// <summary>Lấy danh sách nhân viên theo phòng ban.</summary>
        Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);

        /// <summary>Lấy nhân viên kèm thông tin phòng ban.</summary>
        Task<Employee?> GetWithDepartmentAsync(Guid id, CancellationToken ct = default);

        /// <summary>Lấy tất cả nhân viên kèm thông tin phòng ban.</summary>
        Task<IEnumerable<Employee>> GetAllWithDepartmentAsync(CancellationToken ct = default);

        /// <summary>Tìm nhân viên theo mã nhân viên.</summary>
        Task<Employee?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken ct = default);

        /// <summary>Kiểm tra Email có bị trùng không (loại trừ chính nó khi update).</summary>
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken ct = default);

        /// <summary>Kiểm tra mã nhân viên có bị trùng không.</summary>
        Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, Guid? excludeId = null, CancellationToken ct = default);

        /// <summary>Tìm nhân viên theo Username (dùng cho login).</summary>
        Task<Employee?> GetByUsernameAsync(string username, CancellationToken ct = default);
    }
}