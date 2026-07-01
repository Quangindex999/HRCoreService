using HRCoreDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRCoreService.Services
{
    public class ExpiredContractWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ExpiredContractWorker> _logger;

        public ExpiredContractWorker(IServiceProvider services, ILogger<ExpiredContractWorker> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpiredContractWorker đang khởi động...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("ExpiredContractWorker bắt đầu quét hợp đồng hết hạn lúc: {Time}", DateTimeOffset.Now);

                    using (var scope = _services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<HRCoreDbContext>();
                        var today = DateTime.UtcNow.Date;

                        // Lấy các hợp đồng đang hoạt động (Active) nhưng đã quá ngày kết thúc (EndDate < today)
                        var expiredContracts = await db.Contracts
                            .Where(c => c.Status == "Active" && c.EndDate != null && c.EndDate.Value.Date < today)
                            .ToListAsync(stoppingToken);

                        if (expiredContracts.Any())
                        {
                            _logger.LogInformation("Tìm thấy {Count} hợp đồng hết hạn.", expiredContracts.Count);

                            foreach (var contract in expiredContracts)
                            {
                                contract.Status = "Expired";
                                contract.UpdatedAt = DateTime.UtcNow;

                                // Kiểm tra xem nhân viên có hợp đồng Active nào khác gối đầu không
                                var hasOtherActiveContract = await db.Contracts
                                    .AnyAsync(c => c.EmployeeId == contract.EmployeeId && c.Status == "Active" && c.Id != contract.Id, stoppingToken);

                                if (!hasOtherActiveContract)
                                {
                                    var employee = await db.Employees.FindAsync(new object[] { contract.EmployeeId }, stoppingToken);
                                    if (employee != null && employee.Status == "Active")
                                    {
                                        employee.Status = "Inactive";
                                        _logger.LogWarning("Tài khoản nhân viên {Code} ({Name}) bị vô hiệu hóa do hết hạn hợp đồng.", 
                                            employee.EmployeeCode, employee.FullName);
                                    }
                                }
                            }

                            await db.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("Đã cập nhật trạng thái hợp đồng và vô hiệu hóa tài khoản thành công.");
                        }
                        else
                        {
                            _logger.LogInformation("Không phát hiện hợp đồng nào hết hạn hôm nay.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong ExpiredContractWorker.");
                }

                // Chạy quét lại sau 24 giờ (86400000 ms)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
