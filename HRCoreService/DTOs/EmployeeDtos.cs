using System;
using System.ComponentModel.DataAnnotations;

namespace HRCoreDB.DTOs
{
    // ── READ ──────────────────────────────────────────────────────────────────

    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string ContractType { get; set; } = null!;
        public decimal BaseSalary { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime? HireDate { get; set; }
        public string? TaxCode { get; set; } // Mã số thuế cá nhân
        public int DependentsCount { get; set; } // Số lượng người phụ thuộc
        public string? IdentityNumber { get; set; } // Số CMND/CCCD
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankBranch { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    public class CreateEmployeeDto
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        [MaxLength(20)] //validate luôn mã nhân viên
        public string EmployeeCode { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress] //validate luôn email
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Position { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string ContractType { get; set; } = null!; // Full-time | Part-time | Probation

        [Required]
        [Range(0, double.MaxValue)] //validate luôn lương cơ bản phải >= 0
        public decimal BaseSalary { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public DateTime? HireDate { get; set; }

        [MaxLength(20)]
        public string? TaxCode { get; set; }

        [Range(0, 100)]
        public int DependentsCount { get; set; } = 0;

        [MaxLength(20)]
        public string? IdentityNumber { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(30)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(100)]
        public string? BankBranch { get; set; }
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    public class UpdateEmployeeDto
    {
        public Guid? DepartmentId { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Position { get; set; }

        [MaxLength(50)]
        public string? ContractType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? BaseSalary { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public DateTime? HireDate { get; set; }

        [MaxLength(20)]
        public string? TaxCode { get; set; }

        [Range(0, 100)]
        public int? DependentsCount { get; set; }

        [MaxLength(20)]
        public string? IdentityNumber { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(30)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(100)]
        public string? BankBranch { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; } // Active | Inactive
    }
}