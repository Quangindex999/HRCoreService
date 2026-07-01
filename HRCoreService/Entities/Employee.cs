using System;

namespace HRCoreDB.Entities
{
    public class Employee
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DepartmentId { get; set; }

        public string EmployeeCode { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Position { get; set; } = null!;

        public string ContractType { get; set; } = null!; // Full-time, Part-time, Probation

        public decimal BaseSalary { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public DateTime? HireDate { get; set; }

        public string? TaxCode { get; set; } // Mã số thuế cá nhân

        public int DependentsCount { get; set; } = 0; // Số người phụ thuộc

        public string? IdentityNumber { get; set; } // Số CMND/CCCD

        public string? BankName { get; set; }

        public string? BankAccountNumber { get; set; }

        public string? BankBranch { get; set; }

        public string Status { get; set; } = "Active"; // Active, Inactive

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Login fields ──────────────────────────────────────────────────────
        // Username dùng để đăng nhập, có thể null nếu nhân viên chưa có tài khoản
        public string? Username { get; set; }

        // Password được hash trước khi lưu (dùng BCrypt hoặc ASP.NET Core PasswordHasher)
        public string? PasswordHash { get; set; }

        // Role của nhân viên: Admin | HR | Manager | Employee
        public string Role { get; set; } = "Employee";

        // Navigation properties
        public Department Department { get; set; } = null!;
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}