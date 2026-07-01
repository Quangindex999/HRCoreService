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
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly HRCoreDbContext _context;

        public EmployeeRepository(HRCoreDbContext context)
        {
            _context = context;
        }

        // ── READ ──────────────────────────────────────────────────────────────

        public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Employees
                .FindAsync(new object[] { id }, ct);
        }

        public async Task<Employee?> GetWithDepartmentAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Employees
                .OrderBy(e => e.EmployeeCode)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Employee>> GetAllWithDepartmentAsync(CancellationToken ct = default)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .OrderBy(e => e.EmployeeCode)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.DepartmentId == departmentId)
                .OrderBy(e => e.EmployeeCode)
                .ToListAsync(ct);
        }

        public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken ct = default)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode, ct);
        }

        /// <summary>Tìm nhân viên theo Username để xác thực đăng nhập.</summary>
        public async Task<Employee?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Username == username, ct);
        }

        public async Task<IEnumerable<Employee>> FindAsync(
            Expression<Func<Employee, bool>> predicate,
            CancellationToken ct = default)
        {
            return await _context.Employees
                .Where(predicate)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsAsync(
            Expression<Func<Employee, bool>> predicate,
            CancellationToken ct = default)
        {
            return await _context.Employees.AnyAsync(predicate, ct);
        }

        public async Task<bool> IsEmailUniqueAsync(
            string email,
            Guid? excludeId = null,
            CancellationToken ct = default)
        {
            return !await _context.Employees.AnyAsync(
                e => e.Email == email && (excludeId == null || e.Id != excludeId),
                ct);
        }

        public async Task<bool> IsEmployeeCodeUniqueAsync(
            string employeeCode,
            Guid? excludeId = null,
            CancellationToken ct = default)
        {
            return !await _context.Employees.AnyAsync(
                e => e.EmployeeCode == employeeCode && (excludeId == null || e.Id != excludeId),
                ct);
        }

        // ── CREATE ────────────────────────────────────────────────────────────

        public async Task<Employee> CreateAsync(Employee entity, CancellationToken ct = default)
        {
            await _context.Employees.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        // ── UPDATE ────────────────────────────────────────────────────────────

        public async Task<Employee> UpdateAsync(Employee entity, CancellationToken ct = default)
        {
            _context.Employees.Update(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        // ── DELETE ────────────────────────────────────────────────────────────

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var employee = await _context.Employees.FindAsync(new object[] { id }, ct);
            if (employee is null) return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}