using System;
using System.Collections.Generic;

namespace HRCoreDB.Entities
{
    public class Department
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = null!;

        public string Code { get; set; } = null!;

        public Guid? ParentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Department? Parent { get; set; } //phòng ban cha
        public ICollection<Department> Children { get; set; } = new List<Department>(); //phòng ban con
        public ICollection<Employee> Employees { get; set; } = new List<Employee>(); //nhân viên thuộc phòng ban này
    }
}