using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HRCoreDB.Entities;

namespace HRCoreDB.Repositories
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        /// <summary>Lấy toàn bộ cây phòng ban (bao gồm con, cháu).</summary>
        Task<IEnumerable<Department>> GetTreeAsync(CancellationToken ct = default);

        /// <summary>Lấy danh sách phòng ban con trực tiếp của một phòng ban cha.</summary>
        Task<IEnumerable<Department>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);

        /// <summary>Kiểm tra Code có bị trùng không (loại trừ chính nó khi update).</summary>
        Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null, CancellationToken ct = default);

    }
}