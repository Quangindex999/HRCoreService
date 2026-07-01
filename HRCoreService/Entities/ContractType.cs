using System;
using System.Collections.Generic;

namespace HRCoreDB.Entities
{
    public class ContractType
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = null!; // Ví dụ: Thử việc, Chính thức, Cộng tác viên

        public string Code { get; set; } = null!; // Ví dụ: PROBATION, FULLTIME, PARTTIME

        public decimal DefaultSalaryRatio { get; set; } = 1.0m; // Tỷ lệ lương mặc định (ví dụ: 0.85 cho thử việc)

        public bool IsSocialInsuranceSubject { get; set; } = true; // Có thuộc đối tượng đóng BHXH bắt buộc không

        public string TaxType { get; set; } = "Progressive"; // Progressive (Lũy tiến), Flat10 (Khấu trừ 10%), Exempt (Miễn thuế)

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
