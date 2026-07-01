using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRCoreDB.DTOs
{
    // ── READ ──────────────────────────────────────────────────────────────────

    public class DepartmentDto
    {
        public Guid Id { get; set; }
        // ✅
        public string? Name { get; set; }
        public string Code { get; set; } = null!;
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Cây con (dùng khi lấy toàn bộ cây phòng ban)
        public List<DepartmentDto> Children { get; set; } = new();
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    public class CreateDepartmentDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = null!;

        public Guid? ParentId { get; set; }
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    public class UpdateDepartmentDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(20)]
        public string? Code { get; set; }

        public Guid? ParentId { get; set; }
    }
}