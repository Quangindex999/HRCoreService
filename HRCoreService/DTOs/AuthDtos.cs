using System;
using System.ComponentModel.DataAnnotations;

namespace HRCoreDB.DTOs
{
    public class LoginDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class CreateAccountDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Employee";
    }

    public class ResetPasswordDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = null!;
    }

    public class ChangeRoleDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = null!;
    }

    public class AccountDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public string? Username { get; set; }
        public string Role { get; set; } = null!;
    }
}
