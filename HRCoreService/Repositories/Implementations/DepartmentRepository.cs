using HRCoreDB.Data;
using HRCoreDB.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace HRCoreDB.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly HRCoreDbContext _context;

        public DepartmentRepository(HRCoreDbContext context)
        {
            _context = context;
        }

        // ── READ ──────────────────────────────────────────────────────────────

        public async Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Departments
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(d => d.Id == id, ct);
        }

        public async Task<IEnumerable<Department>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Departments
                .Include(d => d.Parent)
                .OrderBy(d => d.Code)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Department>> FindAsync(
            Expression<Func<Department, bool>> predicate,
            CancellationToken ct = default)
        {
            return await _context.Departments
                .Where(predicate)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Lấy toàn bộ cây phòng ban. Chỉ load 2 cấp (Parent + Children) vào bộ nhớ;
        /// nếu cây sâu hơn, hãy dùng recursive CTE ở tầng query hoặc load riêng.
        /// </summary>
        public async Task<IEnumerable<Department>> GetTreeAsync(CancellationToken ct = default)
        {
            return await _context.Departments
                .Include(d => d.Children) //JOIN để lấy luôn các node con
                .Where(d => d.ParentId == null) // Chỉ lấy node gốc
                .OrderBy(d => d.Code)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Department>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
        {
            return await _context.Departments
                .Where(d => d.ParentId == parentId)
                .OrderBy(d => d.Code)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsAsync(
            Expression<Func<Department, bool>> predicate,
            CancellationToken ct = default)
        {
            return await _context.Departments.AnyAsync(predicate, ct);
        }

        // Kiểm tra mã phòng ban có duy nhất không (dùng khi tạo mới hoặc cập nhật)
        public async Task<bool> IsCodeUniqueAsync(
            string code,
            Guid? excludeId = null,
            CancellationToken ct = default)
        {
            return !await _context.Departments.AnyAsync(
                d => d.Code == code && (excludeId == null || d.Id != excludeId),
                ct);
        }

        // ── CREATE ────────────────────────────────────────────────────────────

        public async Task<Department> CreateAsync(Department entity, CancellationToken ct = default)
        {
            await _context.Departments.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        // ── UPDATE ────────────────────────────────────────────────────────────

        public async Task<Department> UpdateAsync(Department entity, CancellationToken ct = default)
        {
            _context.Departments.Update(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        // ── DELETE ────────────────────────────────────────────────────────────

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var department = await _context.Departments.FindAsync(new object[] { id }, ct);
            if (department is null) return false;

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}