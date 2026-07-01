using HRCoreDB.Data;
using HRCoreDB.DTOs;
using HRCoreDB.Entities;
using HRCoreDB.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace HRCoreDB.Extensions
{
    // ── Dependency Injection ──────────────────────────────────────────────────

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Đăng ký DbContext và các Repository vào DI container.
        /// Gọi trong Program.cs: builder.Services.AddHRCore(builder.Configuration);
        /// </summary>
        public static IServiceCollection AddHRCore(
            this IServiceCollection services,
            string connectionString)
        {
            // Đăng ký DbContext với connection string từ appsettings.json
            services.AddDbContext<HRCoreDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            return services;
        }
    }

    // ── Mapping helpers (dùng thay thế AutoMapper nếu dự án nhỏ) ─────────────

    public static class MappingExtensions
    {
        // Department → DTO
        public static DepartmentDto ToDto(this Department d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Code = d.Code,
            ParentId = d.ParentId,
            ParentName = d.Parent?.Name,
            CreatedAt = d.CreatedAt,
            Children = d.Children.Select(c => c.ToDto()).ToList()
        };

        // CreateDto → Entity
        public static Department ToEntity(this CreateDepartmentDto dto) => new()
        {
            Name = dto.Name,
            Code = dto.Code.ToUpperInvariant(),
            ParentId = dto.ParentId
        };

        // UpdateDto → patch existing entity (chỉ cập nhật các field không null)
        public static void ApplyTo(this UpdateDepartmentDto dto, Department entity)
        {
            entity.Name = dto.Name;
            if (dto.Code is not null)
                entity.Code = dto.Code.ToUpperInvariant();
            entity.ParentId = dto.ParentId;
        }

        // Employee → DTO
        public static EmployeeDto ToDto(this Employee e) => new()
        {
            Id = e.Id,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name ?? string.Empty,
            EmployeeCode = e.EmployeeCode,
            FullName = e.FullName,
            Email = e.Email,
            Position = e.Position,
            ContractType = e.ContractType,
            BaseSalary = e.BaseSalary,
            Phone = e.Phone,
            Address = e.Address,
            HireDate = e.HireDate,
            TaxCode = e.TaxCode,
            DependentsCount = e.DependentsCount,
            IdentityNumber = e.IdentityNumber,
            BankName = e.BankName,
            BankAccountNumber = e.BankAccountNumber,
            BankBranch = e.BankBranch,
            Status = e.Status,
            CreatedAt = e.CreatedAt
        };

        // CreateDto → Entity
        public static Employee ToEntity(this CreateEmployeeDto dto) => new()
        {
            DepartmentId = dto.DepartmentId,
            EmployeeCode = dto.EmployeeCode,
            FullName = dto.FullName,
            Email = dto.Email.ToLowerInvariant(),
            Position = dto.Position,
            ContractType = dto.ContractType,
            BaseSalary = dto.BaseSalary,
            Phone = dto.Phone,
            Address = dto.Address,
            HireDate = dto.HireDate,
            TaxCode = dto.TaxCode,
            DependentsCount = dto.DependentsCount,
            IdentityNumber = dto.IdentityNumber,
            BankName = dto.BankName,
            BankAccountNumber = dto.BankAccountNumber,
            BankBranch = dto.BankBranch
        };

        // UpdateDto → patch existing entity
        public static void ApplyTo(this UpdateEmployeeDto dto, Employee entity)
        {
            if (dto.DepartmentId.HasValue) entity.DepartmentId = dto.DepartmentId.Value;
            if (dto.FullName is not null) entity.FullName = dto.FullName;
            if (dto.Email is not null) entity.Email = dto.Email.ToLowerInvariant();
            if (dto.Position is not null) entity.Position = dto.Position;
            if (dto.ContractType is not null) entity.ContractType = dto.ContractType;
            if (dto.BaseSalary.HasValue) entity.BaseSalary = dto.BaseSalary.Value;
            if (dto.Phone is not null) entity.Phone = dto.Phone;
            if (dto.Address is not null) entity.Address = dto.Address;
            if (dto.HireDate.HasValue) entity.HireDate = dto.HireDate;
            if (dto.TaxCode is not null) entity.TaxCode = dto.TaxCode;
            if (dto.DependentsCount.HasValue) entity.DependentsCount = dto.DependentsCount.Value;
            if (dto.IdentityNumber is not null) entity.IdentityNumber = dto.IdentityNumber;
            if (dto.BankName is not null) entity.BankName = dto.BankName;
            if (dto.BankAccountNumber is not null) entity.BankAccountNumber = dto.BankAccountNumber;
            if (dto.BankBranch is not null) entity.BankBranch = dto.BankBranch;
            if (dto.Status is not null) entity.Status = dto.Status;
        }

        // ContractType → DTO
        public static ContractTypeDto ToDto(this ContractType ct) => new()
        {
            Id = ct.Id,
            Name = ct.Name,
            Code = ct.Code,
            DefaultSalaryRatio = ct.DefaultSalaryRatio,
            IsSocialInsuranceSubject = ct.IsSocialInsuranceSubject,
            TaxType = ct.TaxType,
            Description = ct.Description,
            CreatedAt = ct.CreatedAt,
            UpdatedAt = ct.UpdatedAt
        };

        // CreateContractTypeDto → Entity
        public static ContractType ToEntity(this CreateContractTypeDto dto) => new()
        {
            Name = dto.Name,
            Code = dto.Code.ToUpperInvariant(),
            DefaultSalaryRatio = dto.DefaultSalaryRatio,
            IsSocialInsuranceSubject = dto.IsSocialInsuranceSubject,
            TaxType = dto.TaxType,
            Description = dto.Description
        };

        // UpdateContractTypeDto → patch existing entity
        public static void ApplyTo(this UpdateContractTypeDto dto, ContractType entity)
        {
            if (dto.Name is not null) entity.Name = dto.Name;
            if (dto.Code is not null) entity.Code = dto.Code.ToUpperInvariant();
            if (dto.DefaultSalaryRatio.HasValue) entity.DefaultSalaryRatio = dto.DefaultSalaryRatio.Value;
            if (dto.IsSocialInsuranceSubject.HasValue) entity.IsSocialInsuranceSubject = dto.IsSocialInsuranceSubject.Value;
            if (dto.TaxType is not null) entity.TaxType = dto.TaxType;
            if (dto.Description is not null) entity.Description = dto.Description;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        // Contract → DTO
        public static ContractDto ToDto(this Contract c) => new()
        {
            Id = c.Id,
            ContractNumber = c.ContractNumber,
            EmployeeId = c.EmployeeId,
            EmployeeName = c.Employee?.FullName ?? string.Empty,
            ContractTypeId = c.ContractTypeId,
            ContractTypeName = c.ContractType?.Name ?? string.Empty,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            BasicSalary = c.BasicSalary,
            SalaryRatio = c.SalaryRatio,
            SignDate = c.SignDate,
            Status = c.Status,
            Notes = c.Notes,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };

        // CreateContractDto → Entity
        public static Contract ToEntity(this CreateContractDto dto) => new()
        {
            ContractNumber = dto.ContractNumber,
            EmployeeId = dto.EmployeeId,
            ContractTypeId = dto.ContractTypeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            BasicSalary = dto.BasicSalary,
            SalaryRatio = dto.SalaryRatio,
            SignDate = dto.SignDate,
            Status = dto.Status,
            Notes = dto.Notes
        };

        // UpdateContractDto → patch existing entity
        public static void ApplyTo(this UpdateContractDto dto, Contract entity)
        {
            if (dto.ContractNumber is not null) entity.ContractNumber = dto.ContractNumber;
            if (dto.ContractTypeId.HasValue) entity.ContractTypeId = dto.ContractTypeId.Value;
            if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) entity.EndDate = dto.EndDate;
            if (dto.BasicSalary.HasValue) entity.BasicSalary = dto.BasicSalary.Value;
            if (dto.SalaryRatio.HasValue) entity.SalaryRatio = dto.SalaryRatio.Value;
            if (dto.SignDate.HasValue) entity.SignDate = dto.SignDate.Value;
            if (dto.Status is not null) entity.Status = dto.Status;
            if (dto.Notes is not null) entity.Notes = dto.Notes;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}