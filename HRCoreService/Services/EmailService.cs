using HRCoreDB.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HRCoreService.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendContractNotificationAsync(Employee employee, Contract contract)
        {
            if (employee == null) throw new ArgumentNullException(nameof(employee));
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            string duration = contract.EndDate.HasValue 
                ? $"từ ngày {contract.StartDate:dd/MM/yyyy} đến ngày {contract.EndDate.Value:dd/MM/yyyy}" 
                : $"từ ngày {contract.StartDate:dd/MM/yyyy} (Không xác định thời hạn)";

            _logger.LogInformation("================================================================================");
            _logger.LogInformation("ĐÃ GỬI EMAIL THÔNG BÁO HỢP ĐỒNG CHO NHÂN VIÊN");
            _logger.LogInformation("Gửi đến Email: {Email}", employee.Email);
            _logger.LogInformation("Người nhận: {FullName} (Mã nhân viên: {Code})", employee.FullName, employee.EmployeeCode);
            _logger.LogInformation("Nội dung thông báo: Hợp đồng số '{ContractNumber}' với thời hạn {Duration} đã được thiết lập/ký kết thành công ở trạng thái: '{Status}'. Lương cơ bản: {Salary:N0} VND (Hệ số: {Ratio}).", 
                contract.ContractNumber, duration, contract.Status, contract.BasicSalary, contract.SalaryRatio);
            _logger.LogInformation("================================================================================");

            return Task.CompletedTask;
        }
    }
}
