using HRCoreDB.Data;
using HRCoreDB.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HRCoreDB.Data
{
    /// <summary>
    /// Seed tài khoản demo vào database khi app khởi động.
    /// Chỉ chạy nếu chưa có tài khoản nào (tránh duplicate).
    /// </summary>
    public static class DbSeeder
    {
        private const int Pbkdf2Iterations = 100_000;

        public static async Task SeedAsync(HRCoreDbContext db)
        {
            // 1. Seed ContractTypes
            var hasContractTypes = await db.ContractTypes.AnyAsync();
            if (!hasContractTypes)
            {
                var probation = new ContractType
                {
                    Name = "Thử việc",
                    Code = "PROBATION",
                    DefaultSalaryRatio = 0.85m,
                    IsSocialInsuranceSubject = false,
                    TaxType = "Flat10",
                    Description = "Hợp đồng thử việc, hưởng 85% lương, khấu trừ 10% thuế TNCN, không đóng BHXH"
                };

                var fulltime = new ContractType
                {
                    Name = "Chính thức",
                    Code = "FULLTIME",
                    DefaultSalaryRatio = 1.0m,
                    IsSocialInsuranceSubject = true,
                    TaxType = "Progressive",
                    Description = "Hợp đồng lao động chính thức, đóng BHXH bắt buộc, tính thuế lũy tiến từng phần"
                };

                var seasonal = new ContractType
                {
                    Name = "Cộng tác viên / Thời vụ",
                    Code = "SEASONAL",
                    DefaultSalaryRatio = 1.0m,
                    IsSocialInsuranceSubject = false,
                    TaxType = "Flat10",
                    Description = "Hợp đồng thời vụ hoặc cộng tác viên dưới 3 tháng, không đóng BHXH, khấu trừ 10% thuế TNCN"
                };

                db.ContractTypes.AddRange(probation, fulltime, seasonal);
                await db.SaveChangesAsync();
                Console.WriteLine("[DbSeeder] Seeded ContractTypes!");
            }

            // 2. Seed Department nếu chưa có
            var defaultDept = await db.Departments.FirstOrDefaultAsync();
            if (defaultDept == null)
            {
                defaultDept = new Department
                {
                    Name = "Phòng Nhân sự",
                    Code = "HR"
                };
                db.Departments.Add(defaultDept);
                await db.SaveChangesAsync();
                Console.WriteLine("[DbSeeder] Seeded default Department!");
            }

            // 3. Kiểm tra xem đã có tài khoản demo chưa
            bool hasAccounts = await db.Employees.AnyAsync(e => e.Username != null);
            if (hasAccounts)
            {
                Console.WriteLine("[DbSeeder] Đã có tài khoản, bỏ qua seed.");
                return;
            }

            // Lấy ContractType FULLTIME làm mặc định cho các hợp đồng được tạo kèm
            var ftContractType = await db.ContractTypes.FirstOrDefaultAsync(ct => ct.Code == "FULLTIME");

            // Danh sách tài khoản demo
            var demoAccounts = new[]
            {
                new { Username = "admin",    Password = "123456", Role = "Admin",    FullName = "Quản trị viên",   Code = "DEMO001" },
                new { Username = "hr",       Password = "123456", Role = "HR",       FullName = "Nhân viên HR",    Code = "DEMO002" },
                new { Username = "manager",  Password = "123456", Role = "Manager",  FullName = "Trưởng phòng",   Code = "DEMO003" },
                new { Username = "employee", Password = "123456", Role = "Employee", FullName = "Nhân viên demo", Code = "DEMO004" },
            };

            foreach (var acc in demoAccounts)
            {
                // Kiểm tra EmployeeCode đã tồn tại chưa (tránh conflict)
                bool codeExists = await db.Employees.AnyAsync(e => e.EmployeeCode == acc.Code);
                if (codeExists) continue;

                var hireDate = DateTime.UtcNow.AddYears(-1);
                var emp = new Employee
                {
                    DepartmentId = defaultDept.Id,
                    EmployeeCode = acc.Code,
                    FullName     = acc.FullName,
                    Email        = $"{acc.Username}@company.com",
                    Position     = acc.Role,
                    ContractType = "Full-time",
                    BaseSalary   = 10_000_000,
                    Phone        = "0987654321",
                    Address      = "123 Đường Láng, Đống Đa, Hà Nội",
                    HireDate     = hireDate,
                    TaxCode      = "8012345678",
                    DependentsCount = 0,
                    IdentityNumber = "001095001234",
                    BankName     = "Vietcombank",
                    BankAccountNumber = "10123456789",
                    BankBranch   = "Sở giao dịch Hà Nội",
                    Status       = "Active",
                    Username     = acc.Username,
                    Role         = acc.Role,
                    PasswordHash = HashPassword(acc.Password),
                };

                db.Employees.Add(emp);
                Console.WriteLine($"[DbSeeder] Thêm tài khoản: {acc.Username} / {acc.Password} ({acc.Role})");

                // Tạo hợp đồng tương ứng cho nhân viên nếu có ContractType FULLTIME
                if (ftContractType != null)
                {
                    var contract = new Contract
                    {
                        ContractNumber = $"HDLD/{DateTime.UtcNow.Year}/{acc.Code}",
                        EmployeeId = emp.Id,
                        ContractTypeId = ftContractType.Id,
                        StartDate = hireDate,
                        EndDate = null,
                        BasicSalary = emp.BaseSalary,
                        SalaryRatio = 1.0m,
                        SignDate = hireDate,
                        Status = "Active",
                        Notes = "Hợp đồng lao động chính thức được sinh tự động."
                    };
                    db.Contracts.Add(contract);
                    Console.WriteLine($"[DbSeeder] Tạo hợp đồng cho tài khoản: {acc.Username} ({contract.ContractNumber})");
                }
            }

            await db.SaveChangesAsync();
            Console.WriteLine("[DbSeeder] Seed xong!");
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Pbkdf2Iterations,
                numBytesRequested: 32);
            return $"PBKDF2${Pbkdf2Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }
    }
}
