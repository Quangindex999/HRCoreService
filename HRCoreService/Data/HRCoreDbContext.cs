using HRCoreDB.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRCoreDB.Data
{
    public class HRCoreDbContext : DbContext
    {
        public HRCoreDbContext(DbContextOptions<HRCoreDbContext> options) : base(options) { }

        // Các bảng trong hệ thống
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<ContractType> ContractTypes => Set<ContractType>();
        public DbSet<Contract> Contracts => Set<Contract>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Department ────────────────────────────────────────────────────

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.Id)
                      .HasDefaultValueSql("NEWID()");

                entity.Property(d => d.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(d => d.Code)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.HasIndex(d => d.Code) //code không được trùng
                      .IsUnique();

                entity.Property(d => d.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Self-referencing relationship
                entity.HasOne(d => d.Parent) //một phòng ban có thể có một phòng ban cha
                      .WithMany(d => d.Children) // một phòng ban cha có thể có nhiều phòng ban con
                      .HasForeignKey(d => d.ParentId) //khoá ngoại trỏ đến chính nó
                      .OnDelete(DeleteBehavior.Restrict); //không cho phép xóa phòng ban cha nếu còn phòng ban con, tránh mất dữ liệu con khi xóa cha
            });

            // ── Employee ──────────────────────────────────────────────────────

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .HasDefaultValueSql("NEWID()");

                entity.Property(e => e.EmployeeCode)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.HasIndex(e => e.EmployeeCode)
                      .IsUnique();

                entity.Property(e => e.FullName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(e => e.Email)
                      .IsUnique();

                entity.Property(e => e.ContractType)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.BaseSalary)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Status)
                      .HasMaxLength(20)
                      .HasDefaultValue("Active");

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // ── New fields config ────────────────────────────────────────
                entity.Property(e => e.Phone)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("");

                entity.Property(e => e.Address)
                      .IsRequired()
                      .HasMaxLength(500)
                      .HasDefaultValue("");

                entity.Property(e => e.HireDate)
                      .IsRequired(false);

                entity.Property(e => e.TaxCode)
                      .HasMaxLength(20)
                      .IsRequired(false);

                entity.Property(e => e.DependentsCount)
                      .HasDefaultValue(0);

                entity.Property(e => e.IdentityNumber)
                      .HasMaxLength(20)
                      .IsRequired(false);

                entity.Property(e => e.BankName)
                      .HasMaxLength(100)
                      .IsRequired(false);

                entity.Property(e => e.BankAccountNumber)
                      .HasMaxLength(30)
                      .IsRequired(false);

                entity.Property(e => e.BankBranch)
                      .HasMaxLength(100)
                      .IsRequired(false);

                // ── Login fields config ──────────────────────────────────────
                entity.Property(e => e.Username)
                      .HasMaxLength(50);

                // Index unique cho Username, nhưng chỉ áp dụng khi Username != NULL
                // (để nhiều nhân viên chưa có tài khoản không bị conflict)
                entity.HasIndex(e => e.Username)
                      .IsUnique()
                      .HasFilter("[Username] IS NOT NULL");

                entity.Property(e => e.PasswordHash)
                      .HasMaxLength(255);

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("Employee");

                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ContractType ──────────────────────────────────────────────────
            modelBuilder.Entity<ContractType>(entity =>
            {
                entity.HasKey(ct => ct.Id);
                entity.Property(ct => ct.Id).HasDefaultValueSql("NEWID()");

                entity.Property(ct => ct.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(ct => ct.Code)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasIndex(ct => ct.Code)
                      .IsUnique();

                entity.Property(ct => ct.DefaultSalaryRatio)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(1.0m);

                entity.Property(ct => ct.TaxType)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("Progressive");

                entity.Property(ct => ct.Description)
                      .HasMaxLength(500);

                entity.Property(ct => ct.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(ct => ct.UpdatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // ── Contract ──────────────────────────────────────────────────────
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasDefaultValueSql("NEWID()");

                entity.Property(c => c.ContractNumber)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasIndex(c => c.ContractNumber)
                      .IsUnique();

                entity.Property(c => c.BasicSalary)
                      .HasColumnType("decimal(18,2)");

                entity.Property(c => c.SalaryRatio)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(1.0m);

                entity.Property(c => c.Status)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("Draft");

                entity.Property(c => c.Notes)
                      .HasMaxLength(500);

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(c => c.UpdatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasOne(c => c.Employee)
                      .WithMany(e => e.Contracts)
                      .HasForeignKey(c => c.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ContractType)
                      .WithMany(ct => ct.Contracts)
                      .HasForeignKey(c => c.ContractTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}