using System;

namespace HRCoreDB.Entities
{
    public class Contract
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string ContractNumber { get; set; } = null!; // Số hợp đồng (ví dụ: HDLD/2026/0001)

        public Guid EmployeeId { get; set; }

        public Guid ContractTypeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; } // Nullable cho hợp đồng không thời hạn

        public decimal BasicSalary { get; set; } // Lương cơ bản ghi trên hợp đồng dùng làm gốc tính toán

        public decimal SalaryRatio { get; set; } = 1.0m; // Cho phép điều chỉnh tỉ lệ lương riêng cho từng hợp đồng

        public DateTime SignDate { get; set; }

        public string Status { get; set; } = "Draft"; // Active, Expired, Terminated, Draft

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Employee Employee { get; set; } = null!;
        public ContractType ContractType { get; set; } = null!;
    }
}
