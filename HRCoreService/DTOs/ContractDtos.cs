using System;
using System.ComponentModel.DataAnnotations;

namespace HRCoreDB.DTOs
{
    // ── CONTRACT TYPE DTOs ────────────────────────────────────────────────────

    public class ContractTypeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public decimal DefaultSalaryRatio { get; set; }
        public bool IsSocialInsuranceSubject { get; set; }
        public string TaxType { get; set; } = null!; // Progressive, Flat10, Exempt
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateContractTypeDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required]
        [Range(0, 2.0)]
        public decimal DefaultSalaryRatio { get; set; } = 1.0m;

        public bool IsSocialInsuranceSubject { get; set; } = true;

        [Required]
        [MaxLength(20)]
        public string TaxType { get; set; } = "Progressive";

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateContractTypeDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Code { get; set; }

        [Range(0, 2.0)]
        public decimal? DefaultSalaryRatio { get; set; }

        public bool? IsSocialInsuranceSubject { get; set; }

        [MaxLength(20)]
        public string? TaxType { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    // ── CONTRACT DTOs ─────────────────────────────────────────────────────────

    public class ContractDto
    {
        public Guid Id { get; set; }
        public string ContractNumber { get; set; } = null!;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public Guid ContractTypeId { get; set; }
        public string ContractTypeName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal SalaryRatio { get; set; }
        public DateTime SignDate { get; set; }
        public string Status { get; set; } = null!; // Draft, Active, Expired, Terminated
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateContractDto
    {
        [Required]
        [MaxLength(50)]
        public string ContractNumber { get; set; } = null!;

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public Guid ContractTypeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal BasicSalary { get; set; }

        [Required]
        [Range(0, 2.0)]
        public decimal SalaryRatio { get; set; } = 1.0m;

        [Required]
        public DateTime SignDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateContractDto
    {
        [MaxLength(50)]
        public string? ContractNumber { get; set; }

        public Guid? ContractTypeId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? BasicSalary { get; set; }

        [Range(0, 2.0)]
        public decimal? SalaryRatio { get; set; }

        public DateTime? SignDate { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
